using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;
using Microsoft.Win32;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ListFacturasViewModel : BaseViewModel
    {
        private List<HotelInvoiceItem> _todas = new();
        private string _filtroFactura = "";
        private string _filtroCliente = "";
        private DateTime? _emitidaDesde;
        private DateTime? _emitidaHasta;

        public ObservableCollection<HotelInvoiceItem> FacturasFiltradas { get; } = new();

        public string FiltroFactura
        {
            get => _filtroFactura;
            set { _filtroFactura = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        public string FiltroCliente
        {
            get => _filtroCliente;
            set { _filtroCliente = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        public DateTime? EmitidaDesde
        {
            get => _emitidaDesde;
            set { _emitidaDesde = value; OnPropertyChanged(); Filtrar(); }
        }

        public DateTime? EmitidaHasta
        {
            get => _emitidaHasta;
            set { _emitidaHasta = value; OnPropertyChanged(); Filtrar(); }
        }

        public ICommand RefrescarCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand DescargarPdfCommand { get; }
        public ICommand ReenviarEmailCommand { get; }
        public ICommand SeleccionarClienteFiltroCommand { get; }

        public ListFacturasViewModel()
        {
            RefrescarCommand = new RelayCommand(() => _ = CargarAsync());
            LimpiarFiltrosCommand = new RelayCommand(() =>
            {
                FiltroFactura = "";
                FiltroCliente = "";
                EmitidaDesde = null;
                EmitidaHasta = null;
            });
            DescargarPdfCommand = new RelayCommand<HotelInvoiceItem>(async r => await DescargarPdfAsync(r));
            ReenviarEmailCommand = new RelayCommand<HotelInvoiceItem>(async r => await ReenviarEmailAsync(r));
            SeleccionarClienteFiltroCommand = new RelayCommand(ExecutePickCliente);
            _ = CargarAsync();
        }

        private void ExecutePickCliente()
        {
            try
            {
                var u = GestionUsuarios.ShowPickerDialog();
                if (u != null && u.role == "client")
                    FiltroCliente = u.user_id;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cliente", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task CargarAsync()
        {
            try
            {
                var (ok, err, lista) = await ReservationService.GetInvoicesHistoryAsync();
                if (!ok)
                {
                    MessageBox.Show(string.IsNullOrWhiteSpace(err) ? "No se pudo cargar el listado" : err,
                        "Facturas", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _todas = lista ?? new List<HotelInvoiceItem>();
                Filtrar();
                if (_todas.Count == 0)
                {
                    MessageBox.Show(
                        "No hay facturas en el servidor.\n\n" +
                        "• Reinicia la API (cambios de facturación).\n" +
                        "• Crea una reserva y paga desde la app móvil.\n" +
                        "• O haz checkout en recepción (reservas antiguas se importan al pulsar Actualizar).",
                        "Facturas",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Facturas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filtrar()
        {
            FacturasFiltradas.Clear();
            string fInv = (FiltroFactura ?? "").Trim();
            string fCli = (FiltroCliente ?? "").Trim();
            foreach (var r in _todas.OrderByDescending(x => x.issued_at ?? DateTime.MinValue))
            {
                if (!string.IsNullOrEmpty(fInv) &&
                    (r.invoice_number == null || r.invoice_number.IndexOf(fInv, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;
                if (!string.IsNullOrEmpty(fCli) &&
                    (r.user_id == null || r.user_id.IndexOf(fCli, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;
                var em = r.issued_at;
                if (EmitidaDesde.HasValue && (!em.HasValue || em.Value.Date < EmitidaDesde.Value.Date))
                    continue;
                if (EmitidaHasta.HasValue && (!em.HasValue || em.Value.Date > EmitidaHasta.Value.Date))
                    continue;
                FacturasFiltradas.Add(r);
            }
        }

        private static async Task DescargarPdfAsync(HotelInvoiceItem? r)
        {
            if (r == null || string.IsNullOrWhiteSpace(r.reservation_id)) return;
            var (ok, err, pdf) = await ReservationService.DownloadInvoicePdfAsync(r.reservation_id, r.invoice_number);
            if (!ok || pdf == null || pdf.Length == 0)
            {
                MessageBox.Show(err ?? "Sin PDF", "Descarga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = $"Factura-{r.invoice_number ?? r.reservation_id}.pdf",
                Filter = "PDF (*.pdf)|*.pdf",
                DefaultExt = ".pdf"
            };
            if (dlg.ShowDialog() == true)
            {
                await System.IO.File.WriteAllBytesAsync(dlg.FileName, pdf);
                MessageBox.Show($"Guardado:\n{dlg.FileName}", "Factura", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static async Task ReenviarEmailAsync(HotelInvoiceItem? r)
        {
            if (r == null || string.IsNullOrWhiteSpace(r.reservation_id)) return;
            var res = MessageBox.Show(
                $"¿Reenviar factura {r.invoice_number} al email del cliente?",
                "Correo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            var (ok, msg) = await ReservationService.PostInvoiceEmailAsync(r.reservation_id, null, r.invoice_number);
            if (ok)
                MessageBox.Show("Solicitud enviada al servidor.", "Correo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(msg, "Error al enviar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
