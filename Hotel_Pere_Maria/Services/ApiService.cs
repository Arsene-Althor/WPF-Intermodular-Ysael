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
    //Clase baseica para conexión con la base de datos
    public static class ApiService
    {
        // Creada conexión http y url base para conexiones
        public static readonly HttpClient _httpClient = new HttpClient();
        public const string BaseUrl = "http://localhost:3000/";

    }

}
