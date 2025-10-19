using System.Text.Json.Serialization;
using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.PromotionDTOs
{
    public class UpdatePromotionDTO : BaseEntityDTO
    {
        public string Code { get; set; }
        public DateTime EndedOn { get; set; }
        public int Percentage { get; set; }
        public List<int> CertificationIds { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }
    }
}
