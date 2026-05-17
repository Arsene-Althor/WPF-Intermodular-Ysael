using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>Campo modificado en un evento de auditoría (API: detalle_cambios).</summary>
    public class AuditChangeDetail
    {
        [JsonPropertyName("campo")]
        public string Campo { get; set; } = "";

        [JsonPropertyName("etiqueta")]
        public string Etiqueta { get; set; } = "";

        [JsonPropertyName("antes")]
        public JsonElement? Antes { get; set; }

        [JsonPropertyName("despues")]
        public JsonElement? Despues { get; set; }
    }
}
