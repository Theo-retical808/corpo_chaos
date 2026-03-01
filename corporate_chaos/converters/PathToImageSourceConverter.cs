using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CorporateChaos.Converters
{
    public class PathToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/{path}");
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    // Return fallback image
                    try
                    {
                        var fallbackUri = new Uri("pack://application:,,,/images/assistant.png");
                        var fallbackBitmap = new BitmapImage();
                        fallbackBitmap.BeginInit();
                        fallbackBitmap.UriSource = fallbackUri;
                        fallbackBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        fallbackBitmap.EndInit();
                        return fallbackBitmap;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
