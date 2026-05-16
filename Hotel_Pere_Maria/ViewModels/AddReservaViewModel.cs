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

        public event EventHandler? RequestClose;
        public event EventHandler? DialogCanceled;

        public string RoomId { get => _roomId; set { _roomId = value; OnPropertyChanged(); _ = CalcularPrecio(); } }
        public string UserId { get => _userId; set { _userId = value; OnPropertyChanged(); _ = CalcularPrecio(); } }
        public DateTime? CheckIn { get => _checkIn; set { _checkIn = value; OnPropertyChanged(); ValidarFechas(); } }
        public DateTime? CheckOut { get => _checkOut; set { _checkOut = value; OnPropertyChanged(); ValidarFechas(); } }
        public double PrecioReserva { get => _precioReserva; set { _precioReserva = value; OnPropertyChanged(); } }
        public bool FechasValidas { get => _fechasValidas; set { _fechasValidas = value; OnPropertyChanged(); } }

        public ICommand ConfirmarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand SeleccionarHabitacionCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }

        public AddReservaViewModel()
        {
            ConfirmarCommand = new RelayCommand(async () => await ExecuteConfirmar());
            CancelarCommand = new RelayCommand(() => DialogCanceled?.Invoke(this, EventArgs.Empty));
            SeleccionarHabitacionCommand = new RelayCommand(ExecuteSeleccionarHabitacion);
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
        }

        private void ExecuteSeleccionarCliente()
        {
            try
            {
                var usuario = GestionUsuarios.ShowPickerDialog();
                if (usuario != null)
                {
                    if (usuario.role == "client")
                        UserId = usuario.user_id;
                    else
                        MessageBox.Show("El usuario seleccionado no es un cliente.", "Aviso",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el selector de clientes: {ex.Message}");
            }
        }

        private void ExecuteSeleccionarHabitacion()
        {
            if (!listRoom.TryPickRoom(CheckIn, CheckOut, out var picked) || picked == null)
                return;
            RoomId = picked.RoomId;
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
