using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Helpers
{
    public static class AuditDisplayHelper
    {
        public static string FormatearValor(JsonElement? elemento)
        {
            if (elemento == null || !elemento.HasValue) return "—";
            var e = elemento.Value;
            return e.ValueKind switch
            {
                JsonValueKind.Null => "—",
                JsonValueKind.String => FormatearCadena(e.GetString()),
                JsonValueKind.True => "sí",
                JsonValueKind.False => "no",
                JsonValueKind.Number => e.GetRawText(),
                _ => e.GetRawText(),
            };
        }

        private static string FormatearCadena(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "—";
            if (DateTime.TryParse(s, out var dt))
                return dt.ToString("dd/MM/yyyy HH:mm");
            return s;
        }

        public static List<AuditCambioFila> MapearDetalle(IEnumerable<AuditChangeDetail>? detalle)
        {
            if (detalle == null) return new List<AuditCambioFila>();
            return detalle
                .Select(d => new AuditCambioFila
                {
                    Etiqueta = string.IsNullOrWhiteSpace(d.Etiqueta) ? d.Campo : d.Etiqueta,
                    Antes = FormatearValor(d.Antes),
                    Despues = FormatearValor(d.Despues),
                })
                .ToList();
        }

        public static string ResumenAntesDespues(IEnumerable<AuditCambioFila> cambios)
        {
            if (cambios == null || !cambios.Any()) return "—";
            return string.Join("; ", cambios.Select(c => $"{c.Etiqueta}: {c.Antes}"));
        }

        public static string ResumenSoloDespues(IEnumerable<AuditCambioFila> cambios)
        {
            if (cambios == null || !cambios.Any()) return "—";
            return string.Join("; ", cambios.Select(c => $"{c.Etiqueta}: {c.Despues}"));
        }
    }
}
