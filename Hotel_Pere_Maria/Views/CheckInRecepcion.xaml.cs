using System.Windows;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class CheckInRecepcion : Window
    {
        public CheckInRecepcion(string reservationId)
        {
            InitializeComponent();
            var vm = new CheckInRecepcionViewModel(reservationId);
            DataContext = vm;
            vm.RequestClose += (_, refreshed) =>
            {
                DialogResult = refreshed;
                Close();
            };
        }
    }
}
