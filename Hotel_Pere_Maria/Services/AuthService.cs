using Hotel_Pere_Maria.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
    public static class AuthService
    {
        ///<summary>
        ///Login: envia email + password al API
        /// </summary> 

        // Este método hace el login de verdad
        // Se llama desde MainWindow.xaml.cs cuando el usuario da click en "Iniciar Sesión"
        public static async Task<Usuario> LoginAsync(string email, string password)
        {
            try
            {
                var request = new Usuario
                {
                    email = email,
                    password = password
                };

                // Convertir el objeto C# a JSON para mandarlo por HTTP
                // Serializar a JSON
                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST /api/auth/login
                var response = await ApiService._httpClient.PostAsync(
                    ApiService.BaseUrl + "auth/login",
                    content
                );

                // Leer respuesta
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error {response.StatusCode}: {responseContent}");
                }

                // Si todo está bien, convertir la respuesta JSON a objeto LoginResponse
                // PropertyNameCaseInsensitive = true es para que sea flexible con mayúsculas/minúsculas
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Usuario>(responseContent, opciones);
            }
            catch (Exception ex)
            {

                // Si algo falla, lanzar excepción con el mensaje
                // Esta excepción se captura en MainWindow.xaml.cs en el método ManejarError()
                throw new Exception($"Error en login: {ex.Message}");
            }
        }

        public static async Task LogoutAsync()
        {
            try
            {
                await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "auth/logout", null);
            }
            catch (Exception)
            {
                //Voy a ignorar el error de red, lo importante es limpiar en local
            }
            finally
            {
                //Limpiamos sesión local
                Session.Clear();
            }
        }
    }
}