using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>GET/PUT /settings/invoice — valores efectivos (BD + fallback .env).</summary>
    public class InvoiceSettingsDto
    {
        [JsonPropertyName("hotel_commercial_name")]
        public string hotel_commercial_name { get; set; } = "";

        [JsonPropertyName("hotel_cif")]
        public string hotel_cif { get; set; } = "";

        [JsonPropertyName("hotel_address")]
        public string hotel_address { get; set; } = "";

        [JsonPropertyName("fiscal_notes")]
        public string fiscal_notes { get; set; } = "";

        [JsonPropertyName("iva_rate")]
        public double iva_rate { get; set; }
    }
}
