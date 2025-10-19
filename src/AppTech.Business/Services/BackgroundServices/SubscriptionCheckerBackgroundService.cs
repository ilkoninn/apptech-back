// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class SubscriptionCheckerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SubscriptionCheckerBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredSubscriptions(stoppingToken);
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken); 
        }
    }

    private async Task CheckExpiredSubscriptions(CancellationToken stoppingToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var _subscriptionUserRepository = scope.ServiceProvider.GetRequiredService<ISubscriptionUserRepository>();

            var activeSubscriptions = await _subscriptionUserRepository.GetAllAsync(cu => !cu.IsDeleted);

            foreach (var subscription in activeSubscriptions)
            {
                if (subscription.ExpiredOn <= DateTime.UtcNow)
                {
                    await _subscriptionUserRepository.DeleteAsync(subscription);
                }
            }
        }
    }
}
