using AppTech.Core.Enums;

namespace AppTech.Business.DTOs.FAQDTOs
{
    public class CreateFAQDTO
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public ELanguage Language { get; set; }
    }
}
