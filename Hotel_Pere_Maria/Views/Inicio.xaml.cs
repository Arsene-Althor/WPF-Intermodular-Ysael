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
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// Lógica de interacción para Inicio.xaml
    /// </summary>
    public partial class Inicio : Window
    {
        //Ventana de inicio accedemos a esta tras inicio de sesion
        public Inicio()
        {
            InitializeComponent();
            var viewModel = new InicioViewModel();

            this.DataContext = viewModel;

            viewModel.RequestClose += (s, e) => this.Close();

            Loaded += (_, __) =>
            {
                if (Application.Current.MainWindow != this)
                    Application.Current.MainWindow = this;
            };
        }
    }
}
