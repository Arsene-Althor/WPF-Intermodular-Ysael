using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://localhost:3000/";

        public async Task<List<Reservation>> getAllReservation()
        {
            try
            {
                var respuesta = await _httpClient.GetAsync(BaseUrl + "reservation/all");
                if (respuesta.IsSuccessStatusCode)
                {
                    string contenido = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<Reservation> lista = JsonSerializer.Deserialize<List<Reservation>>(contenido, opciones);
                    return lista;
                }
                return new List<Reservation>();
            }
            catch
            {
                return null;
            }
        }

    }
}
