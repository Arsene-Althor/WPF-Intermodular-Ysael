using Hotel_Pere_Maria.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
    class RoomService
    {
        /// <summary>
        /// Devuelve las habitaciones disponibles entre checkIn y checkOut (para X huéspedes).
        /// Endpoint esperado:
        /// GET {BaseUrl}api/rooms/available?checkIn=YYYY-MM-DD&checkOut=YYYY-MM-DD&guests=N
        /// </summary>
        public static async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int guests)
        {
            if (guests < 1) guests = 1;

            // Formato de fecha estable para APIs
            string ci = checkIn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string co = checkOut.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            string url =$"{ApiService.BaseUrl}room/available" +
                $"?checkIn={ci}&checkOut={co}&guests={guests}";

            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            // AQUÍ vemos el error real
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"ERROR llamando a la API\n\n" +
                    $"URL: {url}\n" +
                    $"Status: {(int)response.StatusCode}\n\n" +
                    $"Respuesta del servidor:\n{responseBody}"
                );
            }

            // Convertir JSON a List<Room>
            var rooms = JsonSerializer.Deserialize<List<Room>>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return rooms ?? new List<Room>();
        }

        /// <summary>
        /// (Opcional) Devuelve TODAS las habitaciones (si tienes endpoint).
        /// Endpoint esperado:
        /// GET {BaseUrl}api/rooms
        /// </summary>
        public static async Task<List<Room>> GetAllRoomsAsync()
        {
            string url = $"{ApiService.BaseUrl}room/all";
            var rooms = await ApiService._httpClient.GetFromJsonAsync<List<Room>>(url);
            return rooms ?? new List<Room>();
        }

        /// <summary>
        /// (Opcional) Devuelve una habitación por su RoomId (si tienes endpoint).
        /// Endpoint esperado:
        /// GET {BaseUrl}api/rooms/{roomId}
        /// </summary>
        public static async Task<Room?> GetRoomByIdAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return null;

            // Importante: escapar por si lleva espacios o caracteres raros
            string safeId = Uri.EscapeDataString(roomId);
            string url = $"{ApiService.BaseUrl}room/one?id={safeId}";

            // Si tu API devuelve 404, mejor controlarlo con HttpClient normal:
            using HttpResponseMessage resp = await ApiService._httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<Room>();
        }

        public static async Task UpdateRoomAsync(object payload)
        {
            string url = $"{ApiService.BaseUrl}room/update";

            var response = await ApiService._httpClient.PutAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status {(int)response.StatusCode}\n{body}");
            }
        }

        public static async Task DeleteRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("room_id vacío");
            string url = $"{ApiService.BaseUrl}room/delete";
            using var req = new HttpRequestMessage(HttpMethod.Delete, url);
            req.Content = JsonContent.Create(new Dictionary<string, string> { ["room_id"] = roomId.Trim() });
            using var resp = await ApiService._httpClient.SendAsync(req);
            string body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception(body);
        }

        public static async Task CreateRoomAsync(object payload)
        {
            string url = $"{ApiService.BaseUrl}room/create";
            using var resp = await ApiService._httpClient.PostAsJsonAsync(url, payload);

            string body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception(body);
        }
    }
}
