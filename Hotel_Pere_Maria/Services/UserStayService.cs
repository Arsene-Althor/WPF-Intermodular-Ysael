using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public static class UserStayService
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

        private static JsonSerializerOptions JsonOpts() => new()
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
        };

        public static async Task<(bool ok, string? err, ClientStayHistoryResponse? data)> GetHistoryAsync(
            string userId,
            int page = 1,
            int limit = 20,
            string? status = "completed",
            DateTime? from = null,
            DateTime? to = null,
            int? year = null,
            string? roomType = null)
        {
            try
            {
                ConfigurarCabeceras();
                var qs = new List<string>
                {
                    $"page={page}",
                    $"limit={limit}",
                };
                if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={Uri.EscapeDataString(status)}");
                if (year.HasValue) qs.Add($"year={year.Value}");
                if (!string.IsNullOrWhiteSpace(roomType)) qs.Add($"room_type={Uri.EscapeDataString(roomType)}");
                if (from.HasValue) qs.Add($"from={Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd"))}");
                if (to.HasValue) qs.Add($"to={Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd"))}");

                string url = $"{ApiService.BaseUrl}users/{Uri.EscapeDataString(userId)}/history?{string.Join("&", qs)}";
                var response = await ApiService._httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) return (false, body, null);
                var data = JsonSerializer.Deserialize<ClientStayHistoryResponse>(body, JsonOpts());
                return (true, null, data);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool ok, string? err, ClientStayStatsDto? stats)> GetStatsAsync(
            string userId,
            int? year = null,
            string? roomType = null,
            string? status = "all")
        {
            try
            {
                ConfigurarCabeceras();
                var qs = new List<string>();
                if (year.HasValue) qs.Add($"year={year.Value}");
                if (!string.IsNullOrWhiteSpace(roomType)) qs.Add($"room_type={Uri.EscapeDataString(roomType)}");
                if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={Uri.EscapeDataString(status)}");
                string q = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
                string url = $"{ApiService.BaseUrl}users/{Uri.EscapeDataString(userId)}/stats{q}";
                var response = await ApiService._httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) return (false, body, null);
                var stats = JsonSerializer.Deserialize<ClientStayStatsDto>(body, JsonOpts());
                return (true, null, stats);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static string BuildHistoryCsv(IEnumerable<ClientStayHistoryItem> items)
        {
            var ci = CultureInfo.InvariantCulture;
            var lines = new List<string>
            {
                "reservation_id;status;check_in;check_out;nights;total_paid;room_id;room_type;rating;comment",
            };
            foreach (var i in items)
            {
                lines.Add(string.Join(";",
                    i.reservation_id,
                    i.status,
                    i.check_in?.ToString("yyyy-MM-dd", ci) ?? "",
                    i.check_out?.ToString("yyyy-MM-dd", ci) ?? "",
                    i.nights.ToString(ci),
                    i.total_paid.ToString("F2", ci),
                    i.room?.room_id ?? "",
                    i.room?.type ?? "",
                    i.rating?.rating.ToString(ci) ?? "",
                    (i.rating?.comment ?? "").Replace(";", ",")));
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
