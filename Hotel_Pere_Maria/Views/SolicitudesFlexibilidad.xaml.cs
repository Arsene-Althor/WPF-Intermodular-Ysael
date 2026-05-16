using System.Windows;
using System.Windows.Controls;

namespace Hotel_Pere_Maria.Views
{
  public partial class SolicitudesFlexibilidad : UserControl
  {
    private readonly ViewModels.SolicitudesFlexibilidadViewModel _vm = new();

    public SolicitudesFlexibilidad()
    {
      InitializeComponent();
      DataContext = _vm;
      Loaded += async (_, _) => await _vm.CargarAsync();
      IsVisibleChanged += async (_, e) =>
      {
        if (IsVisible && e.NewValue is true)
          await _vm.CargarAsync();
      };
    }
  }
}
