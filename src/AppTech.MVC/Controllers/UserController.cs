using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.UserVMs;
using Humanizer;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AppTech.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ITransactionTranslationRepository _transactionTranslationRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _http;

        public UserController(UserManager<User> userManager, ITransactionRepository transactionRepository, ITransactionTranslationRepository transactionTranslationRepository, IEmailService emailService, IExamResultRepository examResultRepository, IHttpContextAccessor http, INotificationRepository notificationRepository)
        {
            _transactionTranslationRepository = transactionTranslationRepository;
            _notificationRepository = notificationRepository;
            _transactionRepository = transactionRepository;
            _examResultRepository = examResultRepository;
            _emailService = emailService;
            _userManager = userManager;
            _http = http;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm = "", string role = "", string status = "", int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                TempData["CurrentSearchTerm"] = searchTerm;
                TempData["CurrentRole"] = role;
                TempData["CurrentStatus"] = status;

                var usersQuery = _userManager.Users.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                        u.UserName.ToLower().Contains(searchTerm.ToLower()) ||
                        u.Email.ToLower().Contains(searchTerm.ToLower()));
                }

                if (!string.IsNullOrEmpty(role))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    usersQuery = usersQuery.Where(u => usersInRole.Contains(u));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    var isBanned = status.ToLower() == "banned";
                    usersQuery = usersQuery.Where(u => u.IsBanned == isBanned);
                }

                var users = await usersQuery.ToListAsync();
                var userRolesViewModel = new List<UserRolesViewModel>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRolesViewModel.Add(new UserRolesViewModel
                    {
                        User = user,
                        Roles = roles.ToList()
                    });
                }

                var totalCount = userRolesViewModel.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedUsers = userRolesViewModel
                    .OrderBy(u => u.User.FullName)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedQueryable<UserRolesViewModel>(paginatedUsers.AsQueryable(), pageIndex, totalPages);

                ViewBag.PageSize = pageSize;

                TempData.Keep("CurrentSearchTerm");
                TempData.Keep("CurrentRole");
                TempData.Keep("CurrentStatus");

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePublicIp(string userId, int ipId)
        {
            try
            {
                var user = await _userManager.Users
                    .Include(u => u.UserPublicIpAddresses)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var ipToRemove = user.UserPublicIpAddresses.FirstOrDefault(ip => ip.Id == ipId);
                if (ipToRemove != null)
                {
                    user.UserPublicIpAddresses.Remove(ipToRemove);
                    await _userManager.UpdateAsync(user);
                }

                TempData["SuccessMessage"] = "Public IP deleted successfully.";
                return RedirectToAction("Details", new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id, int pageIndex = 1, int pageSize = 5)
        {
            try
            {
                var user = await _userManager.Users
                    .Where(u => u.Id == id)
                    .Include(u => u.UserPublicIpAddresses)
                    .Include(u => u.ExamResults)
                        .ThenInclude(er => er.Exam)
                        .ThenInclude(e => e.Certification)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Azerbaijan Standard Time");

                user.LastExamActivity = user.LastExamActivity.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(user.LastExamActivity.Value, timeZone)
                    : (DateTime?)null;

                user.LastActivity = user.LastActivity.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(user.LastActivity.Value, timeZone)
                    : (DateTime?)null;

                var totalResults = user.ExamResults.Count();

                // Convert each ExamResult.CreatedOn (UTC) to local time
                var paginatedResults = user.ExamResults
                    .OrderByDescending(r => r.CreatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r =>
                    {
                        r.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(r.CreatedOn.GetValueOrDefault(), timeZone);
                        return r;
                    })
                    .ToList();

                var allRoles = Enum.GetNames(typeof(EUserRole)).ToList();
                var userRoles = await _userManager.GetRolesAsync(user);

                var model = new UserDetailsViewModel
                {
                    User = user,
                    ExamResults = paginatedResults,
                    PageIndex = pageIndex,
                    TotalPages = (int)Math.Ceiling(totalResults / (double)pageSize),
                    UserRoles = userRoles.ToList(),
                    AllRoles = allRoles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExamResult(int examResultId, int newScore, string isPassed)
        {
            var examResult = await _examResultRepository.GetByIdAsync(er => er.Id == examResultId && !er.IsDeleted);

            try
            {
                if (examResult == null)
                {
                    TempData["ErrorMessage"] = "Exam result not found.";
                    return RedirectToAction("Details", new { id = examResult.UserId });
                }

                examResult.UserScore = newScore;
                examResult.IsPassed = isPassed == "true" ? true : false;

                await _examResultRepository.UpdateAsync(examResult);

                TempData["SuccessMessage"] = "Exam result updated successfully.";
                return RedirectToAction("Details", new { id = examResult.UserId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Details", new { id = examResult.UserId });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBalance(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBalance(string id, string addBalance)
        {
            var lang = new LanguageCatcher(_http).GetLanguage();

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var decimalBalance = Convert.ToDecimal(addBalance, CultureInfo.InvariantCulture);
                var transactionBalance = decimalBalance;

                user.Balance += decimalBalance;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id });
                }

                var orderIdFromResponse = new Random().Next(10000, 99999).ToString();
                var sessionIdFromResponse = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 12)
                    .Select(s => s[new Random().Next(s.Length)]).ToArray());
                var checkToken = GeneratePaymentCheckToken();

                var newTransaction = new Transaction()
                {
                    UserId = id,
                    OrderId = long.Parse(orderIdFromResponse),
                    Amount = transactionBalance,
                    SessionId = sessionIdFromResponse,
                    CheckToken = checkToken,
                    Type = EOrderType.CASH,
                    Status = EOrderStatus.FULLYPAID,
                    IsIncreased = true
                };

                var createdTransaction = await _transactionRepository.AddAsync(newTransaction);

                var newAzTransactionTranslation = new TransactionTranslation
                {
                    TransactionId = createdTransaction.Id,
                    Description = "Balans admin tərəfindən artırıldı.",
                    Language = ELanguage.Az
                };

                await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

                var newEnTransactionTranslation = new TransactionTranslation
                {
                    TransactionId = createdTransaction.Id,
                    Description = "Balance increased by admin.",
                    Language = ELanguage.En
                };

                await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

                var newRuTransactionTranslation = new TransactionTranslation
                {
                    TransactionId = createdTransaction.Id,
                    Description = "Баланс увеличен администратором.",
                    Language = ELanguage.Ru
                };

                await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);

                var notificationTitle = string.Empty;
                var userInfo = user.FullName is not null ? user.FullName : user.UserName;

                switch (lang)
                {
                    case "az":
                        notificationTitle = "Admin tərəfindən balansınız artırıldı.";
                        break;
                    case "ru":
                        notificationTitle = "Администратор пополнил ваш баланс.";
                        break;
                    case "en":
                    default:
                        notificationTitle = "Your balance has been topped up by the admin.";
                        break;
                }

                var newNotification = new Notification
                {
                    Title = notificationTitle,
                    UserId = user.Id,
                };

                await _notificationRepository.AddAsync(newNotification);

                TempData["SuccessMessage"] = "Balance updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(string id, int pageIndex = 1)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                user.IsBanned = true;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                TempData["SuccessMessage"] = $"User {user.UserName} has been banned.";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while banning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnExamUser(string id, int pageIndex = 1)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                user.OnExam = false;
                user.LastExamActivity = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                TempData["SuccessMessage"] = $"User {user.UserName} has been took exam offline.";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while banning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnOnlineUser(string id, int pageIndex = 1)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                user.IsOnline = false;
                user.OnlineTimer = null;
                user.LastActivity = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                TempData["SuccessMessage"] = $"User {user.UserName} has been took user offline.";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while banning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnBanUser(string id, int pageIndex = 1)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                user.IsBanned = false;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                try
                {
                    await _emailService.SendUnbanMailAsync(user.Email, user);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"User was unbanned, but email could not be sent. Error: \n{ex.Message}\n{ex.InnerException?.Message}";
                    return RedirectToAction(nameof(Index), new { pageIndex });
                }

                TempData["SuccessMessage"] = $"User {user.UserName} was successfully unbanned and an email has been sent.";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while unbanning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index), new { pageIndex });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = $"User {user.UserName} deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = $"Role {role} assigned to {user.UserName} successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = $"Role {role} removed from {user.UserName} successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePassword(string id, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private string GeneratePaymentCheckToken()
        {
            byte[] numberBytes = BitConverter.GetBytes(DateTime.Now.Ticks);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(numberBytes);
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashString.Append(b.ToString("x2"));
                }

                return hashString.ToString();
            }
        }
    }
}
