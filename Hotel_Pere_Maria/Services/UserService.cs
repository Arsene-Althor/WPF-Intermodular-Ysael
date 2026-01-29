using Hotel_Pere_Maria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Services
{
    /// <summary>
    /// Servicio centralizado para todas las operaciones CRUD de usuarios
    /// Gestiona la comunicación con la API REST del backend
    /// </summary>
    public static class UserService
    {
        private const string BaseUrl = "http://localhost:3000/api";
        private static HttpClient _client = ApiService._httpClient;

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        public static async Task<List<Usuario>> GetAllUsersAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{BaseUrl}/employees");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var usuarios = JsonSerializer.Deserialize<List<Usuario>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return usuarios ?? new List<Usuario>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error al conectar con la API: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error al procesar respuesta del servidor: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un nuevo empleado (admin puede crear admin o employee)
        /// </summary>
        public static async Task<Usuario> AddUserAsync(Usuario usuario)
        {
            try
            {
                ValidarUsuarioParaCreacion(usuario);

                var content = JsonContent.Create(usuario);
                var response = await _client.PostAsync($"{BaseUrl}/employees", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(ExtraerMensajeError(errorContent));
                }

                var json = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var userElement = doc.RootElement.GetProperty("user");
                    var resultado = JsonSerializer.Deserialize<Usuario>(userElement.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Modifica un usuario existente (incluyendo estado VIP y descuento)
        /// </summary>
        public static async Task<Usuario> ModifyUserAsync(string userId, Usuario usuarioActualizado)
        {
            try
            {
                ValidarUsuarioParaActualizacion(usuarioActualizado);

                var content = JsonContent.Create(usuarioActualizado);
                var response = await _client.PatchAsync($"{BaseUrl}/employees/{userId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(ExtraerMensajeError(errorContent));
                }

                var json = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var userElement = doc.RootElement.GetProperty("user");
                    var resultado = JsonSerializer.Deserialize<Usuario>(userElement.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al modificar usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un usuario del sistema (soft delete - marca como inactivo)
        /// </summary>
        public static async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var response = await _client.DeleteAsync($"{BaseUrl}/employees/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(ExtraerMensajeError(errorContent));
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un usuario específico por su ID
        /// </summary>
        public static async Task<Usuario> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _client.GetAsync($"{BaseUrl}/employees/{userId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var usuario = JsonSerializer.Deserialize<Usuario>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return usuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtra usuarios localmente en tiempo real (sin hacer llamadas a API)
        /// Recibe la lista completa y filtra por los criterios especificados
        /// </summary>
        public static List<Usuario> FiltrarUsuarios(List<Usuario> usuarios,
            string searchId = "", string searchNombre = "", string searchEmail = "",
            string searchDni = "", string searchApellido = "",
            string rolFiltro = "", bool? estadoFiltro = null, bool? vipFiltro = null)
        {
            if (usuarios == null) return new List<Usuario>();

            var resultado = usuarios;

            // Filtro por ID
            if (!string.IsNullOrWhiteSpace(searchId))
                resultado = resultado.Where(u => u.user_id.ToLower().Contains(searchId.ToLower().Trim())).ToList();

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(searchNombre))
                resultado = resultado.Where(u => u.name.ToLower().Contains(searchNombre.ToLower().Trim())).ToList();

            // Filtro por apellido
            if (!string.IsNullOrWhiteSpace(searchApellido))
                resultado = resultado.Where(u => u.surname.ToLower().Contains(searchApellido.ToLower().Trim())).ToList();

            // Filtro por email
            if (!string.IsNullOrWhiteSpace(searchEmail))
                resultado = resultado.Where(u => u.email.ToLower().Contains(searchEmail.ToLower().Trim())).ToList();

            // Filtro por DNI
            if (!string.IsNullOrWhiteSpace(searchDni))
                resultado = resultado.Where(u => u.dni.ToLower().Contains(searchDni.ToLower().Trim())).ToList();

            // Filtro por rol
            if (!string.IsNullOrWhiteSpace(rolFiltro))
                resultado = resultado.Where(u => u.role == rolFiltro).ToList();

            // Filtro por estado
            if (estadoFiltro.HasValue)
                resultado = resultado.Where(u => u.isActive == estadoFiltro.Value).ToList();

            // Filtro por VIP
            if (vipFiltro.HasValue)
                resultado = resultado.Where(u => u.isVIP == vipFiltro.Value).ToList();

            return resultado;
        }

        /// <summary>
        /// Valida que los datos sean correctos antes de crear un usuario
        /// </summary>
        private static void ValidarUsuarioParaCreacion(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.name))
                throw new ArgumentException("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(usuario.surname))
                throw new ArgumentException("El apellido es obligatorio");

            if (string.IsNullOrWhiteSpace(usuario.email) || !usuario.email.Contains("@"))
                throw new ArgumentException("El email es obligatorio y debe ser válido");

            if (string.IsNullOrWhiteSpace(usuario.dni))
                throw new ArgumentException("El DNI es obligatorio");

            if (usuario.birthDate == DateTime.MinValue)
                throw new ArgumentException("La fecha de nacimiento es obligatoria");

            if (string.IsNullOrWhiteSpace(usuario.gender))
                throw new ArgumentException("El género es obligatorio");

            if (!usuario.IsEmployee)
                throw new ArgumentException("El rol debe ser admin o employee");
        }

        /// <summary>
        /// Valida datos antes de actualizar un usuario
        /// </summary>
        private static void ValidarUsuarioParaActualizacion(Usuario usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.name) && string.IsNullOrWhiteSpace(usuario.name.Trim()))
                throw new ArgumentException("El nombre no puede estar vacío");

            if (!string.IsNullOrWhiteSpace(usuario.surname) && string.IsNullOrWhiteSpace(usuario.surname.Trim()))
                throw new ArgumentException("El apellido no puede estar vacío");

            if (usuario.isVIP && (usuario.vipDiscount < 10 || usuario.vipDiscount > 30))
                throw new ArgumentException("El descuento VIP debe estar entre 10 y 30%");
        }

        /// <summary>
        /// Extrae el mensaje de error de la respuesta JSON del servidor
        /// </summary>
        private static string ExtraerMensajeError(string responseContent)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("error", out var errorProp))
                        return errorProp.GetString();

                    if (root.TryGetProperty("message", out var messageProp))
                        return messageProp.GetString();

                    return "Error desconocido del servidor";
                }
            }
            catch
            {
                return "Error al procesar respuesta del servidor";
            }
        }
    }
}