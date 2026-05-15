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
        private ObservableCollection<Reservation> _reservasActivas;
        private ImageSource _imagenPerfil;
        private object _currentPage;

        public event EventHandler RequestClose;

        public ObservableCollection<Reservation> ReservasActivas
        {
            get => _reservasActivas;
            set { _reservasActivas = value; OnPropertyChanged(); }
        }

        public ImageSource ImagenPerfil
        {
            get => _imagenPerfil;
            set { _imagenPerfil = value; OnPropertyChanged(); }
        }

        public string NombreUsuario => Session.User?.name ?? "Usuario";

        /// <summary>Página incrustada (lista/añadir reserva). null = panel principal.</summary>
        public object CurrentPage
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

        private async Task CargarTodo()
        {
            CargarImagenPerfil();
            await CargarReservas();
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
                ReservasActivas = new ObservableCollection<Reservation>(lista ?? new List<Reservation>());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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
