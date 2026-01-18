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
    /// Lógica de interacción para listReservas.xaml
    /// </summary>
    public partial class listReservas : Window
    {
        public listReservas()
        {
            InitializeComponent();
            this.Loaded += Iniciar_Ventana;
        }

        private async void Iniciar_Ventana(object sender, RoutedEventArgs e)
        {
            await Cargar_Reservas();
        }

        private async Task Cargar_Reservas()
        {
            try
            {
                var reservas = await ApiService.getAllReservation();
                if (reservas != null)
                {
                    dgReservas.ItemsSource = reservas;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}");
            }
        }

        //Pendiente implementar
        private void FiltrarDatos(object sender, EventArgs e) { 
        }

        private void Click_LimpiarFiltros(object sender, RoutedEventArgs e) { 
        }
    }
}
