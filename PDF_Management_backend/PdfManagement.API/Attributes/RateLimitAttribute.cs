using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PdfManagement.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        public string Name { get; set; } = "Default";
        public int Seconds { get; set; } = 60;
        public int Limit { get; set; } = 10;

        private static MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{Name}_{ipAddress}";

            // Check if the key exists in cache
            if (!Cache.TryGetValue(key, out int hitCount))
            {
                // First hit, add to cache
                hitCount = 1;
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(Seconds));
                Cache.Set(key, hitCount, cacheEntryOptions);
            }
            else
            {
                // Increment hit count
                hitCount++;
                if (hitCount > Limit)
                {
                    // Rate limit exceeded
                    context.Result = new ContentResult
                    {
                        Content = "Rate limit exceeded. Please try again later.",
                        StatusCode = (int)HttpStatusCode.TooManyRequests
                    };
                    return;
                }
                Cache.Set(key, hitCount);
            }

            // Add rate limit headers
            context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", Limit.ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", (Limit - hitCount).ToString());

            await next();
        }
    }
}
