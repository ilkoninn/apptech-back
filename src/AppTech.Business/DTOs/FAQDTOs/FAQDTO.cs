using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.FAQDTOs
{
    public class FAQDTO : BaseEntityDTO
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Language { get; set; }
    }
}
