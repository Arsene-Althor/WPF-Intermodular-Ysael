using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
  public partial class ConfigFlexibilidad : UserControl
  {
    public ConfigFlexibilidad()
    {
      InitializeComponent();
      DataContext = new ViewModels.ConfigFlexibilidadViewModel();
    }
  }
}
