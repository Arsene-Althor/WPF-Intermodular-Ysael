using System;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    public class GestionarDescuentoViewModel : BaseViewModel
    {
        // ==========================================
        // CAMPOS PRIVADOS
        // ==========================================
        private Usuario _usuario;
        private string _nombreUsuario = "";
        private string _tipoTexto = "";
        private string _tipoColor = "Gray";
        private bool _esVIP;
        private bool _vipHabilitado = true;
        private string _vipTexto = "Habilitar Estatus VIP (Hasta 50%)";
        private double _valorDescuento;
        private double _maxDescuento = 30;
        private string _porcentajeTexto = "0%";

        // ==========================================
        // PROPIEDADES PÚBLICAS
        // ==========================================
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set { _nombreUsuario = value; OnPropertyChanged(); }
        }

        public string TipoTexto
        {
            get => _tipoTexto;
            set { _tipoTexto = value; OnPropertyChanged(); }
        }

        public string TipoColor
        {
            get => _tipoColor;
            set { _tipoColor = value; OnPropertyChanged(); }
        }

        public bool EsVIP
        {
            get => _esVIP;
            set
            {
                _esVIP = value;
                OnPropertyChanged();
                ActualizarLimitesVisuales();
            }
        }

        public bool VipHabilitado
        {
            get => _vipHabilitado;
            set { _vipHabilitado = value; OnPropertyChanged(); }
        }

        public string VipTexto
        {
            get => _vipTexto;
            set { _vipTexto = value; OnPropertyChanged(); }
        }

        public double ValorDescuento
        {
            get => _valorDescuento;
            set
            {
                _valorDescuento = value;
                OnPropertyChanged();
                PorcentajeTexto = $"{value:F0}%";
            }
        }

        public double MaxDescuento
        {
            get => _maxDescuento;
            set { _maxDescuento = value; OnPropertyChanged(); }
        }

        public string PorcentajeTexto
        {
            get => _porcentajeTexto;
            set { _porcentajeTexto = value; OnPropertyChanged(); }
        }

        // ==========================================
        // COMMANDS
        // ==========================================
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        // Evento para cerrar la ventana con DialogResult
        public event Action<bool>? RequestClose;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public GestionarDescuentoViewModel(Usuario usuario)
        {
            _usuario = usuario;

            GuardarCommand = new RelayCommand(async () => await ExecuteGuardar());
            CancelarCommand = new RelayCommand(() => RequestClose?.Invoke(false));

            // Solo clientes pueden ser VIP
            if (_usuario.role != "client")
            {
                VipHabilitado = false;
                VipTexto = "Solo Clientes pueden ser VIP";
            }

            CargarDatos();
        }

        // ==========================================
        // MÉTODOS
        // ==========================================
        private void CargarDatos()
        {
            NombreUsuario = $"Usuario: {_usuario.FullName}";
            EsVIP = _usuario.isVIP;
            ValorDescuento = _usuario.Discount * 100;
        }

        private void ActualizarLimitesVisuales()
        {
            if (_esVIP)
            {
                TipoTexto = "⭐ Cliente VIP (Límite: 50%)";
                TipoColor = "Orange";
                MaxDescuento = 50;
            }
            else
            {
                TipoTexto = "👤 Cliente Normal (Límite: 30%)";
                TipoColor = "#7F8C8D";
                MaxDescuento = 30;

                if (ValorDescuento > 30) ValorDescuento = 30;
            }
        }

        private async System.Threading.Tasks.Task ExecuteGuardar()
        {
            try
            {
                _usuario.isVIP = EsVIP;
                _usuario.Discount = ValorDescuento / 100.0;

                await UserService.ModifyUserAsync(_usuario.user_id, _usuario);

                MessageBox.Show("Datos actualizados correctamente.");
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
