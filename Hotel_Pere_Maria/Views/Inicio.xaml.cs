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
        private async void Iniciar_Ventana(object sender, RoutedEventArgs e) {
            await Cargar_Reservas();
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
            catch (Exception ex) {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}");
            }
        }

        //Metodos para abrir otras ventanas
        private void click_abriraddReserva(object sender, RoutedEventArgs e) { 
            addReserva addreserva = new addReserva();
            addreserva.ShowDialog();
            
        }

        private void click_abrirallReservas(object sender, RoutedEventArgs e) { 
            listReservas listReservas = new listReservas();
            listReservas.ShowDialog();
        }
    }
}
