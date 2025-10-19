// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.DTOs.NotificationDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTech.API.Controllers
{
    public class NotificationController : APIController
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;

        public NotificationController(INotificationRepository notificationRepository, IAccountService accountService, UserManager<User> userManager)
        {
            _notificationRepository = notificationRepository;
            _accountService = accountService;
            _userManager = userManager;
        }

        [HttpPost("mark-as-seen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsSeenAsync([FromBody] NotificationUserDTO dto)
        {
            var userId = dto.userId;
            var user = await _userManager.Users.Where(x => x.Id == userId).Include(n => n.Notifications).FirstOrDefaultAsync();

            foreach (var notification in user.Notifications)
            {
                if (notification == null)
                {
                    return NotFound();
                }

                notification.IsSeen = true;
                notification.UpdatedOn = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);
            }

            return Ok(new { Message = "Notification marked as seen." });
        }

        [HttpPost("delete-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotificationsAsync([FromBody] NotificationUserDTO dto)
        {
            var userId = dto.userId;
            var user = await _userManager.Users.Where(x => x.Id == userId).Include(n => n.Notifications).FirstOrDefaultAsync();

            foreach (var notification in user.Notifications)
            {
                if (notification == null)
                {
                    return NotFound();
                }

                notification.IsSeen = true;
                notification.IsDeleted = true;
                notification.UpdatedOn = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);
            }

            return Ok(new { Message = "Notification marked as seen." });
        }

        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotificationsAsync([FromBody] DeleteNotificationDTO dto)
        {
            var id = dto.Id;
            var notification = await _notificationRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsSeen = true;
            notification.IsDeleted = true;
            notification.UpdatedOn = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);

            return Ok(new { Message = "Notification marked as seen." });
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySlugAsync(string userId)
        {
            var notifications = await _notificationRepository.GetAllAsync(x => !x.IsDeleted && x.UserId == userId);
            var user = await _accountService.CheckNotFoundByIdAsync(userId);
            var unseenCount = notifications.Count(x => !x.IsSeen);

            var notificationDTOs = notifications
                .OrderByDescending(t => t.CreatedOn)
                .Select(n => new NotificationResponseDTO
                {
                    Id = n.Id,
                    UserOrFullName = user.FullName is not null ? user.FullName : user.UserName,
                    Title = n.Title,
                    Description = n.Description,
                    ImageUrl = user.ImageUrl,
                    SendedOn = n.CreatedOn.GetValueOrDefault().ToLocalTime(),
                });

            var notificationData = new
            {
                data = notificationDTOs,
                count = unseenCount
            };

            return Ok(notificationData);
        }
    }
}
