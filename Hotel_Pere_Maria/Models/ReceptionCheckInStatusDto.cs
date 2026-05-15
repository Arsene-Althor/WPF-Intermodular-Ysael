using System;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>Respuesta GET /reservation/{id}/check-in-status</summary>
    public class ReceptionCheckInStatusDto
    {
        [JsonPropertyName("reservation_id")]
        public string ReservationId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("can_register")]
        public bool CanRegister { get; set; }

        [JsonPropertyName("requires_late_confirmation")]
        public bool RequiresLateConfirmation { get; set; }

        [JsonPropertyName("late_fee")]
        public double LateFee { get; set; }

        [JsonPropertyName("reception_check_in_at")]
        public DateTime? ReceptionCheckInAt { get; set; }

        [JsonPropertyName("reception_check_in_late")]
        public bool ReceptionCheckInLate { get; set; }

        [JsonPropertyName("reception_check_in_late_fee")]
        public double ReceptionCheckInLateFee { get; set; }

        [JsonPropertyName("guest_name")]
        public string GuestName { get; set; }

        [JsonPropertyName("guest_dni")]
        public string GuestDni { get; set; }

        [JsonPropertyName("guest_email")]
        public string GuestEmail { get; set; }

        [JsonPropertyName("room_id")]
        public string RoomId { get; set; }

        [JsonPropertyName("check_in")]
        public DateTime CheckIn { get; set; }

        [JsonPropertyName("check_out")]
        public DateTime CheckOut { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("window_start")]
        public DateTime? WindowStart { get; set; }

        [JsonPropertyName("window_end")]
        public DateTime? WindowEnd { get; set; }
    }
}
