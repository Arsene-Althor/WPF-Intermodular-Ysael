using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class modReserva : Window
    {
        public modReserva(Reservation reserva)
        {
            InitializeComponent();
            var viewModel = new ModReservaViewModel(reserva);

            this.DataContext = viewModel;

            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
