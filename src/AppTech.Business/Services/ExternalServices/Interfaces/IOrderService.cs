using AppTech.Business.DTOs.TransactionDTOs;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IOrderService
    {
        Task<IQueryable<OrderDTO>> GetAllAsync(GetAllOrderDTO dto);
        Task<TransactionDTO> PurchaseAsync(CreateTransactionDTO dto);
        Task<TransactionDTO> IncreaseBalanceAsync(IncreaseBalanceDTO dto);
        Task<OrderResponseDTO> PaymentHandlerAsync(string checkToken, bool increaseOrBuy);
        Task<OrderResponseDTO> DecreaseBalanceForBuyExamAsync(DecreaseBalanceForBuyExamDTO dto);
        Task<OrderResponseDTO> DecreaseBalanceForBuyExamPackageAsync(DecreaseBalanceForForBuyExamPackageDTO dto);
    }
}
