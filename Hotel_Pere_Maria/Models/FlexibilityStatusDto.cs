using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
  public class FlexibilityStatusDto
  {
    public string? reservation_id { get; set; }
    public string? loyalty_tier { get; set; }
    public FlexibilityRequestDto? early_checkin_requested { get; set; }
    public FlexibilityRequestDto? late_checkout_requested { get; set; }
    public double? price { get; set; }
  }
}
