using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria.ViewModels
{
    public class GestionUsuariosViewModel : BaseViewModel
    {
        // ==========================================
        // CAMPOS PRIVADOS (backing fields)
        // ==========================================
        // En MVVM, cada propiedad que queremos mostrar en la UI
        // necesita un campo privado + propiedad pública con OnPropertyChanged

        private List<Usuario>? _todosLosUsuarios;
        private List<Usuario>? _usuariosFiltrados;
        private Usuario? _usuarioSeleccionado;

        // Filtros de texto
        private string _filtroDNI = "";
        private string _filtroNombre = "";
        private string _filtroApellido = "";
        private string _filtroEmail = "";
        private string _filtroCiudad = "";

        // Filtros de ComboBox (guardamos el índice seleccionado)
        private int _filtroRolIndex;
        private int _filtroEstadoIndex;
        private int _filtroVIPIndex;
        private int _filtroGeneroIndex;
        private int _filtroDescuentoIndex;

        // Filtros de fecha
        private DateTime? _filtroFechaDesde;
        private DateTime? _filtroFechaHasta;

        // ==========================================
        // PROPIEDADES PÚBLICAS (se bindean en el XAML)
        // ==========================================
        // Cada setter llama a OnPropertyChanged() para que la UI se actualice
        // Los filtros además llaman a AplicarFiltros() automáticamente

        public List<Usuario>? UsuariosFiltrados
        {
            get => _usuariosFiltrados;
            set { _usuariosFiltrados = value; OnPropertyChanged(); }
        }

        public Usuario? UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set { _usuarioSeleccionado = value; OnPropertyChanged(); }
        }

        // --- Filtros de texto ---
        // UpdateSourceTrigger=PropertyChanged en el XAML hace que cada tecla
        // dispare el setter, que a su vez filtra la lista en tiempo real
        public string FiltroDNI
        {
            get => _filtroDNI;
            set { _filtroDNI = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public string FiltroNombre
        {
            get => _filtroNombre;
            set { _filtroNombre = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public string FiltroApellido
        {
            get => _filtroApellido;
            set { _filtroApellido = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public string FiltroEmail
        {
            get => _filtroEmail;
            set { _filtroEmail = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public string FiltroCiudad
        {
            get => _filtroCiudad;
            set { _filtroCiudad = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        // --- Filtros de ComboBox ---
        // SelectedIndex en el XAML se bindea aquí
        public int FiltroRolIndex
        {
            get => _filtroRolIndex;
            set { _filtroRolIndex = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public int FiltroEstadoIndex
        {
            get => _filtroEstadoIndex;
            set { _filtroEstadoIndex = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public int FiltroVIPIndex
        {
            get => _filtroVIPIndex;
            set { _filtroVIPIndex = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public int FiltroGeneroIndex
        {
            get => _filtroGeneroIndex;
            set { _filtroGeneroIndex = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public int FiltroDescuentoIndex
        {
            get => _filtroDescuentoIndex;
            set { _filtroDescuentoIndex = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        // --- Filtros de fecha ---
        public DateTime? FiltroFechaDesde
        {
            get => _filtroFechaDesde;
            set { _filtroFechaDesde = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        public DateTime? FiltroFechaHasta
        {
            get => _filtroFechaHasta;
            set { _filtroFechaHasta = value; OnPropertyChanged(); AplicarFiltros(); }
        }

        // ==========================================
        // COMMANDS (reemplazan los Click="..." del XAML)
        // ==========================================
        // En MVVM, los botones usan Command="{Binding NombreCommand}"
        // en vez de Click="NombreMetodo"

        public ICommand CargarUsuariosCommand { get; }
        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand RecargarCommand { get; }
        public ICommand GestionarDescuentoCommand { get; }
        public ICommand VerHistorialEstanciasCommand { get; }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public GestionUsuariosViewModel()
        {
            // Inicializamos cada Command con su método correspondiente
            // RelayCommand ya existe en tu proyecto (ViewModels/RelayCommand.cs)
            CargarUsuariosCommand = new RelayCommand(async () => await CargarUsuarios());
            NuevoUsuarioCommand = new RelayCommand(ExecuteNuevoUsuario);
            EditarUsuarioCommand = new RelayCommand(ExecuteEditarUsuario);
            EliminarUsuarioCommand = new RelayCommand(async () => await ExecuteEliminarUsuario());
            LimpiarFiltrosCommand = new RelayCommand(ExecuteLimpiarFiltros);
            RecargarCommand = new RelayCommand(ExecuteRecargar);
            GestionarDescuentoCommand = new RelayCommand(ExecuteGestionarDescuento);
            VerHistorialEstanciasCommand = new RelayCommand(ExecuteVerHistorialEstancias);

            // Cargamos los usuarios al construir el ViewModel
            _ = CargarUsuarios();
        }

        // ==========================================
        // MÉTODOS PRIVADOS (la lógica que antes estaba en el .xaml.cs)
        // ==========================================

        /// <summary>
        /// Carga todos los usuarios desde la API y los muestra en el DataGrid.
        /// Es el equivalente al antiguo método CargarUsuarios() del code-behind.
        /// </summary>
        private async System.Threading.Tasks.Task CargarUsuarios()
        {
            try
            {
                _todosLosUsuarios = await UserService.GetAllUsersAsync();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filtra la lista de usuarios según todos los criterios activos.
        /// Se ejecuta automáticamente cada vez que cambia cualquier filtro,
        /// gracias a que los setters de las propiedades llaman a este método.
        /// </summary>
        private void AplicarFiltros()
        {
            if (_todosLosUsuarios == null) return;

            // Convertimos los filtros de texto a minúsculas una sola vez
            string dni = _filtroDNI?.ToLower() ?? "";
            string nombre = _filtroNombre?.ToLower() ?? "";
            string apellido = _filtroApellido?.ToLower() ?? "";
            string email = _filtroEmail?.ToLower() ?? "";
            string ciudad = _filtroCiudad?.ToLower() ?? "";

            var filtrados = _todosLosUsuarios.Where(u =>
            {
                // Filtro DNI
                bool coincideDNI = string.IsNullOrEmpty(dni) ||
                                   (!string.IsNullOrEmpty(u.dni) && u.dni.ToLower().Contains(dni));

                // Filtro Nombre
                bool coincideNombre = string.IsNullOrEmpty(nombre) ||
                                      (!string.IsNullOrEmpty(u.name) && u.name.ToLower().Contains(nombre));

                // Filtro Apellido
                bool coincideApellido = string.IsNullOrEmpty(apellido) ||
                                        (!string.IsNullOrEmpty(u.surname) && u.surname.ToLower().Contains(apellido));

                // Filtro Email
                bool coincideEmail = string.IsNullOrEmpty(email) ||
                                     (!string.IsNullOrEmpty(u.email) && u.email.ToLower().Contains(email));

                // Filtro Ciudad
                bool coincideCiudad = string.IsNullOrEmpty(ciudad) ||
                                      (!string.IsNullOrEmpty(u.city) && u.city.ToLower().Contains(ciudad));

                // Filtro Rol (0=Todos, 1=Admin, 2=Empleado, 3=Cliente)
                bool coincideRol = true;
                if (_filtroRolIndex == 1) coincideRol = u.role == "admin";
                else if (_filtroRolIndex == 2) coincideRol = u.role == "employee";
                else if (_filtroRolIndex == 3) coincideRol = u.role == "client";

                // Filtro Estado (0=Todos, 1=Activos, 2=Inactivos)
                bool coincideEstado = true;
                if (_filtroEstadoIndex == 1) coincideEstado = u.isActive == true;
                else if (_filtroEstadoIndex == 2) coincideEstado = u.isActive == false;

                // Filtro VIP (0=Todos, 1=Solo VIP, 2=No VIP)
                bool coincideVIP = true;
                if (_filtroVIPIndex == 1) coincideVIP = u.isVIP == true;
                else if (_filtroVIPIndex == 2) coincideVIP = u.isVIP == false;

                // Filtro Género (0=Todos, 1=M, 2=F, 3=Other)
                bool coincideGenero = true;
                if (_filtroGeneroIndex == 1) coincideGenero = u.gender == "M";
                else if (_filtroGeneroIndex == 2) coincideGenero = u.gender == "F";
                else if (_filtroGeneroIndex == 3) coincideGenero = u.gender == "Other";

                // Filtro Descuento
                bool coincideDescuento = true;
                if (_filtroDescuentoIndex == 1) coincideDescuento = u.Discount > 0;
                else if (_filtroDescuentoIndex == 2) coincideDescuento = u.Discount == 0;
                else if (_filtroDescuentoIndex == 3) coincideDescuento = u.Discount >= 0.10;
                else if (_filtroDescuentoIndex == 4) coincideDescuento = u.Discount >= 0.20;
                else if (_filtroDescuentoIndex == 5) coincideDescuento = u.Discount >= 0.30;

                // Filtro Fecha Desde
                bool coincideFechaDesde = true;
                if (_filtroFechaDesde.HasValue)
                    coincideFechaDesde = u.createdAt >= _filtroFechaDesde.Value;

                // Filtro Fecha Hasta
                bool coincideFechaHasta = true;
                if (_filtroFechaHasta.HasValue)
                    coincideFechaHasta = u.createdAt <= _filtroFechaHasta.Value.AddDays(1);

                return coincideDNI && coincideNombre && coincideApellido && coincideEmail &&
                       coincideCiudad && coincideRol && coincideEstado && coincideVIP &&
                       coincideGenero && coincideDescuento && coincideFechaDesde && coincideFechaHasta;

            }).ToList();

            // Al asignar a la propiedad pública, OnPropertyChanged() notifica a la UI
            UsuariosFiltrados = filtrados;
        }

        /// <summary>
        /// Abre la ventana InsertarUsuario en modo creación.
        /// Si el usuario guarda, recargamos la lista.
        /// </summary>
        private void ExecuteNuevoUsuario()
        {
            InsertarUsuario ventanaNuevo = new InsertarUsuario();
            ventanaNuevo.Owner = Hotel_Pere_Maria.UiShell.OwnerWindow;
            bool? resultado = ventanaNuevo.ShowDialog();

            if (resultado == true)
            {
                _ = CargarUsuarios();
            }
        }

        /// <summary>
        /// Abre la ventana InsertarUsuario en modo edición con el usuario seleccionado.
        /// </summary>
        private void ExecuteEditarUsuario()
        {
            if (UsuarioSeleccionado != null)
            {
                InsertarUsuario ventanaEditar = new InsertarUsuario(UsuarioSeleccionado);
                ventanaEditar.Owner = Hotel_Pere_Maria.UiShell.OwnerWindow;
                bool? resultado = ventanaEditar.ShowDialog();

                if (resultado == true)
                {
                    _ = CargarUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario de la lista para editar.");
            }
        }

        /// <summary>
        /// Elimina (desactiva) el usuario seleccionado previa confirmación.
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteEliminarUsuario()
        {
            if (UsuarioSeleccionado != null)
            {
                var confirm = MessageBox.Show(
                    $"¿Estás seguro de eliminar a {UsuarioSeleccionado.name}?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserService.DeleteUserAsync(UsuarioSeleccionado.user_id);
                        MessageBox.Show("Usuario eliminado (desactivado) correctamente.");
                        await CargarUsuarios();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario para eliminar.");
            }
        }

        /// <summary>
        /// Resetea todos los filtros a sus valores por defecto.
        /// Al cambiar las propiedades, AplicarFiltros() se ejecuta automáticamente.
        /// </summary>
        private void ExecuteLimpiarFiltros()
        {
            // Temporalmente desactivamos el filtrado individual
            // cambiando los campos privados directamente
            _filtroDNI = "";
            _filtroNombre = "";
            _filtroApellido = "";
            _filtroEmail = "";
            _filtroCiudad = "";
            _filtroRolIndex = 0;
            _filtroEstadoIndex = 0;
            _filtroVIPIndex = 0;
            _filtroGeneroIndex = 0;
            _filtroDescuentoIndex = 0;
            _filtroFechaDesde = null;
            _filtroFechaHasta = null;

            // Notificamos todos los cambios de una vez
            OnPropertyChanged(nameof(FiltroDNI));
            OnPropertyChanged(nameof(FiltroNombre));
            OnPropertyChanged(nameof(FiltroApellido));
            OnPropertyChanged(nameof(FiltroEmail));
            OnPropertyChanged(nameof(FiltroCiudad));
            OnPropertyChanged(nameof(FiltroRolIndex));
            OnPropertyChanged(nameof(FiltroEstadoIndex));
            OnPropertyChanged(nameof(FiltroVIPIndex));
            OnPropertyChanged(nameof(FiltroGeneroIndex));
            OnPropertyChanged(nameof(FiltroDescuentoIndex));
            OnPropertyChanged(nameof(FiltroFechaDesde));
            OnPropertyChanged(nameof(FiltroFechaHasta));

            // Aplicamos el filtro una sola vez (mostrará todos)
            AplicarFiltros();
        }

        /// <summary>
        /// Limpia filtros y recarga desde la API.
        /// </summary>
        private void ExecuteRecargar()
        {
            ExecuteLimpiarFiltros();
            _ = CargarUsuarios();
        }

        /// <summary>
        /// Abre la ventana de gestión de descuento para el usuario seleccionado.
        /// </summary>
        private void ExecuteGestionarDescuento()
        {
            if (UsuarioSeleccionado != null)
            {
                GestionarDescuento ventanaDesc = new GestionarDescuento(UsuarioSeleccionado);
                ventanaDesc.Owner = Hotel_Pere_Maria.UiShell.OwnerWindow;
                bool? resultado = ventanaDesc.ShowDialog();

                if (resultado == true)
                {
                    _ = CargarUsuarios();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un usuario para aplicar descuento.");
            }
        }

        private void ExecuteVerHistorialEstancias()
        {
            if (UsuarioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un cliente.");
                return;
            }
            if (UsuarioSeleccionado.role != "client")
            {
                MessageBox.Show("El historial de estancias solo aplica a clientes.");
                return;
            }
            ClientFichaEstancias.ShowDialogFor(UsuarioSeleccionado);
        }
    }
}
