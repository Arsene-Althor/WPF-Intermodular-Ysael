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
    //Ventana para mostrar una lista con todas las reservas
    public partial class listReservas : Window
    {
        //Tenemos una lista con todas las reservas para evitar peticiones a la api al utilizar filtros
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

        //Cargamos las reservas del mismo modo que en el inicio y guardamos la lista recibida en _todasLasReservas
        private async Task Cargar_Reservas()
        {
            try
            {
                var reservas = await ReservationService.getAllReservation();
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

        //Utilizamos este metodo para filtrar los datos de la lista en tiempo real
        private void FiltrarDatos(object sender, EventArgs e) {
            if (_todasLasReservas == null || slMin == null || slMax == null) return;

            string fId = txtFiltroIdReserva.Text.ToLower().Trim();
            string fuser_id = txtFiltroUser.Text.ToLower().Trim();
            string froom_id = txtFiltroRoom.Text.ToLower().Trim();
            DateTime? fechaDesde = dpDesde.SelectedDate;
            DateTime? fechaHasta = dpHasta.SelectedDate;

            bool verCanceladas = chkCanceladas.IsChecked ?? false;
            bool verVencidas = chkVencidas.IsChecked ?? false;

            double pMin = slMin.Value;
            double pMax = slMax.Value;

            var resultado = _todasLasReservas.Where(r =>
            {
                bool cId = string.IsNullOrEmpty(fId) || r.reservation_id.ToString().ToLower().Contains(fId);
                bool cuser_id = string.IsNullOrEmpty(fuser_id) || r.user_id.ToString().ToLower().Contains(fuser_id);
                bool croom_id = string.IsNullOrEmpty(froom_id) || r.room_id.ToString().ToLower().Contains(froom_id);
                bool cPrecio = r.price >= pMin && r.price <= pMax;

                bool cEstado;
                bool vEstado;
                DateTime hoy = DateTime.Now;

                if (verVencidas == true)
                {
                    vEstado = true;
                }
                else { 
                    vEstado = (r.check_out >= hoy);
                }

                if (verCanceladas == true)
                {
                    cEstado = (r.cancelation_date == null || r.cancelation_date != null);
                }
                else
                {
                    cEstado = (r.cancelation_date == null);
                }

                bool cDesde = !fechaDesde.HasValue || r.check_in.Date >= fechaDesde.Value.Date;
                bool cHasta = !fechaHasta.HasValue || r.check_in.Date <= fechaHasta.Value.Date;
                return cId && cPrecio && cEstado && cDesde && cHasta && vEstado && cuser_id && croom_id;

            }).ToList();

            dgReservas.ItemsSource = resultado;
        }

        //Utilizamos este metodo para limpiar todos los filtros
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

        //Al pulsar doble click en una reserva de la lista abrimos una nueva ventana para modificar esta

        private async void dbClick_modReserva(object sender, MouseButtonEventArgs e) {
            var resSelect = dgReservas.SelectedItem as Reservation;
            if (resSelect != null)
            {
                DateTime ahora = DateTime.Now;
                if (resSelect.cancelation_date == null && resSelect.check_out > ahora)
                {
                    modReserva modReserva = new modReserva(resSelect);
                    modReserva.ShowDialog();
                    await Cargar_Reservas();
                }
                else {
                    MessageBox.Show("No es posibile modificar una reserva cancelada o vencida");
                }
            }
        }
        private void dbClick_selectUser(object sender, MouseButtonEventArgs e)
        {
            try
            {
                GestionUsuarios selector = new GestionUsuarios();
                selector.Owner = this; //Permite usar el selector

                if (selector.ShowDialog() == true)
                {
                    Usuario usuario = selector.UsuarioSeleccionado;
                    if (usuario != null)
                    {
                        // Aquí actualizamos el campo de filtro
                        txtFiltroUser.Text = usuario.user_id;

                        //Forzar el refresco de la lista automáticamente
                        FiltrarDatos(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
