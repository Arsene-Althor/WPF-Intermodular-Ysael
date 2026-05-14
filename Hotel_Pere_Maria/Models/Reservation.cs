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
