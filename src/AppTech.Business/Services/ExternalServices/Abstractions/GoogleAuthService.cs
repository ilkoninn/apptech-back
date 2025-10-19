using System.Globalization;
using System.Security.Claims;
using System.Text;
using AppTech.Business.DTOs.GoogleDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.DAL.Repositories.Interfaces;
using Diacritics.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal.Json;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly IPublicIpAddressService _publicIpAddressService;
        private readonly IPublicIpAddressRepository _publicIpAddressRepository;
        private readonly IHttpContextAccessor _http;

        public GoogleAuthService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IAccountService accountService,
            IPublicIpAddressService publicIpAddressService,
            IHttpContextAccessor http,
            IPublicIpAddressRepository publicIpAddressRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _accountService = accountService;
            _publicIpAddressService = publicIpAddressService;
            _http = http;
            _publicIpAddressRepository = publicIpAddressRepository;
        }
        public async Task<RegisterGoogleUserResponseDTO> RegisterGoogleAccountAsync(RegisterGoogleUserDTO dto)
        {
            var user = dto.Principal;
            var lang = dto.Lang;

            if (user != null)
            {
                var userEmail = user.FindFirstValue(ClaimTypes.Email);
                var userFirstName = user.FindFirstValue(ClaimTypes.GivenName);
                var userLastName = user.FindFirstValue(ClaimTypes.Surname);
                var profileImageUrl = user.FindFirstValue("picture");

                var oldUser = await _userManager.FindByEmailAsync(userEmail);

                if (oldUser != null)
                {
                    var oldUserRole = await _accountService.GetUserRoleAsync(oldUser);
                    var oldUserToken = JwtGenerator.GenerateToken(oldUser, oldUserRole, _configuration);

                    if (oldUser.IsBanned)
                    {
                        return new RegisterGoogleUserResponseDTO
                        {
                            redirectUrl = $"https://apptech.edu.az?error={Uri.EscapeDataString(lang switch
                            {
                                "ru" => "Ваш аккаунт заблокирован за нарушение правил использования. Пожалуйста, свяжитесь с поддержкой для решения вопроса.",
                                "az" => "Hesabınız istifadə qaydalarına riayət etmədiyiniz üçün bloklanıb. Problemin həlli üçün dəstək xidməti ilə əlaqə saxlayın.",
                                _ => "Your account is blocked due to a violation of usage policies. Please contact support for assistance.",
                            })}"
                        };
                    }

                    if (await _accountService.CheckUserStatus(oldUser.Email))
                    {
                        return new RegisterGoogleUserResponseDTO
                        {
                            redirectUrl = $"https://apptech.edu.az?error={Uri.EscapeDataString(lang switch
                            {
                                "ru" => "Пользователь в данный момент используется на другом устройстве.",
                                "az" => "İstifadəçi hazırda digər bir cihaz tərəfindən istifadə olunur.",
                                _ => "User is currently being used by another device.",
                            })}"
                        };
                    }
                    else
                    {
                        if(oldUserRole == EUserRole.Student.ToString())
                        {
                            var isUserHasThisPublicIpAddress = (await _publicIpAddressRepository.GetAllAsync(
                           x => !x.IsDeleted && x.UserId == oldUser.Id && x.PublicIpAddress == dto.IpAddress)).Any();

                            if (!isUserHasThisPublicIpAddress)
                            {
                                var publicIpAddressCount = (await _publicIpAddressRepository.GetAllAsync(
                                    x => !x.IsDeleted && x.UserId == oldUser.Id)).Select(x => x.PublicIpAddress).Distinct().Count();

                                if (publicIpAddressCount >= 3)
                                {
                                    if (oldUser.PublicIpAddressAccessFailed < 3)
                                    {
                                        oldUser.PublicIpAddressAccessFailed += 1;

                                        var resultPublic = await _userManager.UpdateAsync(oldUser);

                                        return new RegisterGoogleUserResponseDTO
                                        {
                                            redirectUrl = $"https://apptech.edu.az?error={Uri.EscapeDataString(lang switch
                                            {
                                                "ru" => "В настоящее время вы нарушаете правила входа. В связи с этим ваш аккаунт может быть заблокирован.",
                                                "az" => "Hal-hazırda giriş qaydalarına riayət etmirsiniz. Buna görə hesabınız bloklana bilər.",
                                                _ => "You are currently not adhering to login rules. Your account may be blocked as a result.",
                                            })}"
                                        };
                                    }

                                    oldUser.IsBanned = true;
                                    oldUser.PublicIpAddressAccessFailed = 0;

                                    var resultBan = await _userManager.UpdateAsync(oldUser);

                                    return new RegisterGoogleUserResponseDTO
                                    {
                                        redirectUrl = $"https://apptech.edu.az?error={Uri.EscapeDataString(lang switch
                                        {
                                            "ru" => "Ваш аккаунт заблокирован за нарушение правил использования. Пожалуйста, свяжитесь с поддержкой для решения вопроса.",
                                            "az" => "Hesabınız istifadə qaydalarına riayət etmədiyiniz üçün bloklanıb. Problemin həlli üçün dəstək xidməti ilə əlaqə saxlayın.",
                                            _ => "Your account is blocked due to a violation of usage policies. Please contact support for assistance.",
                                        })}"
                                    };
                                }

                                var newPublicIpAddress = new UserPublicIpAddress()
                                {
                                    UserId = oldUser.Id,
                                    PublicIpAddress = dto.IpAddress,
                                    ExpiredOn = DateTime.UtcNow.AddDays(1),
                                };

                                await _publicIpAddressRepository.AddAsync(newPublicIpAddress);
                            }
                        }

                        oldUser.IsOnline = true;
                        oldUser.OnlineTimer = DateTime.UtcNow;
                        oldUser.LastActivity = DateTime.UtcNow;
                        _accountService.CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
                    }

                    return new RegisterGoogleUserResponseDTO
                    {
                        statusCode = 200,
                        message = "User has authorized with Google.",
                        token = oldUserToken
                    };
                }

                var newUser = new User
                {
                    UserName = $"{userFirstName.RemoveDiacritics()}{userLastName.RemoveDiacritics()}{new Random().Next(0, 9999)}",
                    FullName = $"{userFirstName} {userLastName}",
                    Email = userEmail,
                    EmailConfirmed = true,
                    ImageUrl = profileImageUrl
                };

                var createResult = await _userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                {
                    _accountService.CheckIdentityResult(createResult);
                    return new RegisterGoogleUserResponseDTO
                    {
                        statusCode = 400,
                        message = "User registration failed."
                    };
                }

                // Add Google login info
                var loginInfo = new UserLoginInfo("Google", user.FindFirstValue(ClaimTypes.NameIdentifier), "Google");
                var addLoginResult = await _userManager.AddLoginAsync(newUser, loginInfo);
                if (!addLoginResult.Succeeded)
                {
                    _accountService.CheckIdentityResult(addLoginResult);
                    return new RegisterGoogleUserResponseDTO
                    {
                        statusCode = 400,
                        message = "Adding Google login info failed."
                    };
                }

                // Assign role to the user
                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, "Student");
                if (!addToRoleResult.Succeeded)
                {
                    _accountService.CheckIdentityResult(addToRoleResult);
                    return new RegisterGoogleUserResponseDTO
                    {
                        statusCode = 400,
                        message = "Adding role to user failed."
                    };
                }

                var userRole = await _accountService.GetUserRoleAsync(newUser);
                var token = JwtGenerator.GenerateToken(newUser, userRole, _configuration);

                await _publicIpAddressService.CheckUserPublicIpAddressAsync(dto.IpAddress, newUser.Email);

                newUser.IsOnline = true;
                newUser.OnlineTimer = DateTime.UtcNow;
                newUser.LastActivity = DateTime.UtcNow;
                _accountService.CheckIdentityResult(await _userManager.UpdateAsync(newUser));

                return new RegisterGoogleUserResponseDTO
                {
                    statusCode = 201,
                    message = "Google authentication successful. User registered.",
                    token = token
                };
            }

            return new RegisterGoogleUserResponseDTO
            {
                statusCode = 400,
                message = "Google authentication error."
            };
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}
