using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace EventEase.Services
{
    public interface IAzureStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<bool> DeleteImageAsync(string blobUrl, string containerName);
        Task<string> GetBlobUrlAsync(string blobName, string containerName);
    }

    public class AzureStorageService : IAzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureStorageService> _logger;
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AzureStorageService(IConfiguration configuration, ILogger<AzureStorageService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") 
                               ?? configuration["AzureStorage:ConnectionString"];
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            
            if (!string.IsNullOrEmpty(_connectionString))
            {
                _blobServiceClient = new BlobServiceClient(_connectionString);
            }
            else
            {
                _logger.LogWarning("Azure Storage connection string not found. Using fallback.");
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided for upload");
                    return null;
                }

                // If Azure Storage is not configured, save file locally
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _logger.LogInformation("Azure Storage not configured, saving file locally");
                    return await SaveImageLocallyAsync(file, containerName);
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

                // Generate unique blob name
                var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload file
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    });
                }

                _logger.LogInformation($"Successfully uploaded {file.FileName} to Azure Storage");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading image to Azure Storage: {ex.Message}");
                // Return fallback URL if Azure upload fails
                return GetFallbackImageUrl(file.FileName);
            }
        }

        public async Task<bool> DeleteImageAsync(string blobUrl, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(blobUrl))
                {
                    return true; // Return true for fallback URLs
                }

                var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"Deleted blob {blobName}: {response.Value}");
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image from Azure Storage: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetBlobUrlAsync(string blobName, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    return GetFallbackImageUrl(blobName);
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                if (await blobClient.ExistsAsync())
                {
                    return blobClient.Uri.ToString();
                }
                
                return GetFallbackImageUrl(blobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting blob URL: {ex.Message}");
                return GetFallbackImageUrl(blobName);
            }
        }

        private async Task<string> SaveImageLocallyAsync(IFormFile file, string containerName)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", containerName);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                var relativePath = $"/images/{containerName}/{uniqueFileName}";
                var absoluteUrl = GetAbsoluteUrl(relativePath);
                _logger.LogInformation($"File saved locally: {relativePath} -> {absoluteUrl}");
                return absoluteUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file locally: {Message}", ex.Message);
                return GetAbsoluteUrl("/images/venues/default-venue.svg");
            }
        }

        private string GetFallbackImageUrl(string fileName)
        {
            // Return a default local image path as absolute URL
            return GetAbsoluteUrl("/images/venues/default-venue.svg");
        }

        private string GetAbsoluteUrl(string relativePath)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var scheme = request.Scheme;
                    var host = request.Host;
                    var pathBase = request.PathBase;
                    var absoluteUrl = $"{scheme}://{host}{pathBase}{relativePath}";
                    _logger.LogDebug($"Converted relative URL '{relativePath}' to absolute URL '{absoluteUrl}'");
                    return absoluteUrl;
                }
                else
                {
                    _logger.LogWarning("HttpContext is null, returning relative path");
                    return relativePath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating absolute URL for '{RelativePath}'", relativePath);
                return relativePath;
            }
        }
    }
}
