using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hotel_Pere_Maria.Models;
using Hotel_Pere_Maria.Services;
using Hotel_Pere_Maria.Views;

namespace Hotel_Pere_Maria
{
    //Pantalla de inicio de sesion
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ApiService._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiRU1QLTAwMDAxIiwiZW1haWwiOiJhZG1pbjJAZ21haWwuY29tIiwicm9sZSI6ImFkbWluIiwiaXNWSVAiOmZhbHNlLCJpYXQiOjE3Njk0NTA3MjUsImV4cCI6MTc2OTUzNzEyNX0.qXUjgqfdI-LUrkdAvCLKgoqEI5rsaEcY0PEY7-4CCb4");
            Inicio inicio = new Inicio();
            inicio.Show();
            this.Close();

        }
    }
}