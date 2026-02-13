using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;
using System.Windows.Input;
using System.Windows;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ListReservasViewModel : BaseViewModel
    {
        private List<Reservation> _todasLasReservas;
        private List<Reservation> _reservasFiltradas;

        // Propiedades de Filtro
        private string _fId;
        private string _fUser;
        private string _fRoom;
        private DateTime? _fechaDesde;
        private DateTime? _fechaHasta;
        private bool _verCanceladas = true;
        private bool _verVencidas = true;
        private double _pMin = 0;
        private double _pMax = 10000;

        // Getters y Setters con OnPropertyChanged y llamada a Filtrar
        public string FiltroId { get => _fId; set { _fId = value; OnPropertyChanged(); Filtrar(); } }
        public string FiltroUser { get => _fUser; set { _fUser = value; OnPropertyChanged(); Filtrar(); } }
        public string FiltroRoom { get => _fRoom; set { _fRoom = value; OnPropertyChanged(); Filtrar(); } }
        public DateTime? FechaDesde { get => _fechaDesde; set { _fechaDesde = value; OnPropertyChanged(); Filtrar(); } }
        public DateTime? FechaHasta { get => _fechaHasta; set { _fechaHasta = value; OnPropertyChanged(); Filtrar(); } }
        public bool VerCanceladas { get => _verCanceladas; set { _verCanceladas = value; OnPropertyChanged(); Filtrar(); } }
        public bool VerVencidas { get => _verVencidas; set { _verVencidas = value; OnPropertyChanged(); Filtrar(); } }
        public double PrecioMin { get => _pMin; set { _pMin = value; OnPropertyChanged(); Filtrar(); } }
        public double PrecioMax { get => _pMax; set { _pMax = value; OnPropertyChanged(); Filtrar(); } }

        // La lista que el DataGrid va a mostrar
        public List<Reservation> ReservasFiltradas
        {
            get => _reservasFiltradas;
            set { _reservasFiltradas = value; OnPropertyChanged(); }
        }

        // Comandos
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand ModificarReservaCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand SeleccionarRoomCommand { get;  }

        public ListReservasViewModel()
        {
            LimpiarFiltrosCommand = new RelayCommand(ExecuteLimpiarFiltros);
            ModificarReservaCommand = new RelayCommand<Reservation>(async (r) => await ExecuteModificar(r));
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
            SeleccionarRoomCommand = new RelayCommand(ExecuteSeleccionarRoom);

            _ = CargarReservas(); // Carga inicial asíncrona
        }

        private async Task CargarReservas()
        {
            try
            {
                _todasLasReservas = await ReservationService.getAllReservation();
                Filtrar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Filtrar()
        {
            if (_todasLasReservas == null) return;

            DateTime hoy = DateTime.Now;

            ReservasFiltradas = _todasLasReservas.Where(r =>
            {
                bool cId = string.IsNullOrEmpty(FiltroId) || r.reservation_id.ToString().ToLower().Contains(FiltroId.ToLower().Trim());
                bool cUser = string.IsNullOrEmpty(FiltroUser) || r.user_id.ToLower().Contains(FiltroUser.ToLower().Trim());
                bool cRoom = string.IsNullOrEmpty(FiltroRoom) || r.room_id.ToLower().Contains(FiltroRoom.ToLower().Trim());
                bool cPrecio = r.price >= PrecioMin && r.price <= PrecioMax;
                bool cDesde = !FechaDesde.HasValue || r.check_in.Date >= FechaDesde.Value.Date;
                bool cHasta = !FechaHasta.HasValue || r.check_in.Date <= FechaHasta.Value.Date;

                bool vEstado = VerVencidas || r.check_out >= hoy;
                bool cEstado = VerCanceladas || r.cancelation_date == null;

                return cId && cUser && cRoom && cPrecio && cDesde && cHasta && vEstado && cEstado;
            }).ToList();
        }

        private void ExecuteLimpiarFiltros()
        {
            _fId = _fUser = _fRoom = "";
            _fechaDesde = _fechaHasta = null;
            _pMin = 0; _pMax = 10000;
            _verCanceladas = true; _verVencidas = true;

            OnPropertyChanged(string.Empty); // Notifica todas las propiedades
            Filtrar();
        }

        private async Task ExecuteModificar(Reservation res)
        {
            if (res == null) return;

            if (res.cancelation_date == null && res.check_out > DateTime.Now)
            {
                modReserva win = new modReserva(res);
                win.ShowDialog();
                await CargarReservas();
            }
            else
            {
                MessageBox.Show("No es posible modificar una reserva cancelada o vencida");
            }
        }

        private void ExecuteSeleccionarCliente()
        {
            GestionUsuarios selector = new GestionUsuarios();
            selector.Owner = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (selector.ShowDialog() == true && selector.UsuarioSeleccionado != null)
            {
                FiltroUser = selector.UsuarioSeleccionado.user_id;
            }
        }

        private void ExecuteSeleccionarRoom() {
            var win = new listRoom(null, null);

            bool? result = win.ShowDialog();

            if (result == true && win.SelectedRoomResult != null)
            {
                FiltroRoom = win.SelectedRoomResult.RoomId;
            }
        }
    }
}
