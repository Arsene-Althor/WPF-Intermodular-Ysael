using Hotel_Pere_Maria.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
    internal static class ExtraServiceCatalogService
    {
        public static async Task<List<ExtraServiceDto>> ListAsync()
        {
            string url = $"{ApiService.BaseUrl}room/extra-services";
            var list = await ApiService._httpClient.GetFromJsonAsync<List<ExtraServiceDto>>(url);
            return list ?? new List<ExtraServiceDto>();
        }

        public static async Task CreateAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nombre vacío");
            string url = $"{ApiService.BaseUrl}room/extra-services";
            using var resp = await ApiService._httpClient.PostAsJsonAsync(url, new { name = name.Trim() });
            string body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception(body);
        }
    }
}
