namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IBankService
    {
        Task<HttpResponseMessage> CreateOrderAsync(
            string companySlug, string certSlug, string checkToken, decimal amount,
            string description);

        Task<HttpResponseMessage> IncreaseBalanceOrderAsync(
            string checkToken, decimal amount,
            string description);

        Task<string> GetOrderInformationAsync(string orderId);

        Task<HttpResponseMessage> RefundPaymentAsync(string orderId, decimal amount);
    }
}
