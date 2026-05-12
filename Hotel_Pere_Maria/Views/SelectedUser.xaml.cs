using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    public partial class SelectedUser : Window
    {
        public Usuario? SelecUser { get; private set; }

        public SelectedUser(List<Usuario> usuarios)
        {
            InitializeComponent();

            var viewModel = new SelectedUserViewModel(usuarios);
            DataContext = viewModel;
        }

        // Se queda en code-behind porque necesita DialogResult y this.Close()
        private void dbClick_SelectUser(object sender, MouseButtonEventArgs e)
        {
            if (Owner != null && dgUsuarios.SelectedItem is Usuario user)
            {
                SelecUser = user;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Abre el selector desde la pantalla de reservas o usuarios.");
            }
        }
    }
}
