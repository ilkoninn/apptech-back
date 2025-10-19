namespace AppTech.Business.DTOs.TransactionDTOs
{
    public class CreateTransactionDTO
    {
        public bool type { get; set; }
        public int? subId { get; set; }
        public string userId { get; set; }
        public int? certificationId { get; set; }
        public string? promotionCode { get; set; }
        public int? installmentNumber { get; set; }
    }
}
