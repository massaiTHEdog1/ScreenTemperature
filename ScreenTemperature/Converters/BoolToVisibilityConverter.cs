using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScreenTemperature.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = value != null && (bool)value;

			if(v)
			{
				return Visibility.Visible;
			}
			else
			{
				return parameter == null ? Visibility.Collapsed : Visibility.Hidden;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
