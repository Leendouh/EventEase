using Microsoft.AspNetCore.Http;

namespace EventEase.Services
{
    public interface IUrlHelperService
    {
        string ConvertToAbsoluteUrl(string url);
        string GetBaseUrl();
    }

    public class UrlHelperService : IUrlHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlHelperService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string ConvertToAbsoluteUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            try
            {
                // If it's already an absolute URL with https, return as is
                if (url.StartsWith("https://"))
                    return url;

                // If it's an absolute URL with http (likely localhost), convert to current domain
                if (url.StartsWith("http://"))
                {
                    var uri = new Uri(url);
                    var relativePath = uri.AbsolutePath;
                    return GetBaseUrl() + relativePath;
                }

                // If it's a relative URL, make it absolute
                if (url.StartsWith("/"))
                    return GetBaseUrl() + url;

                // Otherwise, treat as relative path
                return GetBaseUrl() + "/" + url;
            }
            catch
            {
                // If conversion fails, return original URL
                return url;
            }
        }

        public string GetBaseUrl()
        {
            try
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    return $"{request.Scheme}://{request.Host}";
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
