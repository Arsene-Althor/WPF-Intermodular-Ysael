namespace Hotel_Pere_Maria.Models
{
    public class Usuario
    {
        public string user_id { get; set; } // "Client-001", "Employ-001"
        public string email { get; set; }
        public string name { get; set; }
        public string role { get; set; } // "client", "employee", "admin"

    }


}