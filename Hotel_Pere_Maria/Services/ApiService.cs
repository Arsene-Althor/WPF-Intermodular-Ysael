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
    public static class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://localhost:3000/";

        public static async Task<List<Reservation>> getAllActiveReservation() {
            try
            {
                var respuesta = await _httpClient.GetAsync(BaseUrl + "reservation/allActive");
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

        public static async Task<List<Reservation>> getAllReservation()
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
        public static async Task<(bool exito ,string mensaje)> InsertarReserva(Reservation reserva)
        {
            try
            {

                string json = JsonSerializer.Serialize(reserva);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl + "reservation/add", content);

                string mensajeServidor = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, mensajeServidor);
                }
                else { 
                    return (false, mensajeServidor);
                }
            }
            catch(Exception ex) 
            {
                return (false , "Error de conexión");
            }
        }
    }

}
