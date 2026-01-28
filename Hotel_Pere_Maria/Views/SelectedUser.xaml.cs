using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para SelectedUser.xaml
    /// </summary>
    public partial class SelectedUser : Window
    {
        private List<Usuario> ? _usuarios;
        public SelectedUser(List<Usuario>? usuarios)
        {
            InitializeComponent();
            _usuarios = usuarios;
        }
        private void FiltrarDatos(object sender, EventArgs e)
        {
            
        }

        //Utilizamos este metodo para limpiar todos los filtros
        private void Click_LimpiarFiltros(object sender, RoutedEventArgs e)
        {
            
        }
        private void dbClick_SelectUser(object sender, MouseButtonEventArgs e)
        {
            if (this.Owner is addReserva)
            {
                MessageBox.Show("Ventana abierda desde addReserva");
            }
            else {
                MessageBox.Show("Ventana abierta para modReserva");
            }
            
        }
    }
}
