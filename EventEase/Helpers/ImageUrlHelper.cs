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

                // If it's already an absolute URL with https, return as is
                if (imageUrl.StartsWith("https://"))
                    return imageUrl;

                // If it's an absolute URL with http (likely localhost), convert to current domain
                if (imageUrl.StartsWith("http://"))
                {
                    var uri = new Uri(imageUrl);
                    var relativePath = uri.AbsolutePath;
                    return $"{request.Scheme}://{request.Host}{relativePath}";
                }

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
