using System;
using System.Windows.Input;

namespace Hotel_Pere_Maria.Models
{
  public class SolicitudFlexFila
  {
    public string ReservationId { get; set; } = "";
    public string RoomId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string TipoClave { get; set; } = "";
    public string TipoTexto { get; set; } = "";
    public string HoraSolicitada { get; set; } = "";
    public string RangoFidelidad { get; set; } = "";
    public string Tarifa { get; set; } = "";
    public string Disponibilidad { get; set; } = "";
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public FlexibilityRequestDto? Request { get; set; }
    public bool PuedeRevisar { get; set; }
    public bool EsActiva { get; set; }
    public DateTime FechaOrden { get; set; }
    public string EstadoTexto { get; set; } = "";
    public string? MotivoRechazo { get; set; }
    public bool TieneMotivoRechazo =>
      !string.IsNullOrWhiteSpace(MotivoRechazo);

    public ICommand? VerMotivoCommand { get; set; }
    public ICommand? AprobarCommand { get; set; }
    public ICommand? RechazarCommand { get; set; }
    public ICommand? AbrirReservaCommand { get; set; }
  }
}
