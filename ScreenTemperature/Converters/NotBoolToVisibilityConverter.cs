using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScreenTemperature.Converters
{
	public class NotBoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = value != null && (bool)value;

			if (v)
			{
				return parameter == null ? Visibility.Collapsed : Visibility.Hidden;
			}
			else
			{
				return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
