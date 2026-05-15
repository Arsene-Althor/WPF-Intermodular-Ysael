using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    public class GlobalAuditsResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<BookingAuditEntry> Items { get; set; } = new();
    }
}
