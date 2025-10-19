using System.IdentityModel.Tokens.Jwt;
using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.GoogleDTOs;
using AppTech.Business.DTOs.UserDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.AppUserValidator;
using AppTech.Business.Validators.UserValidators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly IAccountService _accountService;
        private readonly IHttpContextAccessor _http;
        private readonly IGoogleAuthService _googleAuthService;

        public AccountController(IAccountService accountService, IHttpContextAccessor http, IGoogleAuthService googleAuthService)
        {
            _accountService = accountService;
            _http = http;
            _googleAuthService = googleAuthService;
        }

        // Authorize Verifying
        [HttpGet("check-bearer-authorize")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public IActionResult IsBearerAuthorizedAsync()
        {
            return Ok(new { message = "This is a protected method. You have authorized with bearer." });
        }

        [HttpGet("check-token-authorize")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public IActionResult IsCustomAuthorizedAsync()
        {
            return Ok(new { message = "This is a protected method. You have authorized with custom token.", IsLogin = true });
        }

        [HttpGet("get-jwt-token")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public IActionResult GetJwtTokenAsync()
        {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token is missing or invalid" });
            }

            var jwtTokenString = authorizationHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(jwtTokenString);

            return Ok(new { token = jwtToken.RawData });
        }

        // Registration Section
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDTO dto)
        {
            var validations = await new RegisterUserDTOValidator().ValidateAsync(dto);
            var registerResponse = await _accountService.RegisterAsync(dto);

            return validations.IsValid ? Ok(registerResponse) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDTO dto)
        {
            var validations = await new ConfirmEmailDTOValidator().ValidateAsync(dto);
            await _accountService.EmailConfirmationAsync(dto);

            return validations.IsValid ? Ok() : BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("click-to-resend")]
        [AllowAnonymous]
        public async Task<IActionResult> ClickToResendAsync([FromBody] ClickToResendDTO dto)
        {
            var validations = await new ClickToResendDTOValidator().ValidateAsync(dto);
            await _accountService.ClickToResendAsync(dto);

            return validations.IsValid ? Ok() : BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        // Login Section
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserDTO dto)
        {
            var validations = await new LoginUserDTOValidator().ValidateAsync(dto);

            var loginResponse = await _accountService.LoginAsync(dto);

            var token = loginResponse.token;

            if (validations.IsValid)
            {
                return Ok(new { message = "Login successful", token });
            }
            else
            {
                return BadRequest(
                    new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
            }
        }

        [HttpGet("google-login")]
        [EnableCors("AllowReactApp")]
        public IActionResult GoogleLogin([FromQuery] UserIpDTO dto)
        {
            if (Request.Scheme != "https")
            {
                Request.Scheme = "https";
            }

            var redirectUri = Url.Action("GoogleResponse", "Account", values: null, protocol: "https");

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };

            properties.Items["UserIpAddress"] = dto.ipAddress;
            properties.Items["Language"] = dto.lang;

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        [EnableCors("AllowReactApp")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Google authentication failed.");
            }

            var user = authenticateResult.Principal;

            if (user == null || !user.Identity.IsAuthenticated)
            {
                return BadRequest("Google authentication failed.");
            }

            var ipAddress = authenticateResult.Properties.Items["UserIpAddress"];
            var language = authenticateResult.Properties.Items["Language"];

            var dto = new RegisterGoogleUserDTO
            {
                Principal = user,
                IpAddress = ipAddress,
                Lang = language
            };

            var response = await _googleAuthService.RegisterGoogleAccountAsync(dto);

            if (response.redirectUrl is not null)
                return Redirect(response.redirectUrl);

            if (response.statusCode == 200 || response.statusCode == 201)
            {
                Response.Cookies.Append("token", response.token, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Domain = "apptech.edu.az",
                    Path = "/",
                });

                return Redirect("https://apptech.edu.az");
            }
            else
            {
                return Redirect("https://apptech.edu.az");
            }
        }

        [HttpPost("get-user-summary")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> GetProtectedDataAsync([FromBody] JwtDTO dto)
        {
            if (string.IsNullOrEmpty(dto.jwtTokenString))
                return Unauthorized(new { message = "Token is missing or invalid" });

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(dto.jwtTokenString);

            var id = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            var oldUser = await _accountService.CheckNotFoundByIdAsync(id);

            var data = new
            {
                message = "This is protected data",
                userId = id,
                username = oldUser.UserName,
                userEmail = oldUser.Email,
                userImageUrl = oldUser.ImageUrl,
                fullName = oldUser.FullName,
                phone = oldUser.PhoneNumber,
                isConfirmed = oldUser.EmailConfirmed,
                balance = oldUser.Balance,
                onExam = oldUser.OnExam,
            };

            return Ok(data);
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutDTO dto)
        {
            await _accountService.LogoutAsync(dto);

            Response.Cookies.Delete("token", new CookieOptions
            {
                Domain = "apptech.edu.az",
                Path = "/",
                Secure = true,
                HttpOnly = false,
                SameSite = SameSiteMode.None
            });

            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("timer")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> UserTimerAsync([FromBody] UserTimerDTO dto)
        {
            await _accountService.ChangeUserStatus(dto.userId);

            return Ok(new { message = "Timer successfully updated." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordUserDTO dto)
        {
            var validations = await new ForgotPasswordUserDTOValidator().ValidateAsync(dto);
            await _accountService.ForgotPasswordAsync(dto);

            return validations.IsValid ? Ok() :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("confirm-forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmForgotPasswordAsync([FromBody] ForgotPasswordConfirmationDTO dto)
        {
            var validations = await new ConfirmForgotPasswordDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _accountService.ForgotPasswordConfirmationAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordUserDTO dto)
        {
            var validations = await new ResetPasswordUserDTOValidator().ValidateAsync(dto);
            await _accountService.ResetPasswordAsync(dto);

            return validations.IsValid ? Ok() :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        // Update User Section
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UpdateUserDTO dto)
        {
            var validations = await new UpdateUserDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _accountService.Update(dto, id)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpGet("confirm-update-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var dto = new ConfirmUpdateEmailDTO { userId = userId, token = token };
            await _accountService.ConfirmUpdateEmailAsync(dto);

            return Redirect("https://apptech.edu.az/");
        }

        [HttpPut("change-password/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ChangePasswordUserDTO dto)
        {
            var validations = await new ChangePasswordUserDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _accountService.ChangePasswordAsync(dto, id)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
