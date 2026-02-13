using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para modRoom.xaml
    /// </summary>
    public partial class modRoom : Window
    {
        private readonly Room _room;
        private bool _isCreate = false;

        // ── Constructor CREAR (isCreate = true) ──
        public modRoom(Room room, bool isCreate)
        {
            InitializeComponent();

            _room = room;
            _isCreate = isCreate;

            // Preparar ComboBox
            cmbType.SelectedValuePath = "Content";

            // Rellenar inputs con los datos de la habitación
            txtRoomId.Text = _room.RoomId ?? "";
            cmbType.SelectedValue = (_room.Type ?? "").Trim();
            txtPrice.Text = _room.PricePerNight > 0
                ? _room.PricePerNight.ToString(CultureInfo.InvariantCulture)
                : "";
            txtMaxOcc.Text = _room.MaxOccupancy > 0
                ? _room.MaxOccupancy.ToString()
                : "1";
            txtRate.Text = _room.Rate.ToString(CultureInfo.InvariantCulture);
            txtImage.Text = _room.Image ?? "";
            txtDescription.Text = _room.Description ?? "";
            chkAvailable.IsChecked = _room.IsAvailable;

            // Room ID editable solo al crear
            txtRoomId.IsReadOnly = !_isCreate;
        }

        // ── Constructor EDITAR ──
        public modRoom(Room room)
        {
            InitializeComponent();

            _room = room;
            _isCreate = false;

            // Preparar ComboBox
            cmbType.SelectedValuePath = "Content";

            // Rellenar inputs con lo seleccionado
            txtRoomId.Text = _room.RoomId ?? "";
            cmbType.SelectedValue = (_room.Type ?? "").Trim();
            txtPrice.Text = _room.PricePerNight.ToString(CultureInfo.InvariantCulture);
            txtMaxOcc.Text = _room.MaxOccupancy.ToString();
            txtRate.Text = _room.Rate.ToString(CultureInfo.InvariantCulture);
            txtImage.Text = _room.Image ?? "";
            txtDescription.Text = _room.Description ?? "";
            chkAvailable.IsChecked = _room.IsAvailable;

            // Room ID no editable al editar
            txtRoomId.IsReadOnly = true;
        }

        // ── Auto-asignar Max. Ocupación según el tipo ──
        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guardia: puede dispararse antes de que los controles carguen
            if (txtMaxOcc == null) return;

            if (cmbType.SelectedItem is not ComboBoxItem selected)
                return;

            string type = selected.Content?.ToString() ?? "";

            int occupancy = type switch
            {
                "Individual" => 1,
                "Doble" => 2,
                "Suite" => 4,
                _ => 1
            };

            txtMaxOcc.Text = occupancy.ToString();
        }

        // ── Guardar habitación (Crear o Editar) ──
        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            txtMsg.Text = "";

            try
            {
                // ── Recoger valores del formulario ──
                string roomId = txtRoomId.Text.Trim();
                string type = (cmbType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                string description = txtDescription.Text.Trim();
                string image = txtImage.Text.Trim();

                if (!double.TryParse(txtPrice.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                    price = 0;

                if (!int.TryParse(txtMaxOcc.Text, out int maxOcc))
                    maxOcc = 0;

                if (!double.TryParse(txtRate.Text.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double rate))
                    rate = 0;

                bool isAvailable = chkAvailable.IsChecked == true;

                // ── Validación ──
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    MessageBox.Show("El Room ID es obligatorio.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(type))
                {
                    MessageBox.Show("Debes seleccionar un tipo de habitación.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(description))
                {
                    MessageBox.Show("La descripción es obligatoria.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (price <= 0)
                {
                    MessageBox.Show("El precio debe ser mayor que 0.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ── Payload para la API ──
                var payload = new
                {
                    room_id = roomId,
                    type = type,
                    description = description,
                    image = image,
                    price_per_night = price,
                    rate = rate,
                    max_occupancy = maxOcc,
                    isAvailable = isAvailable
                };

                // ── Llamar a la API ──
                if (_isCreate)
                {
                    await RoomService.CreateRoomAsync(payload);
                }
                else
                {
                    await RoomService.UpdateRoomAsync(payload);
                }

                // ── Éxito: cerrar ventana ──
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar la habitación:\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
