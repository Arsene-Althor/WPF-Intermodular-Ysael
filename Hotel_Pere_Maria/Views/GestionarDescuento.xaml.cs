using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class GestionarDescuento : Window
    {
        public GestionarDescuento(Usuario usuario)
        {
            InitializeComponent();

            var viewModel = new GestionarDescuentoViewModel(usuario);
            DataContext = viewModel;

            // Suscribirse al evento de cierre con DialogResult
            viewModel.RequestClose += (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
        }
    }
}