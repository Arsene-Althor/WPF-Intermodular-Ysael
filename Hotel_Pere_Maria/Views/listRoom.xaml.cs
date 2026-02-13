using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// 


    public partial class listRoom : Window
    {

        private readonly bool _editMode;
        private bool _closing = false;

        private readonly DateTime? _checkIn;
        private readonly DateTime? _checkOut;

        private List<Hotel_Pere_Maria.Models.Room> allRooms = new();

        public Room SelectedRoomResult { get; private set; }

        // MODO: ver todas
        public listRoom()
        {
            InitializeComponent();

            _editMode = true;
            Loaded += ListRooms_Loaded;
            // aquí podrías poner fechas por defecto o no filtrar por fechas
        }

        // MODO: ver disponibles entre fechas
        public listRoom(DateTime? checkIn, DateTime? checkOut)
        {
            InitializeComponent();
            if (checkIn != null || checkOut != null) {
                _checkIn = checkIn.Value.Date;
                _checkOut = checkOut.Value.Date;
            }

            
            _editMode = false;

            Loaded += ListRooms_Loaded;

            lvRooms.ItemsSource = allRooms;
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

                lvRooms.ItemsSource = allRooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error cargando habitaciones", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filtros_Changed(object sender, RoutedEventArgs e)
        {

            if (!IsLoaded) return;          // <-- clave
            if (sldPrecioMin == null) return; // <-- por si acaso

            AplicarFiltros();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtIdRoom.Text = "";
            cmbType.SelectedIndex = -1;
            chkSoloDisponibles.IsChecked = false;

            sldCapMin.Value = 1;
            sldPrecioMin.Value = 0;
            sldPrecioMax.Value = 1000;
            chkSoloDisponibles.IsChecked = false;

            lvRooms.ItemsSource = allRooms;
        }
        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            // Si quieres: solo permitir crear en el apartado "Habitaciones"
            // (si esta misma ventana la reutilizas para Reservas, esto evita líos)
            if (!_editMode)
            {
                MessageBox.Show("No se pueden crear habitaciones desde el modo reserva.");
                return;
            }

            // Creamos un Room vacío (tu modelo)
            var newRoom = new Room
            {
                RoomId = "HAB-",        // placeholder, o déjalo vacío si lo permite tu modRoom
                Type = "Individual",
                Description = "",
                Image = "",
                PricePerNight = 0,
                Rate = 0,
                MaxOccupancy = 1,
                IsAvailable = true
            };

            // Abrir modRoom en modo "crear"
            var win = new modRoom(newRoom, isCreate: true);

            if (win.ShowDialog() == true)
            {
                // Recarga la lista para ver la nueva habitación al instante
                allRooms = await RoomService.GetAllRoomsAsync();
                lvRooms.ItemsSource = allRooms;

                // Si usas ListView cards:
                // lvRooms.ItemsSource = allRooms;
            }
        }
        private void AplicarFiltros()
        {
            var q = allRooms.AsEnumerable();

            // RoomId_editMode = false;
            if (!string.IsNullOrWhiteSpace(txtIdRoom.Text))
                q = q.Where(r => (r.RoomId ?? "").Contains(txtIdRoom.Text, StringComparison.OrdinalIgnoreCase));

            // Type
            var type = (cmbType.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(type))
            {
                q = q.Where(r => r.Type == type);
            }

            // Capacidad mínima (Slider)
            int occMin = (int)sldCapMin.Value;
            q = q.Where(r => r.MaxOccupancy >= occMin);

            // Precios (Sliders)
            double pMin = sldPrecioMin.Value;
            double pMax = sldPrecioMax.Value;

            // Evitar min > max
            if (pMin > pMax)
            {
                // opción simple: intercambiarlos
                (pMin, pMax) = (pMax, pMin);
            }

            q = q.Where(r => r.PricePerNight >= pMin && r.PricePerNight <= pMax);

            // Solo disponibles
            if (chkSoloDisponibles.IsChecked == true)
                q = q.Where(r => r.IsAvailable);


            // IMPORTANTÍSIMO: actualizar el DataGrid
            lvRooms.ItemsSource = q.ToList();
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
        // ====== CARGAR DISPONIBLES ======
        // ================================
        private async Task CargarHabitacionesAsync(DateTime checkIn, DateTime checkOut)
        {
            try
            {
                lvRooms.ItemsSource = null;
                var disponibles = await GetRoomsDisponiblesAsync(checkIn, checkOut);
                lvRooms.ItemsSource = disponibles;
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


        private async void LvRooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_closing) return;

            if ((sender as ListViewItem)?.DataContext is not Room room)
                return;

            // ✅ MODO HABITACIONES (editar)
            if (_editMode)
            {
                var editWin = new modRoom(room);

                if (editWin.ShowDialog() == true)
                {
                    // Recarga para ver cambios al instante
                    allRooms = await RoomService.GetAllRoomsAsync();
                    lvRooms.ItemsSource = allRooms;
                }
                return;
            }

            // ✅ MODO RESERVA (seleccionar y cerrar)
            _closing = true;
            SelectedRoomResult = room;

            try { DialogResult = true; } catch { /* por si acaso */ }

            Close();
        }
    }
}
