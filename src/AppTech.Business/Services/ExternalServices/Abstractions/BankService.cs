using System.Text;
using System.Text.Json;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Mysqlx.Crud;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class BankService : IBankService
    {
        private readonly BankClient _client;
        private readonly IConfiguration _config;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionHandler _transactionHandler;
        private readonly IHttpContextAccessor _http;

        public BankService(BankClient client, IConfiguration config,
            ITransactionRepository transactionRepository,
            ITransactionHandler transactionHandler, IHttpContextAccessor http)
        {
            _client = client;
            _config = config;
            _transactionRepository = transactionRepository;
            _transactionHandler = transactionHandler;
            _http = http;
        }

        public async Task<HttpResponseMessage> IncreaseBalanceOrderAsync(
            string checkToken, decimal amount,
            string description)
        {
            var frontUrl = _config["FrontEndUrl"];
            var language = new LanguageCatcher(_http).GetLanguage();

            var order = new
            {
                order = new
                {
                    typeRid = "Order_SMS",
                    amount = amount.ToString("F2"),
                    currency = "AZN",
                    language = language,
                    description = description,
                    hppRedirectUrl = $"{frontUrl}/dashboard/increase-balance",
                    hppCofCapturePurposes = new[] { "Cit" }
                }
            };

            var json = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("order/", content);

            return response;
        }

        public async Task<HttpResponseMessage> CreateOrderAsync(
            string companySlug, string certSlug, string checkToken, decimal amount,
            string description)
        {
            var frontUrl = _config["FrontEndUrl"];
            var lang = new LanguageCatcher(_http).GetLanguage();

            var order = new
            {
                order = new
                {
                    typeRid = "Order_SMS",
                    amount = amount.ToString("F2"),
                    currency = "AZN",
                    language = lang,
                    description = description,
                    hppRedirectUrl = $"{frontUrl}/companies/{companySlug}/{certSlug}",
                    hppCofCapturePurposes = new[] { "Cit" }
                }
            };

            var json = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("order/", content);

            return response;
        }

        public async Task<string> GetOrderInformationAsync(string orderId)
        {
            var response = await _client.GetAsync($"https://e-commerce.kapitalbank.az/api/order/{orderId}");
            //https://e-commerce.kapitalbank.az/api
            return response;
        }

        public async Task<HttpResponseMessage> RefundPaymentAsync(string orderId, decimal amount)
        {
            var tran = new
            {
                tran = new
                {
                    phase = "Single",
                    amount = amount.ToString("F2"),
                    type = "Refund"
                }
            };

            var json = JsonSerializer.Serialize(tran, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"order/{orderId}/exec-tran", content);

            return response;
        }
    }
}
