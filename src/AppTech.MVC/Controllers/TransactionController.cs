using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AppTech.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TransactionController : Controller
    {
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly ISubscriptionUserRepository _subscriptionUserRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly ITransactionHandler _transactionHandler;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IBankService _bankService;

        public TransactionController(ITransactionRepository transactionRepository, ITransactionHandler transactionHandler, IBankService bankService, UserManager<User> userManager, ICertificationUserRepository certificationUserRepository, ISubscriptionUserRepository subscriptionUserRepository, IExamResultRepository examResultRepository, IEmailService emailService)
        {
            _transactionRepository = transactionRepository;
            _transactionHandler = transactionHandler;
            _bankService = bankService;
            _userManager = userManager;
            _certificationUserRepository = certificationUserRepository;
            _subscriptionUserRepository = subscriptionUserRepository;
            _examResultRepository = examResultRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string query = "", string status = "", int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                TempData["CurrentQuery"] = query;
                TempData["CurrentStatus"] = status;

                var isAdmin = User.IsInRole("Admin");
                var transactionsQuery = await _transactionRepository.GetAllAsync(t => isAdmin ? true : !t.IsDeleted, u => u.User);

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EOrderStatus>(status, out var parsedStatus))
                {
                    transactionsQuery = transactionsQuery.Where(t => t.Status == parsedStatus).ToList();
                }

                if (!string.IsNullOrEmpty(query))
                {
                    transactionsQuery = transactionsQuery.Where(t =>
                        t.User.UserName.ToLower().Contains(query.ToLower()) ||
                        t.User.Email.ToLower().Contains(query.ToLower()) ||
                        (t.User.FullName != null && t.User.FullName.ToLower().Contains(query.ToLower())) ||
                        t.OrderId.ToString().Contains(query)
                    ).ToList();
                }

                var totalRecords = transactionsQuery.Count;
                var transactions = transactionsQuery
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var data = new PaginatedQueryable<Transaction>(transactions.AsQueryable(), pageIndex, totalPages);

                ViewBag.PageSize = pageSize;
                TempData.Keep("CurrentQuery");
                TempData.Keep("CurrentStatus");

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var transaction = _transactionHandler.HandleEntityAsync(
                    await _transactionRepository.GetByIdAsync(x => x.Id == id, u => u.User, c => c.Certification));

                return View(transaction);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while fetching the transaction details: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _transactionRepository.DeleteAsync(
                    _transactionHandler.HandleEntityAsync(
                    await _transactionRepository.GetByIdAsync(x => x.Id == id)));

                TempData["SuccessMessage"] = "Transaction deleted successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Transaction?pageIndex={pageIndex}" : "/Transaction";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the transaction: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = _transactionHandler.HandleEntityAsync(
                    await _transactionRepository.GetByIdAsync(x => x.Id == id));

                await _transactionRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Transaction removed successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Transaction?pageIndex={pageIndex}" : "/Transaction";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the transaction: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = _transactionHandler.HandleEntityAsync(
                    await _transactionRepository.GetByIdAsync(x => x.Id == id));
                await _transactionRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Transaction recovered successfully.";
                var redirectUrl = pageIndex != 0 ? $"/Transaction?pageIndex={pageIndex}" : "/Transaction";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the transaction: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }


        // Supportive Methods
        [HttpGet]
        public async Task<IActionResult> Refund(int id)
        {
            try
            {
                // Fetch the transaction entity
                var entity = await _transactionRepository.GetByIdAsync(x => x.Id == id);
                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Call the refund API
                var res = await _bankService.RefundPaymentAsync(entity.OrderId.ToString(), entity.Amount);

                if (res.IsSuccessStatusCode)
                {
                    // Update transaction status to Refunded
                    entity.Status = EOrderStatus.REFUNDED;
                    entity.ResponseBody = await res.Content.ReadAsStringAsync();

                    // Get the associated user
                    var user = await _userManager.Users
                        .Where(x => x.Id == entity.UserId)
                        .Include(x => x.SubscriptionUsers)
                        .ThenInclude(x => x.Subscription)
                        .ThenInclude(x => x.Certification)
                        .ThenInclude(x => x.Exams)
                        .Include(x => x.CertificationUsers)
                        .ThenInclude(x => x.Certification)
                        .Include(x => x.ExamResults)
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Handle refund based on the type of transaction
                    if (entity.IsIncreased)
                    {
                        // If the transaction was a balance increase, reverse the balance
                        user.Balance -= entity.Amount;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        // If the user purchased a certification, remove it
                        if (entity.CertificationId is not null)
                        {
                            var certificationUser = await _certificationUserRepository
                                .GetByIdAsync(x => x.UserId == user.Id && x.CertificationId == entity.CertificationId);

                            if (certificationUser != null)
                            {
                                await _certificationUserRepository.RemoveAsync(certificationUser);
                            }
                        }

                        // If the user purchased a subscription, remove it
                        if (entity.SubscriptionId is not null)
                        {
                            var subscriptionUser = await _subscriptionUserRepository
                                .GetByIdAsync(x => x.UserId == user.Id && x.SubscriptionId == entity.SubscriptionId);

                            if (subscriptionUser != null)
                            {
                                await _subscriptionUserRepository.RemoveAsync(subscriptionUser);
                            }

                            // Remove associated exam results, if any
                            var examResult = await _examResultRepository
                                .GetByIdAsync(x => x.UserId == user.Id && x.ExamId == subscriptionUser.Subscription.Certification.Exams.FirstOrDefault().Id);

                            if (examResult != null)
                            {
                                await _examResultRepository.RemoveAsync(examResult);
                            }
                        }
                    }

                    // Save changes to the transaction
                    await _transactionRepository.UpdateAsync(entity);

                    _emailService.SendRefundMail(entity, user);

                    TempData["SuccessMessage"] = "Transaction refunded successfully.";
                    return RedirectToAction(nameof(Detail), new { id = entity.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Refund failed: {res.RequestMessage}";
                    return RedirectToAction(nameof(Detail), new { id = entity.Id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing the refund: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
