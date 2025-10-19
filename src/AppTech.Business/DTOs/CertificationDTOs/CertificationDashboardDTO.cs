using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class CertificationDashboardDTO : BaseEntityDTO
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int QuestionCount { get; set; }
        public string ImageUrl { get; set; }
        public int? LastVersion { get; set; }
    }
}
