using System.Security.Cryptography;
using System.Text;
using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.DTOs.TransactionDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.Core.Exceptions.CertificationExceptions;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Services;
using Newtonsoft.Json.Linq;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class OrderService : IOrderService
    {
        private readonly ITransactionTranslationRepository _transactionTranslationRepository;
        private readonly ICertificationUserRepository _certificationUserRepository;
        private readonly ISubscriptionUserRepository _subscriptionUserRepository;
        private readonly IPromotionUserRepository _promotionUserRepository;
        private readonly ICertificationRepository _certificationRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExamPackageRepository _examPackageRepository;
        private readonly IExamResultRepository _examResultRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ITransactionHandler _transactionHandler;
        private readonly IAccountService _accountService;
        private readonly IExamRepository _examRepository;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _http;
        private readonly IBankService _bankService;

        public OrderService(IBankService bankService,
            ITransactionRepository transactionRepository,
            ICertificationRepository certificationRepository,
            UserManager<User> userManager,
            IPromotionRepository promotionRepository,
            IAccountService accountService,
            AppDbContext appDbContext,
            ICertificationUserRepository certificationUserRepository,
            IHttpContextAccessor http,
            ITransactionHandler transactionHandler,
            IExamRepository examRepository,
            IExamResultRepository examResultRepository,
            IExamPackageRepository examPackageRepository,
            ITransactionTranslationRepository transactionTranslationRepository,
            IPromotionUserRepository promotionUserRepository,
            ISubscriptionRepository subscriptionRepository,
            ISubscriptionUserRepository subscriptionUserRepository,
            IEmailService emailService)
        {
            _bankService = bankService;
            _transactionRepository = transactionRepository;
            _certificationRepository = certificationRepository;
            _userManager = userManager;
            _promotionRepository = promotionRepository;
            _accountService = accountService;
            _appDbContext = appDbContext;
            _certificationUserRepository = certificationUserRepository;
            _http = http;
            _transactionHandler = transactionHandler;
            _examRepository = examRepository;
            _examResultRepository = examResultRepository;
            _examPackageRepository = examPackageRepository;
            _transactionTranslationRepository = transactionTranslationRepository;
            _promotionUserRepository = promotionUserRepository;
            _subscriptionRepository = subscriptionRepository;
            _subscriptionUserRepository = subscriptionUserRepository;
            _emailService = emailService;
        }

        public async Task<IQueryable<OrderDTO>> GetAllAsync(GetAllOrderDTO dto)
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());
            var query = await _transactionRepository.GetAllAsync(x => !x.IsDeleted && x.UserId == dto.userId,
                t => t.TransactionTranslations);

            return query.Where(x => x.Status == EOrderStatus.FULLYPAID)
                .OrderByDescending(t => t.UpdatedOn)
                .Take(8)
                .Select(t => new OrderDTO
                {
                    Id = t.Id,
                    PaymentOn = t.UpdatedOn.ToLocalTime(),
                    Amount = t.Amount,
                    IsIncreased = t.IsIncreased,
                    Description = t.TransactionTranslations
                    .Where(ct => ct.Language == language && !ct.IsDeleted)
                    .Select(ct => ct.Description)
                    .FirstOrDefault(),
                }).AsQueryable();
        }

        // Dashboard Requests and Methods
        public async Task<OrderResponseDTO> DecreaseBalanceForBuyExamPackageAsync(DecreaseBalanceForForBuyExamPackageDTO dto)
        {

            var checkToken = GeneratePaymentCheckToken();
            var examPackage = await _examPackageRepository.GetByIdAsync(
                x => !x.IsDeleted && x.Id == dto.examPackageId, e => e.Exams);
            var examPackagePrice = examPackage.Price;
            var examPackageId = examPackage.Id;

            var lang = new LanguageCatcher(_http).GetLanguage();

            var orderTitle = string.Empty;
            var orderDescriptionMessage = string.Empty;
            var orderDescription = string.Empty;

            var oldUser = await _userManager.Users.Where(x => x.Id == dto.userId)
                .Include(er => er.ExamResults).FirstOrDefaultAsync();


            var checkBalanceForExamPrice = oldUser.Balance >= examPackagePrice;

            if (!checkBalanceForExamPrice)
            {
                switch (lang)
                {
                    case "az":
                        orderTitle = "Balansınız Yetərsizdir";
                        orderDescription = "Bu alışı tamamlamaq üçün kifayət qədər balansınız yoxdur. Zəhmət olmasa hesabınızı doldurun.";
                        break;
                    case "ru":
                        orderTitle = "Недостаточно Средств";
                        orderDescription = "У вас недостаточно средств для завершения покупки. Пожалуйста, пополните свой счет.";
                        break;
                    case "en":
                    default:
                        orderTitle = "Insufficient Balance";
                        orderDescription = "You do not have enough balance to complete this purchase. Please top up your account.";
                        break;
                }

                return new OrderResponseDTO
                {
                    OrderTitle = orderTitle,
                    OrderDescription = orderDescription,
                    OrderImageUrl = "https://auth.apptech.edu.az/uploads/error.png",
                    OrderStatus = "Declined",
                };
            }

            // Add Exam To Current User
            if (examPackageId == 0)
                throw new Exception("There is an error in payment section.");

            foreach (var exam in examPackage.Exams)
            {
                var newExamResult = new ExamResult
                {
                    UserId = oldUser.Id,
                    ExamId = exam.Id,
                };

                await _examResultRepository.AddAsync(newExamResult);
            }

            oldUser.Balance -= examPackagePrice;

            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            var orderIdFromResponse = new Random().Next(10000, 99999).ToString();

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var sessionIdFromResponse = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var newTransaction = new Transaction()
            {
                UserId = dto.userId,
                OrderId = long.Parse(orderIdFromResponse),
                Amount = examPackagePrice,
                SessionId = sessionIdFromResponse,
                CheckToken = checkToken,
                Type = EOrderType.CASH,
                Status = EOrderStatus.FULLYPAID,
            };

            var createdTransaction = await _transactionRepository.AddAsync(newTransaction);

            var newAzTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "İmtahan paketi alındı.",
                Language = ELanguage.Az
            };

            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

            var newEnTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "Exam package bought.",
                Language = ELanguage.En
            };

            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

            var newRuTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "Пакет экзаменов куплен.",
                Language = ELanguage.Ru
            };

            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);

            var orderStatus = createdTransaction.Status;
            var netStatus = "Successful";
            var orderImageUrlMessage = "https://auth.apptech.edu.az/uploads/check1.png";

            switch (lang)
            {
                case "en":
                    orderTitle = $"Payment Successful";
                    orderDescriptionMessage = "Your payment was successfully completed through the system. The exam has been successfully added to your products page. Thank you for your payment.";
                    break;
                case "ru":
                    orderTitle = "Оплата завершена";
                    orderDescriptionMessage = "Ваш платеж был успешно завершен через систему. Экзамен успешно добавлен на вашу страницу продуктов. Благодарим вас за оплату.";
                    break;
                case "az":
                    orderTitle = "Ödəmə tamamlandı";
                    orderDescriptionMessage = "Ödəməniz sistem vasitəsilə uğurla tamamlandı. İmtahan məhsullar səhifənizə uğurla əlavə edildi. Ödənişiniz üçün təşəkkür edirik.";
                    break;
                default:
                    orderTitle = $"Payment Successful";
                    orderDescriptionMessage = "Your payment was successfully completed through the system. The exam has been successfully added to your products page. Thank you for your payment.";
                    break;
            }

            return new OrderResponseDTO
            {
                OrderTitle = orderTitle,
                OrderDescription = orderDescriptionMessage,
                OrderImageUrl = orderImageUrlMessage,
                OrderStatus = netStatus,
                OrderId = createdTransaction.OrderId,
                ApprovedOn = createdTransaction.UpdatedOn,
            };
        }

        public async Task<OrderResponseDTO> DecreaseBalanceForBuyExamAsync(DecreaseBalanceForBuyExamDTO dto)
        {
            var checkToken = GeneratePaymentCheckToken();
            var exam = await _examRepository.GetByIdAsync(x => x.Slug == dto.slug);
            var examPrice = exam.Price;
            var examId = exam.Id;

            var lang = new LanguageCatcher(_http).GetLanguage();
            var orderTitle = string.Empty;
            var orderDescriptionMessage = string.Empty;
            var orderDescription = string.Empty;

            var oldUser = await _userManager.Users.Where(x => x.Id == dto.userId)
                .Include(er => er.ExamResults).FirstOrDefaultAsync();
            var checkBalanceForExamPrice = oldUser.Balance >= examPrice;

            if (!checkBalanceForExamPrice)
            {
                switch (lang)
                {
                    case "az":
                        orderTitle = "Balansınız Yetərsizdir";
                        orderDescription = "Bu alışı tamamlamaq üçün kifayət qədər balansınız yoxdur. Zəhmət olmasa hesabınızı doldurun.";
                        break;
                    case "ru":
                        orderTitle = "Недостаточно Средств";
                        orderDescription = "У вас недостаточно средств для завершения покупки. Пожалуйста, пополните свой счет.";
                        break;
                    case "en":
                    default:
                        orderTitle = "Insufficient Balance";
                        orderDescription = "You do not have enough balance to complete this purchase. Please top up your account.";
                        break;
                }

                return new OrderResponseDTO
                {
                    OrderTitle = orderTitle,
                    OrderDescription = orderDescription,
                    OrderImageUrl = "https://auth.apptech.edu.az/uploads/error.png",
                    OrderStatus = "Declined",
                };
            }

            // Add Exam To Current User
            if (examId == 0)
                throw new Exception("There is an error in payment section.");

            var newExamResult = new ExamResult
            {
                UserId = oldUser.Id,
                ExamId = examId,
            };

            await _examResultRepository.AddAsync(newExamResult);

            oldUser.Balance -= examPrice;
            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            var orderIdFromResponse = new Random().Next(10000, 99999).ToString();

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var sessionIdFromResponse = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var newTransaction = new Transaction()
            {
                UserId = dto.userId,
                OrderId = long.Parse(orderIdFromResponse),
                Amount = examPrice,
                SessionId = sessionIdFromResponse,
                CheckToken = checkToken,
                Type = EOrderType.CASH,
                Status = EOrderStatus.FULLYPAID,
            };

            var createdTransaction = await _transactionRepository.AddAsync(newTransaction);

            var newAzTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "İmtahan alındı.",
                Language = ELanguage.Az
            };

            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

            var newEnTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "Exam bought.",
                Language = ELanguage.En
            };

            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

            var newRuTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdTransaction.Id,
                Description = "Экзамен куплен.",
                Language = ELanguage.Ru
            };

            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);


            var orderStatus = createdTransaction.Status;
            var netStatus = "Successful";
            var orderImageUrlMessage = "https://auth.apptech.edu.az/uploads/check1.png";

            switch (lang)
            {
                case "en":
                    orderTitle = $"Payment Successful";
                    orderDescriptionMessage = "Your payment was successfully completed through the system. The exam has been successfully added to your products page. Thank you for your payment.";
                    break;
                case "ru":
                    orderTitle = "Оплата завершена";
                    orderDescriptionMessage = "Ваш платеж был успешно завершен через систему. Экзамен успешно добавлен на вашу страницу продуктов. Благодарим вас за оплату.";
                    break;
                case "az":
                    orderTitle = "Ödəmə tamamlandı";
                    orderDescriptionMessage = "Ödəməniz sistem vasitəsilə uğurla tamamlandı. İmtahan məhsullar səhifənizə uğurla əlavə edildi. Ödənişiniz üçün təşəkkür edirik.";
                    break;
                default:
                    orderTitle = $"Payment Successful";
                    orderDescriptionMessage = "Your payment was successfully completed through the system. The exam has been successfully added to your products page. Thank you for your payment.";
                    break;
            }

            return new OrderResponseDTO
            {
                OrderTitle = orderTitle,
                OrderDescription = orderDescriptionMessage,
                OrderImageUrl = orderImageUrlMessage,
                OrderStatus = netStatus,
                OrderId = createdTransaction.OrderId,
                ApprovedOn = createdTransaction.UpdatedOn,
            };
        }

        // Kapital Bank Requests and Methods
        public async Task<TransactionDTO> IncreaseBalanceAsync(IncreaseBalanceDTO dto)
        {
            var checkToken = GeneratePaymentCheckToken();
            var orderDescription = "IncreaseBalanceOrderDescription";
            decimal netPrice = dto.amount;

            var response = await _bankService.IncreaseBalanceOrderAsync(checkToken,
                netPrice, orderDescription);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseBody);

            var orderIdFromResponse = responseJson["order"]?["id"]?.ToString();
            var sessionIdFromResponse = responseJson["order"]?["password"]?.ToString();
            var lang = new LanguageCatcher(_http).GetLanguage();

            var newTransaction = new Transaction()
            {
                UserId = dto.userId,
                OrderId = long.Parse(orderIdFromResponse),
                Amount = netPrice,
                SessionId = sessionIdFromResponse,
                CheckToken = checkToken,
                Type = EOrderType.CASH,
                Status = EOrderStatus.ONPAYMENT,
                ResponseBody = responseBody,
                IsIncreased = true,
            };

            var createdTransaction = await _transactionRepository.AddAsync(newTransaction);

            return new TransactionDTO
            {
                Url = $"{responseJson["order"]?["hppUrl"]?.ToString()}?id={orderIdFromResponse}&password={sessionIdFromResponse}",
                Token = checkToken
            };
        }

        public async Task<decimal> UsePromotion(UsePromotionDTO usePromotionDTO)
        {
            bool check = _appDbContext.CertificationPromotions
                .Any(x => x.Promotion.Code == usePromotionDTO.Code
                       && x.CertificationId == usePromotionDTO.CertificationId);

            if (check)
            {
                var promotion = await _appDbContext.Promotions
                    .FirstOrDefaultAsync(x => x.Code == usePromotionDTO.Code);

                return promotion.Percentage;
            }

            return 0;
        }

        public async Task<TransactionDTO> PurchaseAsync(CreateTransactionDTO dto)
        {
            var subscription = dto.subId is not null ? await _subscriptionRepository.GetByIdAsync(x => !x.IsDeleted && x.Id == dto.subId,
                c => c.Certification) : null;

            var checkToken = GeneratePaymentCheckToken();
            var orderDescription = dto.type ? $"TAKSIT={dto.installmentNumber}" : "PurchaseOrderDesc";
            var userId = dto.userId;
            var certificationId = dto.subId is not null ? subscription?.Certification.Id : dto.certificationId;

            var lang = new LanguageCatcher(_http).GetLanguage();

            var certification = await _certificationRepository.GetByIdAsync(
                x => x.Id == certificationId, x => x.Company);

            var certSlug = certification.Slug;
            var companySlug = certification?.Company?.Slug;
            decimal netPrice;
            var usePromotionDto = new UsePromotionDTO
            { CertificationId = certificationId.GetValueOrDefault(), Code = dto.promotionCode };

            var percentage = await UsePromotion(usePromotionDto);

            if (dto.subId is not null)
            {
                netPrice = subscription.Price;
            }
            else
            {
                var discountCertificationPrice = certification.DiscountPrice == 0 ? certification.Price
                    : certification.DiscountPrice;

                netPrice = discountCertificationPrice - ((discountCertificationPrice * percentage) / 100m);
            }

            var response = await _bankService.CreateOrderAsync(
                companySlug, certSlug, checkToken,
                netPrice, orderDescription);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseBody);

            var orderIdFromResponse = responseJson["order"]?["id"]?.ToString();
            var sessionIdFromResponse = responseJson["order"]?["password"]?.ToString();

            var isUserBoughtThisCertification = (await _certificationUserRepository.GetAllAsync(x => !x.IsDeleted,
                u => u.User,
                c => c.Certification))
                .Any(x => x.UserId == userId && x.CertificationId == certificationId);

            if (isUserBoughtThisCertification)
                throw new UserIdIsNotNullException((await _accountService.CheckNotFoundByIdAsync(dto.userId)).UserName);

            var newTransaction = new Transaction()
            {
                UserId = dto.userId,
                CertificationId = certificationId,
                OrderId = long.Parse(orderIdFromResponse),
                Amount = netPrice,
                SessionId = sessionIdFromResponse,
                CheckToken = checkToken,
                Type = dto.type ? EOrderType.INSTALLMENT : EOrderType.CASH,
                Status = EOrderStatus.ONPAYMENT,
                ResponseBody = responseBody,
                SubscriptionId = dto.subId is not null ? subscription?.Id : null,
                PromotionCode = percentage > 0 ? dto.promotionCode : null,
            };

            await _transactionRepository.AddAsync(newTransaction);

            return new TransactionDTO
            {
                Url = $"{responseJson["order"]?["hppUrl"]?.ToString()}?id={orderIdFromResponse}&password={sessionIdFromResponse}",
                Token = checkToken
            };
        }

        public async Task<OrderResponseDTO> PaymentHandlerAsync(string checkToken, bool increaseOrBuy)
        {
            var transaction = await _transactionRepository.GetByIdAsync(x => x.CheckToken == checkToken,
                u => u.User, c => c.Certification);

            var data = await _bankService.GetOrderInformationAsync(transaction.OrderId.ToString());
            var responseData = JObject.Parse(data);

            EOrderStatus orderStatus;
            orderStatus = Enum.Parse<EOrderStatus>(responseData["order"]?["status"]?.ToString(), true);
            var oldUser = await _accountService.CheckNotFoundByIdAsync(transaction.UserId);

            if (orderStatus == EOrderStatus.FULLYPAID)
            {
                if (increaseOrBuy)
                {
                    oldUser.Balance += transaction.Amount;

                    _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

                    var newAzTransactionTranslation = new TransactionTranslation
                    {
                        TransactionId = transaction.Id,
                        Description = "Balans artırıldı.",
                        Language = ELanguage.Az
                    };

                    await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

                    var newEnTransactionTranslation = new TransactionTranslation
                    {
                        TransactionId = transaction.Id,
                        Description = "Balance increased.",
                        Language = ELanguage.En
                    };

                    await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

                    var newRuTransactionTranslation = new TransactionTranslation
                    {
                        TransactionId = transaction.Id,
                        Description = "Баланс увеличен.",
                        Language = ELanguage.Ru
                    };

                    await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);
                }
                else
                {
                    var userId = oldUser.Id;
                    var certificationId = transaction.CertificationId.GetValueOrDefault();

                    if (certificationId == 0)
                        throw new Exception("There is an error in payment section.");

                    var isUserBoughtThisCertification = (await _certificationUserRepository.GetAllAsync(x => !x.IsDeleted,
                        u => u.User,
                        c => c.Certification))
                        .Any(x => x.UserId == userId && x.CertificationId == certificationId);

                    if (!isUserBoughtThisCertification)
                    {
                        CertificationUser newData;
                        ExamResult newExamResult;
                        SubscriptionUser newSubscriptionUser;

                        var certification = await _certificationRepository.GetByIdAsync(x => !x.IsDeleted &&
                        x.Id == certificationId,
                        e => e.Exams);

                        if (transaction.SubscriptionId is not null)
                        {
                            newData = new CertificationUser
                            {
                                CertificationId = certificationId,
                                UserId = userId,
                            };

                            newSubscriptionUser = new SubscriptionUser
                            {
                                SubscriptionId = transaction.SubscriptionId.GetValueOrDefault(),
                                UserId = userId,
                                ExpiredOn = DateTime.UtcNow.AddMonths(1),
                            };

                            await _subscriptionUserRepository.AddAsync(newSubscriptionUser);

                            if (certification.Exams is not null)
                            {
                                newExamResult = new ExamResult
                                {
                                    UserId = userId,
                                    ExamId = certification.Exams.FirstOrDefault().Id,
                                };

                                await _examResultRepository.AddAsync(newExamResult);
                            }

                            var newAzTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Abonə olundu.",
                                Language = ELanguage.Az
                            };

                            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

                            var newEnTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Subscribed.",
                                Language = ELanguage.En
                            };

                            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

                            var newRuTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Подписался.",
                                Language = ELanguage.Ru
                            };

                            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);
                        }
                        else
                        {
                            newData = new CertificationUser
                            {
                                CertificationId = certificationId,
                                UserId = userId,
                            };

                            var newAzTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Sertifikat alındı.",
                                Language = ELanguage.Az
                            };

                            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

                            var newEnTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Certification bought.",
                                Language = ELanguage.En
                            };

                            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

                            var newRuTransactionTranslation = new TransactionTranslation
                            {
                                TransactionId = transaction.Id,
                                Description = "Сертификация куплена.",
                                Language = ELanguage.Ru
                            };

                            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);
                        }

                        await _certificationUserRepository.AddAsync(newData);

                        var usedPromotion = await _promotionRepository.GetByIdAsync(x => x.Code == transaction.PromotionCode);

                        if(usedPromotion is not null)
                        {
                            var newPromotionUser = new PromotionUser
                            {
                                PromotionId = usedPromotion.Id,
                                UserId = userId,
                            };

                            await _promotionUserRepository.UpdateAsync(newPromotionUser);
                        }
                    }
                    else
                        throw new Exception("There is an error in payment section.");
                }
            }

            Transaction resultTransaction;
            transaction.Status = orderStatus;
            transaction.ResponseBody = responseData.ToString();
            transaction.UpdatedOn = DateTime.UtcNow;

            resultTransaction = await _transactionRepository.UpdateAsync(transaction);

            _emailService.SendPruchaseMail(resultTransaction, oldUser);

            var orderImageUrlMessage = string.Empty;
            var orderDescriptionMessage = string.Empty;
            var orderTitle = string.Empty;

            var netStatus = orderStatus switch
            {
                EOrderStatus.CANCELLED => "Cancelled",
                EOrderStatus.DECLINED => "Declined",
                EOrderStatus.FULLYPAID => "Successful",
                EOrderStatus.REFUSED => "Refused",
                _ => "Unknown"
            };

            orderImageUrlMessage = orderStatus switch
            {
                EOrderStatus.FULLYPAID => increaseOrBuy ? "https://auth.apptech.edu.az/uploads/check1.png" :
                "https://auth.apptech.edu.az/uploads/check2.png",
                EOrderStatus.CANCELLED => "https://auth.apptech.edu.az/uploads/error.png",
                EOrderStatus.DECLINED => "https://auth.apptech.edu.az/uploads/error.png",
                EOrderStatus.REFUSED => "https://auth.apptech.edu.az/uploads/error.png",
                _ => "https://auth.apptech.edu.az/uploads/error.png",
            };

            var lang = new LanguageCatcher(_http).GetLanguage();

            switch (lang)
            {
                case "en":
                    orderTitle = $"Payment {netStatus}";
                    orderDescriptionMessage = netStatus switch
                    {
                        "Cancelled" => "Your payment has been cancelled. This could be due to your request to stop the transaction. If this was unintentional, please try the payment again or contact support.",
                        "Declined" => "Your payment was declined by the system. This might occur due to insufficient funds or the system not authorizing the transaction. Please verify your payment details or contact your bank for assistance.",
                        "Successful" => "Your payment was successfully completed through the system. The exam dumps have been successfully added to your products page. Thank you for your payment.",
                        "Refused" => "Your payment has been refused because some data related to the system was altered. Please review the transaction details or contact support for more information.",
                        _ => "The payment status is unknown. Please contact customer support for more information."
                    };
                    break;
                case "ru":
                    orderTitle = netStatus switch
                    {
                        "Cancelled" => "Оплата отменена",
                        "Declined" => "Оплата отклонена",
                        "Successful" => "Оплата завершена",
                        "Refused" => "Оплата отказана",
                        _ => "Неизвестный статус"
                    };
                    orderDescriptionMessage = netStatus switch
                    {
                        "Cancelled" => "Ваш платеж был отменен. Это могло произойти из-за вашего запроса на остановку транзакции. Если это произошло по ошибке, повторите попытку оплаты или свяжитесь с поддержкой.",
                        "Declined" => "Ваш платеж был отклонен системой. Это может произойти из-за недостаточных средств или неавторизации транзакции системой. Пожалуйста, проверьте данные для оплаты или свяжитесь с банком для получения помощи.",
                        "Successful" => "Ваш платеж был успешно завершен через систему. Экзаменационные материалы успешно добавлены на вашу страницу продуктов. Благодарим вас за оплату.",
                        "Refused" => "Ваш платеж был отклонен, потому что были изменены некоторые данные, связанные с системой. Пожалуйста, проверьте детали транзакции или свяжитесь с поддержкой для получения дополнительной информации.",
                        _ => "Статус платежа неизвестен. Пожалуйста, свяжитесь со службой поддержки для получения дополнительной информации."
                    };
                    break;
                case "az":
                    orderTitle = netStatus switch
                    {
                        "Cancelled" => "Ödəmə ləğv edildi",
                        "Declined" => "Ödəmə rədd edildi",
                        "Successful" => "Ödəmə tamamlandı",
                        "Refused" => "Ödəmə imtina edildi",
                        _ => "Naməlum status"
                    };
                    orderDescriptionMessage = netStatus switch
                    {
                        "Cancelled" => "Ödəməniz ləğv edildi. Bu, sizin əməliyyatı dayandırma istəyiniz səbəbindən baş verə bilər. Əgər bu təsadüfi olubsa, ödənişi yenidən cəhd edin və ya dəstəklə əlaqə saxlayın.",
                        "Declined" => "Ödəməniz sistem tərəfindən rədd edildi. Bu, yetersiz vəsaitlər və ya sistemin əməliyyatı təsdiqləməməsi səbəbindən ola bilər. Ödəmə detalları yoxlayın və ya yardım üçün bankınıza müraciət edin.",
                        "Successful" => "Ödəməniz sistem vasitəsilə uğurla tamamlandı. İmtahan materialları məhsullar səhifənizə uğurla əlavə edildi. Ödənişiniz üçün təşəkkür edirik.",
                        "Refused" => "Ödəməniz, sistem ilə əlaqəli bəzi məlumatların dəyişdirilməsi səbəbindən imtina edilib. Əməliyyat detalları yoxlayın və ya əlavə məlumat üçün dəstəyə müraciət edin.",
                        _ => "Ödəmə statusu naməlumdur. Əlavə məlumat üçün müştəri xidməti ilə əlaqə saxlayın."
                    };
                    break;
                default:
                    orderTitle = $"Payment {netStatus}";
                    orderDescriptionMessage = netStatus switch
                    {
                        "Cancelled" => "Your payment has been cancelled. This could be due to your request to stop the transaction. If this was unintentional, please try the payment again or contact support.",
                        "Declined" => "Your payment was declined by the system. This might occur due to insufficient funds or the system not authorizing the transaction. Please verify your payment details or contact your bank for assistance.",
                        "Successful" => "Your payment was successfully completed through the system. The exam dumps have been successfully added to your products page. Thank you for your payment.",
                        "Refused" => "Your payment has been refused because some data related to the system was altered. Please review the transaction details or contact support for more information.",
                        _ => "The payment status is unknown. Please contact customer support for more information."
                    };
                    break;
            }

            return new OrderResponseDTO
            {
                OrderTitle = orderTitle,
                OrderDescription = orderDescriptionMessage,
                OrderImageUrl = orderImageUrlMessage,
                OrderStatus = netStatus,
                OrderId = resultTransaction.OrderId,
                ApprovedOn = resultTransaction.UpdatedOn,
            };
        }

        public string GeneratePaymentCheckToken()
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
