using Hotel_Pere_Maria.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
    public static class UserService
    {
        private static void ConfigurarCabeceras()
        {
            ApiService._httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(Session.Token))
            {
                ApiService._httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Session.Token);
            }
        }

        public static async Task<List<Usuario>> GetAllUsersAsync()
        {
            ConfigurarCabeceras();

            
            var response = await ApiService._httpClient.GetAsync($"{ApiService.BaseUrl}user/get");

            if (!response.IsSuccessStatusCode)
            {
                
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {response.StatusCode}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<Usuario>();
        }

        public static async Task AddUserAsync(Usuario usuario)
        {
            ConfigurarCabeceras();

            var payload = new
            {
                name = usuario.name,
                surname = usuario.surname,
                dni = usuario.dni,
                email = usuario.email,
                role = usuario.role,
                password = usuario.password,
                confirmPassword = usuario.password,
                birthDate = usuario.birthDate,
                city = usuario.city,
                gender = usuario.gender
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            
            var response = await ApiService._httpClient.PostAsync($"{ApiService.BaseUrl}user/add", content);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error API ({response.StatusCode}): {error}");
            }
        }

        public static async Task ModifyUserAsync(string userId, Usuario usuario)
        {
            ConfigurarCabeceras();

            var payload = new
            {
                name = usuario.name,
                surname = usuario.surname,
                dni = usuario.dni,
                email = usuario.email,
                role = usuario.role,
                city = usuario.city,
                gender = usuario.gender,
                birthDate = usuario.birthDate,
                isVIP = usuario.isVIP,
                discount = usuario.Discount,
                password = string.IsNullOrEmpty(usuario.password) ? null : usuario.password,
                confirmPassword = string.IsNullOrEmpty(usuario.password) ? null : usuario.password
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            
            var response = await ApiService._httpClient.PatchAsync($"{ApiService.BaseUrl}user/modify/{userId}", content);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al modificar: {error}");
            }
        }

        public static async Task DeleteUserAsync(string userId)
        {
            ConfigurarCabeceras();
            
            var response = await ApiService._httpClient.DeleteAsync($"{ApiService.BaseUrl}user/remove/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al eliminar: {error}");
            }
        }

        public static async Task UpdateDiscountAsync(string userId, double discount)
        {
            ConfigurarCabeceras();

            // El backend espera { newDiscount: 0.2 } para un 20%
            var payload = new { newDiscount = discount };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Ruta definida en userRoutes.js: router.patch('/update/:userId', ...)
            var response = await ApiService._httpClient.PatchAsync($"{ApiService.BaseUrl}user/update/{userId}", content);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al actualizar descuento: {error}");
            }
        }

    }
}