using System;
using System.Collections.Generic;
using System.Linq; // Necesario para filtrar
using System.Threading.Tasks;
using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Views
{
    public partial class GestionUsuarios : Window
    {
        private List<Usuario> _todosLosUsuarios; // Lista completa en memoria para filtrar rápido

        public GestionUsuarios()
        {
            InitializeComponent();
            CargarUsuarios();
        }


        private async void CargarUsuarios()
        {
            try
            {
                // Usamos el servicio que ya esta creado
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

            // Obtener valores de los filtros de texto
            string dni = txtFiltroDNI.Text.ToLower();
            string apellido = txtFiltroApellido.Text.ToLower();

            // Obtener índices de los ComboBox
            int indiceRol = cbFiltroRol.SelectedIndex;      // 0=Todos, 1=Clientes, 2=Empleados
            int indiceEstado = cbFiltroEstado.SelectedIndex; // 0=Todos, 1=Activos, 2=Inactivos

            var filtrados = _todosLosUsuarios.Where(u =>
            {
                // Validar coincidencia de texto (DNI y Apellido)
                bool coincideTexto = (string.IsNullOrEmpty(u.dni) || u.dni.ToLower().Contains(dni)) &&
                                     (string.IsNullOrEmpty(u.surname) || u.surname.ToLower().Contains(apellido));

                // Validar coincidencia de ROL
                bool coincideRol = true;
                if (indiceRol == 1) coincideRol = u.role == "client";
                else if (indiceRol == 2) coincideRol = (u.role == "employee" || u.role == "admin");

                // Validar coincidencia de estado
                bool coincideEstado = true;
                if (indiceEstado == 1) // Solo Activos
                {
                    coincideEstado = u.isActive == true;
                }
                else if (indiceEstado == 2) // Solo Inactivos
                {
                    coincideEstado = u.isActive == false;
                }

                // El usuario debe cumplir las 3 condiciones
                return coincideTexto && coincideRol && coincideEstado;

            }).ToList();

            dgUsuarios.ItemsSource = filtrados;
        }

        private void Click_NuevoUsuario(object sender, RoutedEventArgs e)
        {
            // Creamos la ventana de InsertarUsuario sin pasarle ningún parámetro (modo creación)
            InsertarUsuario ventanaNuevo = new InsertarUsuario();
            bool? resultado = ventanaNuevo.ShowDialog();

            // Si la ventana se cerró confirmando la creación, recargamos la lista
            if (resultado == true)
            {
                CargarUsuarios();
            }
        }

        private void Click_EditarUsuario(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuarioSeleccionado)
            {
                // Pasamos el usuario para indicar edición
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
                        // Llamada al servicio existente
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
            // Limpiar los filtros visuales
            txtFiltroDNI.Text = string.Empty;
            txtFiltroApellido.Text = string.Empty;
            cbFiltroRol.SelectedIndex = 0; // Volver a "Todos"

            //Reiniciamos filtro de estado
            if (cbFiltroEstado != null) cbFiltroEstado.SelectedIndex = 0;

            // Recargar datos desde la API
            CargarUsuarios();
        }

        private void Click_GestionarDescuento(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuarioSeleccionado)
            {
                // Abrimos la ventana de descuento
                GestionarDescuento ventanaDesc = new GestionarDescuento(usuarioSeleccionado);
                bool? resultado = ventanaDesc.ShowDialog();

                if (resultado == true)
                {
                    CargarUsuarios(); // Recargamos para ver el cambio en la tabla
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario para aplicar descuento.");
            }
        }

        //Esta propiedad publica la uso para poder devolder el usuario seleccionado
        public Usuario UsuarioSeleccionado { get; private set; }

        private void dgUsuarios_MouseDoubleClick ( object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if para validar que se de click en la fila correcta
            if (dgUsuarios.SelectedItem is Usuario usuario)
            {
                //Compobar si tenemos una ventana "Dueña (Owner) asignada
                if (this.Owner != null)
                {
                    //Detectamos si llamamos desde reservas
                    //Es  esto uso coincidencia de tipos, para saber quien abri la ventana
                    if (this.Owner is addReserva || this.Owner is modReserva || this.Owner is listReservas)
                    {
                        //Aqui creo un caso selector, asi guardamos lo que necesitamos y cerramos
                        UsuarioSeleccionado = usuario;
                        this.DialogResult = true;
                        this.Close();
                        return; //Salimos para no ejecutar la edicion
                    }
                }
                //Si venimos de Inicio, editamos
                Click_EditarUsuario(sender, e);
            }
        }



    }
}