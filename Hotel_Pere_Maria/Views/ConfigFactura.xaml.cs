using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
    public partial class ConfigFactura : UserControl
    {
        public ConfigFactura()
        {
            InitializeComponent();
            DataContext = new ViewModels.ConfigFacturaViewModel();
        }
    }
}
