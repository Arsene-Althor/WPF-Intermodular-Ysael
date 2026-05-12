using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Hotel_Pere_Maria.Services;

namespace Hotel_Pere_Maria.Converters
{
    /// <summary>Convierte URL relativa o absoluta de imagen en BitmapImage para WPF.</summary>
    public class ImageUrlConverter : IValueConverter
    {
        private static readonly string DefaultRoom =
            "https://images.unsplash.com/photo-1513694203232-719a280e022f?q=80&w=2069&auto=format&fit=crop";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                s = DefaultRoom;
            try
            {
                if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return new BitmapImage(new Uri(s, UriKind.Absolute));

                var baseRoot = ApiService.BaseUrl.TrimEnd('/');
                var path = s.TrimStart('/');
                var full = $"{baseRoot}/{path}";
                return new BitmapImage(new Uri(full, UriKind.Absolute));
            }
            catch
            {
                return new BitmapImage(new Uri(DefaultRoom, UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
