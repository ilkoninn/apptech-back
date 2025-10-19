namespace AppTech.Business.DTOs.PromotionDTOs
{
    public class CreatePromotionDTO
    {
        public string Code { get; set; }
        public DateTime EndedOn { get; set; }
        public int Percentage { get; set; }
        public List<int> CertificationIds { get; set; }
    }

}
