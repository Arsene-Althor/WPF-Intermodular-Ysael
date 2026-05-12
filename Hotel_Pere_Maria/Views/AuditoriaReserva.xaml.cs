using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class AuditoriaReserva : Window
    {
        public AuditoriaReserva(Reservation reserva)
        {
            InitializeComponent();
            var vm = new AuditoriaReservaViewModel(reserva?.reservation_id ?? "");
            DataContext = vm;
            Title = vm.TituloVentana;
            vm.RequestClose += (_, _) => Close();
        }
    }
}
