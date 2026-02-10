using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System.Windows.Input;
using System.Windows;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ModReservaViewModel : BaseViewModel
    {
        private Reservation _reservaOriginal;
        private string _userId;
        private string _roomId;
        private DateTime? _checkIn;
        private DateTime? _checkOut;
        private double _precioNuevo;
        private bool _checkInEnabled = true;

        public event EventHandler RequestClose;

        // Propiedades
        public string ReservationId => _reservaOriginal.reservation_id.ToString();
        public double PrecioOriginal => _reservaOriginal.price;

        public string UserId { get => _userId; set { _userId = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public string RoomId { get => _roomId; set { _roomId = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public DateTime? CheckIn { get => _checkIn; set { _checkIn = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public DateTime? CheckOut { get => _checkOut; set { _checkOut = value; OnPropertyChanged(); RecalcularPrecio(); } }
        public double PrecioNuevo { get => _precioNuevo; set { _precioNuevo = value; OnPropertyChanged(); } }
        public bool CheckInEnabled { get => _checkInEnabled; set { _checkInEnabled = value; OnPropertyChanged(); } }

        // Comandos
        public ICommand GuardarCommand { get; }
        public ICommand CancelarReservaCommand { get; }
        public ICommand CerrarCommand { get; }
        public ICommand SeleccionarHabitacionCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }

        public ModReservaViewModel(Reservation reserva)
        {
            _reservaOriginal = reserva;

            // Rellenar formulario (Mapeo)
            _userId = reserva.user_id;
            _roomId = reserva.room_id;
            _checkIn = reserva.check_in;
            _checkOut = reserva.check_out;
            _precioNuevo = reserva.price;
            _checkInEnabled = reserva.check_in > DateTime.Now;

            GuardarCommand = new RelayCommand(async () => await ExecuteGuardar());
            CancelarReservaCommand = new RelayCommand(async () => await ExecuteCancelarReserva());
            CerrarCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));
            SeleccionarHabitacionCommand = new RelayCommand(ExecuteSeleccionarHabitacion);
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
        }

        private async void RecalcularPrecio()
        {
            if (CheckIn == null || CheckOut == null) return;

            // Validaciones básicas
            if (CheckOut < DateTime.Today) return;
            if (CheckIn >= CheckOut) return;

            // Solo llamamos a la API si algo ha cambiado
            if (CheckIn != _reservaOriginal.check_in || CheckOut != _reservaOriginal.check_out ||
                UserId != _reservaOriginal.user_id || RoomId != _reservaOriginal.room_id)
            {
                var (esOk, _, precio) = await ReservationService.getPriceReservation(UserId, RoomId, CheckIn, CheckOut);
                if (esOk) PrecioNuevo = precio;
            }
        }

        private async Task ExecuteGuardar()
        {
            try
            {
                var mod = new Reservation
                {
                    reservation_id = _reservaOriginal.reservation_id,
                    user_id = UserId,
                    room_id = RoomId,
                    check_in = CheckIn ?? DateTime.Now,
                    check_out = CheckOut ?? DateTime.Now,
                    price = PrecioNuevo
                };

                var (esOk, error) = await ReservationService.updateReservation(mod);
                if (esOk)
                {
                    double dif = PrecioNuevo - PrecioOriginal;
                    string msg = dif > 0 ? $"\nCargo extra de {dif}€" : (dif < 0 ? $"\nDevolución de {-dif}€" : " sin cargos");
                    MessageBox.Show("Modificada" + msg);
                    RequestClose?.Invoke(this, EventArgs.Empty);
                }
                else MessageBox.Show(error);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async Task ExecuteCancelarReserva()
        {
            var (esOk, respuesta, precioCancelacion) = await ReservationService.getCancelationPrice(_reservaOriginal.reservation_id, DateTime.Now);
            if (!esOk) {
                MessageBox.Show(respuesta, "No es posible cancelar la reserva");
                return;
            }

            double devolucion = PrecioOriginal - precioCancelacion;
            var result = MessageBox.Show($"Devolución: {devolucion}€\n¿Confirmar?", "Cancelar", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var (ok, error) = await ReservationService.cancelReservation(_reservaOriginal, precioCancelacion);
                if (ok) RequestClose?.Invoke(this, EventArgs.Empty);
                else MessageBox.Show(error);
            }
        }

        private void ExecuteSeleccionarHabitacion() {
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
        private void ExecuteSeleccionarCliente() {
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
    }
}
