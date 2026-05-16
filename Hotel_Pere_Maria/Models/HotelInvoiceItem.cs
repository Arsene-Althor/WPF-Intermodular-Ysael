using System;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>Fila de GET /reservation/invoices/history (colección HotelInvoice).</summary>
    public class HotelInvoiceItem
    {
        public string invoice_number { get; set; } = "";
        public string reservation_id { get; set; } = "";
        public string user_id { get; set; } = "";
        public string room_id { get; set; } = "";
        public string? type { get; set; }
        public string? type_label { get; set; }
        public double amount { get; set; }
        public string? description { get; set; }

        private DateTime? _issued_at;
        public DateTime? issued_at
        {
            get => _issued_at?.ToLocalTime();
            set => _issued_at = value;
        }

        private DateTime? _check_in;
        public DateTime? check_in
        {
            get => _check_in?.ToLocalTime();
            set => _check_in = value;
        }

        private DateTime? _check_out;
        public DateTime? check_out
        {
            get => _check_out?.ToLocalTime();
            set => _check_out = value;
        }

        [JsonIgnore]
        public string DisplayTipo => string.IsNullOrWhiteSpace(type_label) ? (type ?? "—") : type_label;
    }
}
