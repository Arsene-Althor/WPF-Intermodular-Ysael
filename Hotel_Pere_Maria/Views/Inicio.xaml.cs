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
        public Inicio()
        {
            InitializeComponent();
            this.Loaded += Iniciar_Ventana;
        }

        private async void Iniciar_Ventana(object sender, RoutedEventArgs e) {
            await Cargar_Reservas();
        }

        private async Task Cargar_Reservas()
        {
            try
            {
                var reservas = await ApiService.getAllActiveReservation();
                if (reservas != null)
                {
                    listReservation.ItemsSource = reservas;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}");
            }
        }

        private void click_abriraddReserva(object sender, RoutedEventArgs e) { 
            addReserva addreserva = new addReserva();
            addreserva.ShowDialog();
            
        }
    }
}
