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

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para addReserva.xaml
    /// </summary>
    public partial class addReserva : Window
    {
        //Controlador de la ventana para añadir reservas
        public addReserva()
        {
            InitializeComponent();
        }

        //Funcion para el boton de confirmar reserva
        public async void click_confirmarReserva(object sender, RoutedEventArgs e) {

            //En caso de que algun elemento este vacio mostraremos un mensaje

            if (txtRoomId.Text == "" || txtUserId.Text == "" || dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null ||txtprice.Text == "")
            {
                MessageBox.Show("Es necesario rellenar todos los campos");
            
            }else {
                //Si estan todos los elementos rellenados los enviamos a la api para insertar la reserva
                try
                {
                    double price = double.Parse(txtprice.Text);
                    Reservation r = new Reservation();

                    r.room_id = txtRoomId.Text;
                    r.user_id = txtUserId.Text;
                    r.check_in = dpCheckIn.SelectedDate ?? DateTime.Now;
                    r.check_out = dpCheckOut.SelectedDate ?? DateTime.Now;
                    r.price = price;

                    var (esOk, respuestaapi) = await ReservationService.InsertarReserva(r);

                    //Con la respuesta de la api mostramos un mensaje según esta sea error o correcto
                    if (esOk)
                    {
                        MessageBox.Show("¡Reserva confirmada!", "Nueva reserva creada");
                    }
                    else
                    {
                        MessageBox.Show(respuestaapi, "Error al confriamar");
                    }
                }
                catch (Exception err) {
                    MessageBox.Show(err.Message, "Error al insertar ");
                }

                
            }

           

                
        }
    }
}
