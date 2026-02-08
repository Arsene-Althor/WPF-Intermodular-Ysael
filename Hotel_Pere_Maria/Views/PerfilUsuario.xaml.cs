using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Hotel_Pere_Maria.Views
{
    public partial class PerfilUsuario : Window
    {
        public PerfilUsuario()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            //Obtener usuario de la sesión actual
            var user = Session.User;

            if (user != null)
            {
                txtNombre.Text = $"{user.name} {user.surname}";
                txtEmail.Text = user.email;
                txtRol.Text = user.role.ToUpper();
                txtID.Text = user.user_id;

                //Cargar imagen si existe
                if (!string.IsNullOrEmpty(user.profileImage))
                {
                    CargarImagenDesdeUrl(user.profileImage);
                }
            }
        }

        private void CargarImagenDesdeUrl(string rutaRelativa)
        {
            try
            {
                // Construimos la URL completa: http://localhost:3000/uploads/foto.jpg
                // Asegúrar de que ApiService.BaseUrl termina en '/' (ej: http://localhost:3000/api/)
                // Como las imagenes están en la raíz, quizás debamos ajustar la url base.

                // TRUCO: Si ApiService.BaseUrl es ".../api/", quitamos el "api/" para obtener la raiz
                string baseUrl = ApiService.BaseUrl.Replace("api/", "");
                string fullUrl = $"{baseUrl}{rutaRelativa}";

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgPerfil.ImageSource = bitmap;
            }
            catch
            {
                // Si falla, se queda la imagen por defecto
            }
        }

        private async void Click_CambiarFoto(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string rutaArchivo = openFileDialog.FileName;

                // Feedback visual
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                // Llamamos al servicio nuevo
                var (success, message, newServerPath) = await UserService.UpdateProfileImageAsync(Session.User.user_id, rutaArchivo);

                Mouse.OverrideCursor = null;

                if (success)
                {
                    MessageBox.Show("Foto de perfil actualizada.");

                    // Actualizamos la sesión local
                    Session.User.profileImage = newServerPath;

                    // Recargamos la imagen en pantalla
                    CargarImagenDesdeUrl(newServerPath);
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
        }

        private void Click_Cerrar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}