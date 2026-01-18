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
        private DateTime _check_in { get; set; }
        private DateTime _check_out { get; set; }
        private DateTime? _cancelation_date { get; set; }

        public DateTime check_in
        {
            get => _check_in.ToLocalTime();
            set => _check_in = value;
        }

        public DateTime check_out
        {
            get => _check_out.ToLocalTime();
            set => _check_out = value;
        }

        public DateTime? cancelation_date
        {
            get => _cancelation_date?.ToLocalTime();
            set => _cancelation_date = value;
        }


    }
}
