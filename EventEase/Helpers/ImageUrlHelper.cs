using EventEase.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.RegularExpressions;

namespace EventEase.Helpers
{
    public static class ImageUrlHelper
    {
        public static string ConvertImageUrl(this IHtmlHelper html, string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return imageUrl;

            try
            {
                var httpContextAccessor = html.ViewContext.HttpContext.RequestServices
                    .GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
                
                var request = httpContextAccessor?.HttpContext?.Request;
                if (request == null)
                    return imageUrl;

                // Check if it's a localhost URL (http or https) and convert it
                if (imageUrl.StartsWith("http://localhost") || imageUrl.StartsWith("https://localhost"))
                {
                    var uri = new Uri(imageUrl);
                    var relativePath = uri.AbsolutePath;
                    return $"{request.Scheme}://{request.Host}{relativePath}";
                }

                // If it's already an absolute URL with https (and not localhost), return as is
                if (imageUrl.StartsWith("https://") && !imageUrl.Contains("localhost"))
                    return imageUrl;

                // If it's a relative URL, make it absolute
                if (imageUrl.StartsWith("/"))
                    return $"{request.Scheme}://{request.Host}{imageUrl}";

                // Otherwise, treat as relative path
                return $"{request.Scheme}://{request.Host}/{imageUrl}";
            }
            catch
            {
                // If conversion fails, return original URL
                return imageUrl;
            }
        }

        public static string GetDynamicImageUrl(this IHtmlHelper html, string imageUrl)
        {
            return html.ConvertImageUrl(imageUrl);
        }
    }
}
