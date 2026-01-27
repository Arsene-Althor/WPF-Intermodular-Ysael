namespace Hotel_Pere_Maria.Models
{
    // Este objeto es lo que mandamos al servidor cuando queremos hacer login
    // Básicamente: email + contraseña
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}