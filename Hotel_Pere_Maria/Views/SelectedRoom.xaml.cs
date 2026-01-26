using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
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

namespace Hotel_Pere_Maria.Views
{
    public partial class SelectedRoom : Window
    {
        // =================================
        // ====== FECHAS SELECIONADAS ======
        // =================================

        private readonly DateTime _checkIn;
        private readonly DateTime _checkOut;

        // =================================
        // ====== DEVOLVER HABITACION ======
        // =================================
        public Room? SelectedRoomResult { get; private set; }
        
        // PUBLIC: para que la ventana SelectedRoom pueda leerla
        // PRIVATE SET: para que solo esta ventana pueda modificarla
        // ROOM?: puede ser null si el usuario cancela

        public SelectedRoom(DateTime checkIn, DateTime checkOut)
        {
            InitializeComponent();

            _checkIn = checkIn.Date;
            _checkOut = checkOut.Date;

            Loaded += SelectedRoom_Loaded; //Cuando la ventana termine de cargarse -> se ejecuta el metodo
        }

        // ==========================================
        // ====== CUANDO LA VETNANA ES VISIBLE ======
        // ==========================================
        private async void SelectedRoom_Loaded(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = $"Disponibles del {_checkIn:dd/MM/yyyy} al {_checkOut:dd/MM/yyyy}";
            await CargarHabitacionesAsync(_checkIn, _checkOut);
        }

        // ==============================================
        // ====== OBTENER HABITACIONES DISPONIBLES ======
        // ==============================================

        // Recibe fechas y devuelve lista de habitaciones disponibles (API)
        private async Task<List<Room>> GetRoomsDisponiblesAsync(DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-in debe ser anterior a check-out.");

            int guests = 1; // si tu endpoint lo exige, ponlo fijo o pásalo también
            var rooms = await RoomService.GetAvailableRoomsAsync(checkIn, checkOut, guests);

            return rooms ?? new List<Room>();
        }

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

        // --- Botones ---

        // ===============================
        // ====== BOTON SELECCIONAR ======
        // ===============================

        private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem is Room room)
            {
                SelectedRoomResult = room;
                DialogResult = true; // útil si la abres con ShowDialog()
                Close();
            }
            else
            {
                MessageBox.Show("Selecciona una habitación primero.");
            }
        }

        // ============================
        // ====== BOTON CANCELAR ======
        // ============================
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

