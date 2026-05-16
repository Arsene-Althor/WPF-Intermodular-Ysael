using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Helpers;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System.Windows.Input;
using System.Windows;
using Hotel_Pere_Maria.Views;
using Microsoft.Win32;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ModReservaViewModel : BaseViewModel
    {
        private Reservation _reservaOriginal;
        private string _userId;
        private string _roomId;
        private DateTime? _checkIn;
        private DateTime? _checkOut;
        private double _precioNuevo;
        private bool _checkInEnabled = true;

        // Historial de auditoría (pestaña «Historial»)
        private readonly List<HistorialAuditoriaFila> _historialCache = new List<HistorialAuditoriaFila>();
        private bool _historialCargado;
        private string _filtroAccionSeleccionado = "Todas";
        private int _selectedTabIndex;

        public ObservableCollection<HistorialAuditoriaFila> HistorialFilas { get; } = new ObservableCollection<HistorialAuditoriaFila>();

        /// <summary>Índice del <see cref="TabControl"/>: 0 datos, 1 historial de auditoría.</summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value) return;
                _selectedTabIndex = value;
                OnPropertyChanged();
                if (value == 1)
                    _ = CargarHistorialAsync();
            }
        }

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
        public ICommand IrHistorialCommand { get; }

        public event EventHandler? RequestClose;

        // Propiedades
        public string ReservationId => _reservaOriginal.reservation_id.ToString();
        public double PrecioOriginal => _reservaOriginal.price;

        public string UserId { get => _userId; set { _userId = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public string RoomId { get => _roomId; set { _roomId = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public DateTime? CheckIn { get => _checkIn; set { _checkIn = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public DateTime? CheckOut { get => _checkOut; set { _checkOut = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public double PrecioNuevo { get => _precioNuevo; set { _precioNuevo = value; OnPropertyChanged(); } }
        public bool CheckInEnabled { get => _checkInEnabled; set { _checkInEnabled = value; OnPropertyChanged(); } }

        public string InvoiceNumberDisplay =>
            string.IsNullOrWhiteSpace(_reservaOriginal.invoice_number) ? "—" : _reservaOriginal.invoice_number;

        public bool HasInvoice => !string.IsNullOrWhiteSpace(_reservaOriginal.invoice_number);

        public bool CanRegistrarCheckout =>
            Session.User?.IsEmployee == true
            && _reservaOriginal.cancelation_date == null
            && !HasInvoice
            && _reservaOriginal.check_out <= DateTime.Now;

        public bool PuedeGestionarFlexibilidad => Session.User?.IsEmployee == true;

        private FlexibilityRequestDto? _earlyRequest;
        private FlexibilityRequestDto? _lateRequest;
        private string _clienteFidelidadTier = "";

        public FlexibilityRequestDto? EarlyRequest
        {
            get => _earlyRequest;
            set
            {
                _earlyRequest = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarSolicitudesEspeciales));
                OnPropertyChanged(nameof(TieneEarly));
                OnPropertyChanged(nameof(SinEarly));
                OnPropertyChanged(nameof(PuedeRevisarEarly));
            }
        }

        public FlexibilityRequestDto? LateRequest
        {
            get => _lateRequest;
            set
            {
                _lateRequest = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarSolicitudesEspeciales));
                OnPropertyChanged(nameof(TieneLate));
                OnPropertyChanged(nameof(SinLate));
                OnPropertyChanged(nameof(PuedeRevisarLate));
            }
        }

        public bool MostrarSolicitudesEspeciales => PuedeGestionarFlexibilidad;

        public bool TieneEarly => EarlyRequest?.HasRequest == true;
        public bool TieneLate => LateRequest?.HasRequest == true;
        public bool SinEarly => !TieneEarly;
        public bool SinLate => !TieneLate;

        public string ClienteFidelidadTier
        {
            get => _clienteFidelidadTier;
            set { _clienteFidelidadTier = value ?? ""; OnPropertyChanged(); }
        }
        public bool PuedeRevisarEarly => PuedeGestionarFlexibilidad && EarlyRequest?.CanStaffReview == true;
        public bool PuedeRevisarLate => PuedeGestionarFlexibilidad && LateRequest?.CanStaffReview == true;

        // Comandos
        public ICommand GuardarCommand { get; }
        public ICommand CancelarReservaCommand { get; }
        public ICommand CerrarCommand { get; }
        public ICommand SeleccionarHabitacionCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand DescargarFacturaCommand { get; }
        public ICommand DescargarJustificanteCommand { get; }
        public ICommand RegistrarCheckoutCommand { get; }
        public ICommand AprobarEarlyCommand { get; }
        public ICommand RechazarEarlyCommand { get; }
        public ICommand AprobarLateCommand { get; }
        public ICommand RechazarLateCommand { get; }
        public ICommand RefrescarFlexCommand { get; }

        public ModReservaViewModel(Reservation reserva)
        {
            _reservaOriginal = reserva;

            // Rellenar formulario (Mapeo)
            _userId = reserva.user_id;
            _roomId = reserva.room_id;
            _checkIn = reserva.check_in;
            _checkOut = reserva.check_out;
            _precioNuevo = reserva.price;
            _checkInEnabled = reserva.check_in > DateTime.Now;

            GuardarCommand = new RelayCommand(async () => await ExecuteGuardar());
            CancelarReservaCommand = new RelayCommand(async () => await ExecuteCancelarReserva());
            CerrarCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
            SeleccionarHabitacionCommand = new RelayCommand(ExecuteSeleccionarHabitacion);
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
            RefrescarHistorialCommand = new RelayCommand(() => { _ = CargarHistorialAsync(true); });
            IrHistorialCommand = new RelayCommand(() => SelectedTabIndex = 1);
            DescargarFacturaCommand = new RelayCommand(async () => await ExecuteDescargarFacturaAsync(), () => HasInvoice);
            DescargarJustificanteCommand = new RelayCommand(async () => await ExecuteDescargarJustificanteAsync());
            RegistrarCheckoutCommand = new RelayCommand(async () => await ExecuteRegistrarCheckoutAsync(), () => CanRegistrarCheckout);
            AprobarEarlyCommand = new RelayCommand(async () => await RevisarFlexAsync("early", "approved"), () => PuedeRevisarEarly);
            RechazarEarlyCommand = new RelayCommand(async () => await RevisarFlexAsync("early", "rejected"), () => PuedeRevisarEarly);
            AprobarLateCommand = new RelayCommand(async () => await RevisarFlexAsync("late", "approved"), () => PuedeRevisarLate);
            RechazarLateCommand = new RelayCommand(async () => await RevisarFlexAsync("late", "rejected"), () => PuedeRevisarLate);
            RefrescarFlexCommand = new RelayCommand(() => _ = CargarFlexibilidadAsync(true));

            if (PuedeGestionarFlexibilidad)
                _ = CargarFlexibilidadAsync();
        }

        private async Task CargarFlexibilidadAsync(bool forzar = false)
        {
            if (!PuedeGestionarFlexibilidad) return;
            try
            {
                var (ok, err, data) = await FlexibilityService.GetStatusAsync(_reservaOriginal.reservation_id);
                if (!ok)
                {
                    if (forzar)
                        MessageBox.Show(err ?? "No se pudo cargar flexibilidad", "Solicitudes especiales",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                EarlyRequest = data?.early_checkin_requested;
                LateRequest = data?.late_checkout_requested;
                ClienteFidelidadTier = TierLabelFromCode(data?.loyalty_tier);
                if (data?.price is > 0)
                {
                    _precioNuevo = data.price.Value;
                    _reservaOriginal.price = data.price.Value;
                    OnPropertyChanged(nameof(PrecioNuevo));
                }
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                if (forzar) MessageBox.Show(ex.Message, "Solicitudes especiales", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RevisarFlexAsync(string kind, string decision)
        {
            var etiqueta = decision == "approved" ? "aprobar" : "rechazar";
            var tipo = kind == "early" ? "entrada anticipada" : "salida tardía";
            var (confirmed, nota) = FlexibilityReviewNoteDialog.Show(
                "Solicitudes check-in / check-out",
                $"Va a {etiqueta} la solicitud de {tipo} para {_reservaOriginal.reservation_id}.");
            if (!confirmed) return;

            var (ok, err, newPrice) = kind == "early"
                ? await FlexibilityService.ReviewEarlyAsync(_reservaOriginal.reservation_id, decision, nota)
                : await FlexibilityService.ReviewLateAsync(_reservaOriginal.reservation_id, decision, nota);

            if (!ok)
            {
                MessageBox.Show(err ?? "Error", "Revisión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newPrice.HasValue)
            {
                PrecioNuevo = newPrice.Value;
                _reservaOriginal.price = newPrice.Value;
            }
            await CargarFlexibilidadAsync(true);
            MessageBox.Show(decision == "approved" ? "Solicitud aprobada" : "Solicitud rechazada",
                "Solicitudes especiales", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string TierLabelFromCode(string? tier) => tier switch
        {
            "gold" => "Oro",
            "silver" => "Plata",
            "bronze" => "Bronce",
            _ => string.IsNullOrWhiteSpace(tier) ? "—" : tier,
        };

        /// <summary>Carga auditoría la primera vez o si <paramref name="forzar"/> es true.</summary>
        public async Task CargarHistorialAsync(bool forzar = false)
        {
            if (_historialCargado && !forzar) return;

            try
            {
                var (ok, err, lista) = await ReservationService.GetBookingAuditAsync(_reservaOriginal.reservation_id);
                if (!ok)
                {
                    MessageBox.Show(string.IsNullOrWhiteSpace(err) ? "No se pudo cargar el historial" : err,
                        "Historial", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<Usuario> usuarios = new List<Usuario>();
                try
                {
                    usuarios = await UserService.GetAllUsersAsync();
                }
                catch
                {
                    // Si falla el listado de usuarios, mostramos solo actor_id
                }

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
                        Accion = TraducirAccionAuditoria(e.Action),
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
                MessageBox.Show(ex.Message, "Historial", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string TraducirAccionAuditoria(string action)
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

        private async void RecalcularPrecio()
        {
            if (CheckIn == null || CheckOut == null) return;

            // Validaciones básicas
            if (CheckOut < DateTime.Today) return;
            if (CheckIn >= CheckOut) return;

            // Solo llamamos a la API si algo ha cambiado
            if (CheckIn != _reservaOriginal.check_in || CheckOut != _reservaOriginal.check_out ||
                UserId != _reservaOriginal.user_id || RoomId != _reservaOriginal.room_id)
            {
                var (esOk, _, precio) = await ReservationService.getPriceReservation(UserId, RoomId, CheckIn, CheckOut);
                if (esOk) PrecioNuevo = precio;
            }
        }

        private async Task ExecuteGuardar()
        {
            try
            {
                var mod = new Reservation
                {
                    reservation_id = _reservaOriginal.reservation_id,
                    user_id = UserId,
                    room_id = RoomId,
                    check_in = CheckIn ?? DateTime.Now,
                    check_out = CheckOut ?? DateTime.Now,
                    price = PrecioNuevo
                };

                var (esOk, error) = await ReservationService.updateReservation(mod);
                if (esOk)
                {
                    double dif = PrecioNuevo - PrecioOriginal;
                    string msg = dif > 0 ? $"\nCargo extra de {dif}€" : (dif < 0 ? $"\nDevolución de {-dif}€" : " sin cargos");
                    MessageBox.Show("Modificada" + msg);
                    RequestClose?.Invoke(this, EventArgs.Empty);
                }
                else MessageBox.Show(error);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async Task ExecuteCancelarReserva()
        {
            var (esOk, respuesta, precioCancelacion) = await ReservationService.getCancelationPrice(_reservaOriginal.reservation_id, DateTime.Now);
            if (!esOk) {
                MessageBox.Show(respuesta, "No es posible cancelar la reserva");
                return;
            }

            double devolucion = PrecioOriginal - precioCancelacion;
            var result = MessageBox.Show($"Devolución: {devolucion}€\n¿Confirmar?", "Cancelar", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var (ok, error) = await ReservationService.cancelReservation(_reservaOriginal, precioCancelacion);
                if (ok) RequestClose?.Invoke(this, EventArgs.Empty);
                else MessageBox.Show(error);
            }
        }

        private async Task ExecuteDescargarFacturaAsync()
        {
            if (!HasInvoice) return;
            var (ok, err, pdf) = await ReservationService.DownloadInvoicePdfAsync(_reservaOriginal.reservation_id);
            if (!ok || pdf == null || pdf.Length == 0)
            {
                MessageBox.Show(err ?? "Sin PDF", "Factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = $"Factura-{_reservaOriginal.invoice_number}.pdf",
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = ".pdf"
            };
            if (dlg.ShowDialog() == true)
            {
                await System.IO.File.WriteAllBytesAsync(dlg.FileName, pdf);
                MessageBox.Show($"Guardado:\n{dlg.FileName}", "Factura", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task ExecuteDescargarJustificanteAsync()
        {
            var (ok, err, pdf) = await ReservationService.DownloadBookingReceiptPdfAsync(_reservaOriginal.reservation_id);
            if (!ok || pdf == null || pdf.Length == 0)
            {
                MessageBox.Show(err ?? "Sin PDF", "Justificante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = $"Justificante-{_reservaOriginal.reservation_id}.pdf",
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = ".pdf"
            };
            if (dlg.ShowDialog() == true)
            {
                await System.IO.File.WriteAllBytesAsync(dlg.FileName, pdf);
                MessageBox.Show($"Guardado:\n{dlg.FileName}", "Justificante", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task ExecuteRegistrarCheckoutAsync()
        {
            if (!CanRegistrarCheckout) return;
            if (MessageBox.Show(
                    "Registrar checkout y emitir número de factura en la API. ¿Continuar?",
                    "Checkout",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            var (ok, msg, inv) = await ReservationService.PostCheckoutAsync(_reservaOriginal.reservation_id);
            if (!ok)
            {
                MessageBox.Show(msg, "Checkout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _reservaOriginal.invoice_number = inv ?? _reservaOriginal.invoice_number;
            _reservaOriginal.checkout_completed_at = DateTime.Now;
            OnPropertyChanged(nameof(InvoiceNumberDisplay));
            OnPropertyChanged(nameof(HasInvoice));
            OnPropertyChanged(nameof(CanRegistrarCheckout));
            CommandManager.InvalidateRequerySuggested();
            MessageBox.Show($"Checkout registrado.\nFactura: {_reservaOriginal.invoice_number}", "Factura",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteSeleccionarHabitacion() {
            if (!listRoom.TryPickRoom(CheckIn, CheckOut, out var picked) || picked == null)
                return;
            RoomId = picked.RoomId;
        }
        private void ExecuteSeleccionarCliente() {
            try
            {
                var usuario = GestionUsuarios.ShowPickerDialog();
                if (usuario != null)
                {
                    if (usuario.role == "client")
                        UserId = usuario.user_id;
                    else
                        MessageBox.Show("El usuario seleccionado no es un cliente.", "Aviso",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el selector de clientes: {ex.Message}");
            }
        }
    }
}
