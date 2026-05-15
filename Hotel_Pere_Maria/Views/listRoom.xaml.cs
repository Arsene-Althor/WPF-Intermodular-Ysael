using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class listRoom : UserControl
    {
        private readonly bool _editMode;
        private bool _closing;
        private readonly ListRoomViewModel _viewModel;

        public Room? SelectedRoomResult { get; private set; }

        public listRoom() : this(true, null, null) { }

        public listRoom(DateTime? checkIn, DateTime? checkOut) : this(false, checkIn, checkOut) { }

        private listRoom(bool editMode, DateTime? checkIn, DateTime? checkOut)
        {
            InitializeComponent();
            _editMode = editMode;
            _viewModel = new ListRoomViewModel(editMode, checkIn, checkOut);
            DataContext = _viewModel;
        }

        /// <summary>Diálogo modal para elegir habitación (reservas). Devuelve false si cancela.</summary>
        public static bool TryPickRoom(DateTime? checkIn, DateTime? checkOut, out Room? picked)
        {
            picked = null;
            var uc = new listRoom(false, checkIn, checkOut);
            var shell = new Window
            {
                Owner = UiShell.OwnerWindow,
                Title = "Elegir habitación",
                Width = 1060,
                Height = 720,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = uc
            };
            var ok = shell.ShowDialog();
            if (ok == true)
                picked = uc.SelectedRoomResult;
            return ok == true && picked != null;
        }

        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                MessageBox.Show("No se pueden crear habitaciones desde el modo reserva.");
                return;
            }

            var newRoom = new Room
            {
                RoomId = "HAB-",
                Type = "Individual",
                Description = "",
                Image = "",
                PricePerNight = 0,
                Rate = 0,
                MaxOccupancy = 1,
                IsOperational = true
            };

            var win = new modRoom(newRoom, isCreate: true);
            win.Owner = Window.GetWindow(this);

            if (win.ShowDialog() == true)
                await _viewModel.LoadRoomsAsync();
        }

        private async void LvRooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_closing) return;

            if ((sender as ListViewItem)?.DataContext is not Room room)
                return;

            if (_editMode)
            {
                var editWin = new modRoom(room);
                editWin.Owner = Window.GetWindow(this);
                if (editWin.ShowDialog() == true)
                    await _viewModel.LoadRoomsAsync();
                return;
            }

            _closing = true;
            SelectedRoomResult = room;
            var shell = Window.GetWindow(this);
            if (shell != null)
            {
                try { shell.DialogResult = true; } catch { /* no es ventana modal */ }
                shell.Close();
            }
        }

        private async void BtnBulkExtras_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                MessageBox.Show("Solo en modo gestión de habitaciones.");
                return;
            }

            var sel = lvRooms.SelectedItems.Cast<Room>().Distinct().ToList();
            if (sel.Count == 0)
            {
                MessageBox.Show("Selecciona una o varias habitaciones (Ctrl + clic en las tarjetas).", "Servicios extra",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var catalog = await ExtraServiceCatalogService.ListAsync();
                if (catalog == null || catalog.Count == 0)
                {
                    MessageBox.Show("No hay servicios en el catálogo.", "Servicios extra", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dlg = new BulkRoomExtrasDialog(sel, catalog) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() != true)
                    return;
                var ids = dlg.SelectedServiceIds;
                if (ids == null || ids.Count == 0)
                    return;

                foreach (var room in sel)
                {
                    var merged = (room.ExtraServices ?? new List<string>())
                        .Union(ids)
                        .Distinct()
                        .ToList();
                    await RoomService.UpdateRoomAsync(new { room_id = room.RoomId, extra_services = merged });
                }

                MessageBox.Show($"Servicios asociados en {sel.Count} habitación(es).", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                await _viewModel.LoadRoomsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminarHabitacion_Click(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                MessageBox.Show("Solo en modo gestión de habitaciones.");
                return;
            }

            if (Session.User?.role != "admin")
            {
                MessageBox.Show("Solo un administrador puede eliminar habitaciones.", "Permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lvRooms.SelectedItems.Count != 1 || lvRooms.SelectedItems[0] is not Room room)
            {
                MessageBox.Show("Selecciona exactamente una habitación para eliminar.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var r = MessageBox.Show(
                $"¿Eliminar permanentemente la habitación {room.RoomId}? Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes)
                return;

            try
            {
                await RoomService.DeleteRoomAsync(room.RoomId);
                MessageBox.Show("Habitación eliminada.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                await _viewModel.LoadRoomsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
