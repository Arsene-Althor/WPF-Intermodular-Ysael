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
    /// <summary>
    /// Lógica de interacción para addReserva.xaml
    /// </summary>
    public partial class addReserva : Window
    {
        public addReserva()
        {
            InitializeComponent();


            // 1. Creamos la instancia del ViewModel
            var viewModel = new AddReservaViewModel();

            // 2. Lo asignamos al DataContext para que los Bindings del XAML funcionen
            this.DataContext = viewModel;

            // 3. Suscribirse al evento de cierre para que el ViewModel pueda cerrar la ventana
            viewModel.RequestClose += (s, e) => this.Close();
        }
    }
}
