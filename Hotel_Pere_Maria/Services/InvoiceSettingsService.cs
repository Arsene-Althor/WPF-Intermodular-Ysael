using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public static class InvoiceSettingsService
    {
        private static void ConfigurarCabeceras()
        {
            ApiService._httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(Session.Token))
            {
                ApiService._httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Session.Token);
            }
        }

        /// <summary>GET /settings/invoice — admin/empleado.</summary>
        public static async Task<(bool exito, string mensaje, InvoiceSettingsDto? dto)> GetAsync()
        {
            try
            {
                ConfigurarCabeceras();
                var response = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "settings/invoice");
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var dto = JsonSerializer.Deserialize<InvoiceSettingsDto>(body, opts);
                return (true, null, dto);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>PUT /settings/invoice — admin/empleado.</summary>
        public static async Task<(bool exito, string mensaje, InvoiceSettingsDto? dto)> PutAsync(InvoiceSettingsDto data)
        {
            try
            {
                ConfigurarCabeceras();
                var json = JsonSerializer.Serialize(data);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PutAsync(ApiService.BaseUrl + "settings/invoice", content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("settings", out var settingsEl))
                    return (false, body, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var dto = JsonSerializer.Deserialize<InvoiceSettingsDto>(settingsEl.GetRawText(), opts);
                return (true, null, dto);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}
