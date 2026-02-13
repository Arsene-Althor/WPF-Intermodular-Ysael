using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.ViewModels
{
    public class SelectedUserViewModel : BaseViewModel
    {
        // CAMPOS PRIVADOS
        private List<Usuario> _usuarios;
        private List<Usuario>? _usuariosFiltrados;
        private Usuario? _usuarioSeleccionado;

        private string _filtroIdUser = "";
        private string _filtroDNI = "";
        private string _filtroNombre = "";
        private string _filtroApellido = "";
        private string _filtroEmail = "";

        // PROPIEDADES PÚBLICAS
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

        public string FiltroIdUser
        {
            get => _filtroIdUser;
            set { _filtroIdUser = value; OnPropertyChanged(); AplicarFiltros(); }
        }

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

        // COMMANDS
        public ICommand LimpiarFiltrosCommand { get; }

        // CONSTRUCTOR
        public SelectedUserViewModel(List<Usuario> usuarios)
        {
            _usuarios = usuarios;
            UsuariosFiltrados = usuarios;
            LimpiarFiltrosCommand = new RelayCommand(ExecuteLimpiarFiltros);
        }

        // MÉTODOS
        private void AplicarFiltros()
        {
            if (_usuarios == null) return;

            string userId = _filtroIdUser?.ToLower().Trim() ?? "";
            string dni = _filtroDNI?.ToLower().Trim() ?? "";
            string nombre = _filtroNombre?.ToLower().Trim() ?? "";
            string apellido = _filtroApellido?.ToLower().Trim() ?? "";
            string email = _filtroEmail?.ToLower().Trim() ?? "";

            var resultado = _usuarios.Where(u =>
            {
                bool Bid = string.IsNullOrEmpty(userId) || u.user_id.ToString().ToLower().Contains(userId);
                bool Bdni = string.IsNullOrEmpty(dni) || (!string.IsNullOrEmpty(u.dni) && u.dni.ToLower().Contains(dni));
                bool Bnombre = string.IsNullOrEmpty(nombre) || (!string.IsNullOrEmpty(u.name) && u.name.ToLower().Contains(nombre));
                bool Bapellido = string.IsNullOrEmpty(apellido) || (!string.IsNullOrEmpty(u.surname) && u.surname.ToLower().Contains(apellido));
                bool Bemail = string.IsNullOrEmpty(email) || (!string.IsNullOrEmpty(u.email) && u.email.ToLower().Contains(email));

                return Bid && Bdni && Bnombre && Bapellido && Bemail;
            }).ToList();

            UsuariosFiltrados = resultado;
        }

        private void ExecuteLimpiarFiltros()
        {
            _filtroIdUser = "";
            _filtroDNI = "";
            _filtroNombre = "";
            _filtroApellido = "";
            _filtroEmail = "";

            OnPropertyChanged(nameof(FiltroIdUser));
            OnPropertyChanged(nameof(FiltroDNI));
            OnPropertyChanged(nameof(FiltroNombre));
            OnPropertyChanged(nameof(FiltroApellido));
            OnPropertyChanged(nameof(FiltroEmail));

            AplicarFiltros();
        }
    }
}
