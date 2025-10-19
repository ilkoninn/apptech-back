using AppTech.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTech.DAL.Persistence.Configurations
{
    public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
    {
        public void Configure(EntityTypeBuilder<Certification> builder)
        {
            builder.HasMany(c => c.CertificationTranslations)
                   .WithOne()
                   .HasForeignKey(ct => ct.CertificationId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure Slug as an alternate key
            builder.HasAlternateKey(c => c.Slug)
                .HasName("AlternateKey_Slug");
        }
    }
}
