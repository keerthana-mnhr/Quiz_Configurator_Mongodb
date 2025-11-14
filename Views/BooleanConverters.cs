using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Quiz_Configurator.Views
{
    public static class BooleanConverters
    {
        public static readonly IValueConverter NotNullToBooleanConverter = new NotNullToBooleanConverterImpl();
        public static readonly IValueConverter InverseBooleanConverter = new InverseBooleanConverterImpl();
        public static readonly IValueConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverterImpl();
    }

    public class NotNullToBooleanConverterImpl : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverterImpl : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class BooleanToVisibilityConverterImpl : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}