using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ConfigFacturaViewModel : BaseViewModel
    {
        private string _nombreComercial = "";
        private string _cif = "";
        private string _direccion = "";
        private string _otrosFiscales = "";
        private string _ivaPorcentajeTexto = "10";
        private bool _cargando;

        public string NombreComercial
        {
            get => _nombreComercial;
            set { _nombreComercial = value ?? ""; OnPropertyChanged(); }
        }

        public string Cif
        {
            get => _cif;
            set { _cif = value ?? ""; OnPropertyChanged(); }
        }

        public string Direccion
        {
            get => _direccion;
            set { _direccion = value ?? ""; OnPropertyChanged(); }
        }

        public string OtrosFiscales
        {
            get => _otrosFiscales;
            set { _otrosFiscales = value ?? ""; OnPropertyChanged(); }
        }

        /// <summary>IVA en % para la UI (ej. 10 = 10%).</summary>
        public string IvaPorcentajeTexto
        {
            get => _ivaPorcentajeTexto;
            set { _ivaPorcentajeTexto = value ?? ""; OnPropertyChanged(); }
        }

        public bool Cargando
        {
            get => _cargando;
            set
            {
                _cargando = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand CargarCommand { get; }
        public ICommand GuardarCommand { get; }

        public ConfigFacturaViewModel()
        {
            CargarCommand = new RelayCommand(() => _ = CargarAsync(), () => !Cargando);
            GuardarCommand = new RelayCommand(() => _ = GuardarAsync(), () => !Cargando);
            _ = CargarAsync();
        }

        private async Task CargarAsync()
        {
            Cargando = true;
            try
            {
                var (ok, msg, dto) = await InvoiceSettingsService.GetAsync();
                if (!ok || dto == null)
                {
                    MessageBox.Show(msg ?? "Error al cargar", "Configuración factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                NombreComercial = dto.hotel_commercial_name ?? "";
                Cif = dto.hotel_cif ?? "";
                Direccion = dto.hotel_address ?? "";
                OtrosFiscales = dto.fiscal_notes ?? "";
                IvaPorcentajeTexto = (dto.iva_rate * 100.0).ToString("0.##", CultureInfo.CurrentCulture);
            }
            finally
            {
                Cargando = false;
            }
        }

        private async Task GuardarAsync()
        {
            if (!double.TryParse(IvaPorcentajeTexto.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
            {
                MessageBox.Show("IVA % no válido (ej. 10 o 21).", "Configuración factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (pct < 0 || pct > 99)
            {
                MessageBox.Show("IVA % debe estar entre 0 y 99.", "Configuración factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Cargando = true;
            try
            {
                var payload = new InvoiceSettingsDto
                {
                    hotel_commercial_name = NombreComercial.Trim(),
                    hotel_cif = Cif.Trim(),
                    hotel_address = Direccion.Trim(),
                    fiscal_notes = OtrosFiscales.Trim(),
                    iva_rate = Math.Round(pct / 100.0, 4)
                };
                var (ok, msg, dto) = await InvoiceSettingsService.PutAsync(payload);
                if (!ok || dto == null)
                {
                    MessageBox.Show(msg ?? "Error al guardar", "Configuración factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show("Datos de factura guardados. Los nuevos PDF usarán esta información.", "Configuración factura", MessageBoxButton.OK, MessageBoxImage.Information);
                NombreComercial = dto.hotel_commercial_name ?? "";
                Cif = dto.hotel_cif ?? "";
                Direccion = dto.hotel_address ?? "";
                OtrosFiscales = dto.fiscal_notes ?? "";
                IvaPorcentajeTexto = (dto.iva_rate * 100.0).ToString("0.##", CultureInfo.CurrentCulture);
            }
            finally
            {
                Cargando = false;
            }
        }
    }
}
