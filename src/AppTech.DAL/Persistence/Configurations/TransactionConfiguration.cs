using AppTech.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTech.DAL.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasOne(t => t.Certification)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CertificationId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
