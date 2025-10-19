using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using AppTech.API.Middlewares;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AppTech.API
{
    public static class ApiDependencyInjection
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration.GetValue<string>("JwtConfiguration:SecretKey");
            var issuer = configuration.GetValue<string>("JwtConfiguration:Issuer");
            var audience = configuration.GetValue<string>("JwtConfiguration:Audience");

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", builder =>
                {
                    builder.WithOrigins(
                        "https://auth.apptech.edu.az",
                        "http://172.31.16.49:88",
                        "http://172.31.16.49:99",
                        "http://172.31.16.49:999",
                        "https://apptech.edu.az",
                        "http://localhost:5173",
                        "http://localhost:5076"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
             .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
             {
                 options.Cookie.Name = "google-login";
                 options.Cookie.Domain = "apptech.edu.az";
                 options.Cookie.HttpOnly = false;
                 options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                 options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                 options.Cookie.Path = "/";
                 options.ExpireTimeSpan = TimeSpan.FromDays(30);
                 options.SlidingExpiration = true;
             })
            .AddGoogle(options =>
            {
                options.ClientId = "44984051492-v1r6ugi6hi7jgorkfljo4r44vt3tgkk6.apps.googleusercontent.com";
                options.ClientSecret = "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";
                options.CallbackPath = "/signin-google";

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Events.OnCreatingTicket = (context) =>
                {
                    var picture = context.User.GetProperty("picture").GetString();
                    context.Identity.AddClaim(new Claim("picture", picture));
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(opt =>
            {
                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Token failed to authenticate: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated: " + context.SecurityToken);
                        return Task.CompletedTask;
                    }
                };
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["JwtConfiguration:Issuer"],
                    ValidAudience = configuration["JwtConfiguration:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtConfiguration:SecretKey"])),
                    LifetimeValidator = (notBefore, expires, tokenToValidate, tokenValidationParameters) =>
                    {
                        return expires != null && expires > DateTime.UtcNow;
                    }
                };
            });
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.OperationFilter<RedirectFilter>();
                opt.OperationFilter<AcceptedLanguageHeaderFilter>();

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "AppTech.API", Version = "v1" });

                opt.SchemaFilter<EnumSchemaFilter>();
            });
        }

        public static void AddMiddlewares(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            builder.UseMiddleware<XssProtectionMiddleware>();
            builder.UseMiddleware<HttpsSchemeMiddleware>();

            // Web Socket Section
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60), 
                ReceiveBufferSize = 8 * 1024,
                AllowedOrigins = {
                        "https://apptech.edu.az",
                        "http://192.168.1.88:88",
                        "http://192.168.1.88:99",
                        "http://127.0.0.1:5500",
                        "http://localhost:5173",
                        "http://localhost:5076",
                        "https://auth.apptech.edu.az"
                }
            };

            builder.UseWebSockets(webSocketOptions);

            builder.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws-terminal")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        var query = context.Request.Query;
                        var token = query["token"].ToString();

                        var dropletRepository = context.RequestServices.GetRequiredService<IDropletRepository>();

                        var result = (await dropletRepository.GetAllAsync(x => !x.IsDeleted))
                            .FirstOrDefault(x => x.Token == token);

                        if (result == null)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("Droplet not found");
                            return;
                        }

                        var terminalService = context.RequestServices.GetRequiredService<ITerminalService>();

                        var dto = new WebSocketDropletDTO
                        {
                            DropletPublicIp = result.PublicIpAddress,
                            PrivateKeyPath = result.PrivateKeyPath,
                            Username = result.CustomUsername,
                            Password = result.Password
                        };

                        await terminalService.HandleWebSocketConnection(webSocket, dto);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        public static void AddRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy<string>("SlidingWindow", x =>
                {
                    if (x.User?.Identity?.IsAuthenticated == true)
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(x.Request.Headers.Authorization.ToString(), factory =>
                        {
                            return new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = 10,
                                Window = TimeSpan.FromMinutes(10),
                                QueueLimit = 1,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                SegmentsPerWindow = 5
                            };
                        });
                    }

                    return RateLimitPartition.GetSlidingWindowLimiter(x.Connection.RemoteIpAddress.ToString(), factory =>
                    {
                        return new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            SegmentsPerWindow = 2
                        };
                    });
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Request rejected due to rate limiting. Path: {Path}, IP: {IP}",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            Message = $"You have exceeded the rate limit. Please try again after {retryAfter.TotalMinutes} minute(s).",
                            ErrorCode = "RATE_LIMIT_EXCEEDED"
                        }, token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            Message = "You have exceeded the rate limit. Please try again later.",
                            ErrorCode = "RATE_LIMIT_EXCEEDED"
                        }, token);
                    }
                };
            });
        }
    }
}
