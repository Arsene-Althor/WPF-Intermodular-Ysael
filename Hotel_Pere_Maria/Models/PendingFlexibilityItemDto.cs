using System;

namespace Hotel_Pere_Maria.Models
{
  public class PendingFlexibilityItemDto
  {
    public string? reservation_id { get; set; }
    public string? room_id { get; set; }
    public string? user_id { get; set; }
    public DateTime? check_in { get; set; }
    public DateTime? check_out { get; set; }
    public double? price { get; set; }
    public string? type { get; set; }
    public FlexibilityRequestDto? request { get; set; }
    public bool needs_review { get; set; }
    public string? status_summary { get; set; }
    public double? supplement { get; set; }
    public string? description { get; set; }
    public DateTime? issued_at { get; set; }
  }
}
