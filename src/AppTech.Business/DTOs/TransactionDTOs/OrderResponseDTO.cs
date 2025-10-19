namespace AppTech.Business.DTOs.TransactionDTOs
{
    public class OrderResponseDTO
    {
        public string OrderTitle { get; set; }
        public string OrderDescription { get; set; }
        public string OrderImageUrl { get; set; }
        public string OrderStatus { get; set; }
        public long? OrderId { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
