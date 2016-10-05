using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OsrmRoutesEditor.Controls.TrackPlayer.Helpers
{
    public class PlayerStateToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return new BitmapImage(new Uri(@"\Controls\TrackPlayer\Images\pause.png", UriKind.Relative));

            return new BitmapImage(new Uri(@"\Controls\TrackPlayer\Images\play.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
