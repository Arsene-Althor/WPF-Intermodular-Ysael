using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Helpers
{
    public static class AuditDisplayHelper
    {
        private const int MaxValorCelda = 400;

        private static readonly Dictionary<string, string> EstadosSolicitud = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pending"] = "Pendiente",
            ["approved"] = "Aprobada",
            ["rejected"] = "Rechazada",
            ["cancelled"] = "Cancelada",
            ["canceled"] = "Cancelada",
        };

        private static readonly Dictionary<string, string> EtiquetasSubcampo = new(StringComparer.OrdinalIgnoreCase)
        {
            ["status"] = "Estado",
            ["requested_time"] = "Hora solicitada",
            ["requested_at"] = "Solicitado el",
            ["final_fee"] = "Suplemento",
            ["base_fee"] = "Tarifa base",
            ["discount_percent"] = "Descuento %",
            ["loyalty_tier"] = "Rango",
            ["hours_difference"] = "Horas",
            ["rate_per_hour"] = "€/hora",
            ["availability_ok"] = "Disponible",
            ["auto_approved"] = "Auto-aprobado",
            ["approval_mode"] = "Modo",
            ["reviewed_by"] = "Revisado por",
            ["review_note"] = "Nota",
            ["reviewed_at"] = "Revisado el",
            ["client_notified_at"] = "Cliente notificado",
            ["late_mode"] = "Modo salida",
        };

        public static string FormatearValor(JsonElement? elemento)
        {
            if (elemento == null || !elemento.HasValue) return "—";
            var e = elemento.Value;
            return e.ValueKind switch
            {
                JsonValueKind.Null => "—",
                JsonValueKind.String => FormatearCadena(e.GetString()),
                JsonValueKind.True => "Sí",
                JsonValueKind.False => "No",
                JsonValueKind.Number => FormatearNumero(e),
                JsonValueKind.Object => FormatearObjetoPlano(e),
                JsonValueKind.Array => FormatearArray(e),
                _ => Truncar(e.GetRawText()),
            };
        }

        private static string FormatearNumero(JsonElement e)
        {
            if (e.TryGetDecimal(out var d))
            {
                if (d == Math.Truncate(d))
                    return ((long)d).ToString();
                return d.ToString("0.##");
            }
            return e.GetRawText();
        }

        private static string FormatearCadena(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "—";
            if (EstadosSolicitud.TryGetValue(s.Trim(), out var est)) return est;
            if (DateTime.TryParse(s, out var dt))
                return dt.ToString("dd/MM/yyyy HH:mm");
            return Truncar(s);
        }

        private static bool EsVacioDisplay(string s) =>
            string.IsNullOrWhiteSpace(s) || s == "—" || s == "Sin valor";

        private static AuditCambioFila CrearFilaCambio(string etiqueta, string antes, string despues)
        {
            var vacioA = EsVacioDisplay(antes);
            var vacioB = EsVacioDisplay(despues);
            return new AuditCambioFila
            {
                Etiqueta = etiqueta,
                Antes = vacioA ? "Sin valor" : antes,
                Despues = vacioB ? "Sin valor" : despues,
                EsSoloAlta = vacioA && !vacioB,
                EsSoloBaja = !vacioA && vacioB,
            };
        }

        private static string FraseResumenLinea(AuditCambioFila c)
        {
            if (c.EsSoloAlta) return $"{c.Etiqueta}: se estableció «{c.Despues}»";
            if (c.EsSoloBaja) return $"{c.Etiqueta}: se eliminó (antes «{c.Antes}»)";
            return $"{c.Etiqueta}: «{c.Antes}» → «{c.Despues}»";
        }

        private static string FormatearArray(JsonElement e)
        {
            var partes = e.EnumerateArray()
                .Select(x => FormatearValor(x))
                .Where(x => x != "—")
                .ToList();
            return partes.Count == 0 ? "—" : Truncar(string.Join(", ", partes));
        }

        /// <summary>Resumen en una línea cuando no se desglosa (objeto pequeño).</summary>
        private static string FormatearObjetoPlano(JsonElement e)
        {
            if (e.ValueKind != JsonValueKind.Object)
                return Truncar(e.GetRawText());

            var partes = new List<string>();
            foreach (var prop in e.EnumerateObject())
            {
                var etiqueta = EtiquetasSubcampo.TryGetValue(prop.Name, out var lbl) ? lbl : prop.Name;
                var valor = FormatearValor(prop.Value);
                if (valor == "—") continue;
                partes.Add($"{etiqueta}: {valor}");
            }
            return partes.Count == 0 ? "—" : Truncar(string.Join(" · ", partes));
        }

        private static string Truncar(string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return "—";
            var t = texto.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (t.Length <= MaxValorCelda) return t;
            return t.Substring(0, MaxValorCelda) + "…";
        }

        /// <summary>Si la API no envió detalle (log antiguo), muestra al menos hora y reserva.</summary>
        public static List<AuditCambioFila> DetalleFallbackCreated(BookingAuditEntry e)
        {
            var filas = new List<AuditCambioFila>();
            if (e.Timestamp.HasValue)
            {
                filas.Add(CrearFilaCambio(
                    "Registrado en auditoría",
                    "—",
                    e.Timestamp.Value.ToString("dd/MM/yyyy HH:mm")));
            }
            if (!string.IsNullOrWhiteSpace(e.BookingId))
            {
                filas.Add(CrearFilaCambio("ID reserva", "—", e.BookingId));
            }
            if (filas.Count == 0)
            {
                filas.Add(CrearFilaCambio(
                    "Alta de reserva",
                    "—",
                    "Sin datos guardados en el log (recargue con API actualizada)"));
            }
            return filas;
        }

        public static List<AuditCambioFila> MapearDetalle(IEnumerable<AuditChangeDetail>? detalle)
        {
            if (detalle == null) return new List<AuditCambioFila>();
            var filas = new List<AuditCambioFila>();
            foreach (var d in detalle)
                filas.AddRange(ExpandirDetalle(d));
            return filas.Where(c => !EsVacioDisplay(c.Antes) || !EsVacioDisplay(c.Despues)).ToList();
        }

        private static IEnumerable<AuditCambioFila> ExpandirDetalle(AuditChangeDetail d)
        {
            var etiqueta = string.IsNullOrWhiteSpace(d.Etiqueta) ? d.Campo : d.Etiqueta;
            var tieneAntes = d.Antes.HasValue && d.Antes.Value.ValueKind != JsonValueKind.Null;
            var tieneDespues = d.Despues.HasValue && d.Despues.Value.ValueKind != JsonValueKind.Null;

            if (tieneAntes && d.Antes!.Value.ValueKind == JsonValueKind.Object
                || tieneDespues && d.Despues!.Value.ValueKind == JsonValueKind.Object)
            {
                var antesObj = tieneAntes && d.Antes!.Value.ValueKind == JsonValueKind.Object
                    ? d.Antes.Value
                    : default;
                var despuesObj = tieneDespues && d.Despues!.Value.ValueKind == JsonValueKind.Object
                    ? d.Despues.Value
                    : default;
                return ExpandirObjetoComparado(etiqueta, antesObj, despuesObj);
            }

            return new[]
            {
                CrearFilaCambio(etiqueta, FormatearValor(d.Antes), FormatearValor(d.Despues)),
            };
        }

        /// <summary>Desglosa JSON de solicitud P19, desglose factura, etc. en una fila por subcampo.</summary>
        private static bool ObjetoJsonVacio(JsonElement e) =>
            e.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            || (e.ValueKind == JsonValueKind.Object && !e.EnumerateObject().Any());

        private static List<AuditCambioFila> ExpandirObjetoComparado(
            string prefijo,
            JsonElement antesObj,
            JsonElement despuesObj)
        {
            if (ObjetoJsonVacio(antesObj) && despuesObj.ValueKind == JsonValueKind.Object && despuesObj.EnumerateObject().Any())
            {
                return new List<AuditCambioFila>
                {
                    CrearFilaCambio($"{prefijo} registrada", "—", FormatearObjetoPlano(despuesObj)),
                };
            }

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (antesObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in antesObj.EnumerateObject())
                    keys.Add(p.Name);
            }
            if (despuesObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in despuesObj.EnumerateObject())
                    keys.Add(p.Name);
            }

            var filas = new List<AuditCambioFila>();
            foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                JsonElement? a = null;
                JsonElement? b = null;
                if (antesObj.ValueKind == JsonValueKind.Object && antesObj.TryGetProperty(key, out var elA))
                    a = elA;
                if (despuesObj.ValueKind == JsonValueKind.Object && despuesObj.TryGetProperty(key, out var elB))
                    b = elB;

                var sa = FormatearValor(a);
                var sb = FormatearValor(b);
                if (sa == sb) continue;

                var sub = EtiquetasSubcampo.TryGetValue(key, out var lbl) ? lbl : key;
                var et = string.IsNullOrEmpty(prefijo) ? sub : $"{prefijo} · {sub}";
                filas.Add(CrearFilaCambio(et, sa, sb));
            }

            if (filas.Count == 0)
            {
                filas.Add(CrearFilaCambio(
                    prefijo,
                    antesObj.ValueKind == JsonValueKind.Object ? FormatearObjetoPlano(antesObj) : "—",
                    despuesObj.ValueKind == JsonValueKind.Object ? FormatearObjetoPlano(despuesObj) : "—"));
            }

            return filas;
        }

        /// <summary>Texto breve para la columna principal del grid (sin bloques enormes).</summary>
        public static string ResumenCorto(IEnumerable<AuditCambioFila> cambios)
        {
            var lista = cambios?.ToList() ?? new List<AuditCambioFila>();
            if (lista.Count == 0) return "—";

            if (lista.Count == 1)
                return FraseResumenLinea(lista[0]);

            if (lista.Count <= 4)
            {
                return string.Join(
                    Environment.NewLine,
                    lista.Select(c => $"• {FraseResumenLinea(c)}"));
            }

            var nombres = string.Join(", ", lista.Take(4).Select(c => c.Etiqueta));
            return $"{lista.Count} campos modificados{Environment.NewLine}• {nombres}… (clic en la fila para ver todo)";
        }

        /// <summary>Detalle completo (pestaña historial en reserva).</summary>
        public static string ResumenLineas(IEnumerable<AuditCambioFila> cambios) =>
            ResumenCorto(cambios);
    }
}
