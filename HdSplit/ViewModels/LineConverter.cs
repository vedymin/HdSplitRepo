using System;
using System.Globalization;
using System.Windows.Data;

namespace HdSplit.ViewModels
{
    public class LineConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null)
            {
                string line = value.ToString();
                return line;

            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException ();
        }
    }
}