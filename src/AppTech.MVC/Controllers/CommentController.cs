using AppTech.Business.DTOs.CommentDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AppTech.MVC.Controllers
{
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;
        private readonly ICommentRepository _commentRepository;
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        public CommentController(ICommentService commentService, ICommentRepository commentRepository, UserManager<User> userManager, IAccountService accountService, IEmailService emailService)
        {
            _commentService = commentService;
            _commentRepository = commentRepository;
            _userManager = userManager;
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", string userFilter = "", string certificationFilter = "", int pageIndex = 1, int pageSize = 8)
        {
            try
            {
                TempData["CurrentSearchTerm"] = searchTerm;
                TempData["CurrentUserFilter"] = userFilter;
                TempData["CurrentCertificationFilter"] = certificationFilter;

                IEnumerable<Comment> query = await _commentRepository.GetAllAsync(x => true, u => u.User, c => c.Certification);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.Subject.ToLower().Contains(searchTerm.ToLower()));
                }

                if (!string.IsNullOrEmpty(userFilter))
                {
                    query = query.Where(c =>
                        (c.User.FullName != null && c.User.FullName.ToLower().Contains(userFilter.ToLower())) ||
                        c.User.UserName.ToLower().Contains(userFilter.ToLower()));
                }

                if (!string.IsNullOrEmpty(certificationFilter))
                {
                    query = query.Where(c => c.Certification.Title.ToLower().Contains(certificationFilter.ToLower()));
                }

                var count = query.Count();
                var selectedQuery = query
                    .OrderByDescending(t => t.UpdatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .AsQueryable();

                var totalPages = (int)Math.Ceiling(count / (double)pageSize);
                var data = new PaginatedQueryable<Comment>(selectedQuery, pageIndex, totalPages);

                TempData.Keep("CurrentSearchTerm");
                TempData.Keep("CurrentUserFilter");
                TempData.Keep("CurrentCertificationFilter");

                ViewBag.PageSize = pageSize;

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(string id, int pageIndex = 1)
        {
            try
            {
                var oldUser = await _accountService.CheckNotFoundByIdAsync(id);

                if (oldUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                oldUser.IsBanned = true;
                _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

                TempData["SuccessMessage"] = $"User {oldUser.UserName} has been banned successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while banning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnBanUser(string id, int pageIndex = 1)
        {
            try
            {
                var oldUser = await _accountService.CheckNotFoundByIdAsync(id);

                if (oldUser == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                oldUser.IsBanned = false;
                await _emailService.SendUnbanMailAsync(oldUser.Email, oldUser);
                _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

                TempData["SuccessMessage"] = $"User {oldUser.UserName} has been unbanned successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while unbanning the user: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                await _commentService.DeleteAsync(new DeleteCommentDTO() { Id = id });

                TempData["SuccessMessage"] = "Comment deleted successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the comment: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _commentRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Comment not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _commentRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Comment removed successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the comment: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _commentRepository.GetByIdAsync(x => x.Id == id);
                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Comment not found.";
                    return RedirectToAction("Index", new { pageIndex });
                }

                await _commentRepository.RecoverAsync(entity);

                TempData["SuccessMessage"] = "Comment recovered successfully.";
                return RedirectToAction("Index", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the comment: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", new { pageIndex });
            }
        }
 
    }
}
