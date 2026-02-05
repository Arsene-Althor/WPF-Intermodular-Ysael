using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime;
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


namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para listRoom.xaml
    /// </summary>
    public partial class listRoom : Window
    {

        private readonly DateTime? _checkIn;
        private readonly DateTime? _checkOut;

        private List<Hotel_Pere_Maria.Models.Room> allRooms = new();

        public Room SelectedRoomResult { get; private set; }

        private readonly bool _editMode;

        // MODO: ver todas
        public listRoom()
        {
            InitializeComponent();

            _editMode = true;
            Loaded += ListRooms_Loaded;
            // aquí podrías poner fechas por defecto o no filtrar por fechas
        }

        // MODO: ver disponibles entre fechas
        public listRoom(DateTime checkIn, DateTime checkOut)
        {
            InitializeComponent();

            _checkIn = checkIn.Date;
            _checkOut = checkOut.Date;

            _editMode = false;

            Loaded += ListRooms_Loaded;

            dgRooms.ItemsSource = allRooms;
        }

        private async void ListRooms_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Si hay fechas => disponibles
                if (_checkIn.HasValue && _checkOut.HasValue)
                {
                    int guests = 1; // si luego lo pides al usuario, lo pasas aquí
                    allRooms = await RoomService.GetAvailableRoomsAsync(_checkIn.Value, _checkOut.Value, guests);
                }
                else
                {
                    // Si NO hay fechas => todas
                    allRooms = await RoomService.GetAllRoomsAsync();
                }

                dgRooms.ItemsSource = allRooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error cargando habitaciones", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Filtros_Changed(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtIdRoom.Text = "";
            txtTipo.Text = "";
            txtCapMin.Text = "";
            txtPrecioMin.Text = "";
            txtPrecioMax.Text = "";
            chkSoloDisponibles.IsChecked = false;

            dgRooms.ItemsSource = allRooms;
        }

        private void AplicarFiltros()
        {
            var q = allRooms.AsEnumerable();

            // RoomId_editMode = false;
            if (!string.IsNullOrWhiteSpace(txtIdRoom.Text))
                q = q.Where(r => (r.RoomId ?? "").Contains(txtIdRoom.Text, StringComparison.OrdinalIgnoreCase));

            // Type
            if (!string.IsNullOrWhiteSpace(txtTipo.Text))
                q = q.Where(r => (r.Type ?? "").Contains(txtTipo.Text, StringComparison.OrdinalIgnoreCase));

            // MaxOccupancy min/max
            if (int.TryParse(txtCapMin.Text, out int occMin))
                q = q.Where(r => r.MaxOccupancy >= occMin);

            if (int.TryParse(txtCapMin.Text, out int occMax))
                q = q.Where(r => r.MaxOccupancy <= occMax);

            // PricePerNight min/max
            if (TryParseDouble(txtPrecioMin.Text, out double pMin))
                q = q.Where(r => r.PricePerNight >= pMin);

            if (TryParseDouble(txtPrecioMax.Text, out double pMax))
                q = q.Where(r => r.PricePerNight <= pMax);

            // IsAvailable
            if (chkSoloDisponibles.IsChecked == true)
                q = q.Where(r => r.IsAvailable);

            // IMPORTANTÍSIMO: actualizar el DataGrid
            dgRooms.ItemsSource = q.ToList();
        }
        private static bool TryParseDouble(string text, out double value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Acepta coma o punto
            text = text.Trim().Replace(',', '.');

            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
        public class RoomItem
        {
            public int Id { get; set; }
            public string Number { get; set; } = "";
            public string Type { get; set; } = "";
            public int Capacity { get; set; }
            public decimal PricePerNight { get; set; }
            public bool IsAvailable { get; set; }
        }

        // ==========================================
        // ====== CUANDO LA VETNANA ES VISIBLE ======
        // ==========================================

        /*
        private async void SelectedRoom_Loaded(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = $"Disponibles del {_checkIn:dd/MM/yyyy} al {_checkOut:dd/MM/yyyy}";
            await CargarHabitacionesAsync(_checkIn, _checkOut);
        }

        */

        // ================================
        // ====== CARGAR EL DATAGRID ======
        // ================================
        private async Task CargarHabitacionesAsync(DateTime checkIn, DateTime checkOut)
        {
            try
            {
                dgRooms.ItemsSource = null;
                var disponibles = await GetRoomsDisponiblesAsync(checkIn, checkOut);
                dgRooms.ItemsSource = disponibles;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error cargando habitaciones", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================================
        // ====== OBTENER HABITACIONES DISPONIBLES ======
        // ==============================================

        // Recibe fechas y devuelve lista de habitaciones disponibles (API)
        private async Task<List<Hotel_Pere_Maria.Models.Room>> GetRoomsDisponiblesAsync(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-in debe ser anterior a check-out.");

            int guests = 1;

            List<Hotel_Pere_Maria.Models.Room> rooms =
                await RoomService.GetAvailableRoomsAsync(checkIn, checkOut, guests);

            return rooms ?? new List<Hotel_Pere_Maria.Models.Room>();
        }


        private async void dgRooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRooms.SelectedItem is not Hotel_Pere_Maria.Models.Room room) return;

            // ✅ MODO GESTIÓN: editar
            if (_editMode)
            {
                var editWin = new modRoom(room);

                if (editWin.ShowDialog() == true)
                {
                    // recargar tras guardar para verlo al instante
                    if (_checkIn.HasValue && _checkOut.HasValue)
                        allRooms = await RoomService.GetAvailableRoomsAsync(_checkIn.Value, _checkOut.Value, 1);
                    else
                        allRooms = await RoomService.GetAllRoomsAsync();

                    dgRooms.ItemsSource = allRooms;
                }

                return;
            }

            // ✅ MODO RESERVA: seleccionar y cerrar
            SelectedRoomResult = room;
            DialogResult = true;
            Close();
        }
    }
}
