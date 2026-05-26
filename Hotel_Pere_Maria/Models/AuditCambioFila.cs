namespace Hotel_Pere_Maria.Models
{
    /// <summary>Fila Antes / Después para la UI de auditoría.</summary>
    public class AuditCambioFila
    {
        public string Etiqueta { get; set; } = "";
        public string Antes { get; set; } = "—";
        public string Despues { get; set; } = "—";
        /// <summary>Solo valor nuevo (antes vacío).</summary>
        public bool EsSoloAlta { get; set; }
        /// <summary>Solo se eliminó valor (después vacío).</summary>
        public bool EsSoloBaja { get; set; }
    }
}
