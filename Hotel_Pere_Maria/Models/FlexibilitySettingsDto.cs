using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
  public class FlexibilitySettingsDto
  {
    public double early_checkin_rate_per_hour { get; set; }
    public double late_checkout_rate_per_hour { get; set; }
    public double min_billable_hours { get; set; }
    public double max_supplement_eur { get; set; }
    public bool notify_client_on_decision { get; set; }
    public List<string>? free_access_tiers { get; set; }
    public double discount_bronze_percent { get; set; }
    public double discount_silver_percent { get; set; }
    public double discount_gold_percent { get; set; }
    public double early_min_hour { get; set; }
    public double late_max_hour { get; set; }
    public double max_early_hours { get; set; }
    public double max_late_hours { get; set; }
  }
}
