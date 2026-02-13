using System;
using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class InsertarUsuario : Window
    {
        private InsertarUsuarioViewModel _viewModel;

        public InsertarUsuario(Usuario? usuario = null)
        {
            InitializeComponent();

            _viewModel = new InsertarUsuarioViewModel(usuario);
            DataContext = _viewModel;

            // Ocultar opción admin si no es admin
            if (!_viewModel.AdminVisible)
            {
                cbRolAdmin.Visibility = Visibility.Collapsed;
            }

            // Suscribirse al evento de cierre
            _viewModel.RequestClose += (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
        }

        // Click_Guardar se queda en code-behind porque necesitamos
        // pasar el Password desde los PasswordBox (no soportan Binding)
        private async void Click_Guardar(object sender, RoutedEventArgs e)
        {
            await _viewModel.ExecuteGuardarConPassword(
                txtPass.Password,
                txtPassConfirm.Password
            );
        }
    }
}