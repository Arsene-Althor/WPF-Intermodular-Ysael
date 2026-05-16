using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Hotel_Pere_Maria.ViewModels
{
    public class InicioViewModel : BaseViewModel
    {
        private ObservableCollection<Reservation> _reservasActivas = new();
        private List<Reservation> _todasActivas = new();
        private string _filtroReservas = "";
        private string _ordenReservas = "SalidaAsc";
        private ImageSource? _imagenPerfil;
        private object? _currentPage;

        public event EventHandler? RequestClose;

        public ObservableCollection<Reservation> ReservasActivas
        {
            get => _reservasActivas;
            set { _reservasActivas = value; OnPropertyChanged(); }
        }

        public ImageSource? ImagenPerfil
        {
            get => _imagenPerfil;
            set { _imagenPerfil = value; OnPropertyChanged(); }
        }

        public string NombreUsuario => Session.User?.name ?? "Usuario";

        /// <summary>Página incrustada (lista/añadir reserva). null = panel principal.</summary>
        public object? CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHomeVisible));
                OnPropertyChanged(nameof(IsSubPageVisible));
            }
        }

        public bool IsHomeVisible => _currentPage == null;
        public bool IsSubPageVisible => _currentPage != null;

        /// <summary>Admin o empleado: facturas y checkout en API.</summary>
        public bool PuedeGestionFacturas => Session.User?.IsEmployee == true;

        private int _flexPendientesHoy;

        public int FlexPendientesHoy
        {
            get => _flexPendientesHoy;
            set
            {
                _flexPendientesHoy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ResumenFlexPendientes));
                OnPropertyChanged(nameof(HayFlexPendientes));
            }
        }

        public bool HayFlexPendientes => FlexPendientesHoy > 0;

        public string ResumenFlexPendientes =>
            FlexPendientesHoy > 0
                ? $"{FlexPendientesHoy} solicitud(es) pendiente(s) hoy"
                : "Sin solicitudes pendientes hoy";

        public string FiltroReservas
        {
            get => _filtroReservas;
            set { _filtroReservas = value ?? ""; OnPropertyChanged(); AplicarFiltroReservas(); }
        }

        public string OrdenReservas
        {
            get => _ordenReservas;
            set { _ordenReservas = string.IsNullOrWhiteSpace(value) ? "SalidaAsc" : value; OnPropertyChanged(); AplicarFiltroReservas(); }
        }

        public string ContadorReservas =>
            ReservasActivas.Count == _todasActivas.Count
                ? $"{ReservasActivas.Count} reserva(s) activa(s)"
                : $"{ReservasActivas.Count} de {_todasActivas.Count} reserva(s)";

        public ICommand CargarDatosCommand { get; }
        public ICommand IrInicioCommand { get; }
        public ICommand AbrirAllReservasCommand { get; }
        public ICommand AbrirGestionUsuariosCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand AbrirAllRoomsCommand { get; }
        public ICommand AbrirFacturasCommand { get; }
        public ICommand AbrirConfigFacturaCommand { get; }
        public ICommand AbrirAuditoriasCommand { get; }
        public ICommand AbrirCheckInRecepcionCommand { get; }
        public ICommand AbrirSolicitudesFlexCommand { get; }
        public ICommand AbrirConfigFlexCommand { get; }

        public InicioViewModel()
        {
            CargarDatosCommand = new RelayCommand(() => { _ = CargarTodo(); });
            IrInicioCommand = new RelayCommand(() => { _ = IrInicioAsync(); });
            AbrirAllReservasCommand = new RelayCommand(OpenListReservasEmbedded);
            AbrirGestionUsuariosCommand = new RelayCommand(OpenGestionUsuariosEmbedded);
            CerrarSesionCommand = new RelayCommand(async () => await ExecuteLogout());
            AbrirPerfilCommand = new RelayCommand(ExecuteAbrirPerfil);
            AbrirAllRoomsCommand = new RelayCommand(OpenListRoomEmbedded);
            AbrirFacturasCommand = new RelayCommand(OpenFacturasEmbedded, () => PuedeGestionFacturas);
            AbrirConfigFacturaCommand = new RelayCommand(OpenConfigFacturaEmbedded, () => PuedeGestionFacturas);
            AbrirAuditoriasCommand = new RelayCommand(OpenAuditoriasEmbedded, () => PuedeGestionFacturas);
            AbrirCheckInRecepcionCommand = new RelayCommand<Reservation>(AbrirCheckInRecepcion, r => r != null && PuedeGestionFacturas);
            AbrirSolicitudesFlexCommand = new RelayCommand(OpenSolicitudesFlexEmbedded, () => PuedeGestionFacturas);
            AbrirConfigFlexCommand = new RelayCommand(OpenConfigFlexEmbedded, () => PuedeGestionFacturas);

            _ = CargarTodo();
        }

        private void AbrirCheckInRecepcion(Reservation reserva)
        {
            if (reserva == null || string.IsNullOrWhiteSpace(reserva.reservation_id)) return;
            var dlg = new CheckInRecepcion(reserva.reservation_id)
            {
                Owner = UiShell.OwnerWindow,
            };
            bool? ok = dlg.ShowDialog();
            if (ok == true)
                _ = CargarReservas();
        }

        private async Task IrInicioAsync()
        {
            CurrentPage = null;
            await CargarTodo();
        }

        private void OpenGestionUsuariosEmbedded()
        {
            CurrentPage = new GestionUsuarios(false);
        }

        private void OpenListRoomEmbedded()
        {
            CurrentPage = new listRoom();
        }

        private void OpenListReservasEmbedded()
        {
            CurrentPage = new listReservas();
        }

        private void OpenFacturasEmbedded()
        {
            if (!PuedeGestionFacturas) return;
            CurrentPage = new listFacturas();
        }

        private void OpenConfigFacturaEmbedded()
        {
            if (!PuedeGestionFacturas) return;
            CurrentPage = new ConfigFactura();
        }

        private void OpenAuditoriasEmbedded()
        {
            if (!PuedeGestionFacturas) return;
            CurrentPage = new listAuditorias();
        }

        private void OpenSolicitudesFlexEmbedded()
        {
            if (!PuedeGestionFacturas) return;
            CurrentPage = new SolicitudesFlexibilidad();
            _ = CargarFlexPendientesAsync();
        }

        private void OpenConfigFlexEmbedded()
        {
            if (!PuedeGestionFacturas) return;
            CurrentPage = new ConfigFlexibilidad();
        }

        private async Task CargarTodo()
        {
            CargarImagenPerfil();
            await CargarReservas();
            await CargarFlexPendientesAsync();
        }

        private async Task CargarFlexPendientesAsync()
        {
            if (!PuedeGestionFacturas)
            {
                FlexPendientesHoy = 0;
                return;
            }
            try
            {
                var (ok, _, data) = await FlexibilityService.GetPendingAsync(DateTime.Today);
                FlexPendientesHoy = ok ? data?.count ?? 0 : 0;
            }
            catch
            {
                FlexPendientesHoy = 0;
            }
        }

        private void ExecuteAbrirPerfil()
        {
            var perfilWindow = new PerfilUsuario();
            perfilWindow.Owner = Hotel_Pere_Maria.UiShell.OwnerWindow;
            perfilWindow.ShowDialog();
            CargarImagenPerfil();
            OnPropertyChanged(nameof(NombreUsuario));
        }

        private void CargarImagenPerfil()
        {
            try
            {
                if (Session.User != null && !string.IsNullOrEmpty(Session.User.profileImage))
                {
                    string baseUrl = ApiService.BaseUrl.Replace("api/", "");
                    string fullUrl = $"{baseUrl}{Session.User.profileImage}";
                    ImagenPerfil = new BitmapImage(new Uri(fullUrl, UriKind.Absolute));
                }
            }
            catch { /* sin imagen */ }
        }

        private async Task CargarReservas()
        {
            try
            {
                var lista = await ReservationService.getAllActiveReservation();
                _todasActivas = lista ?? new List<Reservation>();
                AplicarFiltroReservas();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void AplicarFiltroReservas()
        {
            var q = _todasActivas.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(FiltroReservas))
            {
                var t = FiltroReservas.Trim();
                q = q.Where(r =>
                    (r.reservation_id?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.room_id?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.GuestDisplayName?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.GuestDisplayDni?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            q = OrdenReservas switch
            {
                "SalidaDesc" => q.OrderByDescending(r => r.EffectiveCheckOut),
                "EntradaAsc" => q.OrderBy(r => r.check_in),
                "EntradaDesc" => q.OrderByDescending(r => r.check_in),
                "Habitacion" => q.OrderBy(r => r.room_id, StringComparer.OrdinalIgnoreCase),
                "Cliente" => q.OrderBy(r => r.GuestDisplayName, StringComparer.OrdinalIgnoreCase),
                "Retraso" => q.OrderByDescending(r => r.IsSalidaRetrasada).ThenBy(r => r.EffectiveCheckOut),
                _ => q.OrderBy(r => r.EffectiveCheckOut),
            };

            ReservasActivas = new ObservableCollection<Reservation>(q.ToList());
            OnPropertyChanged(nameof(ContadorReservas));
        }

        private async Task ExecuteLogout()
        {
            var result = MessageBox.Show("¿Cerrar sesión?", "Logout", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await AuthService.LogoutAsync();
                new MainWindow().Show();
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
