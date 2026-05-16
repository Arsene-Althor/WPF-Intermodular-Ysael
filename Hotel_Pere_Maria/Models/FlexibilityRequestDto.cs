using System;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
  public class FlexibilityRequestDto
  {
    public DateTime? requested_at { get; set; }
    public DateTime? requested_time { get; set; }
    public string? status { get; set; }
    public string? loyalty_tier { get; set; }
    public double? hours_difference { get; set; }
    public double? rate_per_hour { get; set; }
    public double? base_fee { get; set; }
    public double? discount_percent { get; set; }
    public double? final_fee { get; set; }
    public bool? availability_ok { get; set; }
    public bool? auto_approved { get; set; }
    public string? approval_mode { get; set; }
    public string? review_note { get; set; }
    public string? late_mode { get; set; }
    public DateTime? reviewed_at { get; set; }
    public string? reviewed_by { get; set; }

    [JsonIgnore]
    public bool HasRequest => !string.IsNullOrWhiteSpace(status);

    [JsonIgnore]
    public bool IsPending => status == "pending";

    [JsonIgnore]
    public bool CanStaffReview => IsPending;

    public string StatusLabel => status switch
    {
      "pending" => "Pendiente",
      "approved" => "Aprobada",
      "rejected" => "Rechazada",
      _ => status ?? "—"
    };

    public string TierLabel => loyalty_tier switch
    {
      "gold" => "Oro",
      "silver" => "Plata",
      "bronze" => "Bronce",
      _ => loyalty_tier ?? "—"
    };

    public string RequestedTimeText =>
      requested_time.HasValue ? requested_time.Value.ToString("dd/MM/yyyy HH:mm") : "—";

    public string FeeText => final_fee.HasValue ? $"{final_fee.Value:N2} €" : "—";

    public string HoursText =>
      hours_difference.HasValue ? $"{hours_difference.Value:N1} h" : "—";

    public string AvailabilityLabel =>
      availability_ok == false ? "Sin disponibilidad en la franja" : availability_ok == true ? "Hueco disponible" : "—";

    public string ApprovalLabel =>
      auto_approved == true ? "Aprobación automática (fidelidad)" :
      approval_mode == "manual" ? "Revisión manual" : "—";

    public string ReviewNoteDisplay =>
      string.IsNullOrWhiteSpace(review_note) ? "" : $"Nota: {review_note.Trim()}";
  }
}
