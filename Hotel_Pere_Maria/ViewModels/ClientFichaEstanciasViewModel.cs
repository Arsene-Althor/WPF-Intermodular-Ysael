using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Microsoft.Win32;

namespace Hotel_Pere_Maria.ViewModels
{
    public class ClientFichaEstanciasViewModel : BaseViewModel
    {
        private readonly string _userId;
        private readonly string _displayName;
        private ClientStayStatsDto? _stats;
        private DateTime? _histDesde;
        private DateTime? _histHasta;
        private int _page = 1;
        private int _totalPages = 1;
        private string _statusFiltro = "completed";

        public string Titulo => $"Estancias — {_displayName} ({_userId})";
        public ObservableCollection<ClientStayHistoryItem> Historial { get; } = new();

        public ClientStayStatsDto? Stats
        {
            get => _stats;
            set { _stats = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatsNoches)); OnPropertyChanged(nameof(StatsGasto)); OnPropertyChanged(nameof(StatsTier)); OnPropertyChanged(nameof(StatsUltima)); OnPropertyChanged(nameof(StatsExtra)); }
        }

        public string StatsNoches => Stats != null ? $"{Stats.total_nights} noches" : "—";
        public string StatsGasto => Stats != null ? $"{Stats.total_spent:N2} €" : "—";
        public string StatsTier => Stats?.TierDisplay ?? "—";
        public string StatsUltima => Stats?.last_stay_checkout_at?.ToString("dd/MM/yyyy") ?? "—";
        public string StatsExtra => Stats != null
            ? $"Temporada favorita: {Stats.favorite_season ?? "—"} · Habitación top: {Stats.most_booked_room?.room_id ?? "—"} · Racha: {Stats.max_stay_streak}"
            : "";

        public DateTime? HistDesde { get => _histDesde; set { _histDesde = value; OnPropertyChanged(); } }
        public DateTime? HistHasta { get => _histHasta; set { _histHasta = value; OnPropertyChanged(); } }

        public int Page
        {
            get => _page;
            set { _page = value; OnPropertyChanged(); OnPropertyChanged(nameof(PaginacionLabel)); }
        }

        public string PaginacionLabel => $"Página {Page} / {_totalPages}";

        public ICommand RefrescarCommand { get; }
        public ICommand AplicarFiltrosCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PaginaSiguienteCommand { get; }
        public ICommand ExportarCsvCommand { get; }

        public ClientFichaEstanciasViewModel(Usuario usuario)
        {
            _userId = usuario.user_id;
            _displayName = $"{usuario.name} {usuario.surname}".Trim();
            RefrescarCommand = new RelayCommand(() => _ = CargarTodoAsync());
            AplicarFiltrosCommand = new RelayCommand(() => { Page = 1; _ = CargarHistorialAsync(); });
            PaginaAnteriorCommand = new RelayCommand(() => { if (Page > 1) { Page--; _ = CargarHistorialAsync(); } }, () => Page > 1);
            PaginaSiguienteCommand = new RelayCommand(() => { if (Page < _totalPages) { Page++; _ = CargarHistorialAsync(); } }, () => Page < _totalPages);
            ExportarCsvCommand = new RelayCommand(ExportarCsv);
            _ = CargarTodoAsync();
        }

        private async Task CargarTodoAsync()
        {
            await CargarStatsAsync();
            await CargarHistorialAsync();
        }

        private async Task CargarStatsAsync()
        {
            var (ok, err, stats) = await UserStayService.GetStatsAsync(_userId);
            if (!ok)
            {
                MessageBox.Show(err ?? "Error stats", "Estadísticas", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Stats = stats;
        }

        private async Task CargarHistorialAsync()
        {
            var (ok, err, data) = await UserStayService.GetHistoryAsync(
                _userId, Page, 15, _statusFiltro, HistDesde, HistHasta);
            if (!ok || data == null)
            {
                MessageBox.Show(err ?? "Error historial", "Historial", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _totalPages = Math.Max(1, data.total_pages);
            OnPropertyChanged(nameof(PaginacionLabel));
            Historial.Clear();
            foreach (var i in data.items) Historial.Add(i);
        }

        private void ExportarCsv()
        {
            if (Historial.Count == 0)
            {
                MessageBox.Show("No hay filas para exportar.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = $"Historial-{_userId}.csv",
                Filter = "CSV (*.csv)|*.csv",
                DefaultExt = ".csv",
            };
            if (dlg.ShowDialog() != true) return;
            File.WriteAllText(dlg.FileName, UserStayService.BuildHistoryCsv(Historial), Encoding.UTF8);
            MessageBox.Show($"Guardado:\n{dlg.FileName}", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
