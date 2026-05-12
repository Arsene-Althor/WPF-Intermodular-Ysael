using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class GestionUsuarios : UserControl
    {
        public GestionUsuariosViewModel ViewModel { get; }
        public Usuario? UsuarioSeleccionado { get; private set; }

        private readonly bool _modoSelector;

        public GestionUsuarios() : this(false) { }

        /// <param name="modoSelector">true: doble clic devuelve usuario y cierra ventana contenedora.</param>
        public GestionUsuarios(bool modoSelector)
        {
            _modoSelector = modoSelector;
            InitializeComponent();
            ViewModel = new GestionUsuariosViewModel();
            DataContext = ViewModel;
        }

        /// <summary>Abre selector en ventana modal. Devuelve null si cancela.</summary>
        public static Usuario? ShowPickerDialog()
        {
            var uc = new GestionUsuarios(true);
            var shell = new Window
            {
                Owner = UiShell.OwnerWindow,
                Title = "Seleccionar cliente",
                Width = 1100,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = uc
            };
            return shell.ShowDialog() == true ? uc.UsuarioSeleccionado : null;
        }

        private void dgUsuarios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem is not Usuario usuario)
                return;

            if (_modoSelector)
            {
                UsuarioSeleccionado = usuario;
                var w = Window.GetWindow(this);
                if (w != null)
                {
                    w.DialogResult = true;
                    w.Close();
                }
                return;
            }

            ViewModel.EditarUsuarioCommand.Execute(null);
        }
    }
}
