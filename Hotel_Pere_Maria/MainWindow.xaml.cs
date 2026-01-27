using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;
using System;
using System.Text.Json;
using System.Windows;

namespace Hotel_Pere_Maria
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Limpiar cualquier error que haya de antes
                TxtError.Text = "";

                // Obtener valores
                string email = TxtEmail.Text?.Trim() ?? "";
                string password = TxtPassword.Password ?? "";

                // Validar campos vacíos
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TxtError.Text = "Email y contraseña son obligatorios";
                    TxtError.Foreground = System.Windows.Media.Brushes.Orange;
                    return;
                }

                // Validar formato email básico
                if (!email.Contains("@"))
                {
                    TxtError.Text = "Por favor, ingresa un email válido";
                    TxtError.Foreground = System.Windows.Media.Brushes.Orange;
                    return;
                }

                // Desactivar botón durante login
                BtnLogin.IsEnabled = false;
                BtnLogin.Content = "Iniciando sesión...";

                // Llamada al API
                var response = await AuthService.LoginAsync(email, password);

                // Validamos rol - Solo pueden entrar las personas con rol employee y admin
                if (response.user.role != "employee" && response.user.role != "admin")
                {
                    TxtError.Text = $"ACCESO DENEGADO\n\n" +
                                    $"Tu rol es: {response.user.role}\n\n" +
                                    $"Solo estos roles pueden entrar:\n" +
                                    $"• employee (Empleado)\n" +
                                    $"• admin (Administrador)";
                    TxtError.Foreground = System.Windows.Media.Brushes.Red;
                    BtnLogin.IsEnabled = true;
                    BtnLogin.Content = "Iniciar Sesión";
                    return;
                }

                // Guardar en sesión global
                Session.Token = response.token;
                Session.User = response.user;

                MessageBox.Show(
                    $"¡Bienvenido {response.user.name}!\nRol: {response.user.role.ToUpper()}",
                    "Login Exitoso ✅",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Abrir ventana principal
                Inicio ventanaInicio = new Inicio();
                ventanaInicio.Show();

                // Cerrar login
                this.Close();
            }
            catch (Exception ex)
            {
                // Manejo mejorado de errores
                ManejarError(ex);
                BtnLogin.IsEnabled = true;
                BtnLogin.Content = "Iniciar Sesión";
            }
        }

        /// <summary>
        /// Maneja errores de forma inteligente, extrayendo el mensaje del backend
        /// </summary>
        private void ManejarError(Exception ex)
        {
            string mensaje = ex.Message;

            // Deteccion de errores de coxeccion
            if (mensaje.Contains("No se puede establecer una conexión") ||
                mensaje.Contains("Connection refused") ||
                mensaje.Contains("refused") ||
                mensaje.Contains("No route to host") ||
                mensaje.Contains("localhost:3000"))
            {
                TxtError.Text = "No hay conexión con el servidor\n\n" +
                                "Verifica que:\n" +
                                "El backend esté corriendo en http://localhost:3000\n" +
                                "La base de datos esté activa\n" +
                                "No haya firewall bloqueando la conexión";
                TxtError.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            // Si es error del servidor, intenta extraer el mensaje JSON
            if (mensaje.Contains("Error") && mensaje.Contains("{"))
            {
                try
                {
                    // Buscar el JSON en el mensaje de error
                    int startIndex = mensaje.IndexOf("{");
                    int endIndex = mensaje.LastIndexOf("}") + 1;

                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        string jsonError = mensaje.Substring(startIndex, endIndex - startIndex);

                        // Deserializar el JSON
                        using (JsonDocument doc = JsonDocument.Parse(jsonError))
                        {
                            JsonElement root = doc.RootElement;

                            // Buscar campo "error", "mensaje" o "message"
                            if (root.TryGetProperty("error", out JsonElement errorProp))
                            {
                                string errorMsg = errorProp.GetString();
                                TxtError.Text = FormatearMensajeError(errorMsg);
                            }
                            else if (root.TryGetProperty("mensaje", out JsonElement mensajeProp))
                            {
                                string errorMsg = mensajeProp.GetString();
                                TxtError.Text = FormatearMensajeError(errorMsg);
                            }
                            else if (root.TryGetProperty("message", out JsonElement messageProp))
                            {
                                string errorMsg = messageProp.GetString();
                                TxtError.Text = FormatearMensajeError(errorMsg);
                            }
                            else
                            {
                                TxtError.Text = "Credenciales inválidas";
                            }
                        }
                    }
                    else
                    {
                        TxtError.Text = "Credenciales inválidas";
                    }
                }
                catch
                {
                    TxtError.Text = "Credenciales inválidas";
                }
            }
            else if (mensaje.Contains("timeout") || mensaje.Contains("Timeout"))
            {
                TxtError.Text = "Timeout - El servidor tardó demasiado en responder";
            }
            else
            {
                TxtError.Text = $"{mensaje}";
            }

            TxtError.Foreground = System.Windows.Media.Brushes.Red;
        }

        /// <summary>
        /// Formatea el mensaje de error del backend de forma amigable
        /// </summary>
        private string FormatearMensajeError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
                return "Error de autenticación";

            // Detectar tipos de errores comunes
            if (errorMsg.Contains("no existe") || errorMsg.Contains("no found") || errorMsg.Contains("not found"))
                return "Usuario no encontrado\n\nVerifica que el email sea correcto";

            if (errorMsg.Contains("contraseña") || errorMsg.Contains("password") || errorMsg.Contains("incorrecta"))
                return "Contraseña incorrecta\n\nVerifica que la contraseña sea correcta";

            if (errorMsg.Contains("Credenciales") || errorMsg.Contains("credentials"))
                return "Credenciales inválidas\n\nVerifica email y contraseña";

            if (errorMsg.Contains("activo") || errorMsg.Contains("inactivo") || errorMsg.Contains("desactivado"))
                return $"Cuenta no activa\n\n{errorMsg}";

            // Por defecto, mostrar el mensaje con prefijo
            return $"{errorMsg}";
        }
    }
}
