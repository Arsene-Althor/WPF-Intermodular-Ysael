using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Models
{
    public class Room
    {
        // ID de la habitación (ej: HAB-101)
        [JsonPropertyName("room_id")]
        public string RoomId { get; set; }

        // Tipo de habitación (Individual, Doble, Suite…)
        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Descripción de la habitación
        [JsonPropertyName("description")]
        public string Description { get; set; }

        // URL o nombre de imagen
        public string Image { get; set; }

        // Precio por noche
        [JsonPropertyName("price_per_night")]
        public double PricePerNight { get; set; }

        // Valoración (0 - 5)
        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        // Máximo de personas
        [JsonPropertyName("max_occupancy")]
        public int MaxOccupancy { get; set; }

        // Disponible o no (opcional, la API suele calcularlo)
        public bool IsAvailable { get; set; }
    }
}
