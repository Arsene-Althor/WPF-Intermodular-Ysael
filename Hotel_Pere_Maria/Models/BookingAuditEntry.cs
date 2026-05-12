using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>Documento de booking_audit_log devuelto por GET /reservation/:id/audit</summary>
    public class BookingAuditEntry
    {
        [JsonPropertyName("booking_id")]
        public string BookingId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("actor_id")]
        public string ActorId { get; set; }

        [JsonPropertyName("actor_type")]
        public string ActorType { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }

        [JsonPropertyName("resumen_cambios")]
        public List<string> ResumenCambios { get; set; }
    }
}
