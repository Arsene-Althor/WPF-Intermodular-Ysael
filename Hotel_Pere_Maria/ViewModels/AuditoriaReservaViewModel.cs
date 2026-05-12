using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    /// <summary>Ventana solo lectura: historial de auditoría de una reserva (GET /reservation/.../audit).</summary>
    public class AuditoriaReservaViewModel : BaseViewModel
    {
        private readonly string _reservationId;
        private readonly List<HistorialAuditoriaFila> _historialCache = new List<HistorialAuditoriaFila>();
        private bool _historialCargado;
        private string _filtroAccionSeleccionado = "Todas";

        public string TituloVentana => $"Auditoría — {_reservationId}";

        public ObservableCollection<HistorialAuditoriaFila> HistorialFilas { get; } = new ObservableCollection<HistorialAuditoriaFila>();

        public ObservableCollection<string> FiltrosAccion { get; } = new ObservableCollection<string>(new[]
        {
            "Todas", "CREATED", "UPDATED", "CANCELED", "PAYMENT_ADDED", "EXTRA_ADDED"
        });

        public string FiltroAccionSeleccionado
        {
            get => _filtroAccionSeleccionado;
            set
            {
                _filtroAccionSeleccionado = string.IsNullOrWhiteSpace(value) ? "Todas" : value;
                OnPropertyChanged();
                AplicarFiltroHistorial();
            }
        }

        public ICommand RefrescarHistorialCommand { get; }
        public ICommand CerrarCommand { get; }

        public event EventHandler RequestClose;

        public AuditoriaReservaViewModel(string reservationId)
        {
            _reservationId = reservationId ?? "";
            RefrescarHistorialCommand = new RelayCommand(() => { _ = CargarHistorialAsync(true); });
            CerrarCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
            _ = CargarHistorialAsync(false);
        }

        public async Task CargarHistorialAsync(bool forzar = false)
        {
            if (_historialCargado && !forzar) return;

            try
            {
                var (ok, err, lista) = await ReservationService.GetBookingAuditAsync(_reservationId);
                if (!ok)
                {
                    MessageBox.Show(string.IsNullOrWhiteSpace(err) ? "No se pudo cargar el historial" : err,
                        "Auditoría", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<Usuario> usuarios = new List<Usuario>();
                try
                {
                    usuarios = await UserService.GetAllUsersAsync();
                }
                catch { /* solo actor_id */ }

                var mapaNombres = usuarios.Where(u => !string.IsNullOrEmpty(u.user_id))
                    .GroupBy(u => u.user_id)
                    .ToDictionary(g => g.Key, g => g.First().FullName ?? g.First().user_id);

                _historialCache.Clear();
                foreach (var e in lista.OrderBy(x => x.Timestamp ?? DateTime.MinValue))
                {
                    string nombreActor = mapaNombres.TryGetValue(e.ActorId, out var n) ? n : e.ActorId;
                    string resumen = (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                        ? string.Join(Environment.NewLine, e.ResumenCambios)
                        : "—";

                    _historialCache.Add(new HistorialAuditoriaFila
                    {
                        ActionKey = e.Action ?? "",
                        Accion = TraducirAccion(e.Action),
                        ActorId = e.ActorId,
                        ActorNombre = nombreActor,
                        Fecha = e.Timestamp,
                        ResumenTexto = resumen
                    });
                }

                _historialCargado = true;
                AplicarFiltroHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Auditoría", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string TraducirAccion(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "—";
            return action switch
            {
                "CREATED" => "Alta de reserva",
                "UPDATED" => "Modificación",
                "CANCELED" => "Cancelación",
                "PAYMENT_ADDED" => "Pago añadido",
                "EXTRA_ADDED" => "Extra añadido",
                _ => action
            };
        }

        private void AplicarFiltroHistorial()
        {
            HistorialFilas.Clear();
            foreach (var fila in _historialCache.Where(h =>
                         FiltroAccionSeleccionado == "Todas" ||
                         string.Equals(h.ActionKey, FiltroAccionSeleccionado, StringComparison.OrdinalIgnoreCase)))
            {
                HistorialFilas.Add(fila);
            }
        }
    }
}
