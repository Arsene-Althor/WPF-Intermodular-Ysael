namespace Hotel_Pere_Maria.Models
{
    // Esto es lo que nos devuelve el servidor si el login es correcto
    // Si el login falla, el servidor nos devuelve un error
    public class LoginResponse
    {
        public string mensaje { get; set; }
        // token JWT - esto es lo que usamos para que el servidor sepa quiénes somos
        // en las próximas peticiones. Se lo pasamos en el header "Authorization: Bearer {token}"
        public string token { get; set; }
        public Usuario user { get; set; }

    }
}