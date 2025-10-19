using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.ExternalServices.Abstractions;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AppTech.MVC.ViewModels.UserVMs;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTech.MVC.Controllers
{
    public class ExamResultController : Controller
    {
        private readonly IExamResultRepository _examResultRepository;
        private readonly IDigitalOceanService _digitalOceanService;
        private readonly IDropletRepository _dropletRepository;
        private readonly IExamCacheService _examCacheService;
        private readonly UserManager<User> _userManager;
        private readonly IExamService _examService;

        public ExamResultController(IExamResultRepository examResultRepository, IExamService examService, UserManager<User> userManager, IDropletRepository dropletRepository, IDigitalOceanService digitalOceanService, IExamCacheService examCacheService)
        {
            _examResultRepository = examResultRepository;
            _examService = examService;
            _userManager = userManager;
            _dropletRepository = dropletRepository;
            _digitalOceanService = digitalOceanService;
            _examCacheService = examCacheService;
        }

        public async Task<IActionResult> Index(string query, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var baseQuery = await (User.IsInRole("Admin")
                   ? _examResultRepository.GetAllAsync(x => true, e => e.Exam, u => u.User, eu => eu.Droplets)
                   : _examResultRepository.GetAllAsync(x => !x.IsDeleted, e => e.Exam, u => u.User, eu => eu.Droplets));

                if (!string.IsNullOrEmpty(query))
                {
                    baseQuery = baseQuery
                        .Where(x =>
                            x.Exam.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            x.User.UserName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            x.Droplets.Any(d => d.ProjectId.Contains(query, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                var totalCount = baseQuery.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var paginatedExamResults = baseQuery
                    .OrderByDescending(x => x.CreatedOn)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedQueryable<ExamResult>(paginatedExamResults.AsQueryable(), pageIndex, totalPages);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading exam results: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Error");
            }
        }


        public async Task<IActionResult> Create()
        {
            var examResultViewModel = new ExamResultViewModel()
            {
                Exams = await _examService.GetAllAsync(),
                Users = await _userManager.Users.ToListAsync()
            };

            return View(examResultViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExamResultViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var exam = await _examService.GetByIdAsync(new GetByIdExamDTO() { Id = model.ExamId });
                var examResult = new ExamResult()
                {
                    ExamId = model.ExamId,
                    UserId = model.UserId
                };

                await _examResultRepository.AddAsync(examResult);
                TempData["SuccessMessage"] = "Exam result saved successfully for the selected user.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the exam result: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            var examResult = await _examResultRepository.GetByIdAsync(x => x.Id == id, x => x.User, x => x.Exam, x => x.Droplets);
            if (examResult == null)
            {
                TempData["ErrorMessage"] = "Exam result not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateExamResultViewModel()
            {
                Id = examResult.Id,
                UserId = examResult.UserId,
                ExamId = examResult.ExamId,
                ProjectId = examResult.Droplets?.FirstOrDefault()?.ProjectId,
                VpcId = examResult.Droplets?.FirstOrDefault()?.VpcId,
                UserScore = examResult.UserScore.GetValueOrDefault(),
                IsPassed = examResult.IsPassed.GetValueOrDefault(),
                Exams = await _examService.GetAllAsync(),
                Users = await _userManager.Users.ToListAsync(),
                Droplets = examResult.Droplets?.Where(x => x.IsDeleted == false)?.ToList() ?? new List<Droplet>()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(UpdateExamResultViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Exams = await _examService.GetAllAsync();
                model.Users = await _userManager.Users.ToListAsync();
                return View(model);
            }

            try
            {
                var examResult = await _examResultRepository.GetByIdAsync(x => x.Id == model.Id, x => x.Droplets);
                if (examResult == null)
                {
                    TempData["ErrorMessage"] = "Exam result not found.";
                    return RedirectToAction(nameof(Index));
                }

                examResult.UserId = model.UserId;
                examResult.ExamId = model.ExamId;
                examResult.UserScore = model.UserScore;
                examResult.IsPassed = model.IsPassed;
                examResult.UpdatedOn = DateTime.Now;

                if (model.Droplets != null)
                {
                    foreach (var dropletModel in model.Droplets)
                    {
                        var droplet = examResult.Droplets.FirstOrDefault(d => d.Id == dropletModel.Id);
                        if (droplet != null)
                        {
                            droplet.CustomUsername = dropletModel.CustomUsername;
                            droplet.Password = dropletModel.Password;
                            droplet.PublicIpAddress = dropletModel.PublicIpAddress;
                            droplet.UpdatedOn = DateTime.Now;
                        }
                        else
                        {
                            examResult.Droplets.Add(new Droplet
                            {
                                ExamResultId = examResult.Id,
                                CustomUsername = dropletModel.CustomUsername,
                                Password = dropletModel.Password,
                                MachineId = dropletModel.MachineId,
                                ProjectId = dropletModel.ProjectId,
                                PublicIpAddress = dropletModel.PublicIpAddress,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }
                }

                await _examResultRepository.UpdateAsync(examResult);
                TempData["SuccessMessage"] = "Exam result updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the exam result: " + ex.Message;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examResultRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == id);
                await _examResultRepository.DeleteAsync(entity);

                TempData["SuccessMessage"] = "Exam deleted successfully.";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examResultRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("Index", "ExamResult", new { pageIndex });
                }

                await _examResultRepository.RemoveAsync(entity);

                TempData["SuccessMessage"] = "Exam removed successfully.";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while removing the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDroplet(int id)
        {
            try
            {
                var droplet = await _dropletRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == id);
                var dropletCount = (await _dropletRepository.GetAllAsync(x => !x.IsDeleted)).Count;
                var token = Guid.NewGuid().ToString();

                if (droplet == null)
                {
                    return NotFound();
                }

                if(dropletCount == 1)
                {
                    var backUpDroplet = await _dropletRepository
                        .GetByIdAsync(x => !x.IsDeleted && x.Id == id);

                    var dropletMetaData = new DropletMetaDataDTO
                    {
                        ProjectId = backUpDroplet.ProjectId,
                        VolumeId = backUpDroplet.VolumeId,
                        VpcId = backUpDroplet.VpcId
                    };

                    _examCacheService.StoreDropletMetaData(token, dropletMetaData);
                }

                await _digitalOceanService.DeleteDropletAsync(Convert.ToInt64(droplet.MachineId));
                await _dropletRepository.RemoveAsync(droplet);

                if (dropletCount == 1)
                {
                    var metaData = _examCacheService.GetDropletMetaData(token);

                    if (metaData != null)
                    {
                        var checkProject = await _digitalOceanService.DeleteProjectAsync(metaData.ProjectId);

                        if (!checkProject)
                        {
                            throw new Exception("Project has not been deleted.");
                        }

                        await Task.Delay(45000);

                        var checkVpc = await _digitalOceanService.DeleteVpcAsync(metaData.VpcId);

                        if (!checkProject)
                        {
                            throw new Exception("VPC has not been deleted.");
                        }

                        await Task.Delay(5000);

                        var checkVolume = await _digitalOceanService.DeleteVolumeAsync(metaData.VolumeId);

                        _examCacheService.RemoveDropletMetaData(token);
                    }
                }                    

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Recover(int id, int pageIndex = 1)
        {
            try
            {
                var entity = await _examResultRepository.GetByIdAsync(x => x.Id == id);

                if (entity == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("Index", "ExamResult", new { pageIndex });
                }

                await _examResultRepository.RecoverAsync(entity);
                TempData["SuccessMessage"] = "Exam recovered successfully.";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while recovering the exam: \n{ex.Message}\n{ex.InnerException?.Message}";
                return RedirectToAction("Index", "ExamResult", new { pageIndex });
            }
        }
    }
}
