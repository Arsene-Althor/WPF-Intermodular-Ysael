using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private string _temporadaTopId = string.Empty;
        private string _habitacionMasSolicitada = string.Empty;
        private int _reservasCanceladasTotal;
        private double _tasaOcupacionActual;
        private decimal _ingresosMesActual;
        private double _promedioEstanciaDias;
        private double _tasaNoShow;
        private int _checkInsPendientesHoy;
        private int _reservasCanceladasPeriodo;
        private string _periodoReservasSeleccionado = "30 días";
        private string _mensajeDatos = "Cargando datos...";
        private bool _datosDisponibles;
        private bool _modoDemo;
        private List<Reservation> _reservas = new();

        public string TemporadaTopId
        {
            get => _temporadaTopId;
            set { _temporadaTopId = value; OnPropertyChanged(); }
        }

        public string HabitacionMasSolicitada
        {
            get => _habitacionMasSolicitada;
            set { _habitacionMasSolicitada = value; OnPropertyChanged(); }
        }

        public int ReservasCanceladasTotal
        {
            get => _reservasCanceladasTotal;
            set { _reservasCanceladasTotal = value; OnPropertyChanged(); }
        }

        public int ReservasCanceladasPeriodo
        {
            get => _reservasCanceladasPeriodo;
            set { _reservasCanceladasPeriodo = value; OnPropertyChanged(); }
        }

        public string PeriodoReservasSeleccionado
        {
            get => _periodoReservasSeleccionado;
            set
            {
                _periodoReservasSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReservasCanceladasTitulo));
                OnPropertyChanged(nameof(EsPeriodoHoy));
                OnPropertyChanged(nameof(EsPeriodo7Dias));
                OnPropertyChanged(nameof(EsPeriodo30Dias));
                OnPropertyChanged(nameof(EsPeriodo90Dias));
            }
        }

        public string ReservasCanceladasTitulo => $"Reservas canceladas ({PeriodoReservasSeleccionado})";
        public bool EsPeriodoHoy => PeriodoReservasSeleccionado == "Hoy";
        public bool EsPeriodo7Dias => PeriodoReservasSeleccionado == "7 días";
        public bool EsPeriodo30Dias => PeriodoReservasSeleccionado == "30 días";
        public bool EsPeriodo90Dias => PeriodoReservasSeleccionado == "90 días";

        public bool ModoDemo
        {
            get => _modoDemo;
            set
            {
                _modoDemo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModoDemo));
                OnPropertyChanged(nameof(TextoBotonModoDatos));
                OnPropertyChanged(nameof(EtiquetaModoDatos));
            }
        }

        public bool IsModoDemo => ModoDemo;
        public string TextoBotonModoDatos => ModoDemo ? "Usar datos reales" : "Usar datos demo";
        public string EtiquetaModoDatos => ModoDemo ? "Modo demo activo" : "Modo real activo";

        public string MensajeDatos
        {
            get => _mensajeDatos;
            set { _mensajeDatos = value; OnPropertyChanged(); }
        }

        public bool DatosDisponibles
        {
            get => _datosDisponibles;
            set { _datosDisponibles = value; OnPropertyChanged(); }
        }

        public double TasaOcupacionActual
        {
            get => _tasaOcupacionActual;
            set { _tasaOcupacionActual = value; OnPropertyChanged(); OnPropertyChanged(nameof(TasaOcupacionActualTexto)); }
        }

        public string TasaOcupacionActualTexto => $"{TasaOcupacionActual.ToString("0.0", CultureInfo.InvariantCulture)} %";

        public decimal IngresosMesActual
        {
            get => _ingresosMesActual;
            set { _ingresosMesActual = value; OnPropertyChanged(); OnPropertyChanged(nameof(IngresosMesActualTexto)); }
        }

        public string IngresosMesActualTexto => $"{IngresosMesActual:N0} EUR";

        public double PromedioEstanciaDias
        {
            get => _promedioEstanciaDias;
            set { _promedioEstanciaDias = value; OnPropertyChanged(); OnPropertyChanged(nameof(PromedioEstanciaDiasTexto)); }
        }

        public string PromedioEstanciaDiasTexto => $"{PromedioEstanciaDias.ToString("0.0", CultureInfo.InvariantCulture)} noches";

        public double TasaNoShow
        {
            get => _tasaNoShow;
            set { _tasaNoShow = value; OnPropertyChanged(); OnPropertyChanged(nameof(TasaNoShowTexto)); }
        }

        public string TasaNoShowTexto => $"{TasaNoShow.ToString("0.0", CultureInfo.InvariantCulture)} %";

        public int CheckInsPendientesHoy
        {
            get => _checkInsPendientesHoy;
            set { _checkInsPendientesHoy = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RangoUsuarioMetric> DistribucionUsuarios { get; } = new();
        public ObservableCollection<HabitacionDemandaMetric> HabitacionesTopDemanda { get; } = new();
        public ObservableCollection<IngresoMensualMetric> IngresosMensuales { get; } = new();

        public ICommand ActualizarDatosMockCommand { get; }
        public ICommand CambiarPeriodoReservasCommand { get; }
        public ICommand AlternarModoDatosCommand { get; }

        public DashboardViewModel()
        {
            ActualizarDatosMockCommand = new RelayCommand(() => { _ = RefrescarDatosAsync(); });
            CambiarPeriodoReservasCommand = new RelayCommand<string>(CambiarPeriodoReservas);
            AlternarModoDatosCommand = new RelayCommand(() => { _ = AlternarModoDatosAsync(); });
            _ = RefrescarDatosAsync();
        }

        private async Task AlternarModoDatosAsync()
        {
            ModoDemo = !ModoDemo;
            await RefrescarDatosAsync();
        }

        private async Task RefrescarDatosAsync()
        {
            if (ModoDemo)
            {
                CargarDatosDemo();
                return;
            }

            await CargarDatosRealesAsync();
        }

        private void CambiarPeriodoReservas(string periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                return;

            if (periodo != "Hoy" && periodo != "7 días" && periodo != "30 días" && periodo != "90 días")
                return;

            PeriodoReservasSeleccionado = periodo;
            RecalcularCanceladasPeriodo();
        }

        private async Task CargarDatosRealesAsync()
        {
            MensajeDatos = "Cargando datos reales...";
            DatosDisponibles = false;

            try
            {
                var reservasTask = ReservationService.getAllReservation();
                var roomsTask = RoomService.GetAllRoomsAsync();
                var usersTask = UserService.GetAllUsersAsync();

                await Task.WhenAll(reservasTask, roomsTask, usersTask);

                _reservas = reservasTask.Result ?? new List<Reservation>();
                var rooms = roomsTask.Result ?? new List<Room>();
                var users = usersTask.Result ?? new List<Usuario>();
                var roomById = rooms
                    .Where(r => !string.IsNullOrWhiteSpace(r.RoomId))
                    .GroupBy(r => r.RoomId.Trim())
                    .ToDictionary(g => g.Key, g => g.First());

                CalcularTemporadaTop(_reservas);
                CalcularHabitacionTop(_reservas, roomById);
                CalcularCancelaciones(_reservas);
                CalcularOcupacion(_reservas, rooms);
                CalcularIngresos(_reservas);
                CalcularPromedioEstancia(_reservas);
                CalcularNoShowYCheckins(_reservas);
                await CalcularDistribucionUsuariosAsync(users);
                CalcularTopDemandaHabitaciones(_reservas, roomById);

                DatosDisponibles = true;
                MensajeDatos = $"Datos actualizados: {_reservas.Count} reservas analizadas";
            }
            catch (Exception ex)
            {
                DatosDisponibles = false;
                MensajeDatos = $"No se pudo cargar dashboard: {ex.Message}";
            }
        }

        private void CargarDatosDemo()
        {
            MensajeDatos = "Cargando datos demo...";
            DatosDisponibles = false;

            _reservas = ConstruirReservasDemoCanceladas();
            TemporadaTopId = "VER-2026";
            HabitacionMasSolicitada = "Doble Superior (HAB-104)";
            ReservasCanceladasTotal = _reservas.Count(r => r.cancelation_date != null);
            TasaOcupacionActual = 73.2;
            IngresosMesActual = 48750m;
            PromedioEstanciaDias = 3.2;
            TasaNoShow = 2.7;
            CheckInsPendientesHoy = 6;

            DistribucionUsuarios.Clear();
            var rangos = new[]
            {
                new RangoUsuarioMetric("Bronce", 118),
                new RangoUsuarioMetric("Plata", 69),
                new RangoUsuarioMetric("Oro", 29),
            };
            int totalUsers = rangos.Sum(x => x.Usuarios);
            foreach (var rango in rangos)
            {
                rango.Porcentaje = totalUsers == 0 ? 0 : rango.Usuarios * 100d / totalUsers;
                DistribucionUsuarios.Add(rango);
            }

            HabitacionesTopDemanda.Clear();
            var topHabitaciones = new[]
            {
                new HabitacionDemandaMetric("Doble Superior", 76),
                new HabitacionDemandaMetric("Suite Deluxe", 63),
                new HabitacionDemandaMetric("Individual Económica", 52),
                new HabitacionDemandaMetric("Familiar Vista Mar", 38),
            };
            int maxDemandas = topHabitaciones.Max(h => h.Reservas);
            foreach (var item in topHabitaciones)
            {
                item.PorcentajeEscala = maxDemandas == 0 ? 0 : item.Reservas * 100d / maxDemandas;
                HabitacionesTopDemanda.Add(item);
            }

            IngresosMensuales.Clear();
            var ingresos = new[]
            {
                new IngresoMensualMetric("Ene", 38200m),
                new IngresoMensualMetric("Feb", 40150m),
                new IngresoMensualMetric("Mar", 42600m),
                new IngresoMensualMetric("Abr", 45120m),
                new IngresoMensualMetric("May", 46980m),
                new IngresoMensualMetric("Jun", 48750m),
            };
            decimal ingresoMax = ingresos.Max(i => i.Ingreso);
            foreach (var ingreso in ingresos)
            {
                ingreso.PorcentajeEscala = ingresoMax == 0 ? 0 : (double)(ingreso.Ingreso * 100m / ingresoMax);
                IngresosMensuales.Add(ingreso);
            }

            RecalcularCanceladasPeriodo();
            DatosDisponibles = true;
            MensajeDatos = "Demo activa: cifras de muestra moderadas";
        }

        private static List<Reservation> ConstruirReservasDemoCanceladas()
        {
            var ahora = DateTime.Now;
            var cancelaciones = new[]
            {
                ahora.AddHours(-3),
                ahora.AddDays(-1),
                ahora.AddDays(-2),
                ahora.AddDays(-5),
                ahora.AddDays(-6),
                ahora.AddDays(-9),
                ahora.AddDays(-12),
                ahora.AddDays(-15),
                ahora.AddDays(-18),
                ahora.AddDays(-21),
                ahora.AddDays(-24),
                ahora.AddDays(-27),
                ahora.AddDays(-31),
                ahora.AddDays(-35),
                ahora.AddDays(-39),
                ahora.AddDays(-44),
                ahora.AddDays(-49),
                ahora.AddDays(-55),
                ahora.AddDays(-61),
                ahora.AddDays(-66),
                ahora.AddDays(-70),
                ahora.AddDays(-75),
                ahora.AddDays(-80),
                ahora.AddDays(-84),
                ahora.AddDays(-87),
                ahora.AddDays(-89),
            };

            var lista = new List<Reservation>();
            for (int i = 0; i < cancelaciones.Length; i++)
            {
                var c = cancelaciones[i];
                lista.Add(new Reservation
                {
                    reservation_id = $"RSV-DEMO-{(i + 1).ToString("000", CultureInfo.InvariantCulture)}",
                    room_id = $"HAB-{(100 + (i % 9)).ToString(CultureInfo.InvariantCulture)}",
                    user_id = $"CLI-{(10000 + i).ToString(CultureInfo.InvariantCulture)}",
                    check_in = c.AddDays(-10),
                    check_out = c.AddDays(-6),
                    cancelation_date = c,
                    price = 220 + (i * 7),
                });
            }
            return lista;
        }

        private void CalcularTemporadaTop(IEnumerable<Reservation> reservas)
        {
            var topSeason = reservas
                .Where(r => r.cancelation_date == null)
                .GroupBy(r => ObtenerTemporada(r.check_in))
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            TemporadaTopId = string.IsNullOrWhiteSpace(topSeason) ? "Sin datos" : topSeason;
        }

        private void CalcularHabitacionTop(IEnumerable<Reservation> reservas, IReadOnlyDictionary<string, Room> roomById)
        {
            var top = reservas
                .Where(r => r.cancelation_date == null && !string.IsNullOrWhiteSpace(r.room_id))
                .GroupBy(r => r.room_id.Trim())
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (top == null)
            {
                HabitacionMasSolicitada = "Sin datos";
                return;
            }

            string roomId = top.Key;
            string type = roomById.TryGetValue(roomId, out var room)
                ? string.IsNullOrWhiteSpace(room.Type) ? "Habitación" : room.Type
                : "Habitación";

            HabitacionMasSolicitada = $"{type} ({roomId})";
        }

        private void CalcularCancelaciones(IEnumerable<Reservation> reservas)
        {
            ReservasCanceladasTotal = reservas.Count(r => r.cancelation_date != null);
            RecalcularCanceladasPeriodo();
        }

        private void RecalcularCanceladasPeriodo()
        {
            var ahora = DateTime.Now;
            var desde = ObtenerFechaInicioPeriodo(ahora, PeriodoReservasSeleccionado);

            ReservasCanceladasPeriodo = _reservas.Count(r =>
                r.cancelation_date.HasValue &&
                r.cancelation_date.Value >= desde &&
                r.cancelation_date.Value <= ahora);
        }

        private void CalcularOcupacion(IEnumerable<Reservation> reservas, IEnumerable<Room> rooms)
        {
            var listaRooms = rooms.ToList();
            int operativas = listaRooms.Count(r => r.IsOperational);
            if (operativas > 0)
            {
                int ocupadas = listaRooms.Count(r => r.IsOperational && r.IsOccupiedNow);
                TasaOcupacionActual = ocupadas * 100d / operativas;
                return;
            }

            var ahora = DateTime.Now;
            int activas = reservas.Count(r =>
                r.cancelation_date == null &&
                r.check_in <= ahora &&
                r.check_out > ahora);
            TasaOcupacionActual = activas > 0 ? 100 : 0;
        }

        private void CalcularIngresos(IEnumerable<Reservation> reservas)
        {
            var ahora = DateTime.Now;
            var inicioMesActual = new DateTime(ahora.Year, ahora.Month, 1);
            var inicioRango = inicioMesActual.AddMonths(-5);

            var items = new List<IngresoMensualMetric>();
            for (int i = 0; i < 6; i++)
            {
                var inicioMes = inicioRango.AddMonths(i);
                var finMes = inicioMes.AddMonths(1);

                decimal totalMes = reservas
                    .Where(r => r.cancelation_date == null)
                    .Where(r =>
                    {
                        var cierre = ObtenerFechaCierreReserva(r);
                        return cierre >= inicioMes && cierre < finMes;
                    })
                    .Sum(r => (decimal)r.price);

                items.Add(new IngresoMensualMetric(AbrevMes(inicioMes.Month), decimal.Round(totalMes, 2)));
            }

            IngresosMensuales.Clear();
            decimal max = items.Count == 0 ? 0 : items.Max(i => i.Ingreso);
            foreach (var item in items)
            {
                item.PorcentajeEscala = max == 0 ? 0 : (double)(item.Ingreso * 100m / max);
                IngresosMensuales.Add(item);
            }

            IngresosMesActual = items.LastOrDefault()?.Ingreso ?? 0m;
        }

        private void CalcularPromedioEstancia(IEnumerable<Reservation> reservas)
        {
            var completadas = reservas
                .Where(r => r.cancelation_date == null)
                .Where(EsReservaCompletada)
                .ToList();

            if (completadas.Count == 0)
            {
                PromedioEstanciaDias = 0;
                return;
            }

            double media = completadas.Average(r => Math.Max(1, (r.check_out - r.check_in).TotalDays));
            PromedioEstanciaDias = media;
        }

        private void CalcularNoShowYCheckins(IEnumerable<Reservation> reservas)
        {
            var ahora = DateTime.Now;
            var candidatasNoShow = reservas
                .Where(r => r.cancelation_date == null && r.check_out <= ahora)
                .ToList();

            int totalNoShowBase = candidatasNoShow.Count;
            int noShowCount = candidatasNoShow.Count(r => !r.reception_check_in_at.HasValue);
            TasaNoShow = totalNoShowBase == 0 ? 0 : noShowCount * 100d / totalNoShowBase;

            var hoy = DateTime.Today;
            CheckInsPendientesHoy = reservas.Count(r =>
                r.cancelation_date == null &&
                r.check_in.Date == hoy &&
                !r.reception_check_in_at.HasValue);
        }

        private async Task CalcularDistribucionUsuariosAsync(IEnumerable<Usuario> users)
        {
            var clientes = users
                .Where(u => string.Equals(u.role, "client", StringComparison.OrdinalIgnoreCase))
                .Where(u => !string.IsNullOrWhiteSpace(u.user_id))
                .ToList();

            int bronze = 0;
            int silver = 0;
            int gold = 0;

            if (clientes.Count == 0)
            {
                DistribucionUsuarios.Clear();
                return;
            }

            var tareas = clientes.Select(async c =>
            {
                var (ok, _, data) = await LoyaltyService.GetUserLoyaltyStatsAsync(c.user_id);
                if (!ok || data == null || string.IsNullOrWhiteSpace(data.loyalty_tier))
                    return "bronze";
                return data.loyalty_tier.Trim().ToLowerInvariant();
            });

            var tiers = await Task.WhenAll(tareas);
            foreach (var tier in tiers)
            {
                switch (tier)
                {
                    case "gold":
                        gold++;
                        break;
                    case "silver":
                        silver++;
                        break;
                    default:
                        bronze++;
                        break;
                }
            }

            DistribucionUsuarios.Clear();
            var rangos = new[]
            {
                new RangoUsuarioMetric("Bronce", bronze),
                new RangoUsuarioMetric("Plata", silver),
                new RangoUsuarioMetric("Oro", gold),
            };
            int total = rangos.Sum(r => r.Usuarios);
            foreach (var r in rangos)
            {
                r.Porcentaje = total == 0 ? 0 : r.Usuarios * 100d / total;
                DistribucionUsuarios.Add(r);
            }
        }

        private void CalcularTopDemandaHabitaciones(
            IEnumerable<Reservation> reservas,
            IReadOnlyDictionary<string, Room> roomById)
        {
            var top = reservas
                .Where(r => r.cancelation_date == null && !string.IsNullOrWhiteSpace(r.room_id))
                .GroupBy(r =>
                {
                    string roomId = r.room_id.Trim();
                    if (roomById.TryGetValue(roomId, out var room) && !string.IsNullOrWhiteSpace(room.Type))
                        return room.Type.Trim();
                    return roomId;
                })
                .Select(g => new HabitacionDemandaMetric(g.Key, g.Count()))
                .OrderByDescending(x => x.Reservas)
                .Take(4)
                .ToList();

            HabitacionesTopDemanda.Clear();
            int max = top.Count == 0 ? 0 : top.Max(x => x.Reservas);
            foreach (var item in top)
            {
                item.PorcentajeEscala = max == 0 ? 0 : item.Reservas * 100d / max;
                HabitacionesTopDemanda.Add(item);
            }
        }

        private static bool EsReservaCompletada(Reservation reserva)
        {
            if (reserva.cancelation_date != null)
                return false;
            if (reserva.checkout_completed_at.HasValue)
                return true;
            return reserva.check_out <= DateTime.Now;
        }

        private static DateTime ObtenerFechaCierreReserva(Reservation reserva)
        {
            if (reserva.checkout_completed_at.HasValue)
                return reserva.checkout_completed_at.Value;
            return reserva.check_out;
        }

        private static DateTime ObtenerFechaInicioPeriodo(DateTime ahora, string periodo)
        {
            return periodo switch
            {
                "Hoy" => ahora.Date,
                "7 días" => ahora.AddDays(-7),
                "90 días" => ahora.AddDays(-90),
                _ => ahora.AddDays(-30),
            };
        }

        private static string ObtenerTemporada(DateTime fecha)
        {
            string nombre = fecha.Month switch
            {
                12 or 1 or 2 => "INV",
                3 or 4 or 5 => "PRI",
                6 or 7 or 8 => "VER",
                _ => "OTO",
            };
            return $"{nombre}-{fecha.Year}";
        }

        private static string AbrevMes(int month)
        {
            return month switch
            {
                1 => "Ene",
                2 => "Feb",
                3 => "Mar",
                4 => "Abr",
                5 => "May",
                6 => "Jun",
                7 => "Jul",
                8 => "Ago",
                9 => "Sep",
                10 => "Oct",
                11 => "Nov",
                _ => "Dic",
            };
        }
    }

    public class RangoUsuarioMetric
    {
        public RangoUsuarioMetric(string rango, int usuarios)
        {
            Rango = rango;
            Usuarios = usuarios;
        }

        public string Rango { get; }
        public int Usuarios { get; }
        public double Porcentaje { get; set; }
        public string PorcentajeTexto => $"{Porcentaje.ToString("0.0", CultureInfo.InvariantCulture)} %";
    }

    public class HabitacionDemandaMetric
    {
        public HabitacionDemandaMetric(string habitacion, int reservas)
        {
            Habitacion = habitacion;
            Reservas = reservas;
        }

        public string Habitacion { get; }
        public int Reservas { get; }
        public double PorcentajeEscala { get; set; }
    }

    public class IngresoMensualMetric
    {
        public IngresoMensualMetric(string mes, decimal ingreso)
        {
            Mes = mes;
            Ingreso = ingreso;
        }

        public string Mes { get; }
        public decimal Ingreso { get; }
        public double PorcentajeEscala { get; set; }
        public string IngresoTexto => $"{Ingreso:N0} EUR";
    }
}
