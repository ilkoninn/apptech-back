using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.NotificationVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTech.MVC.Controllers
{
    public class NotificationController : Controller
    {
        
        private readonly UserManager<User> _userManager;
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(UserManager<User> userManager, INotificationRepository notificationRepository)
        {
            _userManager = userManager;
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                var query = await (User.IsInRole("Admin")
                    ? _notificationRepository.GetAllAsync(x => true, u => u.User)
                    : _notificationRepository.GetAllAsync(x => !x.IsDeleted, u => u.User));


                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedNotifications = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedQueryable<Notification>(paginatedNotifications.AsQueryable(), pageIndex, totalPages);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading notifications: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new CreateNotificationVM
            {
                Users = users
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please ensure all fields are filled in correctly.";
                    var users = await _userManager.Users.ToListAsync();  
                    model.Users = users;
                    return View(model);
                }

                 var selectedUsers = model.SelectedUserIds;

                if (selectedUsers == null || !selectedUsers.Any())
                {
                    TempData["ErrorMessage"] = "Please select at least one user.";
                    model.Users = await _userManager.Users.ToListAsync();
                    return View(model);
                }

                foreach (var userId in selectedUsers)
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        var newNotification = new Notification
                        {
                            Title = model.Title,
                            Description = model.Description,
                            UserId = user.Id,
                            IsSeen = false,
                        };

                        if (user.Notifications is null)
                        {
                            user.Notifications = new List<Notification>();
                        }
                        await _notificationRepository.AddAsync(newNotification);
                    }
                }

                TempData["SuccessMessage"] = "Notification created successfully and sent to selected users.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the notification: \n{ex.Message}\n{ex.InnerException?.Message}";
                var users = await _userManager.Users.ToListAsync();  
                model.Users = users;
                return View(model);
            }
        }

    }
}
