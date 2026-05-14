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
using System.Windows.Media;

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
        public ICommand NuevaReservaCommand { get; }
        public ICommand ModificarReservaCommand { get; }
        public ICommand AbrirAuditoriaReservaCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand SeleccionarRoomCommand { get;  }

        private static Window? ShellOwner() => Hotel_Pere_Maria.UiShell.OwnerWindow;

        public ListReservasViewModel()
        {
            LimpiarFiltrosCommand = new RelayCommand(ExecuteLimpiarFiltros);
            NuevaReservaCommand = new RelayCommand(async () => await ExecuteNuevaReservaAsync());
            ModificarReservaCommand = new RelayCommand<Reservation>(async (r) => await ExecuteModificar(r));
            AbrirAuditoriaReservaCommand = new RelayCommand<Reservation>(ExecuteAbrirAuditoria);
            SeleccionarClienteCommand = new RelayCommand(ExecuteSeleccionarCliente);
            SeleccionarRoomCommand = new RelayCommand(ExecuteSeleccionarRoom);

            _ = CargarReservas(); // Carga inicial asíncrona
        }

        private async Task ExecuteNuevaReservaAsync()
        {
            var uc = new addReserva();
            var shell = new Window
            {
                Title = "Nueva reserva",
                Content = uc,
                Width = 520,
                Height = 700,
                Owner = ShellOwner(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Background = Application.Current.TryFindResource("Brush.PageBg") as Brush ?? Brushes.AliceBlue
            };
            if (uc.DataContext is AddReservaViewModel vm)
            {
                void OnOk(object? s, EventArgs e)
                {
                    vm.RequestClose -= OnOk;
                    vm.DialogCanceled -= OnCancel;
                    shell.DialogResult = true;
                    shell.Close();
                }
                void OnCancel(object? s, EventArgs e)
                {
                    vm.RequestClose -= OnOk;
                    vm.DialogCanceled -= OnCancel;
                    shell.Close();
                }
                vm.RequestClose += OnOk;
                vm.DialogCanceled += OnCancel;
            }
            shell.ShowDialog();
            await CargarReservas();
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
                win.Owner = ShellOwner();
                win.ShowDialog();
                await CargarReservas();
            }
            else
            {
                MessageBox.Show("No es posible modificar una reserva cancelada o vencida");
            }
        }

        private void ExecuteAbrirAuditoria(Reservation res)
        {
            if (res == null) return;
            var win = new AuditoriaReserva(res);
            win.Owner = ShellOwner();
            win.ShowDialog();
        }

        private void ExecuteSeleccionarCliente()
        {
            try
            {
                var usuario = GestionUsuarios.ShowPickerDialog();
                if (usuario != null)
                {
                    if (usuario.role == "client")
                        FiltroUser = usuario.user_id;
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

        private void ExecuteSeleccionarRoom() {
            if (!listRoom.TryPickRoom(null, null, out var picked) || picked == null)
                return;
            FiltroRoom = picked.RoomId;
        }
    }
}
