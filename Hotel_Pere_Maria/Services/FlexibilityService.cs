using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
  public static class FlexibilityService
  {
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
      PropertyNameCaseInsensitive = true,
    };

    private static void ConfigurarCabeceras()
    {
      ApiService._httpClient.DefaultRequestHeaders.Authorization = null;
      if (!string.IsNullOrEmpty(Session.Token))
      {
        ApiService._httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", Session.Token);
      }
    }

    public static async Task<(bool ok, string? err, FlexibilityStatusDto? data)> GetStatusAsync(string reservationId)
    {
      try
      {
        ConfigurarCabeceras();
        var url = ApiService.BaseUrl + $"reservation/{Uri.EscapeDataString(reservationId)}/flexibility";
        var response = await ApiService._httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, ParseApiError(body), null);
        var data = JsonSerializer.Deserialize<FlexibilityStatusDto>(body, JsonOpts);
        return (true, null, data);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    public static async Task<(bool ok, string? err, PendingFlexibilityListDto? data)> GetPendingAsync(DateTime? day = null)
    {
      try
      {
        ConfigurarCabeceras();
        var d = (day ?? DateTime.Today).ToString("yyyy-MM-dd");
        var url = ApiService.BaseUrl + $"reservation/flexibility/pending?day={d}";
        var response = await ApiService._httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, ParseApiError(body), null);
        var data = JsonSerializer.Deserialize<PendingFlexibilityListDto>(body, JsonOpts);
        return (true, null, data);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    public static async Task<(bool ok, string? err, double? newPrice)> ReviewEarlyAsync(
      string reservationId, string decision, string? reviewNote = null)
      => await ReviewAsync(reservationId, "early-checkin", decision, reviewNote);

    public static async Task<(bool ok, string? err, double? newPrice)> ReviewLateAsync(
      string reservationId, string decision, string? reviewNote = null)
      => await ReviewAsync(reservationId, "late-checkout", decision, reviewNote);

    private static async Task<(bool ok, string? err, double? newPrice)> ReviewAsync(
      string reservationId, string pathKind, string decision, string? reviewNote)
    {
      try
      {
        ConfigurarCabeceras();
        var payload = JsonSerializer.Serialize(new { decision, review_note = reviewNote });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var url = ApiService.BaseUrl +
                  $"reservation/{Uri.EscapeDataString(reservationId)}/flexibility/{pathKind}/review";
        var response = await ApiService._httpClient.PatchAsync(url, content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, ParseApiError(body), null);
        using var doc = JsonDocument.Parse(body);
        double? price = null;
        if (doc.RootElement.TryGetProperty("price", out var p) && p.TryGetDouble(out var pd))
          price = pd;
        return (true, null, price);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    public static async Task<(bool ok, string? err, FlexibilitySettingsDto? dto)> GetSettingsAsync()
    {
      try
      {
        ConfigurarCabeceras();
        var response = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "settings/flexibility");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, body, null);
        var dto = JsonSerializer.Deserialize<FlexibilitySettingsDto>(body, JsonOpts);
        return (true, null, dto);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    public static async Task<(bool ok, string? err, FlexibilitySettingsDto? dto)> PutSettingsAsync(FlexibilitySettingsDto data)
    {
      try
      {
        ConfigurarCabeceras();
        var json = JsonSerializer.Serialize(data);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await ApiService._httpClient.PutAsync(ApiService.BaseUrl + "settings/flexibility", content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) return (false, body, null);
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("settings", out var settingsEl))
          return (false, body, null);
        var dto = JsonSerializer.Deserialize<FlexibilitySettingsDto>(settingsEl.GetRawText(), JsonOpts);
        return (true, null, dto);
      }
      catch (Exception ex)
      {
        return (false, ex.Message, null);
      }
    }

    private static string ParseApiError(string raw)
    {
      if (string.IsNullOrWhiteSpace(raw)) return "Error en la API";
      try
      {
        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.TryGetProperty("error", out var err))
          return err.GetString() ?? raw;
      }
      catch
      {
        // ignore
      }
      return raw.Length > 280 ? raw[..280] + "…" : raw;
    }
  }

  public class PendingFlexibilityListDto
  {
    public int count { get; set; }
    public int pending_count { get; set; }
    public string? day { get; set; }
    public PendingFlexibilityItemDto[]? items { get; set; }
  }
}
