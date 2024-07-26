using System.Collections.Concurrent;
using System.Security.Claims;

namespace SwaggerAnalytics
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var emailClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userId = emailClaim ?? context.User.Identity?.Name ?? "Anonymous";

            if (!ActiveUsersService.ActiveUsers.TryGetValue(userId, out var userActivityInfo))
            {
                userActivityInfo = new UserActivityInfo
                {
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    Headers = context.Request.Headers
                        .Where(h => h.Key != "Authorization") // Skip Authorization header as it contains sensitive token information
                        .ToDictionary(h => h.Key, h => h.Value.ToString())
                };
                ActiveUsersService.ActiveUsers[userId] = userActivityInfo;
            }

            userActivityInfo.LastActivity = DateTime.UtcNow;
            userActivityInfo.RequestPaths.Add(context.Request.Path);

            await _next(context);

            var timeout = TimeSpan.FromMinutes(30);
            var cutoffTime = DateTime.UtcNow - timeout;

            foreach (var user in ActiveUsersService.ActiveUsers.Keys.ToList())
            {
                if (ActiveUsersService.ActiveUsers[user].LastActivity < cutoffTime)
                {
                    ActiveUsersService.ActiveUsers.TryRemove(user, out _);
                }
            }
        }
    }

    public class ActiveUsersService
    {
        public static readonly ConcurrentDictionary<string, UserActivityInfo> ActiveUsers = new();
    }

    public class UserActivityInfo
    {
        public DateTime LastActivity { get; set; }
        public string IpAddress { get; set; }
        public List<string> RequestPaths { get; set; } = new List<string>();
        public IDictionary<string, string> Headers { get; set; }
    }

}

