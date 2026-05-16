using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.ViewModels
{
  public class ConfigFlexibilidadViewModel : BaseViewModel
  {
    private bool _cargando;
    private string _earlyRate = "13.33";
    private string _lateRate = "11.67";
    private string _minBillable = "1";
    private string _maxSupplement = "0";
    private string _dtoBronce = "0";
    private string _dtoPlata = "15";
    private string _dtoOro = "35";
    private string _earlyMinHour = "8";
    private string _lateMaxHour = "18";
    private string _maxEarlyHours = "4";
    private string _maxLateHours = "7";
    private bool _notifyClient = true;
    private bool _bronceGratis;
    private bool _plataGratis;
    private bool _oroGratis;

    public bool Cargando
    {
      get => _cargando;
      set { _cargando = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public string EarlyRatePerHour { get => _earlyRate; set { _earlyRate = value ?? ""; OnPropertyChanged(); } }
    public string LateRatePerHour { get => _lateRate; set { _lateRate = value ?? ""; OnPropertyChanged(); } }
    public string MinBillableHours { get => _minBillable; set { _minBillable = value ?? ""; OnPropertyChanged(); } }
    public string MaxSupplementEur { get => _maxSupplement; set { _maxSupplement = value ?? ""; OnPropertyChanged(); } }
    public string DtoBronce { get => _dtoBronce; set { _dtoBronce = value ?? ""; OnPropertyChanged(); } }
    public string DtoPlata { get => _dtoPlata; set { _dtoPlata = value ?? ""; OnPropertyChanged(); } }
    public string DtoOro { get => _dtoOro; set { _dtoOro = value ?? ""; OnPropertyChanged(); } }
    public string EarlyMinHour { get => _earlyMinHour; set { _earlyMinHour = value ?? ""; OnPropertyChanged(); } }
    public string LateMaxHour { get => _lateMaxHour; set { _lateMaxHour = value ?? ""; OnPropertyChanged(); } }
    public string MaxEarlyHours { get => _maxEarlyHours; set { _maxEarlyHours = value ?? ""; OnPropertyChanged(); } }
    public string MaxLateHours { get => _maxLateHours; set { _maxLateHours = value ?? ""; OnPropertyChanged(); } }
    public bool NotifyClient { get => _notifyClient; set { _notifyClient = value; OnPropertyChanged(); } }
    public bool BronceGratis { get => _bronceGratis; set { _bronceGratis = value; OnPropertyChanged(); } }
    public bool PlataGratis { get => _plataGratis; set { _plataGratis = value; OnPropertyChanged(); } }
    public bool OroGratis { get => _oroGratis; set { _oroGratis = value; OnPropertyChanged(); } }

    public ICommand CargarCommand { get; }
    public ICommand GuardarCommand { get; }

    public ConfigFlexibilidadViewModel()
    {
      CargarCommand = new RelayCommand(() => _ = CargarAsync(), () => !Cargando);
      GuardarCommand = new RelayCommand(() => _ = GuardarAsync(), () => !Cargando);
      _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
      Cargando = true;
      try
      {
        var (ok, msg, dto) = await FlexibilityService.GetSettingsAsync();
        if (!ok || dto == null)
        {
          MessageBox.Show(msg ?? "Error al cargar", "Flexibilidad", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }
        AplicarDto(dto);
      }
      finally { Cargando = false; }
    }

    private void AplicarDto(FlexibilitySettingsDto dto)
    {
      EarlyRatePerHour = dto.early_checkin_rate_per_hour.ToString(CultureInfo.InvariantCulture);
      LateRatePerHour = dto.late_checkout_rate_per_hour.ToString(CultureInfo.InvariantCulture);
      MinBillableHours = dto.min_billable_hours.ToString(CultureInfo.InvariantCulture);
      MaxSupplementEur = dto.max_supplement_eur.ToString(CultureInfo.InvariantCulture);
      DtoBronce = dto.discount_bronze_percent.ToString(CultureInfo.InvariantCulture);
      DtoPlata = dto.discount_silver_percent.ToString(CultureInfo.InvariantCulture);
      DtoOro = dto.discount_gold_percent.ToString(CultureInfo.InvariantCulture);
      EarlyMinHour = dto.early_min_hour.ToString(CultureInfo.InvariantCulture);
      LateMaxHour = dto.late_max_hour.ToString(CultureInfo.InvariantCulture);
      MaxEarlyHours = dto.max_early_hours.ToString(CultureInfo.InvariantCulture);
      MaxLateHours = dto.max_late_hours.ToString(CultureInfo.InvariantCulture);
      NotifyClient = dto.notify_client_on_decision;
      var free = dto.free_access_tiers ?? new List<string>();
      BronceGratis = free.Contains("bronze");
      PlataGratis = free.Contains("silver");
      OroGratis = free.Contains("gold");
    }

    private async Task GuardarAsync()
    {
      if (!TryParse(EarlyRatePerHour, out var earlyR) || !TryParse(LateRatePerHour, out var lateR) ||
          !TryParse(MinBillableHours, out var minB) || !TryParse(MaxSupplementEur, out var maxS) ||
          !TryParse(DtoBronce, out var dB) || !TryParse(DtoPlata, out var dS) || !TryParse(DtoOro, out var dG) ||
          !TryParse(EarlyMinHour, out var eMin) || !TryParse(LateMaxHour, out var lMax) ||
          !TryParse(MaxEarlyHours, out var maxE) || !TryParse(MaxLateHours, out var maxL))
      {
        MessageBox.Show("Revise los valores numéricos.", "Flexibilidad", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      var free = new List<string>();
      if (BronceGratis) free.Add("bronze");
      if (PlataGratis) free.Add("silver");
      if (OroGratis) free.Add("gold");

      var payload = new FlexibilitySettingsDto
      {
        early_checkin_rate_per_hour = earlyR,
        late_checkout_rate_per_hour = lateR,
        min_billable_hours = minB,
        max_supplement_eur = maxS,
        discount_bronze_percent = dB,
        discount_silver_percent = dS,
        discount_gold_percent = dG,
        early_min_hour = eMin,
        late_max_hour = lMax,
        max_early_hours = maxE,
        max_late_hours = maxL,
        notify_client_on_decision = NotifyClient,
        free_access_tiers = free,
      };

      Cargando = true;
      try
      {
        var (ok, msg, dto) = await FlexibilityService.PutSettingsAsync(payload);
        if (!ok || dto == null)
        {
          MessageBox.Show(msg ?? "Error al guardar", "Flexibilidad", MessageBoxButton.OK, MessageBoxImage.Warning);
          return;
        }
        AplicarDto(dto);
        MessageBox.Show("Reglas de flexibilidad guardadas.", "Flexibilidad", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      finally { Cargando = false; }
    }

    private static bool TryParse(string s, out double v)
    {
      return double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out v) && v >= 0;
    }
  }
}
