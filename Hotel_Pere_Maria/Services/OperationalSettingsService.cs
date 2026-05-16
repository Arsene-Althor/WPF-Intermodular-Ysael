using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
  public class OperationalSettingsDto
  {
    public bool booking_audit_enabled { get; set; } = true;
    public int client_flex_request_window_hours { get; set; } = 12;
  }

  public static class OperationalSettingsService
  {
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
      PropertyNameCaseInsensitive = true,
    };

    public static async Task<(bool ok, string? err, OperationalSettingsDto? dto)> GetAsync()
    {
      try
      {
        var response = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "settings/operational");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, body, null);
        var dto = JsonSerializer.Deserialize<OperationalSettingsDto>(body, JsonOpts);
        return (true, null, dto);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    public static async Task<(bool ok, string? err, OperationalSettingsDto? dto)> PutAsync(OperationalSettingsDto data)
    {
      try
      {
        var json = JsonSerializer.Serialize(data);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await ApiService._httpClient.PutAsync(ApiService.BaseUrl + "settings/operational", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, body, null);
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("settings", out var settingsEl))
          return (false, body, null);
        var dto = JsonSerializer.Deserialize<OperationalSettingsDto>(settingsEl.GetRawText(), JsonOpts);
        return (true, null, dto);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }
  }
}
