using System.Globalization;

namespace OsrmRoutesProvider.Helpers
{
    public static class DoubleExtensions
    {
        public static string ToGbString(this double value)
        {
            return value.ToString(CultureInfo.GetCultureInfo("en-GB"));
        }
    }
}
