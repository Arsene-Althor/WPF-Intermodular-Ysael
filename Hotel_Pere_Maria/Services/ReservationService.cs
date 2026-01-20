using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public static class ReservationService
    {
        //Metodo para cancelar una resrva, recibimos la reserva y el precio que se descuenta al cliente al cancelar esta
        public static async Task<(bool exito, string mensaje)> cancelReservation(Reservation r, double precioCancel)
        {
            try
            {
                //Calculamos el nuevo precio de la reserva restando al precio actual el precio de cancelación
                //De este modo quedara constancia del precio final de la reserva
                double precionew = r.price - precioCancel;
                //Creamos un Json con los datos para la api
                var datos = new
                {
                    reservation_id = r.reservation_id,
                    price = precionew
                };
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //Mandamos los datos y almacenamos la respuesta
                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/cancel", content);

                string mensajeServidor = await response.Content.ReadAsStringAsync();

                //Si la respuesta es positiva respondemos true y el mensaje del servidor
                //De lo contrario mandamos false y el mensaje del servidor

                if (response.IsSuccessStatusCode)
                {
                    return (true, mensajeServidor);
                }
                else
                {
                    return (false, mensajeServidor);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error de conexión");
            }
        }

        //Obtenemos todas las reservas actualmente activas
        public static async Task<List<Reservation>> getAllActiveReservation()
        {
            try
            {
                //Mandamos la peticion a la api y almacenamos la respuesta
                var respuesta = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "reservation/allActive");
                //Si la respuesta es positiva almacenamos la respuesta y creamos una lista con los objetos que nos devuelve
                if (respuesta.IsSuccessStatusCode)
                {
                    string contenido = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<Reservation> lista = JsonSerializer.Deserialize<List<Reservation>>(contenido, opciones);
                    //Devolvemos la lista completa de objetos Reservation
                    return lista;
                }
                //En caso de estar vacia devolvemos la lista vacia
                return new List<Reservation>();
            }
            catch
            {
                return null;
            }
        }

        //Realizamos la misma operación que el metodo anterior pero atacando a una ruta que nos devuelva todas las reservas
        public static async Task<List<Reservation>> getAllReservation()
        {
            try
            {
                var respuesta = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "reservation/all");
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

        //Metodo para insertar una reserva
        //Recibimos un objeto reservation
        public static async Task<(bool exito, string mensaje)> InsertarReserva(Reservation reserva)
        {
            try
            {
                //Convertimos el objeto a json para mandarlo a la api
                string json = JsonSerializer.Serialize(reserva);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/add", content);

                string mensajeServidor = await response.Content.ReadAsStringAsync();

                //Es la api la que realiza las validaciones, si el resultado es correcto devolvemos true y el mensaje del servidor
                //De lo contrario devolvemos false y el mensaje del servidor

                if (response.IsSuccessStatusCode)
                {
                    return (true, mensajeServidor);
                }
                else
                {
                    return (false, mensajeServidor);
                }
            }
            catch (Exception ex)
            {
                return (false, "Error de conexión");
            }
        }
    }
}
