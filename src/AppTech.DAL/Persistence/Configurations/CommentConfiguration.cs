using AppTech.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTech.DAL.Persistence.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasOne(c => c.Certification)
                 .WithMany(c => c.Comments)
                 .HasForeignKey(c => c.CertificationSlug)
                 .HasPrincipalKey(c => c.Slug);
        }
    }
}
