using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hotel_Pere_Maria.Models;

namespace Hotel_Pere_Maria.Views
{
    /// <summary>Selección de servicios extra para aplicar a varias habitaciones (unión con las ya asignadas).</summary>
    public sealed class BulkRoomExtrasDialog : Window
    {
        private readonly Dictionary<string, CheckBox> _checks = new();
        public List<string> SelectedServiceIds { get; private set; } = new();

        public BulkRoomExtrasDialog(IEnumerable<Room> rooms, List<ExtraServiceDto> catalog)
        {
            Title = "Asociar servicios extra";
            Width = 460;
            Height = 520;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var root = new StackPanel { Margin = new Thickness(16) };
            root.Children.Add(new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12),
                Text = $"Habitaciones: {string.Join(", ", rooms.Select(r => r.RoomId))}. Se marcan servicios a añadir (se hace unión con los que ya tuviera cada habitación).",
            });

            var scroll = new ScrollViewer { MaxHeight = 300, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var inner = new StackPanel();
            foreach (var c in catalog.Where(x => x.Active))
            {
                var cb = new CheckBox { Content = $"{c.Name} ({c.ServiceId})", Margin = new Thickness(0, 4, 0, 0), Tag = c.ServiceId };
                _checks[c.ServiceId] = cb;
                inner.Children.Add(cb);
            }
            scroll.Content = inner;
            root.Children.Add(scroll);

            var panel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
            var ok = new Button { Content = "Aplicar", Width = 100, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            ok.Click += (_, _) =>
            {
                SelectedServiceIds = _checks.Where(kv => kv.Value.IsChecked == true).Select(kv => kv.Key).ToList();
                DialogResult = true;
                Close();
            };
            var cancel = new Button { Content = "Cancelar", Width = 100, IsCancel = true };
            panel.Children.Add(ok);
            panel.Children.Add(cancel);
            root.Children.Add(panel);

            Content = root;
        }
    }
}
