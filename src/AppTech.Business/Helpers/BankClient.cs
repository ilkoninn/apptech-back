using System.Net.Http.Headers;
using System.Text;

namespace AppTech.Business.Helpers
{
    public class BankClient
    {
        private readonly HttpClient _httpClient;

        public BankClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://e-commerce.kapitalbank.az/api/");
            var basicAuthenticationValue =
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes("TerminalSys/E2550002:hello"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthenticationValue);
        }

        public async Task<string> GetAsync(string request) => await _httpClient.GetStringAsync(request);
        public async Task<HttpResponseMessage> PostAsync(string request, StringContent content) => await _httpClient.PostAsync(request, content);
    }
}
