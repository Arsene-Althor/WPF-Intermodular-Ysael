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
        private double precioReserva = 0;
        //Controlador de la ventana para añadir reservas
        public addReserva()
        {
            InitializeComponent();
        }

        public void Select_Room(object sender, MouseButtonEventArgs e)
        {
            DateTime checkIn = dpCheckIn.SelectedDate.Value;
            DateTime checkOut = dpCheckOut.SelectedDate.Value;

            var win = new listRoom(checkIn, checkOut);

            if (win.ShowDialog() == true && win.SelectedRoomResult != null)
            {
                var room = win.SelectedRoomResult;
                txtRoomId.Text = room.RoomId;
            }
        }
        public void Select_Client(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Creamos la instancia de la ventana de gestión
                GestionUsuarios selector = new GestionUsuarios();

                //Indicamos que addReservas es el Owner
                //Esto activa la logica para recoger el usuario, la logica esta implementada en GestionUsuarios, dgUsuarios_MouseDoubleClick
                selector.Owner = this;

                //Mostramos la ventana y esperamos
                if (selector.ShowDialog() == true)
                {
                    //Recuperamos el usuario seleccionado
                    Usuario usuario = selector.UsuarioSeleccionado;

                    if (usuario != null)
                    {
                        //Validar si es cliente, como capa de seguridad extra
                        if (usuario.role == "client")
                        {
                            txtUserId.Text = usuario.user_id;
                        }
                        else
                        {
                            MessageBox.Show("El usuario seleccionado no es un cliente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir selector: {ex.Message}");
            }
        }
            
            

        public async void fecha_select(object sender, EventArgs e) {
            if (dpCheckIn.SelectedDate != null && dpCheckOut.SelectedDate != null) {

                DateTime ayeronce = DateTime.Today.AddDays(-1).AddHours(11);
                
                if (dpCheckIn.SelectedDate >= dpCheckOut.SelectedDate || dpCheckIn.SelectedDate < ayeronce)
                {
                    MessageBox.Show("La fecha no es valida");
                    txtRoomId.IsEnabled = false;
                    txtUserId.IsEnabled = false;
                    dpCheckIn.SelectedDate = null;
                    dpCheckOut.SelectedDate = null;
                    txtUserId.Text = "CLI-";
                    txtRoomId.Text = "HAB-";
                    lblPrecio.Text = "0 €";
                    return;
                }
                else { 
                    txtRoomId.IsEnabled=true;
                    txtUserId.IsEnabled=true;
                    if (txtUserId.Text != "CLI-" && txtRoomId.Text != "HAB-") {
                        var (esOk, respuesta, precio) = await ReservationService.getPriceReservation(txtUserId.Text, txtRoomId.Text, dpCheckIn.SelectedDate, dpCheckOut.SelectedDate);

                        if (esOk)
                        {
                            precioReserva = precio;
                            lblPrecio.Text = precioReserva + " €";
                        }
                        
                    }
                }
            }

        }

        //Funcion para el boton de confirmar reserva
        public async void click_confirmarReserva(object sender, RoutedEventArgs e) {

            //En caso de que algun elemento este vacio mostraremos un mensaje

            if (txtRoomId.Text == "" || txtUserId.Text == "" || dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Es necesario rellenar todos los campos");
            
            }else {
                //Si estan todos los elementos rellenados los enviamos a la api para insertar la reserva
                try
                {
                    Reservation r = new Reservation();

                    r.room_id = txtRoomId.Text;
                    r.user_id = txtUserId.Text;
                    r.check_in = dpCheckIn.SelectedDate ?? DateTime.Now;
                    r.check_out = dpCheckOut.SelectedDate ?? DateTime.Now;
                    r.price = precioReserva;
                    r.createdBy = Session.User.user_id;

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
