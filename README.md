# WPF — Hotel Pere María (Escritorio)

Aplicación de escritorio desarrollada con **WPF (.NET 8)** y **C#** para la gestión administrativa del Hotel Pere María. Permite a empleados y administradores gestionar usuarios, habitaciones (ofertas, galería, servicios extra), reservas, **panel de control** con filtros, cola de solicitudes especiales (P19), check-in en recepción, facturas y auditoría (con opción de **desactivar** el registro de nuevos eventos). Todos los datos viven en **MongoDB** en el servidor; WPF solo habla con la **API REST** (`HttpClient`).

---

## Tabla de contenidos

- [Requisitos](#requisitos)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Base de datos MongoDB (vía API)](#base-de-datos-mongodb-vía-api)
- [Conexión con la API](#conexión-con-la-api)
- [Gestión de sesión](#gestión-de-sesión)
- [Identidad visual](#identidad-visual)
- [Módulos principales](#módulos-principales)
- [Ejemplos de código](#ejemplos-de-código)
- [Check-in en recepción (panel)](#check-in-en-recepción-panel)
- [P9 · Ficha de estancias del cliente (recepción)](#p9--ficha-de-estancias-del-cliente-recepción)
- [P19 · Flexibilidad (recepción)](#p19--flexibilidad-recepción)
- [Facturas e HotelInvoice](#facturas-e-hotelinvoice)
- [Evolución del proyecto](#evolución-del-proyecto-desde-la-creación)

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
│   ├── Reservation.cs                   # Reserva (+ guest_*, reception_check_in_*)
│   ├── ReceptionCheckInStatusDto.cs     # DTO GET check-in-status
│   ├── Room.cs                          # Habitación (operativa, oferta, galería, servicios)
│   ├── ExtraServiceDto.cs               # Catálogo GET /room/extra-services
│   ├── BookingAuditEntry.cs             # Registro de auditoría (API + detalle_cambios)
│   ├── AuditChangeDetail.cs             # Campo antes/después (API)
│   ├── AuditCambioFila.cs / AuditGlobalRow.cs
│   ├── HistorialAuditoriaFila.cs        # Línea de tiempo con tabla Antes/Después
│   ├── HotelInvoiceItem.cs              # Fila GET /reservation/invoices/history
│   ├── InvoiceSettingsDto.cs            # DTO configuración factura (API)
│   ├── FlexibilitySettingsDto.cs        # P19 · reglas €/h
│   ├── FlexibilityStatusDto.cs          # P19 · estado en reserva
│   └── PendingFlexibilityItemDto.cs     # Cola solicitudes pendientes
│
├── Services/                            # Comunicación con la API
│   ├── ApiService.cs                    # Configuración base (URL + HttpClient)
│   ├── AuthService.cs                   # Login y logout
│   ├── Session.cs                       # Datos de sesión en memoria
│   ├── ReservationService.cs            # Reservas, auditoría, checkout, PDF, histórico HotelInvoice, email
│   ├── FlexibilityService.cs            # P19 · pending, review, settings
│   ├── UserStayService.cs               # P9 · history + stats por userId
│   ├── InvoiceSettingsService.cs        # GET/PUT `/settings/invoice` (datos fiscales emisor en API)
│   ├── OperationalSettingsService.cs    # GET/PUT `/settings/operational` (auditoría on/off)
│   ├── RoomService.cs                   # Habitaciones (all, one, available, update, create)
│   ├── ExtraServiceCatalogService.cs    # Catálogo de servicios extra (API)
│   └── UserService.cs                   # Operaciones de usuarios
│
├── ViewModels/                          # Lógica de presentación (MVVM)
│   ├── BaseViewModel.cs                 # Clase base (INotifyPropertyChanged)
│   ├── RelayCommand.cs                  # Implementación de ICommand
│   ├── LoginViewModel.cs
│   ├── InicioViewModel.cs               # Panel control + tarjetas check-in recepción
│   ├── ListReservasViewModel.cs
│   ├── ListFacturasViewModel.cs         # Histórico facturas (admin/empleado)
│   ├── ConfigFacturaViewModel.cs        # Datos fiscales hotel → API
│   ├── AddReservaViewModel.cs
│   ├── ModReservaViewModel.cs
│   ├── CheckInRecepcionViewModel.cs     # Check-in físico en recepción
│   ├── AuditoriaReservaViewModel.cs     # Historial de auditoría de una reserva
│   ├── ListRoomViewModel.cs
│   ├── ModRoomViewModel.cs
│   ├── GestionUsuariosViewModel.cs      # Botón «Estancias» → ficha P9
│   ├── ClientFichaEstanciasViewModel.cs # P9 historial + stats + export CSV
│   ├── SolicitudesFlexibilidadViewModel.cs
│   ├── ConfigFlexibilidadViewModel.cs
│   ├── ListAuditoriasViewModel.cs       # Auditoría global + toggle registro
│   ├── InsertarUsuarioViewModel.cs
│   ├── PerfilUsuarioViewModel.cs
│   └── GestionarDescuentoViewModel.cs
│
├── Views/                               # Pantallas XAML
│   ├── Inicio.xaml                      # Dashboard
│   ├── listReservas.xaml                # Listado de reservas
│   ├── listFacturas.xaml                # Listado de facturas emitidas (filtros + PDF + reenvío email)
│   ├── ConfigFactura.xaml               # Configuración nombre/CIF/dirección/notas fiscales/IVA
│   ├── addReserva.xaml / modReserva.xaml # Crear / editar reserva
│   ├── CheckInRecepcion.xaml            # Registro check-in recepción (clic en panel)
│   ├── AuditoriaReserva.xaml            # Ventana de auditoría
│   ├── listRoom.xaml / modRoom.xaml     # Habitaciones
│   ├── GestionUsuarios.xaml             # Gestión de usuarios (+ Estancias)
│   ├── ClientFichaEstancias.xaml        # P9 ficha huésped (admin/empleado)
│   ├── SolicitudesFlexibilidad.xaml     # Cola solicitudes pendientes (incrustada desde panel)
│   ├── ConfigFlexibilidad.xaml          # Reglas €/h check-in/check-out
│   ├── listAuditorias.xaml              # Auditoría global (columnas Antes / Después)
│   ├── InsertarUsuario.xaml
│   ├── PerfilUsuario.xaml
│   └── GestionarDescuento.xaml
│
├── Helpers/
│   ├── AuditDisplayHelper.cs            # Formato valores JSON → texto
│   └── AuditUiMapper.cs                 # API → filas UI (global + por reserva)
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

## Base de datos MongoDB (vía API)

WPF **no conecta** a MongoDB. Todas las pantallas leen y escriben datos a través de la **API REST**. La documentación completa de colecciones y relaciones está en el [README de la API — Base de datos](../API-Intermodular-Ysael/README.md#base-de-datos-mongodb-colecciones-y-relaciones).

### Qué pantalla usa qué datos

| Pantalla / módulo WPF | Colecciones Mongo (indirectas) | Endpoints principales |
|------------------------|--------------------------------|------------------------|
| Login, usuarios, perfil | `users` | `/auth/login`, `/user/*` |
| Habitaciones | `rooms`, `extraservices` | `/room/*` |
| Reservas (lista, alta, editar) | `reservations`, `users`, `rooms` | `/reservation/*` |
| Panel Inicio (tarjetas activas) | `reservations`, `rooms`, `users` | `GET /reservation/allActive` |
| Check-in recepción | `reservations` | `POST /reservation/check-in` |
| Facturas | `hotelinvoices`, `reservations` | `/reservation/invoices/history`, PDF |
| Datos factura (emisor PDF) | `invoicesettings` | `/settings/invoice` |
| Auditorías | `booking_audit_log`, `operationalsettings` | `/reservation/audits`, `/settings/operational` |
| Auditoría de una reserva | `booking_audit_log` | `GET …/:id/audit` |
| Cola flexibilidad | `reservations` (P19 embebido) | `/reservation/flexibility/pending` |
| Reglas solicitudes | `flexibilitysettings` | `/settings/flexibility` |
| Ficha estancias cliente (P9) | `reservations`, `reviews`, `clientloyaltystats` | `/users/:id/history`, `/stats` |

### Relaciones que debes conocer en recepción

- Cada **reserva** (`RSV-xxxxx`) apunta a un **huésped** (`CLI-xxxxx`) y una **habitación** (`HAB-xxx`).
- Las **facturas** emitidas viven en **`hotelinvoices`** (puede haber varias por reserva: estancia, P19, ampliación).
- El **historial de cambios** está en **`booking_audit_log`** (solo lectura en WPF); puedes **desactivar** nuevos registros para ahorrar recursos.
- La **fidelidad** del huésped para P19 sale de **`clientloyaltystats`** (un documento por cliente).

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

Fragmento del modelo (propiedades JSON más usadas en UI y persistencia):

```csharp
public class Room
{
    [JsonPropertyName("room_id")] public string RoomId { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("price_per_night")] public double PricePerNight { get; set; }
    [JsonPropertyName("images")] public List<string> Images { get; set; } = new();
    [JsonPropertyName("extra_services")] public List<string> ExtraServices { get; set; } = new();
    [JsonPropertyName("offer_active")] public bool OfferActive { get; set; }
    [JsonPropertyName("offer_percent")] public double OfferPercent { get; set; }
    [JsonPropertyName("effective_price_per_night")] public double? EffectivePricePerNight { get; set; }
    [JsonPropertyName("is_operational")] public bool IsOperational { get; set; } = true;
    [JsonPropertyName("is_occupied_now")] public bool IsOccupiedNow { get; set; }
    public bool EstaLibreAhora => IsOperational && !IsOccupiedNow;
}
```

#### Servicios — `RoomService.cs`

- **`GetRoomByIdAsync`**: `GET {BaseUrl}room/one?id={roomId}` (query correcta; ID escapado).
- **`GetAvailableRoomsAsync`**: `GET room/available?checkIn=yyyy-MM-dd&checkOut=…&guests=N` (reservas / disponibilidad administrativa si se usa).
- **`CreateRoomAsync` / `UpdateRoomAsync`**: `POST room/create`, `PUT room/update` con payload acorde al modelo extendido.

**Detalle por ID** (query correcta para la API):

```csharp
string safeId = Uri.EscapeDataString(roomId);
string url = $"{ApiService.BaseUrl}room/one?id={safeId}";
using HttpResponseMessage resp = await ApiService._httpClient.GetAsync(url);
```

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
| `DownloadBookingReceiptPdfAsync` | `GET` | `/reservation/:id/booking-receipt` (justificante, no fiscal) |
| `DownloadInvoicePdfAsync` | `GET`    | `/reservation/:id/invoice`             |
| `GetInvoicesHistoryAsync` | `GET`    | `/reservation/invoices/history`      |
| `PostInvoiceEmailAsync`   | `POST`   | `/reservation/:id/invoice/email`     |
| `PostCheckoutAsync`     | `POST`   | `/reservation/checkout`               |
| `GetReceptionCheckInStatusAsync` | `GET` | `/reservation/:id/check-in-status` |
| `PostReceptionCheckInAsync` | `POST` | `/reservation/check-in` |
| `InvoiceSettingsService.GetAsync` / `PutAsync` | `GET` / `PUT` | `/settings/invoice` |

#### Facturación P5 (escritorio)

Documentos alineados con la API (dos tipos):

| Tipo | Método servicio | Cuándo |
|------|-----------------|--------|
| **Justificante** | `DownloadBookingReceiptPdfAsync` | Cualquier reserva visible; no requiere checkout |
| **Factura fiscal** | `DownloadInvoicePdfAsync` | Solo si `invoice_number` está relleno |

- **`modReserva`**: bloque azul **Descargar justificante (PDF)** (`DescargarJustificanteCommand`, `SaveFileDialog` → `Justificante-{reservation_id}.pdf`). Si hay `invoice_number`, **Descargar factura (PDF)**. Si el usuario es **admin/empleado**, la estancia ha pasado y no hay factura: **Registrar checkout** → `POST /reservation/checkout`.
- **`listFacturas`**: incrustado desde **Inicio** (botón **Facturas**, solo personal). Carga `GET /reservation/invoices/history` (**colección `HotelInvoice`**: estancia, P19, ampliación…), **filtros** (nº factura, cliente, fechas), **Descargar** PDF (`invoice_number` en query) y **Reenviar** email (SMTP en servidor).
- **`ConfigFactura`**: **Inicio** → **Datos factura** (misma visibilidad que Facturas). Carga/guarda `GET`/`PUT /settings/invoice`: nombre comercial, CIF/NIF, dirección, texto libre “otros datos fiscales” y **IVA %** aplicado al desglose TTC en PDF (persistido en Mongo en servidor; si un texto queda vacío en BD, el PDF usa fallback `.env`).

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

### Auditoría (antes / después)

La API devuelve `resumen_cambios` y `detalle_cambios` (campo, etiqueta, valor **antes**, valor **después**). WPF los muestra en tres sitios:

| Pantalla | Qué ves |
|----------|---------|
| **Auditorías** (`listAuditorias`) | Tabla global con columnas **Antes** y **Después**; filas con detalle despliegan mini-tabla por campo |
| **Modificar reserva** → pestaña *Historial (auditoría)* | Tarjeta por evento + tabla Campo / Antes / Después |
| **Auditoría de reserva** (ventana) | Igual que la pestaña de modificar reserva |

- **`AuditUiMapper`** / **`AuditDisplayHelper`**: mapean `detalle_cambios` de la API a `AuditCambioFila` (texto formateado para fechas, números y JSON).
- **`BookingAuditEntry`**: incluye `DetalleCambios` (`AuditChangeDetail`).
- **`HistorialAuditoriaFila`**: `Cambios`, `ResumenTexto`, `TieneDetalleCambios`.
- Filtro por acción (`CREATED`, `UPDATED`, `CANCELED`, …) y resolución de nombre de actor vía listado de usuarios.

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

## Ejemplos de código

### Catálogo de servicios extra (`ExtraServiceCatalogService.cs`)

Listado y alta contra la misma API que usa Android:

```csharp
public static async Task<List<ExtraServiceDto>> ListAsync()
{
    string url = $"{ApiService.BaseUrl}room/extra-services";
    var list = await ApiService._httpClient.GetFromJsonAsync<List<ExtraServiceDto>>(url);
    return list ?? new List<ExtraServiceDto>();
}

public static async Task CreateAsync(string name)
{
    string url = $"{ApiService.BaseUrl}room/extra-services";
    using var resp = await ApiService._httpClient.PostAsJsonAsync(url, new { name = name.Trim() });
    // … comprobar IsSuccessStatusCode
}
```

### Cuerpo típico al actualizar habitación (`PUT /room/update`)

El ViewModel suele construir un objeto anónimo o DTO con los campos que el controlador Node espera; ejemplo mínimo ilustrativo:

```json
{
  "room_id": "HAB-001",
  "type": "Suite",
  "description": "Vistas al mar",
  "price_per_night": 120,
  "max_occupancy": 3,
  "isOperational": true,
  "images": ["https://ejemplo.com/a.jpg", "https://ejemplo.com/b.jpg"],
  "extra_services": ["EXT-001", "EXT-002"],
  "offer_active": true,
  "offer_percent": 15
}
```

---

## Evolución del proyecto (desde la creación)

Resumen de **funcionalidades que se fueron sumando** al escritorio y cómo encajan entre sí.

### 1. Aplicación base administrativa

- **Login** con JWT (`AuthService`, `Session`), **dashboard** (`Inicio`) y navegación a módulos de reservas, habitaciones y usuarios.
- Comunicación HTTP centralizada en `ApiService` (`BaseUrl` + `HttpClient` compartido).

### 2. Gestión de reservas y verbos REST

- Alta, listado, modificación y cancelación alineados con la API: **`PATCH`** para actualizar, **`DELETE`** para cancelar con `reservation_id` y precio en query.
- Listados de reservas activas y totales según permisos del rol.

### 3. Auditoría de reservas

- Ventana **`AuditoriaReserva`** + `AuditoriaReservaViewModel`: consume `GET /reservation/{id}/audit`, **traduce** acciones al español, **resuelve nombres** de actores y permite **filtrar** por tipo de acción.
- Modelos `BookingAuditEntry` / `HistorialAuditoriaFila` separan JSON de API de filas presentables en grid.

### 4. Habitaciones operativas y estado en tiempo real

- Modelo `Room` con `IsOperational` e `IsOccupiedNow` (la API los calcula / normaliza).
- Textos derivados `EstadoServicioTexto`, `OcupacionTexto`, `EstaLibreAhora` para bindings XAML sin código en code-behind.

### 5. Identidad visual e infraestructura UI

- **`Themes/AppTheme.xaml`**: paleta y estilos unificados.
- **`BoolToVisibilityConverter`**, **`ImageUrlConverter`**: menos lógica condicional repetida en XAML.
- **`UiShell`**: `Owner` correcto en ventanas modales.

### 6. Galería, ofertas y servicios extra (paridad con API y Android)

- **Campos nuevos** en `Room`: listas `Images`, `ExtraServices`, ofertas y `EffectivePricePerNight` opcional desde la API.
- **`ExtraServiceCatalogService`**: carga checkboxes en **modificar habitación** y permite **crear** un servicio nuevo (el servidor asigna `EXT-xxx`).
- **`modRoom`**: UI tipo **tarjeta** (fondo blanco, bordes redondeados, sombra suave) con secciones para oferta, URLs de galería y servicios.
- **`RoomService.GetRoomByIdAsync`**: corrección a **`GET room/one?id=…`** para compatibilidad con el contrato actual de la API.

### 7. Estabilidad del código

- Ajustes en ViewModels (p. ej. eliminación de miembros duplicados) para mantener **compilación limpia** tras el crecimiento del formulario de habitación.

### 8. Justificante PDF en modificar reserva

- **`ReservationService.DownloadBookingReceiptPdfAsync`**: `GET /reservation/{id}/booking-receipt`.
- **`ModReservaViewModel`**: `DescargarJustificanteCommand` guarda el PDF en disco para que recepción o el huésped dispongan de comprobante **antes** del checkout fiscal (paridad con Android).

### 9. Check-in en recepción (panel de control)

Ver también [Check-in en recepción (panel)](#check-in-en-recepción-panel) más arriba en este README.

### 10. Solicitudes check-in / check-out (recepción)

- **Panel inicio:** banner «Check-in / check-out especiales» + **Ver cola** (única entrada a la cola; no hay botón duplicado en la barra).
- **modReserva:** bloque solicitudes + Aprobar/Rechazar por tipo (entrada / salida).
- **Reglas solicitudes** en barra superior → `ConfigFlexibilidad` (€/h, auto-aprobación).
- Android: botones separados; WPF aprueba bronce y configura tarifas.
- [API P19](../API-Intermodular-Ysael/README.md#p19--flexibilidad-entrada-anticipada--salida-tardía).

### 11. P9 · Ficha estancias (gestión usuarios)

- Botón **Estancias** → `ClientFichaEstancias` + `UserStayService` (`/users/:id/history|stats`, export CSV).

### 12. Facturas HotelInvoice

- `listFacturas` + `HotelInvoiceItem`: varios tipos por reserva; PDF con `invoice_number`.

### 13. Panel Inicio, cola flex y auditoría configurable (estado actual)

- **Inicio:** scroll completo, filtro y orden de reservas activas (`InicioViewModel`).
- **Cola:** pestañas Activas/Inactivas; diálogo rechazo ampliado (`FlexibilityReviewNoteDialog`).
- **Auditorías:** interruptor «Registrar auditorías» → `OperationalSettingsService` / `operationalsettings` en Mongo.

### 14. Auditoría con antes / después en UI

- Deserialización de `detalle_cambios` desde la API.
- Tabla **Antes / Después** en auditoría global, pestaña historial de reserva y ventana `AuditoriaReserva`.
- Eliminado flujo legacy `SelectedUser` (selector de usuario vía `GestionUsuarios.ShowPickerDialog`).

---

## P9 · Ficha de estancias del cliente (recepción)

| Elemento | Descripción |
|----------|-------------|
| **Acceso** | Gestión usuarios → usuario → **Estancias** |
| **Ventana** | `ClientFichaEstancias.xaml` + `ClientFichaEstanciasViewModel` |
| **API** | `GET /users/{id}/history`, `GET /users/{id}/stats` |
| **UI** | Resumen (noches, gasto, racha, temporada, habitación top) + historial + **Exportar CSV** |

Huésped: app Android **Estadísticas** + **Mis estancias**. Personal: esta ficha sobre cualquier cliente.

Ver [APP — P9](../APP-Intermodular-Ysael/README.md#p9--mis-estadísticas-cliente) · [API — P9 historial](../API-Intermodular-Ysael/README.md#p9--historial-de-estancias-por-usuario).

---

## P19 · Flexibilidad (recepción)

Entrada **antes de 12:00** o salida **después de 11:00** el **mismo día** (no confundir con **ampliar estancia** en Android). La interfaz WPF **no** muestra la etiqueta «P19» al usuario.

### Panel de control (`Inicio`)

- Banner **Check-in / check-out especiales** con contador de pendientes del día.
- Botón **Ver cola** → incrusta `SolicitudesFlexibilidad` (sustituye el antiguo acceso «Solic. flex.» del menú).
- **Reservas actuales:** área con scroll vertical (sin límite fijo de altura), **filtro** por ID/habitación/nombre/DNI y **orden** (salida, entrada, habitación, cliente, retrasos primero).
- Contador `X de Y reserva(s)` según filtro aplicado.

### Detalle de reserva (`modReserva`)

- Bloque **SOLICITUDES CHECK-IN / CHECK-OUT** (admin/empleado).
- Subbloques **Check-in anticipado** y **Check-out tardío** (estado, hora, horas, suplemento, disponibilidad, modo aprobación, nota).
- **Aprobar / Rechazar** si `pending` (nota opcional → `review_note` en API).
- **Actualizar** → `GET /reservation/:id/flexibility`.

### Cola del día (`SolicitudesFlexibilidad`)

- Acceso desde panel inicio (**Ver cola**), no desde barra superior.
- **DatePicker** por día; pestañas **Activas** / **Inactivas**; orden por fecha (más recientes o más antiguas).
- Aprobar / rechazar (diálogo de nota **más grande**, redimensionable) / ver motivo rechazo / abrir reserva.
- Título en pantalla: «Solicitudes del día».

### Auditorías globales (`listAuditorias`)

- Checkbox **Registrar auditorías** → `PUT /settings/operational` (`booking_audit_enabled`).
- Con auditoría desactivada, la API deja de insertar en `booking_audit_log` (los registros antiguos siguen consultables).
- **DataGrid:** columnas Fecha, Reserva, Acción, Actor, **Antes**, **Después** (resumen por evento).
- Si hay `detalle_cambios`, la fila muestra debajo una **tabla por campo** (Campo · Antes · Después).
- Filtros de carga (reserva, actor, acción) + búsqueda local en texto, Antes y Después.

### Configuración (`ConfigFlexibilidad`)

- Barra superior: **Reglas solicitudes** (antes «Reglas flex.»).
- **Acceso gratuito** por rango (bronce/plata/oro).
- **€/h** entrada y salida; descuentos %; mín. horas facturables; tope €.
- **Límites:** hora mín. early, hora máx. late, máx. horas adelanto/retraso.
- Email al resolver; nota sobre auto-aprobación plata/oro en servidor.

| Acción | Ruta API |
|--------|----------|
| Estado | `GET /reservation/:id/flexibility` |
| Cola | `GET /reservation/flexibility/pending?day=` |
| Revisar | `PATCH …/early-checkin/review` · `…/late-checkout/review` |
| Settings | `GET/PUT /settings/flexibility` |

Plata/oro: auto si hay hueco. Bronce: `pending` hasta revisión (API revalida disponibilidad al aprobar).

---

## Facturas e HotelInvoice

| Concepto | WPF |
|----------|-----|
| Listado | `listFacturas` ← `GET /reservation/invoices/history` |
| Fila | `HotelInvoiceItem` (`type`, `amount`, `invoice_number`) |
| PDF | Descarga con `invoice_number` cuando hay varias por RSV |
| Checkout | `POST /reservation/checkout` en modificar reserva |
| Emisor | `ConfigFactura` → `/settings/invoice` |

[API — HotelInvoice](../API-Intermodular-Ysael/README.md#colección-hotelinvoice-facturación-multi-concepto).

---

## Check-in en recepción (panel)

Registro de la **llegada física** del huésped (no confundir con la fecha `check_in` de la reserva ni con P19 “entrada anticipada”).

| Elemento | Descripción |
|----------|-------------|
| **API** | `POST /reservation/check-in`, `GET …/check-in-status`; campos `reception_check_in_at`, `reception_check_in_late`, `reception_check_in_late_fee` |
| **Ventana** | Día de entrada, 12:00–22:00; fuera → check-in tardío con recargo (`CHECK_IN_LATE_FEE_EUR`, default 25 €) |
| **Inicio** | Tarjetas con `guest_name`, `guest_dni`, badge “Check-in ✓”; clic → `CheckInRecepcion.xaml` |
| **Servicios** | `GetReceptionCheckInStatusAsync`, `PostReceptionCheckInAsync` en `ReservationService.cs` |

**Nota XAML:** bindings en `Run.Text` de propiedades solo lectura usan `Mode=OneWay` (evita `InvalidOperationException`).

---
