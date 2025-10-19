
using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.DTOs.TransactionDTOs;

namespace AppTech.Business.Services.Interfaces
{
    public interface IGiftCardService
    {
        Task<IQueryable<GiftCardDTO>> GetAllAsync();
        Task<GiftCardDTO> GetByIdAsync(GetByIdGiftCardDTO dto);
        Task<GiftCardDTO> AddAsync(CreateGiftCardDTO dto);
        Task<GiftCardDTO> UpdateAsync(UpdateGiftCardDTO dto);
        Task<GiftCardDTO> DeleteAsync(DeleteGiftCardDTO dto);
        Task<OrderResponseDTO> SendMoneyToRecipientEmail(GiftcardRecipientDTO dto);
    }
}
