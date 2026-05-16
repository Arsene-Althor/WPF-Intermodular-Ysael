using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Helpers;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria.ViewModels
{
  public class SolicitudesFlexibilidadViewModel : BaseViewModel
  {
    private DateTime? _fechaDia = DateTime.Today;
    private bool _cargando;
    private string _resumen = "0 solicitudes";
    private bool _ordenRecientePrimero = true;

    public DateTime? FechaDia
    {
      get => _fechaDia;
      set { _fechaDia = value; OnPropertyChanged(); }
    }

    public string Resumen
    {
      get => _resumen;
      set { _resumen = value; OnPropertyChanged(); }
    }

    public bool Cargando
    {
      get => _cargando;
      set { _cargando = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public bool OrdenRecientePrimero
    {
      get => _ordenRecientePrimero;
      set
      {
        _ordenRecientePrimero = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(OrdenToggleTexto));
        AplicarOrden();
      }
    }

    public string OrdenToggleTexto =>
      OrdenRecientePrimero ? "Orden: más recientes primero" : "Orden: más antiguas primero";

    public ObservableCollection<SolicitudFlexFila> FilasActivas { get; } = new();
    public ObservableCollection<SolicitudFlexFila> FilasInactivas { get; } = new();

    public bool SinActivas => FilasActivas.Count == 0 && !Cargando;
    public bool SinInactivas => FilasInactivas.Count == 0 && !Cargando;

    public ICommand CargarCommand { get; }
    public ICommand AlternarOrdenCommand { get; }

    public SolicitudesFlexibilidadViewModel()
    {
      CargarCommand = new RelayCommand(() => _ = CargarAsync(), () => !Cargando);
      AlternarOrdenCommand = new RelayCommand(() => OrdenRecientePrimero = !OrdenRecientePrimero);
    }

    public async Task CargarAsync()
    {
      Cargando = true;
      OnPropertyChanged(nameof(SinActivas));
      OnPropertyChanged(nameof(SinInactivas));
      try
      {
        var (ok, err, data) = await FlexibilityService.GetPendingAsync(FechaDia ?? DateTime.Today);
        if (!ok)
        {
          MessageBox.Show(err ?? "Error", "Solicitudes del día", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }
        FilasActivas.Clear();
        FilasInactivas.Clear();
        var items = data?.items ?? Array.Empty<PendingFlexibilityItemDto>();
        foreach (var it in items)
        {
          var fila = MapFila(it);
          if (fila.EsActiva)
            FilasActivas.Add(fila);
          else
            FilasInactivas.Add(fila);
        }
        AplicarOrden();
        var pendientes = data?.pending_count ?? items.Count(i => i.needs_review);
        Resumen =
          $"{items.Length} total · {FilasActivas.Count} activa(s) · {FilasInactivas.Count} inactiva(s) · {pendientes} pend. revisión";
        OnPropertyChanged(nameof(SinActivas));
        OnPropertyChanged(nameof(SinInactivas));
      }
      finally { Cargando = false; }
    }

    private void AplicarOrden()
    {
      OrdenarColeccion(FilasActivas);
      OrdenarColeccion(FilasInactivas);
    }

    private void OrdenarColeccion(ObservableCollection<SolicitudFlexFila> col)
    {
      var sorted = OrdenRecientePrimero
        ? col.OrderByDescending(f => f.FechaOrden).ToList()
        : col.OrderBy(f => f.FechaOrden).ToList();
      col.Clear();
      foreach (var f in sorted)
        col.Add(f);
    }

    private SolicitudFlexFila MapFila(PendingFlexibilityItemDto it)
    {
      var req = it.request;
      var tipoTexto = it.type switch
      {
        "early_checkin" => "Entrada anticipada",
        "late_checkout" => req?.late_mode == "facilities"
          ? "Instalaciones (sin habitación)"
          : "Salida tardía (habitación)",
        "stay_extension" => "Ampliación de estancia",
        _ => it.type ?? "—",
      };
      var motivo =
        req?.status == "rejected" && !string.IsNullOrWhiteSpace(req.review_note)
          ? req.review_note.Trim()
          : null;
      var estado =
        it.type == "stay_extension"
          ? "Procesada"
          : req?.StatusLabel ?? it.status_summary ?? "—";
      var esActiva = it.needs_review || req?.status == "pending";
      var fechaOrden =
        req?.requested_at
        ?? it.issued_at
        ?? it.check_out
        ?? it.check_in
        ?? DateTime.Now;
      var fila = new SolicitudFlexFila
      {
        ReservationId = it.reservation_id ?? "",
        RoomId = it.room_id ?? "",
        UserId = it.user_id ?? "",
        TipoClave = it.type ?? "",
        TipoTexto = tipoTexto,
        HoraSolicitada =
          it.type == "stay_extension"
            ? (it.issued_at.HasValue ? it.issued_at.Value.ToString("dd/MM/yyyy HH:mm") : "—")
            : req?.RequestedTimeText ?? "—",
        RangoFidelidad = req?.TierLabel ?? "—",
        Tarifa =
          it.type == "stay_extension"
            ? (it.supplement.HasValue ? $"{it.supplement.Value:N2} €" : "—")
            : req?.FeeText ?? "—",
        Disponibilidad =
          it.type == "stay_extension"
            ? (it.description ?? "Factura emitida")
            : req?.availability_ok == false ? "Sin hueco" : "OK",
        EstadoTexto = estado,
        MotivoRechazo = motivo,
        EsActiva = esActiva,
        FechaOrden = fechaOrden,
        PuedeRevisar = it.needs_review && it.type != "stay_extension",
        CheckIn = it.check_in,
        CheckOut = it.check_out,
        Request = req,
        AprobarCommand = new RelayCommand(() => _ = RevisarAsync(it, "approved")),
        RechazarCommand = new RelayCommand(() => _ = RevisarAsync(it, "rejected")),
        AbrirReservaCommand = new RelayCommand(() => AbrirReserva(it)),
      };
      fila.VerMotivoCommand = new RelayCommand(() => MostrarMotivo(fila));
      return fila;
    }

    private async Task RevisarAsync(PendingFlexibilityItemDto it, string decision)
    {
      if (!it.needs_review || it.type == "stay_extension") return;

      var etiqueta = decision == "approved" ? "aprobar" : "rechazar";
      var tipo = it.type == "early_checkin" ? "entrada anticipada" : "salida tardía";
      var (confirmed, nota) = FlexibilityReviewNoteDialog.Show(
        "Solicitudes pendientes",
        $"Va a {etiqueta} {tipo} — {it.reservation_id}.");
      if (!confirmed) return;

      var (ok, err, _) = it.type == "early_checkin"
        ? await FlexibilityService.ReviewEarlyAsync(it.reservation_id!, decision, nota)
        : await FlexibilityService.ReviewLateAsync(it.reservation_id!, decision, nota);

      if (!ok)
      {
        MessageBox.Show(err ?? "Error", "Revisión", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }
      MessageBox.Show(decision == "approved" ? "Aprobada" : "Rechazada", "Revisión",
        MessageBoxButton.OK, MessageBoxImage.Information);
      await CargarAsync();
    }

    private static void MostrarMotivo(SolicitudFlexFila fila)
    {
      var texto = string.IsNullOrWhiteSpace(fila.MotivoRechazo)
        ? "Sin motivo registrado."
        : fila.MotivoRechazo;
      MessageBox.Show(
        texto,
        $"Motivo — {fila.ReservationId}",
        MessageBoxButton.OK,
        MessageBoxImage.Information);
    }

    private static void AbrirReserva(PendingFlexibilityItemDto it)
    {
      var r = new Reservation
      {
        reservation_id = it.reservation_id ?? "",
        room_id = it.room_id ?? "",
        user_id = it.user_id ?? "",
        check_in = it.check_in ?? DateTime.Now,
        check_out = it.check_out ?? DateTime.Now.AddDays(1),
        price = it.price ?? 0,
      };
      var dlg = new modReserva(r) { Owner = UiShell.OwnerWindow };
      dlg.ShowDialog();
    }
  }
}
