namespace Hotel_Pere_Maria.Models
{
    /// <summary>Fila Antes / Después para la UI de auditoría.</summary>
    public class AuditCambioFila
    {
        public string Etiqueta { get; set; } = "";
        public string Antes { get; set; } = "—";
        public string Despues { get; set; } = "—";
    }
}
