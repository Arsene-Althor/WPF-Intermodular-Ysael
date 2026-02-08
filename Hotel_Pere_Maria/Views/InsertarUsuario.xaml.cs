using System;
using System.Windows;
using System.Windows.Controls;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using System.Text.RegularExpressions;

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
                //Validar campos vacíos
                if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtDNI.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Por favor, rellena todos los campos obligatorios.");
                    return;
                }

                //Validamos el formato del dni, y cambiamos la letras a mayusculas para evitar errores
                string dniInput = txtDNI.Text.ToUpper();
                if (!EsDNIValido(dniInput))
                {
                    MessageBox.Show("El DNI no es válido.\nDebe tener 8 números y la letra correcta (Ej: 12345678Z).");
                    return;
                }

                //Validar Formato Email
                if (!EsEmailValido(txtEmail.Text))
                {
                    MessageBox.Show("El formato del correo electrónico no es válido.");
                    return;
                }



                Usuario nuevoUsuario = _esEdicion ? _usuarioEdicion : new Usuario();

                nuevoUsuario.name = txtNombre.Text;
                nuevoUsuario.surname = txtApellido.Text;
                nuevoUsuario.dni = dniInput; //Usamos el DNI ya en mayusculas
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

        private void cbRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Funcion para las regex (Expresiones regulares) asi a la hora de ingresar el email use el formato valido
        private bool EsEmailValido(string email)
        {
            // Patrón simple: texto + @ + texto + . + texto
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
        }

        private bool EsDNIValido(string dni)
        {
            //Validar formato (8 dígitos y 1 letra)
            string patron = @"^\d{8}[A-Z]$";
            if (!Regex.IsMatch(dni, patron)) return false;

            //Validamos que la letra este bien usando formula matematica
            string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            string numeros = dni.Substring(0, 8);
            char letraDada = dni[8];

            int resto = int.Parse(numeros) % 23;
            return letras[resto] == letraDada;
        }

    }
}