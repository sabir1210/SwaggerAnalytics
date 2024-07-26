using SwaggerAnalytics;

namespace SwaggerAnalytics
{
    public class UserCleanupService : IHostedService, IDisposable
    {
        private Timer _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CleanupInactiveUsers, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        private void CleanupInactiveUsers(object state)
        {
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
