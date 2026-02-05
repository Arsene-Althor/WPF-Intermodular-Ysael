using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para modRoom.xaml
    /// </summary>
    public partial class modRoom : Window
    {
        private readonly Room _room;
        private static readonly HttpClient _http = new HttpClient();
        public modRoom(Room room)
        {
            InitializeComponent();

            _room = room;

            cmbType.SelectedValuePath = "Content";
            cmbType.SelectedValue = (room.Type ?? "").Trim();

            // Rellenar inputs con lo seleccionado
            txtRoomId.Text = _room.RoomId;
            txtPrice.Text = _room.PricePerNight.ToString();
            txtMaxOcc.Text = _room.MaxOccupancy.ToString();
            txtRate.Text = _room.Rate.ToString();
            txtImage.Text = _room.Image;
            txtDescription.Text = _room.Description;
            chkAvailable.IsChecked = _room.IsAvailable;
        }

       private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones básicas
                var type = (cmbType.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(type))
                    throw new Exception("El tipo es obligatorio (Individual/Doble/Suite)");

                if (!double.TryParse(txtPrice.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                    throw new Exception("Precio inválido");

                if (!int.TryParse(txtMaxOcc.Text, out int maxOcc))
                    throw new Exception("Ocupación inválida");

                double.TryParse(txtRate.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double rate);

                // 🔴 PAYLOAD EXACTO QUE ESPERA MONGO
                var payload = new
                {
                    room_id = txtRoomId.Text.Trim(),
                    type = type,
                    description = txtDescription.Text.Trim(),
                    image = txtImage.Text.Trim(),
                    price_per_night = price,
                    rate = rate,
                    max_occupancy = maxOcc,
                    isAvailable = chkAvailable.IsChecked == true
                };

                await RoomService.UpdateRoomAsync(payload);

                DialogResult = true; // 🔴 CLAVE
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

