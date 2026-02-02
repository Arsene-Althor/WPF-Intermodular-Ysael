using System;
using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Views
{
    public partial class GestionarDescuento : Window
    {
        private Usuario _usuario;

        public GestionarDescuento(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;

            // SEGURIDAD: Solo permitimos VIP a Clientes (tu requisito)
            if (_usuario.role != "client")
            {
                chkVIP.IsEnabled = false;
                chkVIP.Content = "Solo Clientes pueden ser VIP";
                chkVIP.Foreground = System.Windows.Media.Brushes.Gray;
            }

            CargarDatos();
        }

        private void CargarDatos()
        {
            lblUsuario.Text = $"Usuario: {_usuario.FullName}";

            // 1. Cargar estado VIP actual
            chkVIP.IsChecked = _usuario.isVIP;

            // 2. Configurar Slider
            ActualizarLimitesVisuales();

            // 3. Poner valor actual
            sliderDescuento.Value = _usuario.Discount * 100;
            lblPorcentaje.Text = $"{sliderDescuento.Value:F0}%";
        }

        // Evento que salta al marcar/desmarcar el CheckBox
        private void ChkVIP_CheckChanged(object sender, RoutedEventArgs e)
        {
            ActualizarLimitesVisuales();
        }

        private void ActualizarLimitesVisuales()
        {
            bool esVip = chkVIP.IsChecked == true;

            if (esVip)
            {
                lblTipo.Text = "⭐ Cliente VIP (Límite: 50%)";
                lblTipo.Foreground = System.Windows.Media.Brushes.Orange;
                sliderDescuento.Maximum = 50;
            }
            else
            {
                lblTipo.Text = "👤 Cliente Normal (Límite: 30%)";
                sliderDescuento.Maximum = 30;

                // Si tenía más de 30 y le quitamos el VIP, bajamos el valor automáticamente
                if (sliderDescuento.Value > 30) sliderDescuento.Value = 30;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblPorcentaje != null)
            {
                lblPorcentaje.Text = $"{sliderDescuento.Value:F0}%";
            }
        }

        private async void Click_Guardar(object sender, RoutedEventArgs e)
        {
            try
            {

                // Aplicamos los cambios al usuario existente en memoria
                _usuario.isVIP = chkVIP.IsChecked == true;
                _usuario.Discount = sliderDescuento.Value / 100.0;

                // Enviamos el usuario COMPLETO a la API
                await UserService.ModifyUserAsync(_usuario.user_id, _usuario);

                MessageBox.Show("Datos actualizados correctamente.");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Click_Cancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}