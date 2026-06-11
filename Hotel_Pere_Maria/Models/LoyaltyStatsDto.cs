using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    public class LoyaltyStatsDto
    {
        [JsonPropertyName("user_id")]
        public string? user_id { get; set; }

        [JsonPropertyName("loyalty_tier")]
        public string? loyalty_tier { get; set; }

        [JsonPropertyName("total_nights")]
        public int total_nights { get; set; }

        [JsonPropertyName("total_spent")]
        public double total_spent { get; set; }

        [JsonPropertyName("completed_stays_count")]
        public int completed_stays_count { get; set; }
    }
}
