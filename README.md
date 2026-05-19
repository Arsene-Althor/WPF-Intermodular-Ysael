# WPF — Propuestas P11, P5, P9 y P19

Implementación en escritorio (recepción / administración) de las cuatro propuestas del Proyecto Individual. Consume la [API](../API-Intermodular-Ysael/README.md) con JWT (`Session.Token`).

---

## P11 · Auditoría completa de cambios en la reserva

### Detalle de reserva — pestaña «Historial»

| Elemento | Implementación |
|----------|----------------|
| Ubicación | `modReserva.xaml` — pestaña **Historial (auditoría)** |
| ViewModel | `ModReservaViewModel` — carga `GET /reservation/{id}/audit` |
| Línea de tiempo | Tarjetas por evento: acción traducida, actor (nombre resuelto vía listado usuarios), fecha/hora |
| Diferencia de estado | Tabla **Campo · Antes · Después** a partir de `detalle_cambios` de la API |
| Filtro | Combo por tipo de acción (`CREATED`, `UPDATED`, `CANCELED`, …) sobre caché local |

### Ventana dedicada

- `AuditoriaReserva.xaml` + `AuditoriaReservaViewModel` — misma información en ventana modal (acceso desde listado de reservas).

### Auditoría global

| Elemento | Implementación |
|----------|----------------|
| Pantalla | `listAuditorias.xaml` — menú **Inicio → Auditorías** |
| API | `GET /reservation/audits` |
| Columnas | Fecha, reserva, acción, actor, **Antes**, **Después**; fila expandible con detalle por campo |
| Registro on/off | Checkbox **Registrar auditorías** → `PUT /settings/operational` (`booking_audit_enabled`) |

**Helpers:** `AuditUiMapper.cs`, `AuditDisplayHelper.cs` — formatean JSON de la API para la UI.

---

## P5 · Factura en PDF descargable

### Detalle de reserva

| Elemento | Implementación |
|----------|----------------|
| Botón | **Descargar factura (PDF)** en `modReserva` — visible si `invoice_number` no está vacío |
| Servicio | `ReservationService.DownloadInvoicePdfAsync` → `GET /reservation/{id}/invoice` |
| Guardado | `SaveFileDialog` → nombre tipo `Factura-FAC-2026-0001.pdf` |
| Checkout | Si la estancia terminó y no hay factura: **Registrar checkout** → `POST /reservation/checkout` (solo admin/empleado) |

### Módulo de facturas

| Elemento | Implementación |
|----------|----------------|
| Pantalla | `listFacturas.xaml` — **Inicio → Facturas** |
| Listado | `GET /reservation/invoices/history` — todas las facturas (`HotelInvoiceItem`) |
| Filtros | Nº factura, cliente, rango de fechas |
| Descargar | PDF por `invoice_number` |
| Reenvío email | `POST /reservation/{id}/invoice/email` (SMTP en servidor) |

### Configuración del encabezado

| Elemento | Implementación |
|----------|----------------|
| Pantalla | `ConfigFactura.xaml` — **Inicio → Datos factura** |
| API | `GET/PUT /settings/invoice` |
| Campos | Nombre comercial, CIF/NIF, dirección, notas fiscales, IVA % |

**ViewModels:** `ListFacturasViewModel`, `ConfigFacturaViewModel`, `ModReservaViewModel` (`DescargarFacturaCommand`, `RegistrarCheckoutCommand`).

---

## P9 · Historial de estancias y estadísticas del cliente

### Ficha del cliente (recepción)

| Elemento | Implementación |
|----------|----------------|
| Acceso | `GestionUsuarios` → seleccionar cliente → **Estancias** |
| Ventana | `ClientFichaEstancias.xaml` + `ClientFichaEstanciasViewModel` |
| Historial | Tabla paginada — `GET /users/{id}/history` (filtros fecha vía recarga API; export local) |
| Estadísticas | Card lateral — `GET /users/{id}/stats` (noches, gasto, nivel, temporada favorita, habitación top, racha, última estancia) |
| Exportación | **Exportar CSV** del historial visible (`ExportarCsvCommand`) |

> La propuesta menciona exportación a PDF; en WPF está implementada la exportación a **CSV**.

**Servicio:** `UserStayService.cs`.

---

## P19 · Check-in anticipado y check-out tardío

### Detalle de reserva — solicitudes especiales

| Elemento | Implementación |
|----------|----------------|
| Ubicación | `modReserva.xaml` — bloque **SOLICITUDES CHECK-IN / CHECK-OUT** (admin/empleado) |
| Información | Estado, hora, horas, suplemento, disponibilidad, modo aprobación, nota de revisión |
| Acciones | **Aprobar** / **Rechazar** si `pending` → `PATCH …/flexibility/early-checkin/review` o `…/late-checkout/review` |
| Actualizar | Botón que llama `GET /reservation/{id}/flexibility` |

### Listado de solicitudes pendientes del día

| Elemento | Implementación |
|----------|----------------|
| Acceso | **Inicio** → banner «Check-in / check-out especiales» → **Ver cola** |
| Pantalla | `SolicitudesFlexibilidad.xaml` (incrustada en panel) |
| API | `GET /reservation/flexibility/pending?day=` |
| Acciones | Aprobar, rechazar (diálogo con nota), abrir reserva en `modReserva` |
| Pestañas | Activas / Inactivas; orden por fecha |

### Configuración de reglas

| Elemento | Implementación |
|----------|----------------|
| Pantalla | `ConfigFlexibilidad.xaml` — barra superior **Reglas solicitudes** |
| API | `GET/PUT /settings/flexibility` |
| Opciones | Acceso gratuito por rango (bronce/plata/oro), €/h entrada/salida, descuentos %, mín. horas, tope €, horas máx./mín., notificación por email |

**Servicio:** `FlexibilityService.cs` · **ViewModels:** `ModReservaViewModel`, `SolicitudesFlexibilidadViewModel`, `ConfigFlexibilidadViewModel`.

---

## Archivos principales por propuesta

| Propuesta | Models | Services | Views / ViewModels |
|-----------|--------|----------|-------------------|
| P11 | `BookingAuditEntry`, `AuditChangeDetail`, `HistorialAuditoriaFila` | `ReservationService.GetBookingAuditAsync` | `listAuditorias`, `AuditoriaReserva`, pestaña en `modReserva` |
| P5 | `HotelInvoiceItem`, `InvoiceSettingsDto` | `ReservationService`, `InvoiceSettingsService` | `listFacturas`, `ConfigFactura`, `modReserva` |
| P9 | DTOs en `UserStayService` | `UserStayService` | `ClientFichaEstancias` |
| P19 | `FlexibilitySettingsDto`, `PendingFlexibilityItemDto` | `FlexibilityService` | `SolicitudesFlexibilidad`, `ConfigFlexibilidad`, bloque en `modReserva` |
