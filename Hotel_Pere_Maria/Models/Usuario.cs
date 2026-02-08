using System;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    /// <summary>
    /// Modelo de Usuario correspondiente con la estructura de MongoDB
    /// Soporta los tres tipos de roles: admin, employee, client
    /// </summary>
    public class Usuario
    {
        // Propiedades básicas
        public string user_id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string dni { get; set; }
        public DateTime birthDate { get; set; }
        public string city { get; set; }
        public string gender { get; set; }
        public string profileImage { get; set; }
        public string role { get; set; }

        // Estado y privilegios
        [JsonPropertyName("discount")]
        public double Discount { get; set; }
        public bool isVIP { get; set; }
        public bool isActive { get; set; }

        // Timestamps
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        /// <summary>
        /// Obtiene el nombre completo del usuario (nombre + apellido)
        /// </summary>
        public string FullName => $"{name} {surname}";

        public bool IsEmployee => role == "admin" || role == "employee";

        /// <summary>
        /// Determina si el usuario es cliente
        /// </summary>
        public bool IsClient => role == "client";

        /// <summary>
        /// Obtiene el estado del usuario en formato legible
        /// </summary>
        public string StatusDisplay => isActive ? "Activo" : "Inactivo";

        // Esta propiedad convierte "0.2" en "20%" para que se lea bien en la tabla
        public string DiscountDisplay
        {
            get
            {
                if (Discount <= 0) return "-";
                return $"{Discount * 100:F0}%"; // F0 quita los decimales extra
            }
        }

        // Modificamos VIPDisplay para que use el dato real
        public string VIPDisplay => isVIP ? "VIP" : "Normal";

        // Propiedad para mostrar el género de forma legible
        public string GenderDisplay
        {
            get
            {
                return gender switch
                {
                    "M" => "Masculino",
                    "F" => "Femenino",
                    "Other" => "Otro",
                    _ => gender ?? "-"
                };
            }
        }

        /// <summary>
        /// Obtiene la edad actual del usuario
        /// </summary>
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                int age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        ///<summary>
        ///
        /// </summary>

        //LoginRequest
        public string mensaje { get; set; }
        // token JWT - esto es lo que usamos para que el servidor sepa quiénes somos
        // en las próximas peticiones. Se lo pasamos en el header "Authorization: Bearer {token}"
        public string token { get; set; }
        public Usuario user { get; set; }

    }
}