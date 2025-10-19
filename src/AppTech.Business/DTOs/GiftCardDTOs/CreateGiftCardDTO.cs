
using AppTech.Business.DTOs.Commons;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.GiftCardDTOs
{
    public class CreateGiftCardDTO : IAuditedEntityDTO
    {
        public EGiftCardType Type { get; set; }
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
    }
}
