using System.Windows;
using System.Windows.Controls;

namespace Hotel_Pere_Maria.Helpers
{
  /// <summary>Nota opcional al aprobar/rechazar solicitud P19.</summary>
  public static class FlexibilityReviewNoteDialog
  {
    public static (bool confirmed, string? note) Show(string titulo, string mensaje)
    {
      var win = new Window
      {
        Title = titulo,
        Width = 520,
        Height = 320,
        MinWidth = 480,
        MinHeight = 280,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Owner = Application.Current?.MainWindow,
        ResizeMode = ResizeMode.CanResizeWithGrip,
      };

      var nota = new TextBox
      {
        Height = 100,
        MinHeight = 72,
        Margin = new Thickness(0, 8, 0, 0),
        TextWrapping = TextWrapping.Wrap,
        AcceptsReturn = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
      };
      var panel = new StackPanel { Margin = new Thickness(20) };
      panel.Children.Add(new TextBlock { Text = mensaje, TextWrapping = TextWrapping.Wrap });
      panel.Children.Add(new TextBlock
      {
        Text = "Nota para el cliente (opcional):",
        Margin = new Thickness(0, 14, 0, 0),
        FontWeight = FontWeights.SemiBold,
      });
      panel.Children.Add(nota);

      var buttons = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new Thickness(0, 18, 0, 0),
      };
      var ok = new Button { Content = "Confirmar", Padding = new Thickness(20, 8, 20, 8), Margin = new Thickness(0, 0, 10, 0), MinWidth = 100 };
      ok.Click += (_, _) =>
      {
        win.Tag = string.IsNullOrWhiteSpace(nota.Text) ? null : nota.Text.Trim();
        win.DialogResult = true;
        win.Close();
      };
      var cancel = new Button { Content = "Cancelar", Padding = new Thickness(20, 8, 20, 8), MinWidth = 100 };
      cancel.Click += (_, _) =>
      {
        win.DialogResult = false;
        win.Close();
      };
      buttons.Children.Add(ok);
      buttons.Children.Add(cancel);
      panel.Children.Add(buttons);
      win.Content = panel;
      if (win.ShowDialog() != true) return (false, null);
      return (true, win.Tag as string);
    }
  }
}
