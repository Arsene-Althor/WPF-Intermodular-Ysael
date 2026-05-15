using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    public class CheckInRecepcionViewModel : BaseViewModel
    {
        private readonly string _reservationId;
        private bool _cargando = true;
        private string _mensajeEstado = "";
        private string _guestName = "—";
        private string _guestDni = "—";
        private string _guestEmail = "";
        private string _roomId = "";
        private string _fechasTexto = "";
        private double _precio;
        private string _ventanaHoraria = "";
        private bool _canRegisterNormal;
        private bool _canRegisterLate;
        private bool _alreadyCheckedIn;
        private bool _blocked;
        private double _lateFee;
        private DateTime? _checkedInAt;
        private bool _checkedInLate;

        public event EventHandler<bool>? RequestClose;

        public CheckInRecepcionViewModel(string reservationId)
        {
            _reservationId = reservationId;
            RegistrarCommand = new RelayCommand(async () => await RegistrarAsync(false), () => CanRegisterNormal && !_cargando);
            RegistrarTardeCommand = new RelayCommand(async () => await RegistrarAsync(true), () => CanRegisterLate && !_cargando);
            CerrarCommand = new RelayCommand(() => RequestClose?.Invoke(this, false));
            _ = CargarAsync();
        }

        public string Titulo => $"Check-in recepción — {_reservationId}";

        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoCargando)); }
        }

        public bool NoCargando => !Cargando;

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        public string GuestName
        {
            get => _guestName;
            set { _guestName = value; OnPropertyChanged(); }
        }

        public string GuestDni
        {
            get => _guestDni;
            set { _guestDni = value; OnPropertyChanged(); }
        }

        public string GuestEmail
        {
            get => _guestEmail;
            set { _guestEmail = value; OnPropertyChanged(); OnPropertyChanged(nameof(MostrarEmail)); }
        }

        public bool MostrarEmail => !string.IsNullOrWhiteSpace(GuestEmail);

        public string RoomId
        {
            get => _roomId;
            set { _roomId = value; OnPropertyChanged(); }
        }

        public string FechasTexto
        {
            get => _fechasTexto;
            set { _fechasTexto = value; OnPropertyChanged(); }
        }

        public string PrecioTexto
        {
            get => $"{_precio:F2} €";
        }

        public string VentanaHoraria
        {
            get => _ventanaHoraria;
            set { _ventanaHoraria = value; OnPropertyChanged(); OnPropertyChanged(nameof(MostrarVentana)); }
        }

        public bool MostrarVentana => !string.IsNullOrWhiteSpace(VentanaHoraria);

        public bool CanRegisterNormal
        {
            get => _canRegisterNormal;
            set { _canRegisterNormal = value; OnPropertyChanged(); }
        }

        public bool CanRegisterLate
        {
            get => _canRegisterLate;
            set { _canRegisterLate = value; OnPropertyChanged(); }
        }

        public bool AlreadyCheckedIn
        {
            get => _alreadyCheckedIn;
            set { _alreadyCheckedIn = value; OnPropertyChanged(); }
        }

        public bool Blocked
        {
            get => _blocked;
            set { _blocked = value; OnPropertyChanged(); }
        }

        public string LateFeeTexto => $"{_lateFee:F2} €";

        public string CheckedInAtTexto =>
            _checkedInAt.HasValue ? _checkedInAt.Value.ToString("dd/MM/yyyy HH:mm") : "—";

        public string CheckedInLateTexto =>
            _checkedInLate ? $"Sí (recargo {_lateFee:F2} €)" : "No";

        public ICommand RegistrarCommand { get; }
        public ICommand RegistrarTardeCommand { get; }
        public ICommand CerrarCommand { get; }

        private async Task CargarAsync()
        {
            Cargando = true;
            try
            {
                var (ok, err, dto) = await ReservationService.GetReceptionCheckInStatusAsync(_reservationId);
                if (!ok || dto == null)
                {
                    MensajeEstado = string.IsNullOrWhiteSpace(err) ? "No se pudo cargar el estado." : err;
                    Blocked = true;
                    return;
                }

                GuestName = string.IsNullOrWhiteSpace(dto.GuestName) ? "—" : dto.GuestName;
                GuestDni = string.IsNullOrWhiteSpace(dto.GuestDni) ? "—" : dto.GuestDni;
                GuestEmail = dto.GuestEmail ?? "";
                RoomId = dto.RoomId ?? "";
                _precio = dto.Price;
                OnPropertyChanged(nameof(PrecioTexto));
                FechasTexto =
                    $"Entrada: {dto.CheckIn:dd/MM/yyyy HH:mm}  ·  Salida: {dto.CheckOut:dd/MM/yyyy HH:mm}";
                MensajeEstado = dto.Message ?? "";

                if (dto.WindowStart.HasValue && dto.WindowEnd.HasValue)
                {
                    VentanaHoraria =
                        $"Horario estándar recepción: {dto.WindowStart.Value:dd/MM HH:mm} — {dto.WindowEnd.Value:dd/MM HH:mm}";
                }

                _lateFee = dto.LateFee;
                OnPropertyChanged(nameof(LateFeeTexto));

                AlreadyCheckedIn = dto.Status == "already";
                if (AlreadyCheckedIn)
                {
                    _checkedInAt = dto.ReceptionCheckInAt?.ToLocalTime();
                    _checkedInLate = dto.ReceptionCheckInLate;
                    _lateFee = dto.ReceptionCheckInLateFee;
                    OnPropertyChanged(nameof(CheckedInAtTexto));
                    OnPropertyChanged(nameof(CheckedInLateTexto));
                    OnPropertyChanged(nameof(LateFeeTexto));
                    Blocked = false;
                    CanRegisterNormal = false;
                    CanRegisterLate = false;
                    return;
                }

                Blocked = dto.Status is "too_early" or "expired" or "cancelled";
                CanRegisterNormal = dto.Status == "normal";
                CanRegisterLate = dto.Status == "late" && dto.RequiresLateConfirmation;
            }
            finally
            {
                Cargando = false;
            }
        }

        private async Task RegistrarAsync(bool acceptLate)
        {
            Cargando = true;
            try
            {
                var (ok, err, _) = await ReservationService.PostReceptionCheckInAsync(_reservationId, acceptLate);
                if (!ok)
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(err) ? "Error al registrar check-in." : err,
                        "Check-in",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    acceptLate ? "Check-in tardío registrado (recargo aplicado al precio)." : "Check-in registrado correctamente.",
                    "Check-in",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                RequestClose?.Invoke(this, true);
            }
            finally
            {
                Cargando = false;
            }
        }
    }
}
