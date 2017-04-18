using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FluxLike.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = (bool)value;

			if(v == true)
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
