using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
    public partial class listAuditorias : UserControl
    {
        public listAuditorias()
        {
            InitializeComponent();
            DataContext = new ViewModels.ListAuditoriasViewModel();
        }
    }
}
