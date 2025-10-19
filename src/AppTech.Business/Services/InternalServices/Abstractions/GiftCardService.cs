using System.Security.Cryptography;
using System.Text;
using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.DTOs.TransactionDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.Core.Exceptions.GiftcardExceptions;
using AppTech.Core.Exceptions.UserExceptions;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class GiftCardService : IGiftCardService
    {
        protected readonly ITransactionTranslationRepository _transactionTranslationRepository;
        protected readonly INotificationRepository _notificationRepository;
        protected readonly ITransactionRepository _transactionRepository;
        protected readonly IGiftCardRepository _giftCardRepository;
        protected readonly IGiftCardHandler _giftCardHanlder;
        protected readonly IAccountService _accountService;
        protected readonly UserManager<User> _userManager;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public GiftCardService(IGiftCardRepository giftCardRepository, IMapper mapper,
            IConfiguration configuration, IGiftCardHandler giftCardHanlder, ITransactionRepository transactionRepository, UserManager<User> userManager, IAccountService accountService, IHttpContextAccessor http, ITransactionTranslationRepository transactionTranslationRepository, INotificationRepository notificationRepository)
        {
            _transactionTranslationRepository = transactionTranslationRepository;
            _notificationRepository = notificationRepository;
            _transactionRepository = transactionRepository;
            _giftCardRepository = giftCardRepository;
            _giftCardHanlder = giftCardHanlder;
            _accountService = accountService;
            _userManager = userManager;
            _mapper = mapper;
            _http = http;
        }

        public async Task<IQueryable<GiftCardDTO>> GetAllAsync()
        {
            var query = await _giftCardRepository.GetAllAsync(x => x.IsDeleted == false);

            return query.Select(g => new GiftCardDTO
            {
                Id = g.Id,
                Type = EnumExtensions.EnumToString(g.Type),
                Price = g.Price,
                ImageUrl = g.ImageUrl,
            }).AsQueryable();
        }

        public async Task<GiftCardDTO> GetByIdAsync(GetByIdGiftCardDTO dto)
        {
            var entity = _giftCardHanlder.HandleEntityAsync(
                await _giftCardRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new GiftCardDTO
            {
                Id = entity.Id,
                Type = EnumExtensions.EnumToString(entity.Type),
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
            };
        }

        public async Task<GiftCardDTO> AddAsync(CreateGiftCardDTO dto)
        {
            var map = _mapper.Map<GiftCard>(dto);
            var entity = await _giftCardRepository.AddAsync(map);

            return new GiftCardDTO
            {
                Id = entity.Id,
                Type = EnumExtensions.EnumToString(entity.Type),
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
            };
        }

        public async Task<GiftCardDTO> DeleteAsync(DeleteGiftCardDTO dto)
        {
            var entity = await _giftCardRepository.DeleteAsync(
                _giftCardHanlder.HandleEntityAsync(
                await _giftCardRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new GiftCardDTO
            {
                Id = entity.Id,
                Type = EnumExtensions.EnumToString(entity.Type),
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
            };
        }

        public async Task<GiftCardDTO> UpdateAsync(UpdateGiftCardDTO dto)
        {
            var entity = await _giftCardRepository.UpdateAsync(
                 _giftCardHanlder.HandleEntityAsync(
                _mapper.Map(dto,
                await _giftCardRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new GiftCardDTO
            {
                Id = entity.Id,
                Type = EnumExtensions.EnumToString(entity.Type),
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
            };
        }

        public async Task<OrderResponseDTO> SendMoneyToRecipientEmail(GiftcardRecipientDTO dto)
        {
            var checkCurrentToken = GeneratePaymentCheckToken();
            var checkRecipientToken = GeneratePaymentCheckToken();

            var giftcard = await _giftCardRepository.GetByIdAsync(x => x.Id == dto.giftId);
            var giftcardPrice = giftcard.Price;
            var giftcardId = giftcard.Id;

            var lang = new LanguageCatcher(_http).GetLanguage();
            var orderTitle = string.Empty;
            var orderDescription = string.Empty;
            var orderDescriptionMessage = string.Empty;

            var currentUser = await _accountService.CheckNotFoundByIdAsync(dto.userId);
            var recipientUser = await _accountService.CheckNotFoundForLoginByUsernameOrEmailAsync(dto.email);

            if (!recipientUser.EmailConfirmed || !currentUser.EmailConfirmed)
                throw new RecipientUserEmailOrCurrentUserEmailIsNotConfirmedException();

            if (currentUser.Email == recipientUser.Email)
                throw new UserCanNotSendGiftCardBySelfException();

            var checkBalanceForExamPrice = currentUser.Balance >= giftcardPrice;

            if (!checkBalanceForExamPrice)
            {
                switch (lang)
                {
                    case "az":
                        orderTitle = "Göndərmək Alınmadı";
                        orderDescription = "Ödənişi tamamlamanız üçün kifayət qədər balansınız yoxdur. Zəhmət olmasa hesabınıza vəsait əlavə edin.";
                        break;
                    case "ru":
                        orderTitle = "Платеж Не Удался";
                        orderDescription = "У вас недостаточно средств для завершения платежа. Пожалуйста, пополните свой счет.";
                        break;
                    case "en":
                    default:
                        orderTitle = "Payment Failed";
                        orderDescription = "You do not have enough balance to complete the payment. Please top up your account.";
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

            // Add Gift From Current User To Recipient User
            if (giftcardId == 0)
                throw new Exception("There is an error in payment section.");

            currentUser.Balance -= giftcardPrice;
            recipientUser.Balance += giftcardPrice;

            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(currentUser));
            _accountService.CheckIdentityResult(await _userManager.UpdateAsync(recipientUser));

            var currentOrderIdFromResponse = new Random().Next(10000, 99999).ToString();
            var recipientOrderIdFromResponse = new Random().Next(10000, 99999).ToString();

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var currentSessionIdFromResponse = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
            var recipientSessionIdFromResponse = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var newCurrentTransaction = new Transaction()
            {
                UserId = currentUser.Id,
                OrderId = long.Parse(currentOrderIdFromResponse),
                Amount = giftcardPrice,
                SessionId = currentSessionIdFromResponse,
                CheckToken = checkCurrentToken,
                Type = EOrderType.CASH,
                Status = EOrderStatus.FULLYPAID
            };

            var newRecipientTransaction = new Transaction()
            {
                UserId = recipientUser.Id,
                OrderId = long.Parse(recipientOrderIdFromResponse),
                Amount = giftcardPrice,
                SessionId = recipientSessionIdFromResponse,
                CheckToken = checkRecipientToken,
                Type = EOrderType.CASH,
                Status = EOrderStatus.FULLYPAID,
                IsIncreased = true,
            };

            var createdCurrentTransaction = await _transactionRepository.AddAsync(newCurrentTransaction);

            var newAzTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdCurrentTransaction.Id,
                Description = $"Hədiyyə kartı göndərildi. ({currentUser.UserName ?? currentUser.FullName})",
                Language = ELanguage.Az
            };

            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation);

            var newEnTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdCurrentTransaction.Id,
                Description = $"Gift card sent. ({currentUser.UserName ?? currentUser.FullName})",
                Language = ELanguage.En
            };

            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation);

            var newRuTransactionTranslation = new TransactionTranslation
            {
                TransactionId = createdCurrentTransaction.Id,
                Description = $"Подарочная карта отправлена. ({currentUser.UserName ?? currentUser.FullName})",
                Language = ELanguage.Ru
            };

            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation);

            var createdRecipientTransaction = await _transactionRepository.AddAsync(newRecipientTransaction);

            var newAzTransactionTranslation2 = new TransactionTranslation
            {
                TransactionId = createdRecipientTransaction.Id,
                Description = $"Hədiyyə kartı {currentUser.UserName ?? currentUser.FullName} tərəfindən alındı.",
                Language = ELanguage.Az
            };

            await _transactionTranslationRepository.AddAsync(newAzTransactionTranslation2);

            var newEnTransactionTranslation2 = new TransactionTranslation
            {
                TransactionId = createdRecipientTransaction.Id,
                Description = $"Gift card received from {currentUser.UserName ?? currentUser.FullName}.",
                Language = ELanguage.En
            };

            await _transactionTranslationRepository.AddAsync(newEnTransactionTranslation2);

            var newRuTransactionTranslation2 = new TransactionTranslation
            {
                TransactionId = createdRecipientTransaction.Id,
                Description = $"Подарочная карта получена от {currentUser.UserName ?? currentUser.FullName}.",
                Language = ELanguage.Ru
            };

            await _transactionTranslationRepository.AddAsync(newRuTransactionTranslation2);


            var orderStatus = createdCurrentTransaction.Status;
            var netStatus = "Successful";
            var orderImageUrlMessage = "https://auth.apptech.edu.az/uploads/check1.png";

            switch (lang)
            {
                case "az":
                    orderTitle = "Uğurla Göndərildi";
                    orderDescriptionMessage = "Ödənişiniz uğurla tamamlandı. Vəsait qarşı tərəfə uğurla göndərildi. Ödənişiniz üçün təşəkkür edirik.";
                    break;
                case "ru":
                    orderTitle = "Платеж Успешен";
                    orderDescriptionMessage = "Ваш платеж успешно завершен. Средства успешно отправлены получателю. Благодарим вас за оплату.";
                    break;
                case "en":
                default:
                    orderTitle = "Payment Successful";
                    orderDescriptionMessage = "Your payment was successful. The funds have been successfully sent to the recipient. Thank you for your payment.";
                    break;
            }

            var notificationTitle = string.Empty;
            var userInfo = currentUser.FullName is not null ? currentUser.FullName : currentUser.UserName;

            switch (lang)
            {
                case "az":
                    notificationTitle = $"{userInfo} tərəfindən gift card göndərildi."; 
                    break;
                case "ru":
                    notificationTitle = $"{userInfo} отправил подарочную карту."; 
                    break;
                case "en":
                default:
                    notificationTitle = $"{userInfo} has sent a gift card."; 
                    break;
            }

            var newNotification = new Notification
            {
                Title = notificationTitle,
                UserId = recipientUser.Id,
                Description = dto.description,
            };

            await _notificationRepository.AddAsync(newNotification);

            return new OrderResponseDTO
            {
                OrderTitle = orderTitle,
                OrderDescription = orderDescriptionMessage,
                OrderImageUrl = orderImageUrlMessage,
                OrderStatus = netStatus,
                OrderId = createdCurrentTransaction.OrderId,
                ApprovedOn = createdCurrentTransaction.UpdatedOn,
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
