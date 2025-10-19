using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.TransactionDTOs
{
    public class OrderDTO : BaseEntityDTO
    {
        public DateTime PaymentOn { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncreased { get; set; }
        public string Description { get; set; }
    }
}
