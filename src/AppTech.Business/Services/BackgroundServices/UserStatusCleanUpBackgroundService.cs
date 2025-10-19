using AppTech.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class UserStatusCleanUpBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public UserStatusCleanUpBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var offlineThreshold = DateTime.UtcNow.AddMinutes(-1);

                var users = userManager.Users.Where(u => u.OnlineTimer < offlineThreshold).ToList();

                foreach (var user in users)
                {
                    user.IsOnline = false;
                    user.OnlineTimer = null;
                    user.LastActivity = DateTime.UtcNow;
                    await userManager.UpdateAsync(user);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
