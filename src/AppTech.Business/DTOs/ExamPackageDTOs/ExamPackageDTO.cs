using AppTech.Business.DTOs.Commons;
using AppTech.Business.DTOs.ExamDTOs;

namespace AppTech.Business.DTOs.ExamPackageDTOs
{
    public class ExamPackageDTO : BaseEntityDTO
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public IEnumerable<string> CertificationTitles { get; set; }
        public IEnumerable<ExamDTO> ExamDTOs { get; set; }
    }
}
