using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Views
{
    public partial class GestionUsuarios : Window
    {
        private List<Usuario> _todosLosUsuarios;

        public GestionUsuarios()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private async void CargarUsuarios()
        {
            try
            {
                _todosLosUsuarios = await UserService.GetAllUsersAsync();
                dgUsuarios.ItemsSource = _todosLosUsuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarUsuarios_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_todosLosUsuarios == null) return;

            // Filtros de texto
            string dni = txtFiltroDNI?.Text?.ToLower() ?? "";
            string nombre = txtFiltroNombre?.Text?.ToLower() ?? "";
            string apellido = txtFiltroApellido?.Text?.ToLower() ?? "";
            string email = txtFiltroEmail?.Text?.ToLower() ?? "";
            string ciudad = txtFiltroCiudad?.Text?.ToLower() ?? "";

            // Filtros de ComboBox
            int indiceRol = cbFiltroRol?.SelectedIndex ?? 0;        // 0=Todos, 1=Admin, 2=Empleado, 3=Cliente
            int indiceEstado = cbFiltroEstado?.SelectedIndex ?? 0;  // 0=Todos, 1=Activos, 2=Inactivos
            int indiceVIP = cbFiltroVIP?.SelectedIndex ?? 0;        // 0=Todos, 1=Solo VIP, 2=No VIP
            int indiceGenero = cbFiltroGenero?.SelectedIndex ?? 0;  // 0=Todos, 1=M, 2=F, 3=Other
            int indiceDescuento = cbFiltroDescuento?.SelectedIndex ?? 0;

            // Filtros de fecha
            DateTime? fechaDesde = dpFechaDesde?.SelectedDate;
            DateTime? fechaHasta = dpFechaHasta?.SelectedDate;

            var filtrados = _todosLosUsuarios.Where(u =>
            {
                // Filtro DNI
                bool coincideDNI = string.IsNullOrEmpty(dni) ||
                                   (!string.IsNullOrEmpty(u.dni) && u.dni.ToLower().Contains(dni));

                // Filtro Nombre
                bool coincideNombre = string.IsNullOrEmpty(nombre) ||
                                      (!string.IsNullOrEmpty(u.name) && u.name.ToLower().Contains(nombre));

                // Filtro Apellido
                bool coincideApellido = string.IsNullOrEmpty(apellido) ||
                                        (!string.IsNullOrEmpty(u.surname) && u.surname.ToLower().Contains(apellido));

                // Filtro Email
                bool coincideEmail = string.IsNullOrEmpty(email) ||
                                     (!string.IsNullOrEmpty(u.email) && u.email.ToLower().Contains(email));

                // Filtro Ciudad
                bool coincideCiudad = string.IsNullOrEmpty(ciudad) ||
                                      (!string.IsNullOrEmpty(u.city) && u.city.ToLower().Contains(ciudad));

                // Filtro Rol
                bool coincideRol = true;
                if (indiceRol == 1) coincideRol = u.role == "admin";
                else if (indiceRol == 2) coincideRol = u.role == "employee";
                else if (indiceRol == 3) coincideRol = u.role == "client";

                // Filtro Estado
                bool coincideEstado = true;
                if (indiceEstado == 1) coincideEstado = u.isActive == true;
                else if (indiceEstado == 2) coincideEstado = u.isActive == false;

                // Filtro VIP
                bool coincideVIP = true;
                if (indiceVIP == 1) coincideVIP = u.isVIP == true;
                else if (indiceVIP == 2) coincideVIP = u.isVIP == false;

                // Filtro Género
                bool coincideGenero = true;
                if (indiceGenero == 1) coincideGenero = u.gender == "M";
                else if (indiceGenero == 2) coincideGenero = u.gender == "F";
                else if (indiceGenero == 3) coincideGenero = u.gender == "Other";

                // Filtro Descuento
                bool coincideDescuento = true;
                if (indiceDescuento == 1) coincideDescuento = u.Discount > 0;           // Con descuento
                else if (indiceDescuento == 2) coincideDescuento = u.Discount == 0;     // Sin descuento
                else if (indiceDescuento == 3) coincideDescuento = u.Discount >= 0.10;  // 10% o más
                else if (indiceDescuento == 4) coincideDescuento = u.Discount >= 0.20;  // 20% o más
                else if (indiceDescuento == 5) coincideDescuento = u.Discount >= 0.30;  // 30% o más

                // Filtro Fecha Desde
                bool coincideFechaDesde = true;
                if (fechaDesde.HasValue)
                {
                    coincideFechaDesde = u.createdAt >= fechaDesde.Value;
                }

                // Filtro Fecha Hasta
                bool coincideFechaHasta = true;
                if (fechaHasta.HasValue)
                {
                    // Añadimos un día para incluir todo el día seleccionado
                    coincideFechaHasta = u.createdAt <= fechaHasta.Value.AddDays(1);
                }

                return coincideDNI && coincideNombre && coincideApellido && coincideEmail &&
                       coincideCiudad && coincideRol && coincideEstado && coincideVIP &&
                       coincideGenero && coincideDescuento && coincideFechaDesde && coincideFechaHasta;

            }).ToList();

            dgUsuarios.ItemsSource = filtrados;
        }

        private void Click_LimpiarFiltros(object sender, RoutedEventArgs e)
        {
            // Limpiar TextBoxes
            txtFiltroDNI.Text = string.Empty;
            txtFiltroNombre.Text = string.Empty;
            txtFiltroApellido.Text = string.Empty;
            txtFiltroEmail.Text = string.Empty;
            txtFiltroCiudad.Text = string.Empty;

            // Limpiar ComboBoxes
            cbFiltroRol.SelectedIndex = 0;
            cbFiltroEstado.SelectedIndex = 0;
            cbFiltroVIP.SelectedIndex = 0;
            cbFiltroGenero.SelectedIndex = 0;
            cbFiltroDescuento.SelectedIndex = 0;

            // Limpiar DatePickers
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;

            // Mostrar todos los usuarios
            dgUsuarios.ItemsSource = _todosLosUsuarios;
        }

        private void Click_NuevoUsuario(object sender, RoutedEventArgs e)
        {
            InsertarUsuario ventanaNuevo = new InsertarUsuario();
            bool? resultado = ventanaNuevo.ShowDialog();

            if (resultado == true)
            {
                CargarUsuarios();
            }
        }

        private void Click_EditarUsuario(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuarioSeleccionado)
            {
                InsertarUsuario ventanaEditar = new InsertarUsuario(usuarioSeleccionado);
                bool? resultado = ventanaEditar.ShowDialog();

                if (resultado == true)
                {
                    CargarUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario de la lista para editar.");
            }
        }

        private async void Click_EliminarUsuario(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuarioSeleccionado)
            {
                var confirm = MessageBox.Show($"¿Estás seguro de eliminar a {usuarioSeleccionado.name}?",
                                              "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserService.DeleteUserAsync(usuarioSeleccionado.user_id);
                        MessageBox.Show("Usuario eliminado (desactivado) correctamente.");
                        CargarUsuarios();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario para eliminar.");
            }
        }

        private void Click_RecargarUsuarios(object sender, RoutedEventArgs e)
        {
            Click_LimpiarFiltros(sender, e);
            CargarUsuarios();
        }

        private void Click_GestionarDescuento(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuarioSeleccionado)
            {
                GestionarDescuento ventanaDesc = new GestionarDescuento(usuarioSeleccionado);
                bool? resultado = ventanaDesc.ShowDialog();

                if (resultado == true)
                {
                    CargarUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario para aplicar descuento.");
            }
        }

        public Usuario UsuarioSeleccionado { get; private set; }

        private void dgUsuarios_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuario)
            {
                if (this.Owner != null)
                {
                    if (this.Owner is addReserva || this.Owner is modReserva || this.Owner is listReservas)
                    {
                        UsuarioSeleccionado = usuario;
                        this.DialogResult = true;
                        this.Close();
                        return;
                    }
                }
                Click_EditarUsuario(sender, e);
            }
        }
    }
}