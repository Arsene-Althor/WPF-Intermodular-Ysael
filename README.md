# WPF — Hotel Pere María (Escritorio)

Aplicación de escritorio desarrollada con **WPF (.NET 8)** y **C#** para la gestión administrativa del Hotel Pere María. Permite a empleados y administradores gestionar usuarios, habitaciones, reservas y consultar el historial de auditoría. Se comunica con la API REST del proyecto intermodular mediante `HttpClient`.

---

## Tabla de contenidos

- [Requisitos](#requisitos)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Conexión con la API](#conexión-con-la-api)
- [Gestión de sesión](#gestión-de-sesión)
- [Módulos principales](#módulos-principales)
- [Cambios recientes](#cambios-recientes)

---

## Requisitos

- Visual Studio 2022 (con carga de trabajo .NET Desktop)
- .NET 8 SDK
- Conexión a la API del proyecto (`API-Intermodular-Ysael`)

---

## Tecnologías utilizadas

| Tecnología       | Uso                                                  |
|------------------|------------------------------------------------------|
| C# / .NET 8      | Lenguaje y framework principal                       |
| WPF (XAML)       | Interfaz de usuario con binding de datos             |
| HttpClient       | Cliente HTTP para consumir la API REST               |
| System.Text.Json | Serialización y deserialización JSON                 |
| MVVM             | Patrón de arquitectura (Model-View-ViewModel)        |

---

## Estructura del proyecto

```
Hotel_Pere_Maria/
├── App.xaml / App.xaml.cs               # Punto de entrada de la aplicación
├── MainWindow.xaml / MainWindow.xaml.cs  # Ventana de login
├── Hotel_Pere_Maria.csproj              # Configuración del proyecto (.NET 8)
│
├── Models/                              # Clases de datos
│   ├── Usuario.cs                       # Modelo de usuario
│   ├── Reservation.cs                   # Modelo de reserva
│   ├── Room.cs                          # Modelo de habitación
│   ├── BookingAuditEntry.cs             # Registro de auditoría (API)
│   └── HistorialAuditoriaFila.cs        # Fila de presentación (UI)
│
├── Services/                            # Comunicación con la API
│   ├── ApiService.cs                    # Configuración base (URL + HttpClient)
│   ├── AuthService.cs                   # Login y logout
│   ├── Session.cs                       # Datos de sesión en memoria
│   ├── ReservationService.cs            # Operaciones de reservas + auditoría
│   ├── RoomService.cs                   # Operaciones de habitaciones
│   └── UserService.cs                   # Operaciones de usuarios
│
├── ViewModels/                          # Lógica de presentación (MVVM)
│   ├── BaseViewModel.cs                 # Clase base (INotifyPropertyChanged)
│   ├── RelayCommand.cs                  # Implementación de ICommand
│   ├── LoginViewModel.cs
│   ├── InicioViewModel.cs
│   ├── ListReservasViewModel.cs
│   ├── AddReservaViewModel.cs
│   ├── ModReservaViewModel.cs
│   ├── ListRoomViewModel.cs
│   ├── ModRoomViewModel.cs
│   ├── GestionUsuariosViewModel.cs
│   ├── InsertarUsuarioViewModel.cs
│   ├── SelectedUserViewModel.cs
│   ├── PerfilUsuarioViewModel.cs
│   └── GestionarDescuentoViewModel.cs
│
├── Views/                               # Pantallas XAML
│   ├── Inicio.xaml                      # Pantalla principal (dashboard)
│   ├── listReservas.xaml                # Listado de reservas
│   ├── addReserva.xaml                  # Formulario de nueva reserva
│   ├── modReserva.xaml                  # Edición de reserva
│   ├── listRoom.xaml                    # Listado de habitaciones
│   ├── modRoom.xaml                     # Edición de habitación
│   ├── GestionUsuarios.xaml             # Gestión de usuarios
│   ├── InsertarUsuario.xaml             # Crear usuario
│   ├── SelectedUser.xaml                # Detalle de usuario
│   ├── PerfilUsuario.xaml               # Perfil del usuario logueado
│   └── GestionarDescuento.xaml          # Gestión de descuentos
│
└── Resources/                           # Imágenes y recursos
    ├── HotelLogo.png
    └── userIcon.png
```

---

## Arquitectura

La aplicación sigue el patrón **MVVM**:

```
View (XAML + Binding) → ViewModel (INotifyPropertyChanged) → Service (HttpClient) → API REST
```

- **View**: ventanas XAML con data binding a las propiedades del ViewModel.
- **ViewModel**: implementa `INotifyPropertyChanged` y `ICommand` para gestionar la lógica de presentación sin acoplar la UI.
- **Service**: clases estáticas que encapsulan las peticiones HTTP a la API.
- **Model**: clases de datos (`data class` equivalentes) con atributos `[JsonPropertyName]` para la deserialización.

---

## Conexión con la API

### `ApiService.cs`

Clase base que expone el `HttpClient` compartido y la URL del servidor:

```csharp
public static class ApiService
{
    public static readonly HttpClient _httpClient = new HttpClient();
    public const string BaseUrl = "http://51.255.198.93:3011/";
}
```

Todos los servicios (`AuthService`, `ReservationService`, `RoomService`, `UserService`) utilizan este `HttpClient` y componen la URL a partir de `BaseUrl`.

### `AuthService.cs`

Gestiona el login y logout contra la API:

```csharp
public static class AuthService
{
    public static async Task<Usuario> LoginAsync(string email, string password)
    {
        var request = new Usuario { email = email, password = password };
        string json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await ApiService._httpClient.PostAsync(
            ApiService.BaseUrl + "auth/login", content
        );

        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error {response.StatusCode}: {responseContent}");

        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<Usuario>(responseContent, opciones);
    }

    public static async Task LogoutAsync()
    {
        try { await ApiService._httpClient.PostAsync(ApiService.BaseUrl + "auth/logout", null); }
        catch { /* se ignora el error de red */ }
        finally { Session.Clear(); }
    }
}
```

---

## Gestión de sesión

### `Session.cs`

Almacena en memoria el token JWT y el objeto del usuario logueado. Está disponible globalmente desde cualquier ventana:

```csharp
public static class Session
{
    // Token JWT para las cabeceras Authorization: Bearer {token}
    public static string Token { get; set; }

    // Objeto con todos los datos del usuario logueado
    public static Usuario User { get; set; }

    // Propiedad de lectura: true si hay sesión activa
    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token) && User != null;

    // Limpia la sesión al hacer logout
    public static void Clear()
    {
        Token = null;
        User = null;
    }
}
```

Los servicios que requieren autenticación configuran la cabecera antes de cada petición:

```csharp
private static void ConfigurarCabeceras()
{
    ApiService._httpClient.DefaultRequestHeaders.Authorization = null;
    if (!string.IsNullOrEmpty(Session.Token))
    {
        ApiService._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Session.Token);
    }
}
```

---

## Módulos principales

### Reservas (`ReservationService.cs`)

Operaciones disponibles:

| Método                    | Verbo HTTP | Ruta API                            | Descripción                        |
|---------------------------|-----------|--------------------------------------|------------------------------------|
| `InsertarReserva`         | `POST`    | `/reservation/add`                   | Crear una nueva reserva            |
| `updateReservation`       | `PATCH`   | `/reservation/update`                | Actualizar una reserva existente   |
| `cancelReservation`       | `DELETE`  | `/reservation/cancel/:id?price=X`    | Cancelar una reserva               |
| `getAllActiveReservation`  | `GET`     | `/reservation/allActive`             | Obtener reservas activas           |
| `getAllReservation`        | `GET`     | `/reservation/all`                   | Obtener todas las reservas         |
| `getPriceReservation`     | `POST`    | `/reservation/getPrice`              | Calcular precio de reserva         |
| `getCancelationPrice`     | `POST`    | `/reservation/getCancelationPrice`   | Calcular penalización              |
| `GetBookingAuditAsync`    | `GET`     | `/reservation/:id/audit`             | Historial de auditoría             |

#### Cancelación con `DELETE`

```csharp
public static async Task<(bool exito, string mensaje)> cancelReservation(Reservation r, double precioCancel)
{
    double? precionew = r.price - precioCancel;
    string priceStr = (precionew ?? 0d).ToString(CultureInfo.InvariantCulture);

    // Construye la URL con el ID en la ruta y el precio en query string
    string cancelUrl = $"{ApiService.BaseUrl}reservation/cancel/{Uri.EscapeDataString(r.reservation_id)}?price={priceStr}";
    var response = await ApiService._httpClient.DeleteAsync(cancelUrl);

    string mensajeServidor = await response.Content.ReadAsStringAsync();
    return response.IsSuccessStatusCode
        ? (true, mensajeServidor)
        : (false, mensajeServidor);
}
```

#### Actualización con `PATCH`

```csharp
public static async Task<(bool exito, string mensaje)> updateReservation(Reservation reservamod)
{
    string json = JsonSerializer.Serialize(reservamod);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await ApiService._httpClient.PatchAsync(
        ApiService.BaseUrl + "reservation/update", content
    );

    string mensajeServidor = await response.Content.ReadAsStringAsync();
    return response.IsSuccessStatusCode
        ? (true, mensajeServidor)
        : (false, mensajeServidor);
}
```

### Auditoría

La aplicación WPF puede consultar el historial de auditoría de cualquier reserva.

#### Modelo (`BookingAuditEntry.cs`)

Deserializa la respuesta de `GET /reservation/{id}/audit`:

```csharp
public class BookingAuditEntry
{
    [JsonPropertyName("booking_id")]       public string BookingId { get; set; }
    [JsonPropertyName("action")]           public string Action { get; set; }
    [JsonPropertyName("actor_id")]         public string ActorId { get; set; }
    [JsonPropertyName("actor_type")]       public string ActorType { get; set; }
    [JsonPropertyName("timestamp")]        public DateTime? Timestamp { get; set; }
    [JsonPropertyName("resumen_cambios")]  public List<string> ResumenCambios { get; set; }
}
```

#### Modelo de presentación (`HistorialAuditoriaFila.cs`)

Transforma los datos de auditoría para su visualización en la interfaz:

```csharp
public class HistorialAuditoriaFila
{
    public string Accion { get; set; }
    public string ActorId { get; set; }
    public DateTime? Fecha { get; set; }

    // Formato legible para la UI: "11/05/2026 01:18"
    public string FechaFormateada =>
        Fecha.HasValue ? Fecha.Value.ToString("dd/MM/yyyy HH:mm") : "—";

    public string ResumenTexto { get; set; }
}
```

#### Servicio (`ReservationService.cs`)

```csharp
public static async Task<(bool exito, string mensaje, List<BookingAuditEntry> lista)>
    GetBookingAuditAsync(string reservation_id)
{
    ConfigurarCabeceras();
    string url = $"{ApiService.BaseUrl}reservation/{Uri.EscapeDataString(reservation_id)}/audit";
    var response = await ApiService._httpClient.GetAsync(url);
    string cuerpo = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        return (false, cuerpo, null);

    var opts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };
    var lista = JsonSerializer.Deserialize<List<BookingAuditEntry>>(cuerpo, opts)
                ?? new List<BookingAuditEntry>();

    return (true, null, lista);
}
```

### Gestión de usuarios

El módulo de usuarios permite operaciones CRUD completas (crear, listar, editar, eliminar) y gestión de descuentos VIP. Solo accesible para roles de administrador y empleado.

### Habitaciones

Permite visualizar, editar y gestionar la disponibilidad de las habitaciones del hotel.

---

## Cambios recientes

### Integración de auditoría

- Se añadieron los modelos `BookingAuditEntry` y `HistorialAuditoriaFila` para representar y visualizar el historial de cambios en reservas.
- Se implementó `GetBookingAuditAsync` en `ReservationService.cs` para consultar el endpoint `GET /reservation/:id/audit`.

### Refactorización de verbos HTTP

- **Cancelación**: migrado de `POST /cancel` (con body) a `DELETE /cancel/:reservation_id?price=X` (ID en ruta, precio en query string).
- **Actualización**: migrado de `PUT /update` a `PATCH /update` para reflejar correctamente que las actualizaciones son parciales.

---
