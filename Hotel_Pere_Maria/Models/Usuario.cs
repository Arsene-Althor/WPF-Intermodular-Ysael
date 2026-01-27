namespace Hotel_Pere_Maria.Models
{
    public class Usuario
    {
        // Representa a un usuario de la aplicación
        // Esto es lo que devuelve el servidor en el LoginResponse
        public string user_id { get; set; } // "Client-001", "Employ-001"
        public string email { get; set; }
        public string name { get; set; }

        // En MainWindow validamos que sea employee o admin, sino lo sacamos
        public string role { get; set; } // "client", "employee", "admin"

    }


}