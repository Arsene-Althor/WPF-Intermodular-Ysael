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

namespace Hotel_Pere_Maria.Views
{
    //Ventana para mostrar una lista con todas las reservas
    public partial class listReservas : Window
    {
        public listReservas()
        {
            InitializeComponent();

            // Asignamos el ViewModel como fuente de datos de la ventana
            this.DataContext = new ListReservasViewModel();
        }
    }
}
