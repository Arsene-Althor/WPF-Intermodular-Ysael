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
    //Controller para mofificar reservas
    public partial class modReserva : Window
    {
        //La variable _reserva es la reserva que vamos a modificar
        private Reservation _reserva;
        public modReserva(Reservation reserva)
        {
            InitializeComponent();
            this._reserva = reserva;
            relleanForm();

        }

        //Esta función muestra el precio de la modificación dinamicamente
        private void RecalcularPrecio(object sender, EventArgs e) {
            DateTime? newCheckIn = dpCheckIn.SelectedDate;
            DateTime? newCheckOut = dpCheckOut.SelectedDate;

            String? newuser_id = txtUserId.Text ?? null;
            String? newroom_id = txtRoomId.Text ?? null;

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

            double precioAmpliacion = Reservation.CalcularPrecio(_reserva,newuser_id,newroom_id,newCheckIn, newCheckOut);

            lblPrecioFinal.Text = precioAmpliacion + " €";

        }

        //Funcion para el boton de cancelar reserva

        public async void Click_CancelarReserva(object sender, RoutedEventArgs e) { 
            //Obtenemos la fecha actual y caluclamos el precio que hay que devolver al cliente con esta
            DateTime fechaCancelacion = DateTime.Now;
            double precioCancel = _reserva.CalcularPrecioCancelacion(fechaCancelacion);

            //Mostramos un mensaje para convirmar la cancelación

            MessageBoxResult respuesta = MessageBox.Show(
                "La devolución por cancelación serian: " + precioCancel + " €\n" +
                "¿Está seguro de que quieres cancelar la reserva?","Cancelación de reserva", MessageBoxButton.YesNo);

            //Si la respuesta es afriamtiva mandamos la peticion a la api
            if (respuesta == MessageBoxResult.Yes) {

                try
                {
                    var (esOk, respuestaapi) = await ReservationService.cancelReservation(_reserva, precioCancel);
                    //Si la respusta es afirmativa mostramos un mensaje y cerramos la ventan de modificación
                    if (esOk)
                    {
                        MessageBox.Show("Reserva cancleada", "Cancelación realizada!");
                        this.Close();
                    }
                    //En caso contrario mostramos la respuesta de la api
                    else {
                        MessageBox.Show(respuestaapi, "Error de cancelación");
                    }
                }
                catch (Exception ex) { 
                    MessageBox.Show(ex.Message);
                }

                
            } 
        }

        //Cargamos los datos de la reserva a modificar en el formulario
        private void relleanForm() {
            nReserva.Text = nReserva.Text + _reserva.reservation_id;
            dpCheckIn.SelectedDate = _reserva.check_in;
            dpCheckOut.SelectedDate = _reserva.check_out;
            txtUserId.Text = _reserva.user_id;
            txtRoomId.Text = _reserva.room_id;
        }
        private async void Click_Guardar(object sender, RoutedEventArgs e)
        { 
        }
        private async void Click_Cancelar(object sender, RoutedEventArgs e)
        {
        }

        private void dbClick_SelectUser(object sender, MouseButtonEventArgs e)
        {
            SelectedUser selected = new SelectedUser(null);
            selected.Show();
        }

    }
}
