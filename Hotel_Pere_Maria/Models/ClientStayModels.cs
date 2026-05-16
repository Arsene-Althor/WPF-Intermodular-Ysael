using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hotel_Pere_Maria.Models
{
    public class ClientStayHistoryResponse
    {
        public string user_id { get; set; } = "";
        public int page { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<ClientStayHistoryItem> items { get; set; } = new();
    }

    public class ClientStayHistoryItem
    {
        public string reservation_id { get; set; } = "";
        public string status { get; set; } = "";
        public double total_paid { get; set; }
        public int nights { get; set; }
        public StayRoomInfo? room { get; set; }
        public StayRatingInfo? rating { get; set; }

        private DateTime? _check_in;
        public DateTime? check_in
        {
            get => _check_in?.ToLocalTime();
            set => _check_in = value;
        }

        private DateTime? _check_out;
        public DateTime? check_out
        {
            get => _check_out?.ToLocalTime();
            set => _check_out = value;
        }

        [JsonIgnore]
        public string RoomLabel => room?.type ?? room?.room_id ?? "—";

        [JsonIgnore]
        public string RatingLabel =>
            rating?.rating > 0 ? $"{rating.rating}/5" : "Sin valorar";
    }

    public class StayRoomInfo
    {
        public string room_id { get; set; } = "";
        public string? name { get; set; }
        public string? type { get; set; }
        public string? description { get; set; }
        public string? image { get; set; }
    }

    public class StayRatingInfo
    {
        public string? review_id { get; set; }
        public int rating { get; set; }
        public string? comment { get; set; }
    }

    public class ClientStayStatsDto
    {
        public string user_id { get; set; } = "";
        public string? loyalty_tier { get; set; }
        public int total_nights { get; set; }
        public double total_spent { get; set; }
        public int completed_stays_count { get; set; }
        public string? favorite_season { get; set; }
        public int? favorite_month { get; set; }
        public MostBookedRoomDto? most_booked_room { get; set; }
        public int max_stay_streak { get; set; }

        private DateTime? _last_stay_checkout_at;
        public DateTime? last_stay_checkout_at
        {
            get => _last_stay_checkout_at?.ToLocalTime();
            set => _last_stay_checkout_at = value;
        }

        [JsonIgnore]
        public string TierDisplay => loyalty_tier switch
        {
            "gold" => "Oro",
            "silver" => "Plata",
            "bronze" => "Bronce",
            _ => loyalty_tier ?? "—"
        };
    }

    public class MostBookedRoomDto
    {
        public string room_id { get; set; } = "";
        public string? type { get; set; }
        public int bookings_count { get; set; }
    }
}
