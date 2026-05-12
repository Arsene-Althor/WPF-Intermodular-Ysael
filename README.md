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
├── UiShell.cs                           # Helper para Owner de ventanas modales
│
├── Models/                              # Clases de datos
│   ├── Usuario.cs                       # Modelo de usuario
│   ├── Reservation.cs                   # Modelo de reserva
│   ├── Room.cs                          # Modelo de habitación (isOperational, isOccupiedNow)
│   ├── BookingAuditEntry.cs             # Registro de auditoría (API)
│   └── HistorialAuditoriaFila.cs        # Fila de presentación para auditoría (UI)
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
│   ├── InicioViewModel.cs               # Dashboard principal
│   ├── ListReservasViewModel.cs
│   ├── AddReservaViewModel.cs
│   ├── ModReservaViewModel.cs
│   ├── AuditoriaReservaViewModel.cs     # Historial de auditoría de una reserva
│   ├── ListRoomViewModel.cs
│   ├── ModRoomViewModel.cs
│   ├── GestionUsuariosViewModel.cs
│   ├── InsertarUsuarioViewModel.cs
│   ├── SelectedUserViewModel.cs
│   ├── PerfilUsuarioViewModel.cs
│   └── GestionarDescuentoViewModel.cs
│
├── Views/                               # Pantallas XAML
│   ├── Inicio.xaml                      # Dashboard
│   ├── listReservas.xaml                # Listado de reservas
│   ├── addReserva.xaml / modReserva.xaml # Crear / editar reserva
│   ├── AuditoriaReserva.xaml            # Ventana de auditoría
│   ├── listRoom.xaml / modRoom.xaml     # Habitaciones
│   ├── GestionUsuarios.xaml             # Gestión de usuarios
│   ├── InsertarUsuario.xaml / SelectedUser.xaml
│   ├── PerfilUsuario.xaml
│   └── GestionarDescuento.xaml
│
├── Converters/                          # Value Converters para XAML
│   ├── BoolToVisibilityConverter.cs     # Bool → Visible/Collapsed (con soporte "invert")
│   └── ImageUrlConverter.cs             # Validación y carga de URLs de imagen
│
├── Themes/
│   └── AppTheme.xaml                    # Estilos y recursos globales de la aplicación
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
- **ViewModel**: implementa `INotifyPropertyChanged` y `ICommand` para gestionar la lógica de presentación.
- **Service**: clases estáticas que encapsulan las peticiones HTTP a la API.
- **Model**: clases de datos con atributos `[JsonPropertyName]` para la deserialización.
- **Converters**: implementaciones de `IValueConverter` para transformar datos en la vista (por ejemplo, `bool` → `Visibility`).

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

### `AuthService.cs`

Gestiona login y logout:

```csharp
public static async Task<Usuario> LoginAsync(string email, string password)
{
    var request = new Usuario { email = email, password = password };
    string json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await ApiService._httpClient.PostAsync(
        ApiService.BaseUrl + "auth/login", content
    );
    // Deserializa la respuesta a un objeto Usuario
}
```

---

## Gestión de sesión

### `Session.cs`

Almacena en memoria el token JWT y el objeto del usuario logueado:

```csharp
public static class Session
{
    public static string Token { get; set; }
    public static Usuario User { get; set; }
    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token) && User != null;

    public static void Clear()
    {
        Token = null;
        User = null;
    }
}
```

Los servicios que requieren autenticación configuran la cabecera `Authorization: Bearer` antes de cada petición:

```csharp
private static void ConfigurarCabeceras()
{
    ApiService._httpClient.DefaultRequestHeaders.Authorization = null;
    if (!string.IsNullOrEmpty(Session.Token))
        ApiService._httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Session.Token);
}
```

### `UiShell.cs`

Utilidad para obtener la ventana activa como `Owner` de diálogos modales:

```csharp
public static class UiShell
{
    public static Window? OwnerWindow =>
        Application.Current?.MainWindow
        ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
}
```

---

## Módulos principales

### Habitaciones

#### Modelo — `Room.cs`

```csharp
public class Room
{
    [JsonPropertyName("room_id")]         public string RoomId { get; set; }
    [JsonPropertyName("type")]            public string Type { get; set; }
    [JsonPropertyName("description")]     public string Description { get; set; }
    [JsonPropertyName("image")]           public string Image { get; set; }
    [JsonPropertyName("price_per_night")] public double PricePerNight { get; set; }
    [JsonPropertyName("max_occupancy")]   public int MaxOccupancy { get; set; }

    /// <summary>En servicio (true) o fuera de servicio (false).</summary>
    [JsonPropertyName("is_operational")]  public bool IsOperational { get; set; } = true;

    /// <summary>Calculado por API: hay reserva activa sin cancelar.</summary>
    [JsonPropertyName("is_occupied_now")] public bool IsOccupiedNow { get; set; }

    /// <summary>Libre ahora: en servicio y sin huésped en curso.</summary>
    public bool EstaLibreAhora => IsOperational && !IsOccupiedNow;

    [JsonIgnore] public string EstadoServicioTexto => IsOperational ? "En servicio" : "Fuera de servicio";
    [JsonIgnore] public string OcupacionTexto => IsOccupiedNow ? "Reservada ahora" : "Libre ahora";
}
```

