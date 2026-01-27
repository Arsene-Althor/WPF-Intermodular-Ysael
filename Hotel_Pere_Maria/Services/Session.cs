using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    ///<summary>
    ///Este apartado almacenara los datos de la sesión actual en memoria (globales)
    /// </summary> 


    // Los datos del usuario logueado se guardaran en memoria
    public static class Session
    {

        //Aqui guardamos el JWT que nos da el servidor, con esto podemos hacer peticiones a la api
        // Se usa asi: Authorization: Bearer {token}
        public static string Token {  get; set; }

        // El objeto Usuario con todos sus datos, asi podremos acceder desde cualquier ventana
        public static Usuario User { get; set; }

        // Propiedad de lectura que nos dice si hay alguien logueado o no
        // Devuelve true si hay token Y hay usuario
        public static bool IsLoggedIn => ! string.IsNullOrEmpty(Token) && User != null;

        // Método para limpiar la sesión cuando el usuario hace logout
        public static void Clear()
        {
            Token = null;
            User = null;
        }
    }

}