using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppTech.Core.Entities.Commons;
using Microsoft.AspNetCore.Identity;

namespace AppTech.Core.Entities.Identity
{
    public class User : IdentityUser
    {
        public bool OnExam { get; set; }
        public bool IsResent { get; set; }
        public bool IsBanned { get; set; }
        public bool IsOnline { get; set; }
        public string? ImageUrl { get; set; }
        public string? FullName { get; set; }
        public decimal Balance { get; set; } = 0;
        public int? ConfirmationCode { get; set; }
        public DateTime? OnlineTimer { get; set; }
        public DateTime? LastActivity { get; set; }
        public DateTime? LastExamActivity { get; set; }
        public ICollection<Comment> Comment { get; set; }
        public int PublicIpAddressAccessFailed { get; set; }
        public DateTime? ConfirmationCodeSentAt { get; set; }
        public ICollection<ExamResult>? ExamResults { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<PromotionUser> PromotionUsers { get; set; }
        public ICollection<SubscriptionUser> SubscriptionUsers { get; set; }
        public ICollection<CertificationUser> CertificationUsers { get; set; }
        public ICollection<UserPublicIpAddress> UserPublicIpAddresses { get; set; }
    }

    public class ExamResult : BaseEntity, IAuditedEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        public bool? IsPassed { get; set; }
        public double? UserScore { get; set; }
        public ICollection<Droplet>? Droplets { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
