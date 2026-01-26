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
        /// 

        public static async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    email = email,
                    password = password
                };

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

                // Deserializar respuesta
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<LoginResponse>(responseContent, opciones);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en login: {ex.Message}");
            }
        }
    }
}