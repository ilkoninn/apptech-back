// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppTech.Business.Services.BackgroundServices
{
    public class UserPublicIpAddressCleanUpBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserPublicIpAddressCleanUpBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RemoveExpiredIpAddresses(stoppingToken);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); 
            }
        }

        private async Task RemoveExpiredIpAddresses(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _userPublicIpAddressRepository = scope.ServiceProvider.GetRequiredService<IPublicIpAddressRepository>();

                var expiredIpAddresses = await _userPublicIpAddressRepository.GetAllAsync(ip =>
                    !ip.IsDeleted && ip.ExpiredOn <= DateTime.UtcNow);

                foreach (var ipAddress in expiredIpAddresses)
                {
                    await _userPublicIpAddressRepository.RemoveAsync(ipAddress);
                }
            }
        }
    }

}
