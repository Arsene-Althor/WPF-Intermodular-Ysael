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
        public Usuario? SelecUser { get; private set; }
        public SelectedUser(List<Usuario>? usuarios)
        {
            InitializeComponent();
            _usuarios = usuarios;
            dgUsuarios.ItemsSource = _usuarios;
        }

        private void FiltrarDatos(object sender, EventArgs e)
        {
            if (_usuarios == null) return;

            string user_id = txtFiltroIdUser.Text.ToLower().Trim();
            string dni = txtFiltroDNI.Text.ToLower().Trim();
            string nombre = txtFiltroNombre.Text.ToLower().Trim();
            string apellido = txtFiltroApellido.Text.ToLower().Trim();
            string email = txtFiltroEmail.Text.ToLower().Trim();

            var resultado = _usuarios.Where(u =>
            {
                bool Bid = string.IsNullOrEmpty(user_id) || u.user_id.ToString().ToLower().Contains(user_id);
                bool Bdni = string.IsNullOrEmpty(dni) || u.dni.ToString().ToLower().Contains(dni);
                bool Bnombre = string.IsNullOrEmpty(nombre) || u.name.ToString().ToLower().Contains(nombre);
                bool Bapellido = string.IsNullOrEmpty(apellido) || u.surname.ToString().ToLower().Contains(apellido);
                bool Bemail = string.IsNullOrEmpty(email) || u.email.ToString().ToLower().Contains(email);

          
                return Bid && Bdni && Bnombre && Bapellido && Bemail;

            }).ToList();

            dgUsuarios.ItemsSource = resultado;
        }

        //Utilizamos este metodo para limpiar todos los filtros
        private void Click_LimpiarFiltros(object sender, RoutedEventArgs e)
        {
            
        }
        private void dbClick_SelectUser(object sender, MouseButtonEventArgs e)
        {
            if ((this.Owner is addReserva || this.Owner is modReserva || this.Owner is listReservas) && dgUsuarios.SelectedItem is Usuario user)
            {
                SelecUser = user;
                DialogResult = true;
                Close();

            }
            else {
                MessageBox.Show("Ventana abierta para modusuario");
            }
            
        }
    }
}
