using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hotel_Pere_Maria.Views
{
    public partial class listAuditorias : UserControl
    {
        public listAuditorias()
        {
            InitializeComponent();
            DataContext = new ViewModels.ListAuditoriasViewModel();
        }

        private void CerrarDetalleFila_Click(object sender, RoutedEventArgs e)
        {
            GridAuditoria.SelectedItem = null;
            e.Handled = true;
        }

        /// <summary>Segundo clic en la misma fila ya seleccionada cierra el detalle.</summary>
        private void AuditRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGridRow row || !row.IsSelected)
                return;

            GridAuditoria.SelectedItem = null;
            e.Handled = true;
        }
    }
}
