using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    public class ExtraServiceDto
    {
        [JsonPropertyName("service_id")]
        public string ServiceId { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;
    }
}
