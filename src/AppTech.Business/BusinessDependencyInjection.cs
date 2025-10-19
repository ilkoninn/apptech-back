using System.Text.Json;
using AppTech.Business.Helpers;
using AppTech.Business.Services.BackgroundServices;
using AppTech.Business.Services.ExternalServices.Abstractions;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Services.InternalServices.Abstractions;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Business.Validators;
using AppTech.Shared.Implementations;
using AppTech.Shared.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTech.Business
{
    public static class BusinessDependencyInjection
    {
        public static IServiceCollection AddBusiness(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServices();
            services.RegisterAutoMapper();
            services.AddControllers(options =>
            {
                options.Conventions.Add(new PluralizedRouteConvention());
                options.ModelValidatorProviders.Clear();
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateCompanyDTOValidator>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            return services;
        }

        private static void AddServices(this IServiceCollection services)
        {
            // Internal Services
            services.AddScoped<IClaimService, ClaimService>();

            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyTranslationService, CompanyTranslationService>();

            services.AddScoped<ICertificationService, CertificationService>();
            services.AddScoped<ICertificationTranslationService, CertificationTranslationService>();

            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<INewsTranslationService, NewsTranslationService>();

            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IExamTranslationService, ExamTranslationService>();
            services.AddScoped<IExamPackageService, ExamPackageService>();

            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<ISettingTranslationService, SettingTranslationService>();

            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IPublicIpAddressService, PublicIpAddressService>();

            services.AddScoped<IPartnerService, PartnerService>();

            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IVariantService, VariantService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddScoped<IGiftCardService, GiftCardService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IFAQService, FAQService>();

            services.AddScoped<IAvatarService, AvatarService>();

            // External Services
            services.AddScoped<IExamCacheService, ExamCacheService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IFileManagerService, FileManagerService>();
            services.AddScoped<HttpClient>();

            // Background Services
            services.AddHostedService<UserStatusCleanUpBackgroundService>();
            services.AddHostedService<ExamStatusCleanUpBackgroundService>();
            services.AddHostedService<SubscriptionCheckerBackgroundService>();
            services.AddHostedService<UserPublicIpAddressCleanUpBackgroundService>();

            // Singleton Services
            services.AddSingleton<IDigitalOceanService, DigitalOceanService>();
            services.AddSingleton<ITerminalService, TerminalService>();
        }

        private static void RegisterAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(BusinessDependencyInjection));
        }

        public class PluralizedRouteConvention : IControllerModelConvention
        {
            public void Apply(ControllerModel controller)
            {
                if (controller == null)
                    throw new ArgumentNullException(nameof(controller));

                var pluralizedName = NameHelper.PluralizeControllerName(controller.ControllerName);

                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel != null)
                    {
                        selector.AttributeRouteModel.Template = $"api/{pluralizedName}";
                    }
                }
            }
        }
    }
}
