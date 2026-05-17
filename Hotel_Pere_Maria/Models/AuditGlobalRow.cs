using System;
using System.Collections.Generic;

namespace Hotel_Pere_Maria.Models
{
    public sealed class AuditGlobalRow
    {
        public DateTime? Fecha { get; set; }
        public string FechaTxt => Fecha.HasValue ? Fecha.Value.ToString("dd/MM/yyyy HH:mm") : "—";
        public string ReservaId { get; set; } = "";
        public string Accion { get; set; } = "";
        public string Actor { get; set; } = "";
        public string Resumen { get; set; } = "";
        public string Antes { get; set; } = "—";
        public string Despues { get; set; } = "—";
        public List<AuditCambioFila> Cambios { get; set; } = new();
        public bool TieneDetalleCambios => Cambios != null && Cambios.Count > 0;
    }
}
