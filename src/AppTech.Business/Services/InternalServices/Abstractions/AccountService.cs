using System.Net;
using AppTech.Business.DTOs.UserDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Exceptions.IdentityErrors;
using AppTech.Core.Exceptions.UserExceptions;
using AppTech.DAL.Persistence;
using AutoMapper;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _appDbContext;
        private readonly IFileManagerService _fileManagerService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _http;
        private readonly IPublicIpAddressService _publicIpAddressService;

        public AccountService(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration,
            IEmailService emailService,
            IMapper mapper,
            IWebHostEnvironment environment,
            IFileManagerService fileManagerService,
            IHttpContextAccessor http,
            IPublicIpAddressService publicIpAddressService,
            AppDbContext appDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
            _environment = environment;
            _fileManagerService = fileManagerService;
            _http = http;
            _publicIpAddressService = publicIpAddressService;
            _appDbContext = appDbContext;
        }

        // Registeration Section
        public async Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserDTO dto)
        {
            ChangeDefaultIndetityErrorDescriber(new LanguageCatcher(_http).GetLanguage());

            var newUser = _mapper.Map<User>(dto);
            var number = GenerateConfirmationNumber();

            newUser.ConfirmationCode = number;
            newUser.ConfirmationCodeSentAt = DateTime.UtcNow;

            var filePath = Path.Combine(_environment.WebRootPath, "uploads");

            newUser.ImageUrl = await _fileManagerService.UploadFileAsync(
                _fileManagerService.GetFile("profileimage.png", filePath));

            CheckIdentityResult(await _userManager.CreateAsync(newUser, dto.password));
            CheckIdentityResult(await _userManager.AddToRoleAsync(newUser, "Student"));

            await _emailService.SendConfirmationCodeMessageAsync(newUser, number);

            return new RegisterUserResponseDTO
            {
                email = dto.email,
            };
        }

        public async Task ClickToResendAsync(ClickToResendDTO dto)
        {
            var oldUser = await _userManager.FindByEmailAsync(dto.email);
            CheckConfirmationCodeSendAt(oldUser);

            var number = GenerateConfirmationNumber();

            oldUser.IsResent = true;
            oldUser.ConfirmationCode = number;
            oldUser.ConfirmationCodeSentAt = DateTime.UtcNow;

            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            await _emailService.SendConfirmationCodeMessageAsync(oldUser, number);
        }

        public async Task EmailConfirmationAsync(ConfirmEmailDTO dto)
        {
            ChangeDefaultIndetityErrorDescriber(new LanguageCatcher(_http).GetLanguage());

            var oldUser = await _userManager.FindByEmailAsync(dto.email);
            CheckConfirmationNumber(oldUser.ConfirmationCode, dto.number);

            oldUser.EmailConfirmed = true;
            oldUser.ConfirmationCode = null;
            oldUser.ConfirmationCodeSentAt = null;
            oldUser.IsResent = false;

            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        // Login Section
        public async Task<LoginUserResponseDTO> LoginAsync(LoginUserDTO dto)
        {
            var user = await CheckNotFoundForLoginByUsernameOrEmailAsync(dto.usernameOrEmail);
            var userRole = await GetUserRoleAsync(user);

            if (user.IsBanned)
                throw new UserIsBlockedByServer();

            await CheckUserPasswordAsync(user, dto.password);

            if (await CheckUserStatus(dto.usernameOrEmail))
                throw new UserCurrentlyBeingUsedByAnotherDeviceException();
            else
            {
                await _publicIpAddressService.CheckUserPublicIpAddressAsync(dto.ipAddress, dto.usernameOrEmail);

                try
                {
                    user.IsOnline = true;
                    user.OnlineTimer = DateTime.UtcNow;
                    user.LastActivity = DateTime.UtcNow;
                    CheckIdentityResult(await _userManager.UpdateAsync(user));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await _appDbContext.Entry(user).ReloadAsync();

                    // Retry the update
                    user.IsOnline = true;
                    user.OnlineTimer = DateTime.UtcNow;
                    user.LastActivity = DateTime.UtcNow;
                    CheckIdentityResult(await _userManager.UpdateAsync(user));
                }
            }

            var token = JwtGenerator.GenerateToken(user, userRole, _configuration);

            return new LoginUserResponseDTO
            {
                token = token,
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordUserDTO dto)
        {
            var oldUser = await CheckNotFoundForForgotPasswordByEmailAsync(dto.email);
            var number = GenerateConfirmationNumber();

            oldUser.ConfirmationCode = number;
            oldUser.ConfirmationCodeSentAt = DateTime.UtcNow;

            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            await _emailService.SendConfirmationCodeMessageAsync(oldUser, number);
        }

        public async Task<ForgotPasswordConfirmationResponseDTO> ForgotPasswordConfirmationAsync(ForgotPasswordConfirmationDTO dto)
        {
            var oldUser = await _userManager.FindByEmailAsync(dto.email);

            CheckConfirmationNumber(oldUser.ConfirmationCode, dto.number);

            oldUser.ConfirmationCode = null;
            oldUser.ConfirmationCodeSentAt = null;

            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            var userToken = await _userManager.GeneratePasswordResetTokenAsync(oldUser);
            var encodedToken = WebUtility.UrlEncode(userToken);

            return new ForgotPasswordConfirmationResponseDTO
            {
                token = encodedToken,
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordUserDTO dto)
        {
            var oldUser = await _userManager.FindByEmailAsync(dto.email);
            var decodedToken = WebUtility.UrlDecode(dto.token);

            CheckIdentityResult(await _userManager.ResetPasswordAsync(oldUser, decodedToken, dto.newPassword));
        }

        public async Task LogoutAsync(LogoutDTO dto)
        {
            var oldUser = await CheckNotFoundByIdAsync(dto.userId);

            oldUser.IsOnline = false;
            oldUser.OnlineTimer = null;
            oldUser.LastActivity = DateTime.UtcNow;
            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        // Update User Section
        public async Task<UpdateUserResponseDTO> Update(UpdateUserDTO dto, string userId)
        {
            var oldUser = await CheckNotFoundByIdAsync(userId);
            var oldEmailAddress = oldUser.Email;

            _mapper.Map(dto, oldUser);
            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

            if (oldEmailAddress != dto.email)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(oldUser);
                Console.WriteLine("Original Token: " + token);
                var encodedToken = WebUtility.UrlEncode(token);
                Console.WriteLine("Encoded Token: " + encodedToken);

                oldUser.EmailConfirmed = false;
                CheckIdentityResult(await _userManager.UpdateAsync(oldUser));

                await _emailService.SendConfirmationCodeMessageAsync(
                currentUser: oldUser, toUser: dto.email, number: 0, numberOrLink: false, token: encodedToken);
            }

            var message = new LanguageCatcher(_http).GetLanguage() switch
            {
                "en" => "Your account information has been successfully changed.",
                "ru" => "Информация вашего аккаунта была успешно изменена.",
                "az" => "Hesab məlumatlarınız uğurla dəyişdirildi.",
                _ => "Your account information has been successfully changed."
            };

            return new UpdateUserResponseDTO
            {
                Message = message
            };
        }

        public async Task ConfirmUpdateEmailAsync(ConfirmUpdateEmailDTO dto)
        {
            var oldUser = await CheckNotFoundByIdAsync(dto.userId);

            CheckIdentityResult(await _userManager.ConfirmEmailAsync(oldUser, dto.token));
        }

        public async Task<ChangePasswordResponseDTO> ChangePasswordAsync(ChangePasswordUserDTO dto, string userId)
        {
            ChangeDefaultIndetityErrorDescriber(new LanguageCatcher(_http).GetLanguage());
            var oldUser = await CheckNotFoundByIdAsync(userId);

            if(oldUser.PasswordHash is not null)
            {
                CheckIdentityResult(await _userManager.ChangePasswordAsync(oldUser, dto.currentPassword, dto.newPassword));
            }
            else
            {
                var setPasswordResult = await _userManager.AddPasswordAsync(oldUser, dto.newPassword);
                CheckIdentityResult(setPasswordResult);
            }

            var message = new LanguageCatcher(_http).GetLanguage() switch
            {
                "en" => "Your password has been successfully changed.",
                "ru" => "Ваш пароль был успешно изменен.",
                "az" => "Şifrəniz uğurla dəyişdirildi.",
                _ => "Your password has been successfully changed."
            };

            return new ChangePasswordResponseDTO
            {
                Message = message
            };
        }


        // Supportive Methods
        public int GenerateConfirmationNumber()
        {
            Random random = new Random();
            var digits = Enumerable.Range(0, 10).OrderBy(x => random.Next()).Take(6).ToArray();

            while (digits[0] == 0)
            {
                digits = Enumerable.Range(0, 10).OrderBy(x => random.Next()).Take(6).ToArray();
            }

            return int.Parse(string.Join("", digits));
        }

        public async Task<bool> CheckUserStatus(string userNameOrEmail)
        {
            var oldUser = await CheckNotFoundForLoginByUsernameOrEmailAsync(userNameOrEmail);

            return oldUser.IsOnline;
        }

        public async Task ChangeUserStatus(string userId)
        {
            var oldUser = await CheckNotFoundByIdAsync(userId);

            oldUser.IsOnline = true;
            oldUser.OnlineTimer = DateTime.UtcNow;
            oldUser.LastActivity = DateTime.UtcNow;
            CheckIdentityResult(await _userManager.UpdateAsync(oldUser));
        }

        public async Task<string> GetUserRoleAsync(User user)
        {
            return (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        }

        public void CheckIdentityResult(IdentityResult result)
        {
            if (result.Errors.Any(e => e.Code == "TokenExpired"))
                throw new UserTokenExpiredException();

            if (!result.Succeeded)
                throw new UserIdentityResultException($"{result.Errors.FirstOrDefault()?.Description}");
        }

        public void ChangeDefaultIndetityErrorDescriber(string lang)
        {
            var describer = (CustomIdentityErrorDescriber)_userManager.ErrorDescriber;
            describer.SetLanguage(lang);
        }

        public void CheckConfirmationNumber(int? userConfirmationNumber, int number)
        {
            if (userConfirmationNumber != number)
                throw new ConfirmationNumberIsNotValidException();
        }

        public void CheckConfirmationCodeSendAt(User oldUser)
        {
            if (oldUser.ConfirmationCodeSentAt.HasValue &&
                (DateTime.UtcNow - oldUser.ConfirmationCodeSentAt.Value).TotalMinutes < 1
                && oldUser.IsResent)
                throw new ConfirmationNumberIsNotValidException();
        }

        public async Task CheckUserPasswordAsync(User user, string password)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (!user.EmailConfirmed)
                throw new EmailIsNotConfirmedException();

            if (result.IsLockedOut)
                throw new UserLockOutException();

            if (!result.Succeeded)
                throw new UsernameOrEmailAddressNotFoundException();
        }

        public async Task<User> CheckNotFoundByIdAsync(string userId)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(x => x.Id == userId) ??
                throw new UsernameOrEmailAddressNotFoundException();
        }

        public async Task<User> CheckNotFoundForLoginByUsernameOrEmailAsync(string userNameOrEmail)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName.ToLower() == userNameOrEmail.ToLower()
                                        || x.Email.ToLower() == userNameOrEmail.ToLower());

            return user ?? throw new UsernameOrEmailAddressNotFoundException();
        }

        public async Task<User> CheckNotFoundForForgotPasswordByEmailAsync(string userEmail)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == userEmail.ToLower()) ??
                throw new UsernameOrEmailAddressNotFoundException();
        }

    }
}
