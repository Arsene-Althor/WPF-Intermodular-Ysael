using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
    public partial class listFacturas : UserControl
    {
        public listFacturas()
        {
            InitializeComponent();
            DataContext = new ViewModels.ListFacturasViewModel();
        }
    }
}
