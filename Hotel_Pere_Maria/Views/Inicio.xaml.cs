using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para Inicio.xaml
    /// </summary>
    public partial class Inicio : Window
    {
        //Ventana de inicio accedemos a esta tras inicio de sesion
        public Inicio()
        {
            //Iniciamos la ventana cargando los datos de todas las reservas activas
            InitializeComponent();
            this.Loaded += Iniciar_Ventana;
        }

        //Este metodo lo usamos para refrescar la ventana lo asignamos al boton de refrescar e inicio de ventana
        private async void Iniciar_Ventana(object sender, RoutedEventArgs e)
        {
            CargarImagenPerfil();

            await Cargar_Reservas();
        }

        private void CargarImagenPerfil()
        {
            try
            {
                // Verificamos si hay usuario y si tiene imagen
                if (Session.User != null && !string.IsNullOrEmpty(Session.User.profileImage))
                {
                    // Construimos la URL. 
                    // ApiService.BaseUrl suele ser "http://localhost:3000/api/"
                    // Las imagenes están en "http://localhost:3000/uploads/..."
                    // Así que quitamos el "api/" para obtener la raíz.
                    string baseUrl = ApiService.BaseUrl.Replace("api/", "");
                    string fullUrl = $"{baseUrl}{Session.User.profileImage}";

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullUrl, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgAvatar.ImageSource = bitmap;
                }
            }
            catch (Exception)
            {
                // Si falla, se queda la imagen por defecto del XAML (/Resources/userIcon.png)
            }
        }

        //Cargamos los datos de las reservas activas en la lista de listReservation 
        private async Task Cargar_Reservas()
        {
            try
            {
                var reservas = await ReservationService.getAllActiveReservation();
                if (reservas != null)
                {
                    listReservation.ItemsSource = reservas;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}");
            }
        }

        //Metodos para abrir otras ventanas
        private async void click_abriraddReserva(object sender, RoutedEventArgs e)
        {
            addReserva addreserva = new addReserva();
            addreserva.ShowDialog();
            await Cargar_Reservas();

        }

        private async void click_abrirallReservas(object sender, RoutedEventArgs e)
        {
            listReservas listReservas = new listReservas();
            listReservas.ShowDialog();
            await Cargar_Reservas();
        }


        private void click_abrirRooms(object sender, RoutedEventArgs e)
        {
            DateTime fecha = DateTime.Now;
            SelectedRoom room = new SelectedRoom(fecha.AddDays(-5), fecha);
            room.ShowDialog();

        }

        private void click_abrirGestionUsuarios(object sender, RoutedEventArgs e)
        {
            GestionUsuarios ventanaGestion = new GestionUsuarios();
            ventanaGestion.ShowDialog();
        }

        private async void Click_CerrarSesion(object sender, RoutedEventArgs e)
        {
            var confrim = MessageBox.Show("¿Seguro que quieres cerrar sesion?", "Cerrar Sesion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confrim == MessageBoxResult.Yes)
            {
                await AuthService.LogoutAsync();

                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();

                //Cerrar ventana actual
                this.Close();
            }
        }

        private void Click_AbrirPerfil(object sender, RoutedEventArgs e)
        {
            PerfilUsuario perfil = new PerfilUsuario();
            perfil.ShowDialog();
        }

    }
}
