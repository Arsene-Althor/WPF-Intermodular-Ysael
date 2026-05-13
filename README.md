# WPF — Hotel Pere María (Escritorio)

Aplicación de escritorio desarrollada con **WPF (.NET 8)** y **C#** para la gestión administrativa del Hotel Pere María. Permite a empleados y administradores gestionar usuarios, habitaciones (incluidas **ofertas**, **galería** y **servicios extra** vía API), reservas y consultar el historial de auditoría. Se comunica con la API REST del proyecto intermodular mediante `HttpClient`.

---

## Tabla de contenidos

- [Requisitos](#requisitos)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Conexión con la API](#conexión-con-la-api)
- [Gestión de sesión](#gestión-de-sesión)
- [Identidad visual](#identidad-visual)
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

## Identidad visual

La interfaz de escritorio busca un aspecto **profesional y legible**, cercano a un panel de administración moderno:

- **Recursos globales** en `Themes/AppTheme.xaml`: paleta y estilos compartidos (menos duplicación en XAML).
- **Formularios de gestión** (p. ej. habitación): superficie **blanca** con borde redondeado y **sombra muy suave**, márgenes amplios y jerarquía tipográfica clara (Segoe UI).
- **Converters** (`BoolToVisibilityConverter`, `ImageUrlConverter`) mantienen el código-behind limpio y evitan lógica visual repetida.

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
│   ├── Room.cs                          # Habitación (operativa, oferta, galería, servicios)
│   ├── ExtraServiceDto.cs               # Catálogo GET /room/extra-services
│   ├── BookingAuditEntry.cs             # Registro de auditoría (API)
│   └── HistorialAuditoriaFila.cs        # Fila de presentación para auditoría (UI)
│
├── Services/                            # Comunicación con la API
│   ├── ApiService.cs                    # Configuración base (URL + HttpClient)
│   ├── AuthService.cs                   # Login y logout
│   ├── Session.cs                       # Datos de sesión en memoria
│   ├── ReservationService.cs            # Operaciones de reservas + auditoría
│   ├── RoomService.cs                   # Habitaciones (all, one, available, update, create)
│   ├── ExtraServiceCatalogService.cs    # Catálogo de servicios extra (API)
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

Propiedades relevantes (mapeo JSON con `System.Text.Json`):

| Área | Propiedades |
|------|----------------|
| Identificación | `RoomId`, `Type`, `Description`, `MaxOccupancy`, `Rate` |
| Precio | `PricePerNight`, `OfferActive`, `OfferPercent`, `EffectivePricePerNight` |
| Multimedia | `Image` (legacy / join), `Images` (lista) |
| Servicios | `ExtraServices` (IDs del catálogo, p. ej. `EXT-001`) |
| Estado | `IsOperational`, `IsOccupiedNow`; helpers `EstaLibreAhora`, `EstadoServicioTexto`, `OcupacionTexto` |

La API devuelve ya `effective_price_per_night` e `images` normalizados; el escritorio los consume tal cual para mantener **paridad** con la app Android.

#### Servicios — `RoomService.cs`

- **`GetRoomByIdAsync`**: `GET {BaseUrl}room/one?id={roomId}` (query correcta; ID escapado).
- **`GetAvailableRoomsAsync`**: `GET room/available?checkIn=yyyy-MM-dd&checkOut=…&guests=N` (reservas / disponibilidad administrativa si se usa).
- **`CreateRoomAsync` / `UpdateRoomAsync`**: `POST room/create`, `PUT room/update` con payload acorde al modelo extendido.

#### UI — `modRoom.xaml` + `ModRoomViewModel.cs`

- Tarjeta central **blanca**, bordes redondeados y sombra ligera (coherente con el resto del shell).
- Secciones para **oferta** (activar + porcentaje), **galería** (URLs por línea, añadir/quitar) y **servicios extra** (catálogo + crear nombre nuevo → API genera `EXT-xxx`).

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

### Habitaciones, ofertas y catálogo (2026)

- Modelo `Room` ampliado: galería (`Images` / `Image`), `ExtraServices`, ofertas y `EffectivePricePerNight` desde la API.
- **`ExtraServiceDto`** + **`ExtraServiceCatalogService`**: consumo de `GET/POST /room/extra-services` para rellenar checkboxes y crear servicios nuevos desde `modRoom`.
- **`RoomService.GetRoomByIdAsync`**: corrección a `GET room/one?id=…` (antes la URL podía construirse de forma incorrecta).
- **`modRoom.xaml` / `ModRoomViewModel`**: formulario renovado (estética tipo tarjeta, oferta, galería URL, servicios); build estable tras limpieza del ViewModel.

### Identidad visual e infraestructura

- Sección dedicada arriba; **`AppTheme.xaml`** y converters como base de UI consistente.
- **`UiShell.cs`**: ventana propietaria correcta en modales.

### Funcionalidad ya documentada (recordatorio breve)

- Auditoría de reserva en ventana dedicada con filtros y textos en español.
- Reservas: `DELETE` cancelación, `PATCH` actualización.

---
