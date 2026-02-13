using System.Windows;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class PerfilUsuario : Window
    {
        public PerfilUsuario()
        {
            InitializeComponent();

            var viewModel = new PerfilUsuarioViewModel();
            DataContext = viewModel;

            // Suscribirse al evento de cierre
            viewModel.RequestClose += () => this.Close();
        }
    }
}