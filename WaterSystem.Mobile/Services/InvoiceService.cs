using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace WaterSystem.Mobile.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _httpClient;

        private const string ApiUrl = "https://10.0.2.2:7276/";

        public InvoiceService()
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

        public async Task<List<InvoiceResponse>> GetInvoicesAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new List<InvoiceResponse>();
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/InvoiceApi");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<InvoiceResponse>();
                }

                return await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>()
                    ?? new List<InvoiceResponse>();
            }
            catch (Exception)
            {
                return new List<InvoiceResponse>();
            }
        }


        public async Task<InvoiceDetailsResponse?> GetInvoiceDetailsAsync(int id)
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    System.Diagnostics.Debug.WriteLine("ERRO: Token não encontrado.");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"api/InvoiceApi/{id}");

                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(
                    $"INVOICE DETAILS - Status: {(int)response.StatusCode}");

                System.Diagnostics.Debug.WriteLine(
                    $"INVOICE DETAILS - Response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return System.Text.Json.JsonSerializer.Deserialize<InvoiceDetailsResponse>(
                    content,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"INVOICE DETAILS - Exception: {ex}");

                return null;
            }
        }


        public class InvoiceDetailsResponse
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
            public bool Payment { get; set; }
            public string Status { get; set; } = string.Empty;
            public int MeterId { get; set; }
            public int ConsumptionId { get; set; }
            public DateTime ConsumptionDate { get; set; }
            public decimal ConsumptionVolume { get; set; }
        }
    }
}

