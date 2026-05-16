using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class ClientFichaEstancias : Window
    {
        public ClientFichaEstancias(Usuario usuario)
        {
            InitializeComponent();
            DataContext = new ClientFichaEstanciasViewModel(usuario);
            Owner = UiShell.OwnerWindow;
        }

        public static void ShowDialogFor(Usuario usuario)
        {
            if (usuario == null || usuario.role != "client") return;
            new ClientFichaEstancias(usuario).ShowDialog();
        }
    }
}
