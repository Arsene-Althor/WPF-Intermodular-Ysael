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
    /// Lógica de interacción para listReservas.xaml
    /// </summary>
    public partial class listReservas : Window
    {
        private List<Reservation> _todasLasReservas;
        public listReservas()
        {
            InitializeComponent();
            this.Loaded += Iniciar_Ventana;
        }

        private async void Iniciar_Ventana(object sender, RoutedEventArgs e)
        {
            await Cargar_Reservas();
        }

        private async Task Cargar_Reservas()
        {
            try
            {
                var reservas = await ApiService.getAllReservation();
                if (reservas != null)
                {
                    dgReservas.ItemsSource = reservas;
                    _todasLasReservas = reservas;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la API: {ex.Message}");
            }
        }

        //Pendiente implementar IDcliente y Habitación
        private void FiltrarDatos(object sender, EventArgs e) {
            if (_todasLasReservas == null || slMin == null || slMax == null) return;

            string fId = txtFiltroIdReserva.Text.ToLower().Trim();
            DateTime? fechaDesde = dpDesde.SelectedDate;
            DateTime? fechaHasta = dpHasta.SelectedDate;

            bool verCanceladas = chkCanceladas.IsChecked ?? false;

            double pMin = slMin.Value;
            double pMax = slMax.Value;

            var resultado = _todasLasReservas.Where(r =>
            {
                bool cId = string.IsNullOrEmpty(fId) || r.reservation_id.ToString().ToLower().Contains(fId);
                bool cPrecio = r.price >= pMin && r.price <= pMax;

                bool cEstado;

                if (verCanceladas == true)
                {
                    cEstado = (r.cancelation_date == null);
                }
                else { 
                    cEstado = (r.cancelation_date != null);
                }

                bool cDesde = !fechaDesde.HasValue || r.check_in.Date >= fechaDesde.Value.Date;
                bool cHasta = !fechaHasta.HasValue || r.check_in.Date <= fechaHasta.Value.Date;
                return cId && cPrecio && cEstado && cDesde && cHasta;

            }).ToList();

            dgReservas.ItemsSource = resultado;
        }

        private void Click_LimpiarFiltros(object sender, RoutedEventArgs e) {
            if (_todasLasReservas == null) return;
            if(txtFiltroIdReserva != null) txtFiltroIdReserva.Text = "";
            
            if (dpDesde != null) dpDesde.SelectedDate = null;
            if (dpHasta != null) dpHasta.SelectedDate = null;

            if (slMin != null) slMin.Value = 0;
            if (slMax != null) slMax.Value = 10000;

            if (chkCanceladas != null) chkCanceladas.IsChecked = true;

            FiltrarDatos(null, null);
        }

        private void dbClick_modReserva(object sender, MouseButtonEventArgs e) {
            var resSelect = dgReservas.SelectedItem as Reservation;
            if (resSelect != null) { 
                modReserva modReserva = new modReserva(resSelect);
                modReserva.ShowDialog();
            }
        }
    }
}
