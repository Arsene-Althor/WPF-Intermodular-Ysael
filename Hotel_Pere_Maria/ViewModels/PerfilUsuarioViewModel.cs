using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hotel_Pere_Maria.Services;
using Microsoft.Win32;

namespace Hotel_Pere_Maria.ViewModels
{
    public class PerfilUsuarioViewModel : BaseViewModel
    {

        // CAMPOS PRIVADOS
        private string _nombreCompleto = "-";
        private string _email = "-";
        private string _rol = "-";
        private string _userId = "-";
        private BitmapImage? _imagenPerfil;

        // PROPIEDADES PÚBLICAS
        public string NombreCompleto
        {
            get => _nombreCompleto;
            set { _nombreCompleto = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Rol
        {
            get => _rol;
            set { _rol = value; OnPropertyChanged(); }
        }

        public string UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(); }
        }

        public BitmapImage? ImagenPerfil
        {
            get => _imagenPerfil;
            set { _imagenPerfil = value; OnPropertyChanged(); }
        }

        // COMMANDS
        public ICommand CambiarFotoCommand { get; }
        public ICommand CerrarCommand { get; }

        // Evento para cerrar la ventana
        public event Action? RequestClose;

        // CONSTRUCTOR
        public PerfilUsuarioViewModel()
        {
            CambiarFotoCommand = new RelayCommand(async () => await ExecuteCambiarFoto());
            CerrarCommand = new RelayCommand(() => RequestClose?.Invoke());

            CargarDatos();
        }

        // MÉTODOS
        private void CargarDatos()
        {
            var user = Session.User;

            if (user != null)
            {
                NombreCompleto = $"{user.name} {user.surname}";
                Email = user.email;
                Rol = user.role.ToUpper();
                UserId = user.user_id;

                if (!string.IsNullOrEmpty(user.profileImage))
                {
                    CargarImagenDesdeUrl(user.profileImage);
                }
                else
                {
                    // Imagen por defecto
                    try
                    {
                        ImagenPerfil = new BitmapImage(new Uri("pack://application:,,,/Resources/userIcon.png"));
                    }
                    catch { }
                }
            }
        }

        private void CargarImagenDesdeUrl(string rutaRelativa)
        {
            try
            {
                string baseUrl = ApiService.BaseUrl.Replace("api/", "");
                string fullUrl = $"{baseUrl}{rutaRelativa}";

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ImagenPerfil = bitmap;
            }
            catch
            {
                // Si falla, se queda la imagen por defecto
            }
        }

        private async System.Threading.Tasks.Task ExecuteCambiarFoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                string rutaArchivo = openFileDialog.FileName;

                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                var (success, message, newServerPath) =
                    await UserService.UpdateProfileImageAsync(Session.User.user_id, rutaArchivo);

                System.Windows.Input.Mouse.OverrideCursor = null;

                if (success)
                {
                    MessageBox.Show("Foto de perfil actualizada.");
                    Session.User.profileImage = newServerPath;
                    CargarImagenDesdeUrl(newServerPath);
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
        }
    }
}
