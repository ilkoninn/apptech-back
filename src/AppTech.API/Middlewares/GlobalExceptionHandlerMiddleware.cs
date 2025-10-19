using AppTech.Business.Helpers;
using AppTech.Core.Exceptions.CertificationExceptions;
using AppTech.Core.Exceptions.Commons;
using AppTech.Core.Exceptions.GiftcardExceptions;
using AppTech.Core.Exceptions.UploadException;
using AppTech.Core.Exceptions.UserExceptions;
using Newtonsoft.Json;

namespace AppTech.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _http;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, IHttpContextAccessor http)
        {
            _next = next;
            _http = http;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var lang = new LanguageCatcher(_http).GetLanguage();
            var code = StatusCodes.Status500InternalServerError;
            var exMessage = ex.Message ?? ex.InnerException.Message;

            // Handle specific exceptions and set appropriate status codes
            switch (ex)
            {
                case UserNotBoughtCurrentCertificationException userNotBought:
                    code = StatusCodes.Status400BadRequest;
                    exMessage = GetMessageUserNotBoughtCurrentCertificationException(lang);
                    break;
                case RecipientUserEmailOrCurrentUserEmailIsNotConfirmedException recipientOrCurrentEmailEx:
                    code = StatusCodes.Status400BadRequest;
                    exMessage = GetMessageForRecipientUserEmailOrCurrentUserEmailIsNotConfirmed(lang);
                    break;
                case UserCanNotSendGiftCardBySelfException userCanNotEx:
                    code = StatusCodes.Status400BadRequest;
                    exMessage = GetMessageForUserCanNotSendGiftCardBySelfException(lang);
                    break;
                case EntityNotFoundException entityNotEx:
                    code = StatusCodes.Status404NotFound;
                    exMessage = GetMessageForEntityNotFoundException(entityNotEx.EntityName, lang);
                    break;
                case UsernameOrEmailAddressNotFoundException usernameOrEmailEx:
                    code = StatusCodes.Status404NotFound;
                    exMessage = GetMessageForUsernameOrEmailAddressNotFoundException(lang);
                    break;
                case FileIsNotValidException fileIsNotEx:
                    code = StatusCodes.Status400BadRequest;
                    exMessage = GetMessageForFileIsNotValidException(lang);
                    break;
                case UserTokenExpiredException userTokenEx:
                    code = StatusCodes.Status401Unauthorized;
                    exMessage = GetMessageForUserTokenExpiredException(lang);
                    break;
                case EmailIsNotConfirmedException:
                    code = StatusCodes.Status403Forbidden;
                    exMessage = GetMessageForEmailIsNotConfirmedException(lang);
                    break;
                case UserCurrentlyBeingUsedByAnotherDeviceException userCurEx:
                    code = StatusCodes.Status423Locked;
                    exMessage = GetMessageForUserCurrentlyBeingUsedByAnotherDeviceException(lang);
                    break;
                case UserIpAddressException userIpEx:
                    code = StatusCodes.Status403Forbidden;
                    exMessage = GetMessageForUserIpAddressException(lang);
                    break;
                case UserCanBeBlockedByToManyPublicIpAddress userCanBeBlock:
                    code = StatusCodes.Status423Locked;
                    exMessage = GetMessageForUserCanBeBlockedByTooManyPublicIpAddressException(lang);
                    break;
                case UserIsBlockedByServer userIsBlocked:
                    code = StatusCodes.Status423Locked;
                    exMessage = GetMessageForUserIsBlockedByServerException(lang);
                    break;
                case UserLockOutException:
                    code = StatusCodes.Status423Locked;
                    exMessage = GetMessageForUserLockOutException(lang);
                    break;
                case ConfirmationNumberIsNotValidException:
                    code = StatusCodes.Status400BadRequest;
                    exMessage = GetMessageForConfirmationNumberIsNotValidException(lang);
                    break;
                case UserIdIsNotNullException userIdEx:
                    code = StatusCodes.Status401Unauthorized;
                    exMessage = GetMessageForUserIdIsNotNullException(userIdEx.Username, lang);
                    break;
            }

            var errorResponse = new
            {
                status = code,
                message = exMessage,
                inner = ex.InnerException?.Message,
                inner2 = ex.InnerException?.InnerException?.Message,
            };

            var result = JsonConvert.SerializeObject(errorResponse);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;

            return context.Response.WriteAsync(result);
        }

        private static string GetMessageUserNotBoughtCurrentCertificationException(string lang)
        {
            return lang switch
            {
                "ru" => "Пользователь не приобрел текущую сертификацию, поэтому не может оставить комментарий.",
                "az" => "Hal-hazırda sertifikatı əldə etmədiyiniz üçün ona şərh əlavə edə bilmirsiniz.",
                _ => "You cannot leave a comment because you have not purchased the certification.",
            };
        }


        private static string GetMessageForRecipientUserEmailOrCurrentUserEmailIsNotConfirmed(string lang)
        {
            return lang switch
            {
                "ru" => "Электронная почта получателя или текущего пользователя не подтверждена.",
                "az" => "Qəbul edənin və ya cari istifadəçinin e-poçtu təsdiqlənməyib.",
                _ => "Recipient's or current user's email is not confirmed.",
            };
        }

        private static string GetMessageForUserCanNotSendGiftCardBySelfException(string lang)
        {
            return lang switch
            {
                "ru" => "Пользователь не может отправить подарочную карту самому себе.",
                "az" => "İstifadəçi özünə hədiyyə kartı göndərə bilməz.",
                _ => "User cannot send a gift card to themselves.",
            };
        }

        private string GetMessageForUsernameOrEmailAddressNotFoundException(string lang)
        {
            return lang switch
            {
                "ru" => "Имя пользователя/электронная почта или пароль неверны.",
                "az" => "İstifadəçi adı/e-poçt ünvanı və ya parol yanlışdır.",
                _ => "Username/Email address or password is not valid.",
            };

        }

        private static string GetMessageForUserTokenExpiredException(string lang)
        {
            return lang switch
            {
                "ru" => "Срок действия токена пользователя истек.",
                "az" => "İstifadəçi tokeninin müddəti bitib.",
                _ => "User token has expired.",
            };
        }

        private static string GetMessageForUserCanBeBlockedByTooManyPublicIpAddressException(string lang)
        {
            return lang switch
            {
                "ru" => "В настоящее время вы нарушаете правила входа. В связи с этим ваш аккаунт может быть заблокирован.",
                "az" => "Hal-hazırda giriş qaydalarına riayət etmirsiniz. Buna görə hesabınız bloklana bilər.",
                _ => "You are currently not adhering to login rules. Your account may be blocked as a result.",
            };
        }

        private static string GetMessageForUserIsBlockedByServerException(string lang)
        {
            return lang switch
            {
                "ru" => "Ваш аккаунт заблокирован за нарушение правил использования. Пожалуйста, свяжитесь с поддержкой для решения вопроса.",
                "az" => "Hesabınız istifadə qaydalarına riayət etmədiyiniz üçün bloklanıb. Problemin həlli üçün dəstək xidməti ilə əlaqə saxlayın.",
                _ => "Your account is blocked due to a violation of usage policies. Please contact support for assistance.",
            };
        }

        private static string GetMessageForUserLockOutException(string lang)
        {
            return lang switch
            {
                "ru" => "Из-за слишком большого количества неудачных попыток входа пользователь временно заблокирован. Попробуйте снова через некоторое время.",
                "az" => "Çoxlu giriş cəhdlərindən sonra istifadəçi müvəqqəti olaraq bloklanıb. Bir müddət sonra yenidən yoxlayın.",
                _ => "Due to too many failed login attempts, the user is temporarily locked out. Please try again later.",
            };
        }

        private static string GetMessageForUserIpAddressException(string lang)
        {
            return lang switch
            {
                "ru" => "Ошибка IP-адреса пользователя.",
                "az" => "İstifadəçi IP ünvanı xətası.",
                _ => "User IP address error.",
            };
        }

        private static string GetMessageForUserCurrentlyBeingUsedByAnotherDeviceException(string lang)
        {
            return lang switch
            {
                "ru" => "Пользователь в данный момент используется на другом устройстве.",
                "az" => "İstifadəçi hazırda digər bir cihaz tərəfindən istifadə olunur.",
                _ => "User is currently being used by another device.",
            };
        }

        private static string GetMessageForEmailIsNotConfirmedException(string lang)
        {
            return lang switch
            {
                "ru" => "Электронная почта не подтверждена.",
                "az" => "E-poçt təsdiqlənməyib.",
                _ => "Email is not confirmed.",
            };
        }

        private static string GetMessageForUserIdIsNotNullException(string userName, string lang)
        {
            return lang switch
            {
                "ru" => $"Этот сертификат уже приобретен пользователем {userName}.",
                "az" => $"Bu sertifikat artıq {userName} tərəfindən əldə edilib.",
                _ => $"This certification has already been acquired by {userName}.",
            };
        }

        private static string GetMessageForEntityNotFoundException(string entityName, string lang)
        {
            return lang switch
            {
                "ru" => $"Сущность {entityName} не найдена.",
                "az" => $"{entityName} adlı obyekt tapılmadı.",
                _ => $"Entity {entityName} not found.",
            };
        }

        private static string GetMessageForFileIsNotValidException(string lang)
        {
            return lang switch
            {
                "ru" => "Файл недействителен.",
                "az" => "Fayl etibarsızdır.",
                _ => "File is not valid.",
            };
        }

        private static string GetMessageForConfirmationNumberIsNotValidException(string lang)
        {
            return lang switch
            {
                "ru" => "Номер подтверждения недействителен.",
                "az" => "Təsdiq nömrəsi etibarsızdır.",
                _ => "Confirmation number is not valid.",
            };
        }
    }
}
