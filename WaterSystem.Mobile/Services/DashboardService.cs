using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace WaterSystem.Mobile.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;

        private const string ApiUrl = "https://10.0.2.2:7276/";

        public DashboardService()
        {
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, certificate, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
#else
        _httpClient = new HttpClient();
#endif

            _httpClient.BaseAddress = new Uri(ApiUrl);
        }

        public async Task<ConsumptionResponse?> GetCurrentConsumptionAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    "api/DashboardApi/current-consumption");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ConsumptionResponse>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<InvoiceResponse?> GetLastInvoiceAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    "api/DashboardApi/last-invoice");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class InvoiceResponse
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public bool Payment { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class ConsumptionResponse
    {
        public int MeterId { get; set; }
        public bool IsActive { get; set; }
        public decimal Consumption { get; set; }
        public string Unit { get; set; } = "m³";
        public DateTime? CurrentReadingDate { get; set; }
        public DateTime? PreviousReadingDate { get; set; }
        public string? Message { get; set; }
    }
}
