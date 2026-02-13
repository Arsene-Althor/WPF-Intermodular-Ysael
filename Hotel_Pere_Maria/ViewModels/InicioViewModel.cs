using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        private string _nombreUsuario;

        public event EventHandler RequestClose; // Para cerrar sesión

        // Propiedades bindeables
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

        // Comandos
        public ICommand CargarDatosCommand { get; }
        public ICommand AbrirAddReservaCommand { get; }
        public ICommand AbrirAllReservasCommand { get; }
        public ICommand AbrirGestionUsuariosCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand AbrirAllRoomsCommand { get; }

        public InicioViewModel()
        {
            CargarDatosCommand = new RelayCommand(async () => await CargarTodo());
            AbrirAddReservaCommand = new RelayCommand(async () => await AbrirVentana(new addReserva()));
            AbrirAllReservasCommand = new RelayCommand(async () => await AbrirVentana(new listReservas()));
            AbrirGestionUsuariosCommand = new RelayCommand(() => new GestionUsuarios().ShowDialog());
            CerrarSesionCommand = new RelayCommand(async () => await ExecuteLogout());
            AbrirPerfilCommand = new RelayCommand(ExecuteAbrirPerfil);
            AbrirAllRoomsCommand = new RelayCommand(() => new listRoom().ShowDialog());

            _ = CargarTodo(); // Carga inicial
        }

        private async Task CargarTodo()
        {
            CargarImagenPerfil();
            await CargarReservas();
        }

        private void ExecuteAbrirPerfil() { 
            PerfilUsuario perfilWindow = new PerfilUsuario();
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
            catch { /* Fallback a imagen por defecto en XAML */ }
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

        private async Task AbrirVentana(Window ventana)
        {
            ventana.ShowDialog();
            await CargarReservas(); // Refrescar tras cerrar el diálogo
        }

        private async Task ExecuteLogout()
        {
            var result = MessageBox.Show("¿Cerrar sesión?", "Logout", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await AuthService.LogoutAsync();
                new MainWindow().Show(); // Abrir Login
                RequestClose?.Invoke(this, EventArgs.Empty); // Cerrar Inicio
            }
        }
    }
}
