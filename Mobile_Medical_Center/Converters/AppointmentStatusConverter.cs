using Mobile_Medical_Center.Models;
using System.Globalization;

namespace Mobile_Medical_Center.Converters
{
    public class AppointmentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppointmentStatus status)
            {
                return status switch
                {
                    AppointmentStatus.Scheduled => "Запланований",
                    AppointmentStatus.Completed => "Виконаний",
                    AppointmentStatus.Canceled => "Скасований",
                    _ => value.ToString()
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
