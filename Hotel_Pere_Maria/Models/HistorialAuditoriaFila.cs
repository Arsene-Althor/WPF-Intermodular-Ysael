using System;
using System.Collections.Generic;

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
        /// <summary>Campos modificados con valores antes y después.</summary>
        public List<AuditCambioFila> Cambios { get; set; } = new();
        public bool TieneDetalleCambios => Cambios != null && Cambios.Count > 0;
    }
}
