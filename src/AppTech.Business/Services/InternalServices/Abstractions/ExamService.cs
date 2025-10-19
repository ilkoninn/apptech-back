using System;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.DTOs.UserDTOs;
using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Abstractions;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class ExamService : IExamService
    {
        protected readonly IDropZoneDragVariantRepository _dragZoneDragVariantRepository;
        protected readonly ICertificationUserRepository _certificationUserRepository;
        protected readonly ISubscriptionUserRepository _subscriptionUserRepository;
        protected readonly IDragDropQuestionRepository _dragDropQuestionRepository;
        protected readonly ICertificationRepository _certificationRepository;
        protected readonly ISubscriptionRepository _subscriptionRepository;
        protected readonly IDragVariantRepository _dragVariantRepository;
        protected readonly IExamResultRepository _examResultRepository;
        protected readonly IDigitalOceanService _digitalOceanService;
        protected readonly IQuestionRepository _questionRepository;
        protected readonly IDropZoneRepository _dropZoneRepository;
        protected readonly IDropletRepository _dropletRepository;
        protected readonly ICompanyRepository _companyRepository;
        protected readonly IExamCacheService _examCacheService;
        protected readonly IAccountService _accountService;
        protected readonly IExamRepository _examRepository;
        protected readonly UserManager<User> _userManager;
        protected readonly IHttpContextAccessor _http;
        protected readonly IExamHandler _examHandler;
        protected readonly IMapper _mapper;

        public ExamService(IExamRepository ExamRepository, IMapper mapper,
            IExamHandler examHandler,
            IQuestionRepository questionRepository,
            ICertificationRepository certificationRepository, UserManager<User> userManager,
            IHttpContextAccessor http, ICompanyRepository companyRepository, IAccountService accountService,
            ICertificationUserRepository certificationUserRepository, IExamResultRepository examResultRepository,
            IExamCacheService examCacheService, IDigitalOceanService digitalOceanService,
            IDragDropQuestionRepository dragDropQuestionRepository, IDropZoneRepository dropZoneRepository,
            IDropZoneDragVariantRepository dragZoneDragVariantRepository, IDragVariantRepository dragVariantRepository,
            ISubscriptionRepository subscriptionRepository, ISubscriptionUserRepository subscriptionUserRepository,
            IDropletRepository dropletRepository)
        {
            _dragZoneDragVariantRepository = dragZoneDragVariantRepository;
            _certificationUserRepository = certificationUserRepository;
            _subscriptionUserRepository = subscriptionUserRepository;
            _dragDropQuestionRepository = dragDropQuestionRepository;
            _certificationRepository = certificationRepository;
            _subscriptionRepository = subscriptionRepository;
            _dragVariantRepository = dragVariantRepository;
            _examResultRepository = examResultRepository;
            _digitalOceanService = digitalOceanService;
            _dropZoneRepository = dropZoneRepository;
            _questionRepository = questionRepository;
            _companyRepository = companyRepository;
            _dropletRepository = dropletRepository;
            _examCacheService = examCacheService;
            _examRepository = ExamRepository;
            _accountService = accountService;
            _examHandler = examHandler;
            _userManager = userManager;
            _mapper = mapper;
            _http = http;
        }

        public async Task<ResponseTerminalDTO> StartExamWithDropletAsync(StartTerminalExamDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var examCertCode = (await _examRepository.GetByIdAsync(
                x => !x.IsDeleted && x.Id == dto.examId, c => c.Certification)).Certification.Code;
            var username = user.UserName ?? "TerminalUser_" + Guid.NewGuid().ToString();
            var password = GenerateOneTimePassword();

            var dropletDTO = new CreateDropletDTO
            {
                Region = "fra1",
                Size = "s-2vcpu-4gb",
                Image = "centos-stream-9-x64",
                Username = username,
                Password = password,
                CertificationCode = examCertCode
            };

            var droplets = await _digitalOceanService.CreateDropletAndAttachVolumeAsync(dropletDTO);

            if (droplets.Count != 0)
            {
                var dropletTokens = new List<string>();

                foreach (var item in droplets)
                {
                    await _dropletRepository.AddAsync(item);

                    dropletTokens.Add(item.Token);
                }

                var wsUrls = dropletTokens.Select(tok =>
                    $"wss://auth.apptech.edu.az/ws-terminal?token={tok}").ToList();

                return new ResponseTerminalDTO
                {
                    WebSocketUrls = wsUrls,
                };
            }

            else
            {
                return new ResponseTerminalDTO
                {
                    WebSocketUrls = new List<string> { "Error" }
                };
            }
        }

        private string GenerateOneTimePassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<RandomExamDTO> ResetExamAsync(CreateRandomExamDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var examCertCode = (await _examRepository.GetByIdAsync(
                x => !x.IsDeleted && x.Id == dto.examId, c => c.Certification)).Certification.Code;
            var username = user.UserName ?? "TerminalUser_" + Guid.NewGuid().ToString();
            var password = GenerateOneTimePassword();

            var dropletDTO = new CreateDropletDTO
            {
                Region = "fra1",
                Size = "s-2vcpu-4gb",
                Image = "centos-stream-9-x64",
                Username = username,
                Password = password,
                CertificationCode = examCertCode
            };

            var storedExam = _examCacheService.GetStoredExam(dto.examToken);
            var backUpToken = ExtractTokenFromUrl(storedExam.Terminals.FirstOrDefault());
            var backUpDroplet = await _dropletRepository
                .GetByIdAsync(x => !x.IsDeleted && x.VolumeId != null) ??
                await _dropletRepository
                .GetByIdAsync(x => !x.IsDeleted && x.Token == backUpToken);

            var dropletMetaData = new DropletMetaDataDTO
            {
                ProjectId = backUpDroplet.ProjectId,
                VolumeId = backUpDroplet.VolumeId,
                VpcId = backUpDroplet.VpcId
            };

            _examCacheService.StoreDropletMetaData(dto.examToken, dropletMetaData);

            if (storedExam == null)
            {
                throw new Exception("Exam not found in cache.");
            }

            foreach (var terminal in storedExam.Terminals)
            {
                var token = ExtractTokenFromUrl(terminal);
                var droplet = await _dropletRepository.GetByIdAsync(x => !x.IsDeleted && x.Token == token);

                await _digitalOceanService.DeleteDropletAsync(Convert.ToInt64(droplet.MachineId));

                await Task.Delay(3000);                

                await _dropletRepository.RemoveAsync(droplet);
            }

            var metaData = _examCacheService.GetDropletMetaData(dto.examToken);

            if (metaData != null)
            {
                await Task.Delay(5000);

                for (int i = 0; i < 3; i++)
                {
                    var success = await _digitalOceanService.DeleteProjectAsync(metaData.ProjectId);

                    if (success) break;

                    await Task.Delay(3000);
                }

                await Task.Delay(5000);

                var checkVpc = await _digitalOceanService.DeleteVpcAsync(metaData.VpcId);

                await Task.Delay(5000);

                var checkVolume = await _digitalOceanService.DeleteVolumeAsync(metaData.VolumeId);

                _examCacheService.RemoveDropletMetaData(dto.examToken);
            }

            _examCacheService.RemoveExam(dto.examToken);

            var droplets = await _digitalOceanService.CreateDropletAndAttachVolumeAsync(dropletDTO);

            var dropletTokens = new List<string>();

            foreach (var item in droplets)
            {
                await _dropletRepository.AddAsync(item);

                dropletTokens.Add(item.Token);
            }

            var newWsUrls = dropletTokens.Select(tok =>
                $"wss://auth.apptech.edu.az/ws-terminal?token={tok}").ToList();

            storedExam.Terminals = newWsUrls;

            if(dto.examTime is not null && dto.isReset)
            {
                storedExam.Duration = dto.examTime;
            }

            _examCacheService.StoreExam(dto.examToken, storedExam);

            return storedExam;
        }

        private string ExtractTokenFromUrl(string url)
        {
            var tokenKey = "token=";

            var tokenStartIndex = url.IndexOf(tokenKey, StringComparison.Ordinal);
            if (tokenStartIndex == -1)
            {
                throw new ArgumentException("The URL does not contain a token parameter.");
            }

            tokenStartIndex += tokenKey.Length;

            var tokenEndIndex = url.IndexOf('&', tokenStartIndex);
            if (tokenEndIndex == -1) 
            {
                return url.Substring(tokenStartIndex);
            }

            return url.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);
        }

        // Exam Create and Get Process
        public async Task<RandomExamDTO> TakeExamAsync(CreateRandomExamDTO dto)
        {
            if (dto.examToken is not null)
            {
                if (dto.isReset)
                {
                    var storedData = await ResetExamAsync(dto);

                    if (storedData != null)
                    {
                        return storedData;
                    }
                }
                else
                {
                    var storedExam = _examCacheService.GetStoredExam(dto.examToken);

                    storedExam.Duration = dto.examTime;

                    if (storedExam != null)
                    {
                        return storedExam;
                    }
                }
            }

            var exam = await _examRepository.GetByIdAsync(c => c.Id == dto.examId,
                c => c.Certification,
                q => q.Certification.Questions);

            var isUserBoughtThisCertification = (await _certificationUserRepository.GetAllAsync(x => !x.IsDeleted,
               u => u.User,
               c => c.Certification))
               .Any(x => x.UserId == dto.userId && x.CertificationId == exam.CertificationId);

            var certificationCount = exam?.Certification?.Questions?.Count ?? 0;
            var questionCount = exam.QuestionCount;
            int count;

            if (isUserBoughtThisCertification)
            {
                count = questionCount;
            }
            else
            {
                var perCertificationCount = (int)(certificationCount * 0.35);

                if (questionCount > perCertificationCount)
                {
                    var remainingQuestionsNeeded = questionCount - perCertificationCount;

                    var randomAdditionalCount = Math.Min(certificationCount - perCertificationCount, remainingQuestionsNeeded);

                    count = perCertificationCount + randomAdditionalCount;
                }
                else
                {
                    count = questionCount;
                }
            }


            var examToken = Guid.NewGuid().ToString();
            var examDurationMinutes = exam.Duration;
            var examDuration = TimeSpan.FromMinutes(examDurationMinutes);
            var examDurationInSeconds = examDurationMinutes * 60;

            var dragDropQuestion = (await _questionRepository.GetAllAsync(
                q => !q.IsDeleted
                     && q.CertificationId == exam.CertificationId
                     && q.Type == EQuestionType.DragAndDrop,
                c => c.Certification,
                q => q.Variants,
                q => q.QuestionImages,
                q => q.DragDropQuestion))
                .OrderBy(q => Guid.NewGuid())
                .Take(1)
                .ToList();

            var otherQuestions = (await _questionRepository.GetAllAsync(
                q => !q.IsDeleted
                     && q.CertificationId == exam.CertificationId
                     && q.Type != EQuestionType.DragAndDrop,
                c => c.Certification,
                q => q.Variants,
                q => q.QuestionImages,
                q => q.DragDropQuestion))
                .OrderBy(q => Guid.NewGuid())
                .Take(dragDropQuestion.Any() ? count - 1 : count)
                .ToList();

            var randomQuestions = dragDropQuestion.Concat(otherQuestions)
                .OrderBy(q => Guid.NewGuid())
                .ToList();

            var questionDtos = new List<QuestionDTO>();

            foreach (var q in randomQuestions)
            {
                DragDropQuestionResponseDTO dragDropQuestionResponseDTO = null;

                if (q.Type == EQuestionType.DragAndDrop && q.DragDropQuestion != null)
                {
                    var dropZoneDragVariants = await _dragZoneDragVariantRepository.GetAllAsync(
                        dzdv => dzdv.DragDropQuestionId == q.DragDropQuestion.Id,
                        dzdv => dzdv.DropZone,
                        dzdv => dzdv.DragVariant
                    );

                    dragDropQuestionResponseDTO = new DragDropQuestionResponseDTO
                    {
                        Id = q.DragDropQuestion.Id,
                        ImageUrl = q.DragDropQuestion.ImageUrl,
                        dropZones = dropZoneDragVariants.Select(dzdv => new DropZoneResponseDTO
                        {
                            Id = dzdv.DropZone.Id,
                            X = dzdv.DropZone.X,
                            Y = dzdv.DropZone.Y,
                            Width = dzdv.DropZone.Width,
                            Height = dzdv.DropZone.Height
                        }).ToList(),
                        dragVariants = dropZoneDragVariants.Select(dzdv => new DragVariantResponseDTO
                        {
                            Id = dzdv.DragVariant.Id,
                            ImageUrl = dzdv.DragVariant.ImageUrl
                        }).ToList(),
                    };
                }

                var questionDto = new QuestionDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    Server = q.Server,
                    Type = EnumExtensions.EnumToString(q.Type),
                    ImageUrls = q.QuestionImages.Select(qi => qi.ImageUrl).ToList(),
                    Variants = q.Variants?.Select(v => new VariantDTO
                    {
                        Id = v.Id,
                        Text = v.Text
                    }).ToList(),
                    DragDropQuestion = dragDropQuestionResponseDTO
                };

                questionDtos.Add(questionDto);
            }

            var examTitle = exam.Certification.Title.ToLower();

            var isThisExamRedHat = (examTitle.Contains("294") && examTitle.Contains("red"))
                || (examTitle.Contains("200") && examTitle.Contains("red"));

            if (isThisExamRedHat)
            {
                var responseTerminalDTO = await StartExamWithDropletAsync(
                    new StartTerminalExamDTO { userId = dto.userId, examId = exam.Id });
                var title = string.Empty;
                var description = string.Empty;
                var imageUrl = string.Empty;
                var language = new LanguageCatcher(_http).GetLanguage();


                if (!responseTerminalDTO.WebSocketUrls
                    .FirstOrDefault().Contains("Error"))
                {
                    var introduction = (await _questionRepository.GetByIdAsync(x => !x.IsDeleted && x.Server == 0)).Content;

                    var randomExam = new RandomExamDTO
                    {
                        IsTerminal = true,
                        ExamToken = examToken,
                        Questions = questionDtos.Where(x => x.Server != 0).ToList(),
                        Duration = examDurationInSeconds,
                        Title = exam.Certification.Title,
                        QuestionCount = randomQuestions.Count,
                        ServerCount = questionDtos
                        .Where(x => x.Server != 0 && x.Server != null)
                        .Select(x => x.Server)
                        .Distinct()
                        .Count(),
                        Terminals = responseTerminalDTO.WebSocketUrls.ToList(),
                        Introduction = introduction,
                    };

                    _examCacheService.StoreExam(examToken, randomExam);

                    return randomExam;
                }
                else
                {
                    switch (language)
                    {
                        case "az":
                            title = "Maşın limiti xətası";
                            break;
                        case "ru":
                            title = "Ошибка лимита машин";
                            break;
                        default:
                            title = "Machine limit error";
                            break;
                    }

                    switch (language)
                    {
                        case "az":
                            description = "Platformamızın maşınları öz limitinə çatıb, zəhmət olmasa müəyyən bir müddət sonra yenidən yoxlayın.";
                            break;
                        case "ru":
                            description = "Машины нашей платформы достигли своего предела, пожалуйста, попробуйте еще раз через некоторое время.";
                            break;
                        default:
                            description = "Our platform machines have reached their limit. Please try again after some time.";
                            break;
                    }

                    imageUrl = "https://auth.apptech.edu.az/uploads/time-quarter-past.png";

                    var oldUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == dto.userId);
                    oldUser.OnExam = false;

                    await _userManager.UpdateAsync(oldUser);

                    return new RandomExamDTO
                    {
                        AlertTitle = title,
                        AlertImageUrl = imageUrl,
                        AlertDescription = description,
                    };
                }
            }
            else
            {
                var randomExam = new RandomExamDTO
                {
                    IsTerminal = false,
                    ExamToken = examToken,
                    Questions = questionDtos,
                    Duration = examDurationInSeconds,
                    Title = exam.Certification.Title,
                    QuestionCount = randomQuestions.Count
                };

                _examCacheService.StoreExam(examToken, randomExam);

                return randomExam;
            }
        }

        public async Task<ExamResultDTO> CheckExamAsync(SubmitExamDTO dto)
        {
            await Task.Delay(1000);
            _examCacheService.RemoveExam(dto.examToken);

            var exam = await _examRepository.GetByIdAsync(x => x.Id == dto.examId, c => c.Certification);
            var cert = await _certificationRepository.GetByIdAsync(x => x.Id == exam.CertificationId);
            var currentUser = await _accountService.CheckNotFoundByIdAsync(dto.userId);
            var examCert = cert.Title.ToLower();

            double totalScore = 0;

            var isThisExamRedHat = (examCert.Contains("294") && examCert.Contains("red"))
                || (examCert.Contains("200") && examCert.Contains("red"));

            if (dto.answers is not null && dto.answers.Count > 0)
            {
                var questionIds = dto.answers.Select(a => a.questionId).ToList();
                var questions = await _questionRepository.GetAllAsync(q => questionIds.Contains(q.Id), q => q.Variants);

                var dragDropQuestionIds = questions.Where(q => q.Type == EQuestionType.DragAndDrop).Select(q => q.Id).ToList();
                var dragDropQuestions = await _dragDropQuestionRepository.GetAllAsync(dq => dragDropQuestionIds.Contains(dq.QuestionId));
                var dropZones = await _dragZoneDragVariantRepository.GetAllAsync(x => !x.IsDeleted && dragDropQuestionIds.Contains(x.DragDropQuestionId), dr => dr.DropZone);

                foreach (var answer in dto.answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.questionId);

                    if (question == null)
                        continue;

                    if ((answer.variantIds is not null && answer.variantIds.Count != 0) ||
                        (answer.dropZoneAnswers is not null && answer.dropZoneAnswers.Count != 0))
                    {
                        switch (question.Type)
                        {
                            case EQuestionType.SingleChoice:
                            {
                                var correctVariant = question.Variants.FirstOrDefault(v => v.IsCorrect);
                                totalScore += correctVariant != null && answer.variantIds.Contains(correctVariant.Id)
                                    ? question.Point
                                    : 0;
                                break;
                            }
                            case EQuestionType.MultipleChoice:
                            {
                                var correctVariants = question.Variants.Where(v => v.IsCorrect).Select(v => v.Id).ToList();
                                var selectedVariants = answer.variantIds;

                                bool hasMistake = selectedVariants.Any(v => !correctVariants.Contains(v)) ||
                                                  correctVariants.Any(v => !selectedVariants.Contains(v));

                                totalScore += !hasMistake ? question.Point : 0;
                                break;
                            }
                            case EQuestionType.DragAndDrop:
                            {
                                var dragDropQuestion = dragDropQuestions.FirstOrDefault(dq => dq.QuestionId == question.Id);
                                if (dragDropQuestion == null)
                                    break;

                                var questionDropZones = dropZones
                                    .Where(dz => dz.DragDropQuestionId == dragDropQuestion.Id)
                                    .Select(dz => dz.DropZone)
                                    .ToList();

                                bool isCorrect = true;

                                foreach (var zoneAnswer in answer.dropZoneAnswers)
                                {
                                    var dropZone = questionDropZones.FirstOrDefault(dz => dz.Id == zoneAnswer.dropZoneId);
                                    if (dropZone == null)
                                    {
                                        isCorrect = false;
                                        break;
                                    }

                                    var dropZoneDragVariants = dropZones
                                        .Where(x => x.DropZoneId == dropZone.Id)
                                        .Select(x => x.DragVariant)
                                        .ToList();

                                    var correctDragVariant = dropZoneDragVariants
                                        .FirstOrDefault(dv => dv.Id == zoneAnswer.dragVariantId);

                                    if (correctDragVariant == null)
                                    {
                                        isCorrect = false;
                                        break;
                                    }
                                }

                                totalScore += isCorrect ? question.Point : 0;
                                break;
                            }
                            default:
                                break;
                        }
                    }
                }
            }

            var isUserPassed = totalScore >= exam.PassScore;

            var examResult = (await _examResultRepository
                .GetAllAsync(x => !x.IsDeleted && x.UserScore == null
                && (x.ExamId == dto.examId && x.UserId == dto.userId))).FirstOrDefault();

            examResult.UserScore = totalScore;
            examResult.IsPassed = isUserPassed;

            await _examResultRepository.UpdateAsync(examResult);

            currentUser.OnExam = false;

            await _userManager.UpdateAsync(currentUser);

            if (isThisExamRedHat)
            {
                var oldDroplets = new List<Droplet>();

                var tokenKey = "token=";

                var terminalTokens = dto.terminals
                    .Select(url => url.Substring(url.IndexOf(tokenKey) + tokenKey.Length))
                    .ToList();

                foreach (var token in terminalTokens)
                {
                    oldDroplets.Add(await _dropletRepository
                        .GetByIdAsync(x => !x.IsDeleted && x.Token == token));
                }

                foreach (var droplet in oldDroplets)
                {
                    droplet.ExamResultId = examResult.Id;

                    await _dropletRepository.UpdateAsync(droplet);
                }
            }

            var language = new LanguageCatcher(_http).GetLanguage();

            var title = string.Empty;
            bool isTerminal = false;
            var description = string.Empty;
            var imageUrl = string.Empty;
            var maxScore = exam.MaxScore;
            var passScore = exam.PassScore;
            var userScore = totalScore;

            if (dto.time < 1 || dto.time is null)
            {
                switch (language)
                {
                    case "az":
                        title = "Vaxt bitdi";
                        break;
                    case "ru":
                        title = "Время вышло";
                        break;
                    default:
                        title = "Time is up";
                        break;
                }

                imageUrl = "https://auth.apptech.edu.az/uploads/time-quarter-past.png";

                if (isThisExamRedHat)
                {
                    switch (language)
                    {
                        case "az":
                            description = "İmtahan yoxlanıldıq sonra sizə geri dönüş ediləcək.";
                            break;
                        case "ru":
                            description = "Мы свяжемся с вами после проверки экзамена.";
                            break;
                        default:
                            description = "We will get back to you after the exam has been checked.";
                            break;
                    }

                    isUserPassed = true;
                    isTerminal = true;
                }
                else
                {
                    if (userScore >= passScore)
                    {
                        switch (language)
                        {
                            case "az":
                                description = "Müvəffəqiyyət";
                                break;
                            case "ru":
                                description = "Успешно";
                                break;
                            default:
                                description = "Pass";
                                break;
                        }

                    }
                    else
                    {
                        switch (language)
                        {
                            case "az":
                                description = "Uğursuz";
                                break;
                            case "ru":
                                description = "Провал";
                                break;
                            default:
                                description = "Fail";
                                break;
                        }
                    }
                }
            }
            else
            {
                if (isThisExamRedHat)
                {
                    switch (language)
                    {
                        case "az":
                            title = "İmtahan bitdi.";
                            description = "İmtahan yoxlanıldıq sonra sizə geri dönüş ediləcək.";
                            break;
                        case "ru":
                            title = "Экзамен окончен.";
                            description = "Мы свяжемся с вами после проверки экзамена.";
                            break;
                        default:
                            title = "Exam finished.";
                            description = "We will get back to you after the exam has been checked.";
                            break;
                    }

                    imageUrl = "https://auth.apptech.edu.az/uploads/time-quarter-past.png";
                    isUserPassed = true;
                    isTerminal = true;
                }
                else
                {
                    if (userScore >= passScore)
                    {
                        switch (language)
                        {
                            case "az":
                                title = "Müvəffəqiyyət";
                                break;
                            case "ru":
                                title = "Успешно";
                                break;
                            default:
                                title = "Pass";
                                break;
                        }

                        imageUrl = "https://auth.apptech.edu.az/uploads/book-check.png";
                    }
                    else
                    {
                        switch (language)
                        {
                            case "az":
                                title = "Uğursuz";
                                break;
                            case "ru":
                                title = "Провал";
                                break;
                            default:
                                title = "Fail";
                                break;
                        }

                        imageUrl = "https://auth.apptech.edu.az/uploads/book-remove.png";
                    }
                }
            }

            var isUserSubscribed = (await _subscriptionUserRepository.GetAllAsync(x => !x.IsDeleted))
                .Any(x => x.UserId == dto.userId && x.ExpiredOn > DateTime.UtcNow);

            if (isUserSubscribed)
            {
                var newExamResult = new ExamResult
                {
                    UserId = dto.userId,
                    ExamId = exam.Id,
                };

                await _examResultRepository.AddAsync(newExamResult);
            }

            return new ExamResultDTO
            {
                Title = title,
                ImageUrl = imageUrl,
                MaxScore = maxScore,
                PassScore = passScore,
                UserScore = userScore,
                IsTerminal = isTerminal,
                IsPassed = isUserPassed,
                Description = description,
            };
        }

        public async Task StartExamAsync(UserTimerDTO dto)
        {
            var oldUser = await _accountService.CheckNotFoundByIdAsync(dto.userId);
            oldUser.OnExam = true;
            oldUser.LastExamActivity = DateTime.UtcNow;

            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        public async Task EndExamAsync(UserTimerDTO dto)
        {
            var oldUser = await _accountService.CheckNotFoundByIdAsync(dto.userId);
            oldUser.OnExam = false;
            oldUser.LastExamActivity = DateTime.UtcNow;

            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        public async Task ChangeExamStatus(string userId)
        {
            var oldUser = await _accountService.CheckNotFoundByIdAsync(userId);

            oldUser.LastExamActivity = DateTime.UtcNow;
            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        public async Task<bool> CheckExamStatus(string userId)
        {
            var oldUser = await _accountService.CheckNotFoundByIdAsync(userId);

            return oldUser.OnExam;
        }


        // CRUD section
        public async Task<IQueryable<ExamDTO>> GetAllAsync(GetExamBySlugCompanyDTO dto)
        {
            var company = await _companyRepository.GetByIdAsync(x => x.Slug == dto.slug);

            if (company == null)
            {
                return Enumerable.Empty<ExamDTO>().AsQueryable();
            }

            var exams = (await _examRepository.GetAllAsync(
                x => !x.IsDeleted,
                x => x.Certification,
                x => x.ExamTranslations))
                .Where(x => x.Certification.CompanyId == company.Id);

            var subscribedCertificationIds = (await _subscriptionUserRepository.GetAllAsync(x => !x.IsDeleted && x.UserId == dto.userId && x.ExpiredOn > DateTime.UtcNow,
                s => s.Subscription,
                sc => sc.Subscription.Certification))
                .Select(x => x.Subscription.CertificationId)
                .ToList();

            var examDTOs = exams
                .Where(exam => !subscribedCertificationIds.Contains(exam.CertificationId))
                .Select(exam => new ExamDTO
                {
                    Id = exam.Id,
                    Slug = exam.Slug,
                    ImageUrl = exam.ImageUrl,
                    CertificationTitle = exam.Certification.Title,
                    CompanyTitle = company.Title,
                    Price = exam.Price
                }).AsQueryable();

            return examDTOs;
        }



        public async Task<IQueryable<ExamUserResponseDTO>> GetAllExamsByUser(GetAllExamByUser dto)
        {
            var isUserSubscribed = (await _subscriptionUserRepository.GetAllAsync(x => !x.IsDeleted))
                .Any(x => x.UserId == dto.userId && x.ExpiredOn > DateTime.UtcNow);

            var examResults = (await _examResultRepository.GetAllAsync(x =>
                !x.IsDeleted,
                e => e.Exam,
                c => c.Exam.Certification,
                cm => cm.Exam.Certification.Company))
                .Where(er => er.UserId == dto.userId && !er.Exam.IsDeleted);

            var userExams = examResults
                .Where(er => !er.IsDeleted && er.IsPassed == null && er.UserScore == null)
                .GroupBy(er => er.ExamId)
                .Select(g => new ExamUserResponseDTO
                {
                    ImageUrl = g.First().Exam.ImageUrl,
                    CompanyTitle = g.First().Exam.Certification.Company.Title,
                    Title = g.First().Exam.Certification.Title,
                    Slug = g.First().Exam.Slug,
                    Count = g.Count(),
                    IsSub = isUserSubscribed,
                    LastVersion = g.First().Exam.Certification.LastVersion,
                })
                .AsQueryable();

            return userExams;
        }

        public async Task<IQueryable<ExamResultDTO>> GetAllExamResultsByUserAsync(UserTimerDTO dto)
        {
            var language = new LanguageCatcher(_http).GetLanguage();

            var examResults = await _examResultRepository.GetAllAsync(x => !x.IsDeleted && x.UserId == dto.userId, e => e.Exam.Certification);

            return examResults
                .Where(er => er.IsPassed != null && er.UserScore != null)
                .OrderByDescending(t => t.UpdatedOn)
                .Take(8)
                .Select(er =>
                {
                    string title;

                    if (er.IsPassed.Value)
                    {
                        switch (language)
                        {
                            case "az":
                                title = "Müvəffəqiyyət";
                                break;
                            case "ru":
                                title = "Успешно";
                                break;
                            default:
                                title = "Pass";
                                break;
                        }
                    }
                    else
                    {
                        switch (language)
                        {
                            case "az":
                                title = "Uğursuz";
                                break;
                            case "ru":
                                title = "Провал";
                                break;
                            default:
                                title = "Fail";
                                break;
                        }
                    }

                    return new ExamResultDTO
                    {
                        Description = title,
                        ExamOn = er.UpdatedOn.ToLocalTime(),
                        MaxScore = er.Exam.MaxScore,
                        UserScore = er.UserScore.Value,
                        IsPassed = er.IsPassed.Value,
                        ImageUrl = er.Exam.ImageUrl,
                        Title = er.Exam.Certification.Title,
                        LastVersion = er.Exam.Certification.LastVersion,
                    };
                })
                .AsQueryable();
        }

        public async Task<IQueryable<ExamDTO>> GetAllAsync()
        {
            var entities = (await _examRepository.GetAllAsync(
                x => x.IsDeleted == false,
                x => x.ExamTranslations))
                .AsEnumerable()
                .Select(e => new ExamDTO
                {
                    Id = e.Id,
                    Code = e.Code,
                    Description = e.ExamTranslations
                    .Where(ct => ct.Language == ELanguage.Az && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                    PassScore = e.PassScore,
                    MaxScore = e.MaxScore,
                    Duration = e.Duration,
                    Price = e.Price,
                    QuestionCount = e.QuestionCount,
                });

            return entities.AsQueryable();
        }

        public async Task<ExamDTO> GetByIdAsync(GetByIdExamDTO dto)
        {
            var entity = _examHandler.HandleEntityAsync(
                await _examRepository.GetByIdAsync(x => x.Id == dto.Id, et => et.ExamTranslations));

            return new ExamDTO
            {
                Id = entity.Id,
                CertificationId = entity.CertificationId,
                Code = entity.Code,
                MaxScore = entity.MaxScore,
                PassScore = entity.PassScore,
                Duration = entity.Duration,
                Price = entity.Price,
                QuestionCount = entity.QuestionCount,
                Translations = entity.ExamTranslations.Select(ct => new ExamTranslationDTO
                {
                    Id = ct.Id,
                    ExamId = ct.ExamId,
                    Description = ct.Description,
                    Language = EnumExtensions.EnumToString(ct.Language),
                }).ToList()
            };
        }

        public async Task<ExamDTO> GetBySlugAsync(GetBySlugExamDTO dto)
        {
            var entity = _examHandler.HandleEntityAsync(
                await _examRepository.GetByIdAsync(x => x.Slug == dto.slug,
                c => c.Certification.Company,
                et => et.ExamTranslations));

            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var questionTypes = (await _questionRepository
                .GetAllAsync(x => !x.IsDeleted && x.CertificationId == entity.CertificationId))
                .Where(q => !q.IsDeleted)
                .Select(q => q.Type)
                .Distinct()
                .Select(type => type switch
                {
                    EQuestionType.SingleChoice => "Single",
                    EQuestionType.MultipleChoice => "Multiple",
                    EQuestionType.DragAndDrop => "Drag and Drop",
                    EQuestionType.Terminal => "Terminal",
                    _ => "unknown"
                })
                .ToList();

            return new ExamDTO
            {
                Id = entity.Id,
                Slug = entity.Slug,
                Code = entity.Code,
                Price = entity.Price,
                PassScore = entity.PassScore,
                MaxScore = entity.MaxScore,
                Duration = entity.Duration,
                ImageUrl = entity.ImageUrl,
                QuestionCount = entity.QuestionCount,
                LastVersion = entity.Certification.LastVersion,
                Description = entity.ExamTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                CertificationSlug = entity.Certification.Slug,
                CertificationTitle = entity.Certification.Title,
                CompanyTitle = entity.Certification.Company.Title,
                Types = questionTypes
            };
        }

        public async Task<ExamDTO> AddAsync(CreateExamDTO dto)
        {
            var newExam = _mapper.Map<Exam>(dto);
            newExam.Slug = SlugCreator.GenerateSlug(newExam.Code);

            var entity = await _examRepository.AddAsync(newExam);

            return new ExamDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Price = entity.Price,
            };
        }

        public async Task<ExamDTO> DeleteAsync(DeleteExamDTO dto)
        {
            var entity = await _examRepository.DeleteAsync(
                _examHandler.HandleEntityAsync(
                await _examRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new ExamDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Price = entity.Price,
            };
        }

        public async Task<ExamDTO> UpdateAsync(UpdateExamDTO dto)
        {
            var entity = await _examRepository.UpdateAsync(_mapper.Map(dto,
                _examHandler.HandleEntityAsync(
                await _examRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new ExamDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                Price = entity.Price,
            };
        }
    }
}
