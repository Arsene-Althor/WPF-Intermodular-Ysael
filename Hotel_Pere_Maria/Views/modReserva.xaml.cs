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
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.ViewModels;
using Microsoft.VisualBasic;

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
