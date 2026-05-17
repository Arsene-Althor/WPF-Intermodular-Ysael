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
            var cambios = AuditDisplayHelper.MapearDetalle(e.DetalleCambios);
            var resumen = (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                ? string.Join(Environment.NewLine, e.ResumenCambios)
                : cambios.Count > 0
                    ? string.Join(Environment.NewLine, cambios.Select(c => $"{c.Etiqueta}: {c.Antes} → {c.Despues}"))
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

        public static AuditGlobalRow ToGlobalRow(BookingAuditEntry e, string nombreActor)
        {
            var cambios = AuditDisplayHelper.MapearDetalle(e.DetalleCambios);
            var resumen = (e.ResumenCambios != null && e.ResumenCambios.Count > 0)
                ? string.Join("; ", e.ResumenCambios)
                : cambios.Count > 0
                    ? string.Join("; ", cambios.Select(c => $"{c.Etiqueta}: {c.Antes} → {c.Despues}"))
                    : "—";

            return new AuditGlobalRow
            {
                Fecha = e.Timestamp,
                ReservaId = e.BookingId ?? "",
                Accion = e.Action ?? "",
                Actor = string.IsNullOrEmpty(nombreActor)
                    ? (e.ActorId ?? "")
                    : $"{nombreActor} ({e.ActorId})",
                Resumen = resumen,
                Antes = AuditDisplayHelper.ResumenAntesDespues(cambios),
                Despues = AuditDisplayHelper.ResumenSoloDespues(cambios),
                Cambios = cambios,
            };
        }
    }
}
