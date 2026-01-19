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
    /// Lógica de interacción para modReserva.xaml
    /// </summary>
    public partial class modReserva : Window
    {
        private Reservation _reserva;
        public modReserva(Reservation reserva)
        {
            InitializeComponent();
            this._reserva = reserva;
            relleanForm();

        }

        private void RecalcularPrecio(object sender, EventArgs e) { 
        }

        private void relleanForm() {
            nReserva.Text = nReserva.Text + _reserva.reservation_id;
        }
        private async void Click_Guardar(object sender, RoutedEventArgs e)
        { 
        }
        private async void Click_Cancelar(object sender, RoutedEventArgs e)
        {
        }

    }
}
