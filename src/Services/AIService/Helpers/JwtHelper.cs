using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AIService.Helpers
{
    public static class JwtHelper
    {
        public static Guid? GetUserIdFromHttpContext(HttpContext httpContext)
        {
            var userIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            // 尝试其他可能的声明名称
            userIdClaim = httpContext?.User?.FindFirst("sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var subUserId))
            {
                return subUserId;
            }

            userIdClaim = httpContext?.User?.FindFirst("userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId2))
            {
                return userId2;
            }

            return null;
        }

        public static string? GetUsernameFromHttpContext(HttpContext httpContext)
        {
            return httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                   httpContext?.User?.FindFirst("username")?.Value;
        }

        public static string? GetUserEmailFromHttpContext(HttpContext httpContext)
        {
            return httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ??
                   httpContext?.User?.FindFirst("email")?.Value;
        }

        public static string? GetClientIp(HttpContext httpContext)
        {
            return httpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        public static string? GetUserAgent(HttpContext httpContext)
        {
            return httpContext?.Request?.Headers["User-Agent"].ToString();
        }
    }
}