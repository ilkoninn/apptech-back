using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.CertificationDTOs
{
    public class GetByIdCertificationDTO : BaseEntityDTO { }

    public class GetBySlugCertificationDTO
    {
        public string slug { get; set; }
    }

    public class GetByIdCertificationTranslationDTO : BaseEntityDTO { }
}
