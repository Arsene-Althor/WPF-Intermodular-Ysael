using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria.ViewModels
{
    public class AddReservaViewModel : BaseViewModel
    {
        private string _roomId = "HAB-";
        private string _userId = "CLI-";
        private DateTime? _checkIn;
        private DateTime? _checkOut;
        private double _precioReserva;
        private bool _fechasValidas;

        public event EventHandler RequestClose;

        public string RoomId { get => _roomId; set { _roomId = value; OnPropertyChanged(); CalcularPrecio(); } }
        public string UserId { get => _userId; set { _userId = value; OnPropertyChanged(); CalcularPrecio(); } }
        public DateTime? CheckIn { get => _checkIn; set { _checkIn = value; OnPropertyChanged(); ValidarFechas(); } }
        public DateTime? CheckOut { get => _checkOut; set { _checkOut = value; OnPropertyChanged(); ValidarFechas(); } }
        public double PrecioReserva { get => _precioReserva; set { _precioReserva = value; OnPropertyChanged(); } }
        public bool FechasValidas { get => _fechasValidas; set { _fechasValidas = value; OnPropertyChanged(); } }

        public ICommand ConfirmarCommand { get; }
        public ICommand SeleccionarHabitacionCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }

        public AddReservaViewModel()
        {
            ConfirmarCommand = new RelayCommand(async () => await ExecuteConfirmar());
            SeleccionarHabitacionCommand = new RelayCommand(ExecuteSeleccionarHabitacion);
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
        }

        private void ExecuteSeleccionarCliente()
        {
            try
            {
                // Creamos la instancia de la ventana de gestión
                GestionUsuarios selector = new GestionUsuarios();
                selector.Owner = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

                // Mostramos la ventana y esperamos el resultado
                if (selector.ShowDialog() == true)
                {
                    // Recuperamos el usuario seleccionado de la ventana
                    var usuario = selector.UsuarioSeleccionado;

                    if (usuario != null)
                    {
                        if (usuario.role == "client")
                        {
                            // Actualizamos la propiedad del ViewModel
                            // Esto refrescará automáticamente el TextBox en la UI
                            UserId = usuario.user_id;
                        }
                        else
                        {
                            MessageBox.Show("El usuario seleccionado no es un cliente.", "Aviso",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el selector de clientes: {ex.Message}");
            }
        }

        private void ExecuteSeleccionarHabitacion()
        {
            // Pasamos las fechas actuales al constructor de la lista
            var win = new listRoom(CheckIn, CheckOut);

            // Si usas Owner, asegúrate de pasárselo correctamente
            if (win.ShowDialog() == true)
            {
                // IMPORTANTE: Verifica que SelectedRoomResult no sea nulo 
                // y que no estés accediendo a un array vacío dentro de listRoom
                if (win.SelectedRoomResult != null)
                {
                    RoomId = win.SelectedRoomResult.RoomId;
                }
            }
        }

        private async void ValidarFechas()
        {
            if (CheckIn == null || CheckOut == null) {
                FechasValidas = false;
                return;
            } 

            DateTime ayeronce = DateTime.Today.AddDays(-1).AddHours(11);
            if (CheckIn >= CheckOut || CheckIn < ayeronce)
            {
                MessageBox.Show("La fecha no es válida");
                FechasValidas = false;
                ResetFormulario();
            }
            else
            {
                FechasValidas = true;
                await CalcularPrecio();
            }
        }

        private async Task CalcularPrecio()
        {
            if (UserId != "CLI-" && RoomId != "HAB-" && CheckIn.HasValue && CheckOut.HasValue)
            {
                var (esOk, respuesta, precio) = await ReservationService.getPriceReservation(UserId, RoomId, CheckIn, CheckOut);
                if (esOk) PrecioReserva = precio;
            }
        }

        private async Task ExecuteConfirmar()
        {
            //En caso de que algun elemento este vacio mostraremos un mensaje

            if (_roomId == "" || _userId == "" || _checkIn == null || _checkOut == null)
            {
                MessageBox.Show("Es necesario rellenar todos los campos");

            }
            else
            {
                //Si estan todos los elementos rellenados los enviamos a la api para insertar la reserva
                try
                {
                    Reservation r = new Reservation();

                    r.room_id = _roomId;
                    r.user_id = _userId;
                    r.check_in = _checkIn ?? DateTime.Now;
                    r.check_out = _checkOut ?? DateTime.Now;
                    r.price = _precioReserva;
                    r.createdBy = Session.User.user_id;

                    var (esOk, respuestaapi) = await ReservationService.InsertarReserva(r);

                    //Con la respuesta de la api mostramos un mensaje según esta sea error o correcto
                    if (esOk)
                    {
                        MessageBox.Show("¡Reserva confirmada!", "Nueva reserva creada");
                        RequestClose?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show(respuestaapi, "Error al confriamar");
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error al insertar ");
                }
            }
        }

        private void ResetFormulario()
        {
            _checkIn = null; _checkOut = null;
            _userId = "CLI-"; _roomId = "HAB-";
            _precioReserva = 0;
            OnPropertyChanged(string.Empty);
        }

    }
}
