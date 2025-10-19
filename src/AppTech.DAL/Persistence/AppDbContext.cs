using System.Reflection;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using AppTech.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AppTech.DAL.Persistence
{
    public class AppDbContext : IdentityDbContext<User>
    {
        private readonly IClaimService _claimService;

        public AppDbContext(DbContextOptions<AppDbContext> options, IClaimService claimService) : base(options)
        {
            _claimService = claimService;
        }

        public DbSet<PromotionUser> PromotionUsers { get; set; }
        public DbSet<CertificationUser> CertificationUsers { get; set; }

        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<CertificationPromotion> CertificationPromotions { get; set; }
        public DbSet<CertificationTranslation> CertificationTranslations { get; set; }

        public DbSet<Droplet> Droplets { get; set; }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<ExamPackage> ExamPackages { get; set; }
        public DbSet<ExamTranslation> ExamTranslations { get; set; }


        public DbSet<Variant> Variants { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionImage> QuestionImages { get; set; }

        public DbSet<DropZone> DropZones { get; set; }
        public DbSet<DragVariant> DragVariants { get; set; }
        public DbSet<DragDropQuestion> DragDropQuestions { get; set; }
        public DbSet<DropZoneDragVariant> DropZoneDragVariants { get; set; }

        public DbSet<News> News { get; set; }
        public DbSet<NewsTranslation> NewsTranslations { get; set; }

        public DbSet<Setting> Settings { get; set; }
        public DbSet<SettingTranslation> SettingsTranslations { get; set; }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyTranslation> CompanyTranslations { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionUser> SubscriptionsUsers { get; set; }
        public DbSet<TransactionTranslation> TransactionTranslations { get; set; }

        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<GiftCard> GiftCards { get; set; }

        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Certification>()
       .Property(c => c.DiscountPrice)
       .HasPrecision(18, 2);

            modelBuilder.Entity<Certification>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Exam>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ExamPackage>()
                .Property(ep => ep.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<GiftCard>()
                .Property(gc => gc.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("nvarchar(max)");
            });
            base.OnModelCreating(modelBuilder);
        }

        public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _claimService.GetUserId() ?? "ByServer";
                        entry.Entity.CreatedOn = DateTime.UtcNow;

                        entry.Entity.UpdatedBy = _claimService.GetUserId() ?? "ByServer";
                        entry.Entity.UpdatedOn = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedBy = _claimService.GetUserId() ?? "ByServer";
                        entry.Entity.UpdatedOn = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
