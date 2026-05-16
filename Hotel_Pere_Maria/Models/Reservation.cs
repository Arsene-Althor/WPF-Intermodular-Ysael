using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Models
{
    //Modelo para reservas
    public class Reservation
    {
        public string reservation_id {  get; set; }
        public string room_id { get; set; }
        public string user_id { get; set; }
        private DateTime _check_in { get; set; }
        private DateTime _check_out { get; set; }
        public double price { get; set; }
        public string createdBy { get; set; }

        [JsonPropertyName("room_image")]
        public string RoomImage { get; set; }

        //La fecha de cancelación puede ser nula 
        private DateTime? _cancelation_date { get; set; }

        /// <summary>Nº factura tras checkout (API).</summary>
        public string? invoice_number { get; set; }

        private DateTime? _checkout_completed_at;

        /// <summary>Fecha/hora de checkout (emisión factura), UTC desde API.</summary>
        public DateTime? checkout_completed_at
        {
            get => _checkout_completed_at?.ToLocalTime();
            set => _checkout_completed_at = value;
        }

        public bool HasInvoice => !string.IsNullOrWhiteSpace(invoice_number);

        [JsonPropertyName("guest_name")]
        public string? GuestName { get; set; }

        [JsonPropertyName("guest_dni")]
        public string? GuestDni { get; set; }

        private DateTime? _reception_check_in_at;

        [JsonPropertyName("reception_check_in_at")]
        public DateTime? reception_check_in_at
        {
            get => _reception_check_in_at?.ToLocalTime();
            set => _reception_check_in_at = value;
        }

        [JsonPropertyName("reception_check_in_late")]
        public bool reception_check_in_late { get; set; }

        [JsonPropertyName("reception_check_in_late_fee")]
        public double reception_check_in_late_fee { get; set; }

        public bool HasReceptionCheckIn => reception_check_in_at.HasValue;

        [JsonPropertyName("early_checkin_requested")]
        public FlexibilityRequestDto? EarlyCheckinRequested { get; set; }

        [JsonPropertyName("late_checkout_requested")]
        public FlexibilityRequestDto? LateCheckoutRequested { get; set; }

        /// <summary>Entrada efectiva (P19 aprobado o estándar).</summary>
        public DateTime EffectiveCheckIn =>
            EarlyCheckinRequested?.status == "approved" && EarlyCheckinRequested.requested_time.HasValue
                ? EarlyCheckinRequested.requested_time.Value.ToLocalTime()
                : check_in;

        /// <summary>Salida efectiva en habitación (no aplica modo instalaciones).</summary>
        public DateTime EffectiveCheckOut =>
            LateCheckoutRequested?.status == "approved"
            && LateCheckoutRequested.late_mode != "facilities"
            && LateCheckoutRequested.requested_time.HasValue
                ? LateCheckoutRequested.requested_time.Value.ToLocalTime()
                : check_out;

        public bool IsSalidaRetrasada =>
            cancelation_date == null
            && !HasInvoice
            && DateTime.Now > EffectiveCheckOut;

        public string SalidaRetrasoTexto =>
            IsSalidaRetrasada
                ? $"⚠ Retraso {Math.Max(1, (int)Math.Ceiling((DateTime.Now - EffectiveCheckOut).TotalHours))} h"
                : "";

        public string GuestDisplayName =>
            string.IsNullOrWhiteSpace(GuestName) ? user_id ?? "—" : GuestName;

        public string GuestDisplayDni =>
            string.IsNullOrWhiteSpace(GuestDni) ? "—" : GuestDni;

        //Al obtener una fecha la convertimos a la hora del equipo local ya que la base de datos la guarda en formato universal
        public DateTime check_in
        {
            get => _check_in.ToLocalTime();
            set => _check_in = value;
        }

        public DateTime check_out
        {
            get => _check_out.ToLocalTime();
            set => _check_out = value;
        }

        public DateTime? cancelation_date
        {
            get => _cancelation_date?.ToLocalTime();
            set => _cancelation_date = value;
        }

        //Metodo para Calcular el precio de cancelación 
        //Falta calcular precio
        public double CalcularPrecioCancelacion(DateTime fechaCancelacion) {
            return 10;
        }

    }
}
