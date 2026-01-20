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
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Microsoft.VisualBasic;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para modReserva.xaml
    /// </summary>
    public partial class modReserva : Window
    {
        private Reservation _reserva;
        public modReserva(Reservation reserva)
        {
            InitializeComponent();
            this._reserva = reserva;
            relleanForm();

        }

        private void RecalcularPrecio(object sender, EventArgs e) {
            DateTime? newCheckIn = dpCheckIn.SelectedDate;
            DateTime? newCheckOut = dpCheckOut.SelectedDate;

            DateTime ahora = DateTime.Now;
            ahora = ahora.AddDays(-1);

            if (newCheckIn < ahora) {
                MessageBox.Show("No es posible modificar la fehca de entrada a antes de hoy");
                dpCheckIn.SelectedDate = null;
                return;
            }
            if (newCheckOut < ahora) {
                MessageBox.Show("No es posible modificar la fehca de salida a antes de hoy");
                dpCheckOut.SelectedDate = null; 
                return;
            }
            if (newCheckIn >= newCheckOut) {
                MessageBox.Show("La fecha de entrada no puede ser la misma o menor que la de salida");
                dpCheckIn.SelectedDate = null;
                dpCheckOut.SelectedDate = null;
                return;
            }

        }

        public async void Click_CancelarReserva(object sender, RoutedEventArgs e) { 
            DateTime fechaCancelacion = DateTime.Now;
            double precioCancel = 0;

            MessageBoxResult respuesta = MessageBox.Show(
                "La devolución por cancelación serian: " + precioCancel + " €\n" +
                "¿Está seguro de que quieres cancelar la reserva?","Cancelación de reserva", MessageBoxButton.YesNo);

            if (respuesta == MessageBoxResult.Yes) {

                try
                {
                    var (esOk, respuestaapi) = await ApiService.cancelReservation(_reserva, precioCancel);
                    if (esOk)
                    {
                        MessageBox.Show("Reserva cancleada", "Cancelación realizada!");
                        this.Close();
                    }
                    else {
                        MessageBox.Show(respuestaapi, "Error de cancelación");
                    }
                }
                catch (Exception ex) { 
                    MessageBox.Show(ex.Message);
                }

                
            } 
        }

        private void relleanForm() {
            nReserva.Text = nReserva.Text + _reserva.reservation_id;
        }
        private async void Click_Guardar(object sender, RoutedEventArgs e)
        { 
        }
        private async void Click_Cancelar(object sender, RoutedEventArgs e)
        {
        }

    }
}
