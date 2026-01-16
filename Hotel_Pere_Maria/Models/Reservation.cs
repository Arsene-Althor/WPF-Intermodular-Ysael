using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Pere_Maria.Models
{
    public class Reservation
    {
        public string reservation_id {  get; set; }
        public string room_id { get; set; }
        public string user_id { get; set; }
        public DateTime check_in { get; set; }
        public DateTime check_out { get; set; }
        public DateTime? cancelation_date { get; set; }

        public override string ToString()
        {
            return $"Id reserva: {reservation_id}\nRoom_id: {room_id}\nUser_id: {user_id}\nCheck_in: {check_in.Date}\nCheck_out: {check_out.Date}";
        }

    }
}
