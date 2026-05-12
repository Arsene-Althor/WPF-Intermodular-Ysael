using System.Linq;
using System.Windows;

namespace Hotel_Pere_Maria
{
    /// <summary>Ventana shell (Inicio) u otra activa para Owner de diálogos.</summary>
    public static class UiShell
    {
        public static Window? OwnerWindow =>
            Application.Current?.MainWindow
            ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }
}
