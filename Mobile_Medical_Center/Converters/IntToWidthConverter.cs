using System.Globalization;

namespace Mobile_Medical_Center.Converters
{
    public class IntToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // Convert appointment count to width (30 pixels per appointment, max 300)
                return Math.Min(intValue * 30, 300);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
