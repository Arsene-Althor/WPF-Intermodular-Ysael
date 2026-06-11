using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public static class LoyaltyService
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

        public static async Task<(bool ok, string? err, LoyaltyStatsDto? data)> GetUserLoyaltyStatsAsync(string userId)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}loyalty/user/{Uri.EscapeDataString(userId)}";
                var response = await ApiService._httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
                };
                var dto = JsonSerializer.Deserialize<LoyaltyStatsDto>(body, options);
                return (true, null, dto);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}
