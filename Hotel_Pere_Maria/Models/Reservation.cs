using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Models
{
    //Modelo para reservas
    public class Reservation
    {
        public string reservation_id {  get; set; }
        public string room_id { get; set; }
        public string user_id { get; set; }
        private DateTime _check_in { get; set; }
        private DateTime _check_out { get; set; }
        public double price { get; set; }
        public string createdBy { get; set; }

        //La fecha de cancelación puede ser nula 
        private DateTime? _cancelation_date { get; set; }

        //Al obtener una fecha la convertimos a la hora del equipo local ya que la base de datos la guarda en formato universal
        public DateTime check_in
        {
            get => _check_in.ToLocalTime();
            set => _check_in = value;
        }

        public DateTime check_out
        {
            get => _check_out.ToLocalTime();
            set => _check_out = value;
        }

        public DateTime? cancelation_date
        {
            get => _cancelation_date?.ToLocalTime();
            set => _cancelation_date = value;
        }

        //Metodo para Calcular el precio de cancelación 
        //Falta calcular precio
        public double CalcularPrecioCancelacion(DateTime fechaCancelacion) {
            return 10;
        }

        //Metodo estatico para calcular el precio de una habitación
        public static double CalcularPrecio(Reservation? r,Usuario? usuario, Room? habitacion, DateTime? entrada , DateTime? salida) {

            if (usuario!= null && habitacion != null && entrada.HasValue && salida.HasValue) {
                DateTime nuevaEntrada = new DateTime(entrada.Value.Year, entrada.Value.Month, entrada.Value.Day, 12, 0, 0);
                DateTime nuevaSalida = new DateTime(salida.Value.Year, salida.Value.Month, salida.Value.Day, 11, 0, 0);

                // 2. Calcular la diferencia (nuevaSalida - nuevaEntrada)
                TimeSpan diferencia = nuevaSalida - nuevaEntrada;

                // 3. Convertir a milisegundos y dividir (diferencia / (1000 * 60 * 60 * 24))
                double diferenciaMs = diferencia.TotalMilliseconds;
                double dias = Math.Ceiling(diferenciaMs / (1000 * 60 * 60 * 24));

                // 4. Calcular precio final
                double precioReserva = dias * habitacion.PricePerNight;

                return precioReserva;
            }
            return 0;
            
        }


    }
}
