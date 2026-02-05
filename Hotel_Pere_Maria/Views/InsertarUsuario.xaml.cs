using System;
using System.Windows;
using System.Windows.Controls; // Necesario para ComboBoxItem
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Views
{
    public partial class InsertarUsuario : Window
    {
        private Usuario _usuarioEdicion;
        private bool _esEdicion = false;

        // Constructor que acepta null (crear) o un usuario (editar)
        public InsertarUsuario(Usuario usuario = null)
        {
            InitializeComponent();
            _usuarioEdicion = usuario;

            //Si el usuario logueado No es admin, quitamos la opcion de crear admin del comboBox
            if(Session.User.role != "admin")
            {
                //Buscamos el item 'admin' y lo ocultamos o removemos.
                foreach (ComboBoxItem item in cbRol.Items)
                {
                    if (item.Content.ToString() == "admin")
                    {
                        item.Visibility = Visibility.Collapsed; //Ocultamos la opcion
                        break;
                    }
                }
            }

            if (_usuarioEdicion != null)
            {
                _esEdicion = true;
                CargarDatosUsuario();
            }
        }

        private void CargarDatosUsuario()
        {
            // Nota: Por seguridad, no cargamos la contraseña antigua en el cuadro de texto.
            // Si el usuario la deja vacía en edición, asumimos que no quiere cambiarla.

            // Rellenar campos para editar
            txtNombre.Text = _usuarioEdicion.name;
            txtApellido.Text = _usuarioEdicion.surname;
            txtDNI.Text = _usuarioEdicion.dni;
            txtEmail.Text = _usuarioEdicion.email;
            dpFechaNacimiento.SelectedDate = _usuarioEdicion.birthDate;
            cbCiudad.Text = _usuarioEdicion.city;

            // Selección de Rol
            foreach (ComboBoxItem item in cbRol.Items)
            {
                if (item.Content.ToString() == _usuarioEdicion.role)
                {
                    cbRol.SelectedItem = item;
                    break;
                }
            }
        }

        private async void Click_Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validaciones Básicas (Requisito PDF)
                if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtDNI.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Por favor, rellena todos los campos obligatorios.");
                    return;
                }

                // 2. Construir objeto Usuario
                Usuario nuevoUsuario = _esEdicion ? _usuarioEdicion : new Usuario();

                nuevoUsuario.name = txtNombre.Text;
                nuevoUsuario.surname = txtApellido.Text;
                nuevoUsuario.dni = txtDNI.Text;
                nuevoUsuario.email = txtEmail.Text;
                nuevoUsuario.city = cbCiudad.Text;
                nuevoUsuario.birthDate = dpFechaNacimiento.SelectedDate ?? DateTime.Now;

                // Obtener Rol
                if (cbRol.SelectedItem is ComboBoxItem selectedRole)
                    nuevoUsuario.role = selectedRole.Content.ToString();


                // Obtener Género
                if (rbHombre.IsChecked == true) nuevoUsuario.gender = "M";
                else if (rbMujer.IsChecked == true) nuevoUsuario.gender = "F";
                else nuevoUsuario.gender = "Other";

                // Contraseña (Solo validamos confirmación si se escribe algo)
                string pass = txtPass.Password;
                string passConfirm = txtPassConfirm.Password;

                if (!_esEdicion || !string.IsNullOrEmpty(pass))
                {
                    if (pass != passConfirm)
                    {
                        MessageBox.Show("Las contraseñas no coinciden.");
                        return;
                    }
                    if (pass.Length < 8)
                    {
                        MessageBox.Show("La contraseña debe tener al menos 8 caracteres.");
                        return;
                    }
                    nuevoUsuario.password = pass; // En backend se encriptará
                }

                // 3. Llamada a la API
                if (_esEdicion)
                {
                    // Lógica para UPDATE (PATCH)
                    // El userController.js espera confirmPassword si se envía password
                    // Como tu backend usa req.body.confirmPassword en update, hay que manejarlo o 
                    // ajustar el objeto que enviamos. Para simplificar, asumimos que el servicio lo gestiona.
                    await UserService.ModifyUserAsync(nuevoUsuario.user_id, nuevoUsuario);
                    MessageBox.Show("Usuario actualizado correctamente.");
                }
                else
                {

                    // 'addEmployee' genera el ID automáticamente.
                    await UserService.AddUserAsync(nuevoUsuario);
                    MessageBox.Show("Usuario creado correctamente.");
                }

                this.DialogResult = true; // Cierra y notifica éxito
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }

        private void Click_Cancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}