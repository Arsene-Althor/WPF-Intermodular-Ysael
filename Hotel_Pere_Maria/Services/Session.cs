using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Services
{
    ///<summary>
    ///Este apartado almacenara los datos de la sesión actual en memoria (globales)
    /// </summary> 

    public static class Session
    {
        public static string Token {  get; set; }
        public static Usuario User { get; set; }
        public static bool IsLoggedIn => ! string.IsNullOrEmpty(Token) && User != null;

        public static void Clear()
        {
            Token = null;
            User = null;
        }
    }

}