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
			bool v = (bool)value;

			if (v == true)
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
