using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OsrmRoutesEditor.Helpers
{
    public class CheckingToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if((bool)value)
                return new BitmapImage(new Uri(@"\Images\sign-check-icon.png", UriKind.Relative));

            return new BitmapImage(new Uri(@"\Images\sign-error-icon.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
