using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Hotel_Pere_Maria.Models
{
    public class Room
    {
        [JsonPropertyName("room_id")]
        public string RoomId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("price_per_night")]
        public double PricePerNight { get; set; }

        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        [JsonPropertyName("max_occupancy")]
        public int MaxOccupancy { get; set; }

        /// <summary>Empleado: la habitación puede ofrecerse al público. Si false, la app cliente no la lista.</summary>
        [JsonPropertyName("is_operational")]
        public bool IsOperational { get; set; } = true;

        /// <summary>Calculado en API: hay reserva activa (check_in ≤ ahora &lt; check_out) sin cancelar.</summary>
        [JsonPropertyName("is_occupied_now")]
        public bool IsOccupiedNow { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new List<string>();

        [JsonPropertyName("extra_services")]
        public List<string> ExtraServices { get; set; } = new List<string>();

        [JsonPropertyName("offer_active")]
        public bool OfferActive { get; set; }

        [JsonPropertyName("offer_percent")]
        public double OfferPercent { get; set; }

        [JsonPropertyName("effective_price_per_night")]
        public double? EffectivePricePerNight { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailableLegacy { get; set; }

        /// <summary>Libre en este momento para nueva estancia (en servicio y sin huésped en curso).</summary>
        public bool EstaLibreAhora => IsOperational && !IsOccupiedNow;

        [JsonIgnore]
        public string EstadoServicioTexto => IsOperational ? "En servicio" : "Fuera de servicio";

        [JsonIgnore]
        public string OcupacionTexto => IsOccupiedNow ? "Reservada ahora" : "Libre ahora";
    }
}
