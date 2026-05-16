using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Globalization;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    public static class ReservationService
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

        /// <summary>GET /reservation/:id/invoice — PDF binario (opcional ?invoice_number=).</summary>
        public static async Task<(bool exito, string mensaje, byte[] pdf)> DownloadInvoicePdfAsync(
            string reservation_id,
            string? invoice_number = null)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/invoice";
                if (!string.IsNullOrWhiteSpace(invoice_number))
                    url += $"?invoice_number={Uri.EscapeDataString(invoice_number.Trim())}";
                var response = await ApiService._httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    return (false, body, null);
                }
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                return (true, null, bytes);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>GET /reservation/:id/booking-receipt — justificante PDF (no fiscal, sin checkout).</summary>
        public static async Task<(bool exito, string mensaje, byte[] pdf)> DownloadBookingReceiptPdfAsync(string reservation_id)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/booking-receipt";
                var response = await ApiService._httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    return (false, body, null);
                }
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                return (true, null, bytes);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>GET /reservation/invoices/history — solo admin/empleado.</summary>
        public static async Task<(bool exito, string mensaje, List<HotelInvoiceItem> lista)> GetInvoicesHistoryAsync()
        {
            try
            {
                ConfigurarCabeceras();
                var response = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "reservation/invoices/history");
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                };
                var lista = JsonSerializer.Deserialize<List<HotelInvoiceItem>>(body, opts) ?? new List<HotelInvoiceItem>();
                return (true, null, lista);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>POST /reservation/:id/invoice/email — solo personal.</summary>
        public static async Task<(bool exito, string mensaje)> PostInvoiceEmailAsync(
            string reservation_id,
            string? overrideTo = null,
            string? invoice_number = null)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/invoice/email";
                object payload = string.IsNullOrWhiteSpace(overrideTo) && string.IsNullOrWhiteSpace(invoice_number)
                    ? new { }
                    : new
                    {
                        to = string.IsNullOrWhiteSpace(overrideTo) ? null : overrideTo.Trim(),
                        invoice_number = string.IsNullOrWhiteSpace(invoice_number) ? null : invoice_number.Trim(),
                    };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PostAsync(url, content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body);
                return (true, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>GET /reservation/:id/check-in-status</summary>
        public static async Task<(bool exito, string mensaje, ReceptionCheckInStatusDto? dto)> GetReceptionCheckInStatusAsync(
            string reservation_id)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/check-in-status";
                var response = await ApiService._httpClient.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                };
                var dto = JsonSerializer.Deserialize<ReceptionCheckInStatusDto>(body, opts);
                return (true, null, dto);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>POST /reservation/check-in — registrar llegada en recepción.</summary>
        public static async Task<(bool exito, string mensaje, Reservation? reserva)> PostReceptionCheckInAsync(
            string reservation_id,
            bool acceptLate)
        {
            try
            {
                ConfigurarCabeceras();
                var datos = new { reservation_id, accept_late = acceptLate };
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/check-in", content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                };
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("reservation", out var resEl))
                {
                    var reserva = JsonSerializer.Deserialize<Reservation>(resEl.GetRawText(), opts);
                    return (true, null, reserva);
                }
                return (true, body, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>POST /reservation/checkout — solo personal.</summary>
        public static async Task<(bool exito, string mensaje, string? invoice_number)> PostCheckoutAsync(string reservation_id)
        {
            try
            {
                ConfigurarCabeceras();
                var datos = new { reservation_id };
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/checkout", content);
                string body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, body, null);
                string? inv = null;
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("invoice_number", out var el))
                        inv = el.GetString();
                }
                catch { /* ignore */ }
                return (true, body, inv);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>Historial de auditoría (GET /reservation/{id}/audit).</summary>
        public static async Task<(bool exito, string mensaje, List<BookingAuditEntry> lista)> GetBookingAuditAsync(string reservation_id)
        {
            try
            {
                ConfigurarCabeceras();
                string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/audit";
                var response = await ApiService._httpClient.GetAsync(url);
                string cuerpo = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return (false, cuerpo, null);
                }
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                };
                var lista = JsonSerializer.Deserialize<List<BookingAuditEntry>>(cuerpo, opts) ?? new List<BookingAuditEntry>();
                return (true, null, lista);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>GET /reservation/audits — listado global (admin/empleado).</summary>
        public static async Task<(bool exito, string mensaje, List<BookingAuditEntry> lista)> GetGlobalAuditsAsync(
            string? bookingId = null,
            string? actorId = null,
            string? action = null,
            string? fromIso = null,
            string? toIso = null,
            int limit = 200)
        {
            try
            {
                ConfigurarCabeceras();
                var qs = new List<string>();
                if (!string.IsNullOrWhiteSpace(bookingId)) qs.Add($"booking_id={Uri.EscapeDataString(bookingId.Trim())}");
                if (!string.IsNullOrWhiteSpace(actorId)) qs.Add($"actor_id={Uri.EscapeDataString(actorId.Trim())}");
                if (!string.IsNullOrWhiteSpace(action)) qs.Add($"action={Uri.EscapeDataString(action.Trim())}");
                if (!string.IsNullOrWhiteSpace(fromIso)) qs.Add($"from={Uri.EscapeDataString(fromIso.Trim())}");
                if (!string.IsNullOrWhiteSpace(toIso)) qs.Add($"to={Uri.EscapeDataString(toIso.Trim())}");
                qs.Add($"limit={Math.Clamp(limit, 1, 500)}");
                string url = $"{ApiService.BaseUrl}reservation/audits?" + string.Join("&", qs);
                var response = await ApiService._httpClient.GetAsync(url);
                string cuerpo = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, cuerpo, null);
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
                };
                var env = JsonSerializer.Deserialize<GlobalAuditsResponse>(cuerpo, opts);
                return (true, null, env?.Items ?? new List<BookingAuditEntry>());
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool exito, string mensaje, double precio)> getCancelationPrice(string reservation_id, DateTime? cancelation_date)
        {
            try
            {
                //Convertimos el objeto a json para mandarlo a la api
                var datos = new
                {
                    reservation_id = reservation_id,
                    cancelation_date = cancelation_date

                };
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/getCancelationPrice", content);

                string mensajeServidor = await response.Content.ReadAsStringAsync();

                //Es la api la que realiza las validaciones, si el resultado es correcto devolvemos true y el mensaje del servidor
                //De lo contrario devolvemos false y el mensaje del servidor

                if (response.IsSuccessStatusCode)
                {
                    // 3. Intentamos extraer el precio del JSON que devuelve la API
                    // Asumiendo que la API devuelve algo como: {"mensaje": "OK", "precio": 150.50}
                    using (JsonDocument doc = JsonDocument.Parse(mensajeServidor))
                    {
                        double precioExtraido = 0.0;
                        if (doc.RootElement.TryGetProperty("precio", out JsonElement precioElement))
                        {
                            precioExtraido = precioElement.GetDouble();
                        }

                        return (true, "Precio calculado", precioExtraido);
                    }
                }
                else
                {
                    return (false, "Error en la API: " + mensajeServidor, 0.0);
                }
            }
            catch (Exception)
            {
                return (false, "Error de conexión", 0.0);
            }
        }
        public static async Task<(bool exito, string mensaje, double precio)> getPriceReservation(string user_id, string room_id, DateTime? check_in, DateTime? check_out) {
            try
            {
                //Convertimos el objeto a json para mandarlo a la api
                var datos = new
                {
                    user_id = user_id,
                    room_id = room_id,
                    check_in = check_in,
                    check_out = check_out
                };
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "reservation/getPrice", content);

                string mensajeServidor = await response.Content.ReadAsStringAsync();

                //Es la api la que realiza las validaciones, si el resultado es correcto devolvemos true y el mensaje del servidor
                //De lo contrario devolvemos false y el mensaje del servidor

                if (response.IsSuccessStatusCode)
                {
                    // 3. Intentamos extraer el precio del JSON que devuelve la API
                    // Asumiendo que la API devuelve algo como: {"mensaje": "OK", "precio": 150.50}
                    using (JsonDocument doc = JsonDocument.Parse(mensajeServidor))
                    {
                        double precioExtraido = 0.0;
                        if (doc.RootElement.TryGetProperty("precio", out JsonElement precioElement))
                        {
                            precioExtraido = precioElement.GetDouble();
                        }

                        return (true, "Precio calculado", precioExtraido);
                    }
                }
                else
                {
                    return (false, "Error en la API: " + mensajeServidor, 0.0);
                }
            }
            catch (Exception)
            {
                return (false, "Error de conexión" ,0.0);
            }
        }
        public static async Task<(bool exito, string mensaje)> updateReservation(Reservation reservamod) {
            try
            {
                //Convertimos el objeto a json para mandarlo a la api
                string json = JsonSerializer.Serialize(reservamod);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiService._httpClient.PatchAsync(ApiService.BaseUrl + "reservation/update", content);

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
            catch (Exception)
            {
                return (false, "Error de conexión");
            }
        }
        //Metodo para cancelar una resrva, recibimos la reserva y el precio que se descuenta al cliente al cancelar esta
        public static async Task<(bool exito, string mensaje)> cancelReservation(Reservation r, double precioCancel)
        {
            try
            {
                //Calculamos el nuevo precio de la reserva restando al precio actual el precio de cancelación
                //De este modo quedara constancia del precio final de la reserva
                double? precionew = r.price - precioCancel;
                // API: DELETE /reservation/cancel/:reservation_id?price= (misma lógica que el POST /cancel antiguo)
                string priceStr = (precionew ?? 0d).ToString(CultureInfo.InvariantCulture);
                string cancelUrl = $"{ApiService.BaseUrl}reservation/cancel/{Uri.EscapeDataString(r.reservation_id)}?price={priceStr}";
                var response = await ApiService._httpClient.DeleteAsync(cancelUrl);

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
            catch (Exception)
            {
                return (false, "Error de conexión");
            }
        }

        //Obtenemos todas las reservas actualmente activas
        public static async Task<List<Reservation>> getAllActiveReservation()
        {
            try
            {
                ConfigurarCabeceras();
                var respuesta = await ApiService._httpClient.GetAsync(ApiService.BaseUrl + "reservation/allActive");
                if (respuesta.IsSuccessStatusCode)
                {
                    string contenido = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                    };
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
            catch (Exception)
            {
                return (false, "Error de conexión");
            }
        }
    }
}
