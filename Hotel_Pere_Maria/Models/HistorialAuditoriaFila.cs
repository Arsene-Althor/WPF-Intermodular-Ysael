using System;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>Fila mostrada en la línea de tiempo de auditoría (detalle de reserva).</summary>
    public class HistorialAuditoriaFila
    {
        public string ActionKey { get; set; }
        public string Accion { get; set; }
        public string ActorNombre { get; set; }
        public string ActorId { get; set; }
        public DateTime? Fecha { get; set; }
        public string FechaFormateada =>
            Fecha.HasValue ? Fecha.Value.ToString("dd/MM/yyyy HH:mm") : "—";
        public string ResumenTexto { get; set; }
    }
}
