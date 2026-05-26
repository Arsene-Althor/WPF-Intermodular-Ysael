using System;
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

        /// <summary>DataGrid sin scroll interno: la rueda mueve el ScrollViewer de la página.</summary>
        private void GridAuditoria_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ScrollPrincipal == null) return;
            var offset = ScrollPrincipal.VerticalOffset - e.Delta;
            if (offset < 0) offset = 0;
            else if (offset > ScrollPrincipal.ScrollableHeight) offset = ScrollPrincipal.ScrollableHeight;
            ScrollPrincipal.ScrollToVerticalOffset(offset);
            e.Handled = true;
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
