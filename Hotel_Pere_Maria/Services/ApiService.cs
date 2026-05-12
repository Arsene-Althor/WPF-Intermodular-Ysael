using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    // Clase básica para conexión con la API (Node/Express)
    public static class ApiService
    {
        public static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// URL base de la API (debe terminar en /). Por defecto alineado con <c>.env</c> del repo API (PORT=3011).
        /// Sobrescribe con <c>HOTEL_API_BASE</c> si usas otro host o puerto (ej. http://localhost:3000/).
        /// </summary>
        public static string BaseUrl { get; } = NormalizarBaseUrl(
            Environment.GetEnvironmentVariable("HOTEL_API_BASE") ?? "http://localhost:3011/");

        private static string NormalizarBaseUrl(string url)
        {
            url = (url ?? "").Trim();
            if (string.IsNullOrEmpty(url))
                return "http://localhost:3011/";
            return url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";
        }
    }

}
