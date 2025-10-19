using System.Globalization;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Exceptions.IdentityErrors;
using AppTech.DAL.Handlers.Implementations;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Abstractions;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppTech.DAL
{
    public static class DALDependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddIdentity();
            services.AddHandlers();
            services.AddRepositories();
            services.AddHttpContextAccessor();

            return services;
        }

        private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            //var connectionString = configuration.GetConnectionString("DefaultConnection");
            //services.AddDbContext<AppDbContext>(options =>
            //    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

            //services.AddDbContext<AppDbContext>(options =>
            //    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)),
            //    mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()));

            var connectionString = Environment.GetEnvironmentVariable("CloudConnection")
                                         ?? configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });
        }

        private static void AddIdentity(this IServiceCollection services)
        {
            services.AddScoped<IdentityErrorDescriber, CustomIdentityErrorDescriber>();

            services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyTranslationRepository, CompanyTranslationRepository>();

            services.AddScoped<ICertificationRepository, CertificationRepository>();
            services.AddScoped<ICertificationTranslationRepository, CertificationTranslationRepository>();

            services.AddScoped<ICertificationUserRepository, CertificationUserRepository>();
            services.AddScoped<ICertificationPromotionRepository, CertificationPromotionRepository>();

            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IPromotionUserRepository, PromotionUserRepository>();

            services.AddScoped<IPublicIpAddressRepository, PublicIpAddressRepository>();

            services.AddScoped<IDropletRepository, DropletRepository>();

            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IExamResultRepository, ExamResultRepository>();
            services.AddScoped<IExamPackageRepository, ExamPackageRepository>();
            services.AddScoped<IExamTranslationRepository, ExamTranslationRepository>();

            services.AddScoped<INewsRepository, NewsRepository>();
            services.AddScoped<INewsTranslationRepository, NewsTranslationRepository>();

            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<ISettingTranslationRepository, SettingTranslationRepository>();

            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IQuestionImageRepository, QuestionImageRepository>();

            services.AddScoped<IDropZoneDragVariantRepository, DropZoneDragVariantRepository>();
            services.AddScoped<IDragDropQuestionRepository, DragDropQuestionRepository>();
            services.AddScoped<IDragVariantRepository, DragVariantRepository>();
            services.AddScoped<IDropZoneRepository, DropZoneRepository>();

            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ISubscriptionUserRepository, SubscriptionUserRepository>();
            services.AddScoped<ITransactionTranslationRepository, TransactionTranslationRepository>();

            services.AddScoped<IFAQRepository, FAQRepository>();
            services.AddScoped<IAvatarRepository, AvatarRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IGiftCardRepository, GiftCardRepository>();
            services.AddScoped<IContactUsRepository, ContactUsRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
        }

        public static void AddHandlers(this IServiceCollection services)
        {
            services.AddScoped<ICompanyHandler, CompanyHandler>();
            services.AddScoped<ICompanyTranslationHandler, CompanyTranslationHandler>();

            services.AddScoped<ICertificationHandler, CertificationHandler>();
            services.AddScoped<ICertificationTranslationHandler, CertificationTranslationHandler>();

            services.AddScoped<IExamHandler, ExamHandler>();
            services.AddScoped<IExamTranslationHandler, ExamTranslationHandler>();
            services.AddScoped<IExamPackageHandler, ExamPackageHandler>();

            services.AddScoped<INewsHandler, NewsHandler>();
            services.AddScoped<INewsTranslationHandler, NewsTranslationHandler>();

            services.AddScoped<ISettingHandler, SettingHandler>();
            services.AddScoped<ISettingTranslationHandler, SettingTranslationHandler>();

            services.AddScoped<ITransactionHandler, TransactionHandler>();
            services.AddScoped<ITransactionTranslationHandler, TransactionTranslationHandler>();

            services.AddScoped<IFAQHandler, FAQHandler>();
            services.AddScoped<IAvatarHandler, AvatarHandler>();
            services.AddScoped<IPartnerHandler, PartnerHandler>();
            services.AddScoped<ICommentHandler, CommentHandler>();
            services.AddScoped<IVariantHandler, VariantHandler>();
            services.AddScoped<IQuestionHandler, QuestionHandler>();
            services.AddScoped<IGiftCardHandler, GiftCardHandler>();
            services.AddScoped<IContactUsHandler, ContactUsHandler>();
            services.AddScoped<IPromotionHandler, PromotionHandler>();
        }
    }
}
