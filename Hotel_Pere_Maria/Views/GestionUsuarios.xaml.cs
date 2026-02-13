using System.Windows;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.ViewModels;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>
    /// MVVM: El code-behind ahora está casi vacío.
    /// Toda la lógica (filtros, CRUD, carga de datos) vive en GestionUsuariosViewModel.
    /// Aquí solo queda:
    ///   1. Asignar el DataContext al ViewModel
    ///   2. El doble-clic para seleccionar usuario (porque necesita DialogResult)
    /// </summary>
    public partial class GestionUsuarios : Window
    {
        // Exponemos el ViewModel por si alguna otra ventana necesita acceder a él
        public GestionUsuariosViewModel ViewModel { get; }

        // Esta propiedad se usa cuando GestionUsuarios se abre como selector
        // (desde addReserva, modReserva, etc.) para devolver el usuario elegido
        public Usuario? UsuarioSeleccionado { get; private set; }

        public GestionUsuarios()
        {
            InitializeComponent();

            // MVVM: Creamos el ViewModel y lo asignamos como DataContext.
            // A partir de aquí, todos los {Binding} del XAML buscan sus propiedades
            // en este ViewModel automáticamente.
            ViewModel = new GestionUsuariosViewModel();
            DataContext = ViewModel;
        }

        /// <summary>
        /// MVVM: Este evento se queda en el code-behind porque necesita
        /// acceso a DialogResult y this.Close(), que son cosas de la VISTA (UI),
        /// no de la lógica de negocio. Esto es perfectamente válido en MVVM.
        /// </summary>
        private void dgUsuarios_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario usuario)
            {
                // Si esta ventana fue abierta desde otra ventana (como selector)
                if (this.Owner != null)
                {
                    if (this.Owner is addReserva || this.Owner is modReserva || this.Owner is listReservas)
                    {
                        UsuarioSeleccionado = usuario;
                        this.DialogResult = true;
                        this.Close();
                        return;
                    }
                }

                // Si no es selector, abrimos edición a través del ViewModel
                ViewModel.EditarUsuarioCommand.Execute(null);
            }
        }
    }
}