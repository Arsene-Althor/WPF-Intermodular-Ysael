using System;
using System.Collections.Generic;
using System.Linq;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Helpers
{
    public static class AuditUiMapper
    {
        public static string TraducirAccion(string? action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "—";
            return action switch
            {
                "CREATED" => "Alta de reserva",
                "UPDATED" => "Modificación",
                "CANCELED" => "Cancelación",
                "PAYMENT_ADDED" => "Pago añadido",
                "EXTRA_ADDED" => "Extra añadido",
                _ => action,
            };
        }

        public static HistorialAuditoriaFila ToHistorialFila(BookingAuditEntry e, string nombreActor)
        {
            var cambios = ResolverCambios(e);
            string resumen;
            if (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                resumen = string.Join(Environment.NewLine, e.ResumenCambios.Take(4));
            else if (cambios.Count > 0)
                resumen = AuditDisplayHelper.ResumenCorto(cambios);
            else if (e.Action == "CREATED")
                resumen = "CREATED — reserva nueva";
            else
                resumen = (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                    ? string.Join(Environment.NewLine, e.ResumenCambios)
                    : "—";

            return new HistorialAuditoriaFila
            {
                ActionKey = e.Action ?? "",
                Accion = TraducirAccion(e.Action),
                ActorId = e.ActorId ?? "",
                ActorNombre = nombreActor,
                Fecha = e.Timestamp,
                ResumenTexto = resumen,
                Cambios = cambios,
            };
        }

        private static List<AuditCambioFila> ResolverCambios(BookingAuditEntry e)
        {
            var cambios = AuditDisplayHelper.MapearDetalle(e.DetalleCambios);
            if (cambios.Count > 0 || !string.Equals(e.Action, "CREATED", StringComparison.OrdinalIgnoreCase))
                return cambios;
            return AuditDisplayHelper.DetalleFallbackCreated(e);
        }

        public static AuditGlobalRow ToGlobalRow(BookingAuditEntry e, string nombreActor)
        {
            var cambios = ResolverCambios(e);
            var resumen = (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                ? string.Join("; ", e.ResumenCambios)
                : cambios.Count > 0
                    ? string.Join("; ", cambios.Select(c => $"{c.Etiqueta}: {c.Antes} → {c.Despues}"))
                    : "—";

            string resumenUi;
            if (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                resumenUi = LimpiarResumenApi(
                    string.Join(Environment.NewLine, e.ResumenCambios.Take(3)));
            else if (cambios.Count > 0)
                resumenUi = AuditDisplayHelper.ResumenCorto(cambios);
            else if (e.Action == "CREATED")
                resumenUi = e.Timestamp.HasValue
                    ? $"CREATED — {e.BookingId} · {e.Timestamp.Value:dd/MM/yyyy HH:mm}"
                    : $"CREATED — {e.BookingId}";
            else
                resumenUi = LimpiarResumenApi(resumen);

            return new AuditGlobalRow
            {
                Fecha = e.Timestamp,
                ReservaId = e.BookingId ?? "",
                AccionCodigo = e.Action ?? "",
                AccionLegible = TraducirAccion(e.Action),
                Actor = string.IsNullOrEmpty(nombreActor)
                    ? (e.ActorId ?? "")
                    : $"{nombreActor} ({e.ActorId})",
                Resumen = resumenUi,
                Cambios = cambios,
            };
        }

        /// <summary>Evita pegar JSON crudo si la API devolvió resumen largo.</summary>
        private static string LimpiarResumenApi(string resumen)
        {
            if (string.IsNullOrWhiteSpace(resumen) || resumen == "—") return "—";
            if (resumen.Contains('{') && resumen.Length > 120)
                return "Ver detalle al seleccionar la fila";
            return resumen;
        }
    }
}