- **`IsOperational`**: controlado por el empleado/administrador desde la vista de habitaciones.
- **`IsOccupiedNow`**: calculado en la API a partir de reservas activas.
- **`EstaLibreAhora`**: propiedad calculada localmente para la interfaz.
- **`EstadoServicioTexto`** / **`OcupacionTexto`**: textos para binding directo en XAML.

### Reservas (`ReservationService.cs`)

| Método                    | Verbo    | Ruta API                            |
|---------------------------|----------|--------------------------------------|
| `InsertarReserva`         | `POST`   | `/reservation/add`                   |
| `updateReservation`       | `PATCH`  | `/reservation/update`                |
| `cancelReservation`       | `DELETE` | `/reservation/cancel/:id?price=X`    |
| `getAllActiveReservation`  | `GET`    | `/reservation/allActive`             |
| `getAllReservation`        | `GET`    | `/reservation/all`                   |
| `getPriceReservation`     | `POST`   | `/reservation/getPrice`              |
| `getCancelationPrice`     | `POST`   | `/reservation/getCancelationPrice`   |
| `GetBookingAuditAsync`    | `GET`    | `/reservation/:id/audit`             |

#### Cancelación con `DELETE`

```csharp
string cancelUrl = $"{ApiService.BaseUrl}reservation/cancel/{Uri.EscapeDataString(r.reservation_id)}?price={priceStr}";
var response = await ApiService._httpClient.DeleteAsync(cancelUrl);
```

#### Actualización con `PATCH`

```csharp
var response = await ApiService._httpClient.PatchAsync(
    ApiService.BaseUrl + "reservation/update", content
);
```

### Auditoría

#### `AuditoriaReservaViewModel.cs`

ViewModel dedicado a la ventana de historial de auditoría de una reserva. Características principales:

- **Carga asíncrona** del historial con `CargarHistorialAsync()`.
- **Resolución de nombres**: mapea `actor_id` a nombres completos consultando la lista de usuarios.
- **Filtrado por acción**: permite filtrar por `CREATED`, `UPDATED`, `CANCELED`, etc.
- **Traducción de acciones**: convierte las acciones de la API a textos legibles en español.

```csharp
public class AuditoriaReservaViewModel : BaseViewModel
{
    public ObservableCollection<HistorialAuditoriaFila> HistorialFilas { get; }
    public ObservableCollection<string> FiltrosAccion { get; }  // "Todas", "CREATED", "UPDATED"...
    public string FiltroAccionSeleccionado { get; set; }        // Filtra al cambiar

    public async Task CargarHistorialAsync(bool forzar = false)
    {
        var (ok, err, lista) = await ReservationService.GetBookingAuditAsync(_reservationId);
        // Resuelve nombres de actores
        // Traduce acciones: "CREATED" → "Alta de reserva", "CANCELED" → "Cancelación"
        // Aplica filtro seleccionado
    }
}
```

#### Modelos de auditoría

- **`BookingAuditEntry.cs`**: deserializa `GET /reservation/:id/audit`, incluye `ResumenCambios`.
- **`HistorialAuditoriaFila.cs`**: modelo de presentación con `FechaFormateada` (`dd/MM/yyyy HH:mm`) y `ResumenTexto`.

### Converters

#### `BoolToVisibilityConverter.cs`

Convierte valores booleanos a `Visibility` en XAML, con soporte para inversión mediante parámetro:

```csharp
// Uso en XAML:
// Visibility="{Binding IsOperational, Converter={StaticResource BoolToVisibility}}"
// Visibility="{Binding IsOperational, Converter={StaticResource BoolToVisibility}, ConverterParameter=invert}"
```

#### `ImageUrlConverter.cs`

Valida y convierte URLs de imagen para su uso en controles `Image` de WPF.

### Temas — `AppTheme.xaml`

Diccionario de recursos XAML que define los estilos globales de la aplicación: colores, tipografías y estilos de controles reutilizables.

---

## Cambios recientes

### Habitaciones — `IsOperational` e `IsOccupiedNow`

- El modelo `Room.cs` incorpora los nuevos campos `IsOperational` e `IsOccupiedNow` (mapeados desde `is_operational` e `is_occupied_now`).
- Se añadieron las propiedades calculadas `EstaLibreAhora`, `EstadoServicioTexto` y `OcupacionTexto` para binding en XAML.
- La vista de habitaciones permite activar/desactivar el estado operativo de una habitación.

### Auditoría — Vista completa

- Nueva vista `AuditoriaReserva.xaml` con su ViewModel `AuditoriaReservaViewModel.cs`.
- Carga el historial de auditoría, resuelve nombres de actores, y permite filtrar por tipo de acción.
- Las acciones se traducen al español (`"CREATED"` → `"Alta de reserva"`, etc.).

### Infraestructura UI

- **`UiShell.cs`**: utilidad para obtener la ventana `Owner` correcta en diálogos modales.
- **`Converters/`**: nuevos value converters (`BoolToVisibilityConverter`, `ImageUrlConverter`) para simplificar la lógica visual en XAML.
- **`Themes/AppTheme.xaml`**: diccionario de recursos globales con estilos unificados para toda la aplicación.

### Refactorización de verbos HTTP

- Cancelación: `DELETE /cancel/:reservation_id?price=X`.
- Actualización: `PATCH /update`.

---
