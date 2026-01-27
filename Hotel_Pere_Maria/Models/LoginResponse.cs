namespace Hotel_Pere_Maria.Models
{
    public class LoginResponse
    {
        public string mensaje { get; set; }
        public string token { get; set; }
        public Usuario user { get; set; }

    }
}