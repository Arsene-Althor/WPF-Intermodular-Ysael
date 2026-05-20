using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Helpers;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ListAuditoriasViewModel : BaseViewModel
    {
        private List<AuditGlobalRow> _todas = new();
        private string _filtroReserva = "";
        private string _filtroActor = "";
        private string _filtroAccion = "";
        private string _filtroTexto = "";

        public ObservableCollection<AuditGlobalRow> Filas { get; } = new();

        public string FiltroReserva
        {
            get => _filtroReserva;
            set { _filtroReserva = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        public string FiltroActor
        {
            get => _filtroActor;
            set { _filtroActor = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        public string FiltroAccion
        {
            get => _filtroAccion;
            set { _filtroAccion = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set { _filtroTexto = value ?? ""; OnPropertyChanged(); Filtrar(); }
        }

        private bool _auditoriaActiva = true;
        private bool _guardandoAuditoria;

        public bool AuditoriaActiva
        {
            get => _auditoriaActiva;
            set
            {
                if (_auditoriaActiva == value) return;
                _auditoriaActiva = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstadoAuditoriaTexto));
                _ = GuardarAuditoriaActivaAsync();
            }
        }

        public string EstadoAuditoriaTexto =>
            AuditoriaActiva
                ? "Registro de auditoría activo (consume recursos en cada cambio de reserva)"
                : "Registro de auditoría desactivado — no se guardan nuevos eventos";

        public ICommand CargarCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }

        public ListAuditoriasViewModel()
        {
            CargarCommand = new RelayCommand(() => _ = CargarAsync());
            LimpiarFiltrosCommand = new RelayCommand(LimpiarFiltrosUi);
            _ = InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            await CargarConfigOperativaAsync();
            await CargarAsync();
        }

        private async Task CargarConfigOperativaAsync()
        {
            var (ok, _, dto) = await OperationalSettingsService.GetAsync();
            if (ok && dto != null)
            {
                _auditoriaActiva = dto.booking_audit_enabled;
                OnPropertyChanged(nameof(AuditoriaActiva));
                OnPropertyChanged(nameof(EstadoAuditoriaTexto));
            }
        }

        private async Task GuardarAuditoriaActivaAsync()
        {
            if (_guardandoAuditoria) return;
            _guardandoAuditoria = true;
            try
            {
                var (ok, err, _) = await OperationalSettingsService.PutAsync(new OperationalSettingsDto
                {
                    booking_audit_enabled = AuditoriaActiva,
                });
                if (!ok)
                {
                    MessageBox.Show(err ?? "No se pudo guardar", "Auditoría", MessageBoxButton.OK, MessageBoxImage.Warning);
                    await CargarConfigOperativaAsync();
                }
            }
            finally
            {
                _guardandoAuditoria = false;
            }
        }

        private void LimpiarFiltrosUi()
        {
            _filtroReserva = "";
            _filtroActor = "";
            _filtroAccion = "";
            _filtroTexto = "";
            OnPropertyChanged(nameof(FiltroReserva));
            OnPropertyChanged(nameof(FiltroActor));
            OnPropertyChanged(nameof(FiltroAccion));
            OnPropertyChanged(nameof(FiltroTexto));
            _ = CargarAsync();
        }

        private async Task CargarAsync()
        {
            try
            {
                var (ok, msg, lista) = await ReservationService.GetGlobalAuditsAsync(
                    bookingId: string.IsNullOrWhiteSpace(FiltroReserva) ? null : FiltroReserva.Trim(),
                    actorId: string.IsNullOrWhiteSpace(FiltroActor) ? null : FiltroActor.Trim(),
                    action: string.IsNullOrWhiteSpace(FiltroAccion) ? null : FiltroAccion.Trim(),
                    fromIso: null,
                    toIso: null,
                    limit: 400);
                if (!ok || lista == null)
                {
                    MessageBox.Show(msg ?? "Error", "Auditorías", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                List<Usuario> usuarios = new();
                try { usuarios = await UserService.GetAllUsersAsync(); } catch { /* */ }
                var nombres = usuarios.Where(u => !string.IsNullOrEmpty(u.user_id))
                    .GroupBy(u => u.user_id)
                    .ToDictionary(g => g.Key, g => g.First().FullName ?? g.First().user_id);

                _todas = lista.Select(e =>
                {
                    nombres.TryGetValue(e.ActorId ?? "", out var an);
                    return AuditUiMapper.ToGlobalRow(e, an ?? "");
                }).ToList();
                Filtrar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Auditorías", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filtrar()
        {
            var q = _todas.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(FiltroTexto))
            {
                var t = FiltroTexto.Trim();
                q = q.Where(r =>
                    (r.ReservaId?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Actor?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Resumen?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.AccionLegible?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.AccionCodigo?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false));
            }
            Filas.Clear();
            foreach (var r in q.OrderByDescending(x => x.Fecha ?? DateTime.MinValue))
                Filas.Add(r);
        }
    }
}
