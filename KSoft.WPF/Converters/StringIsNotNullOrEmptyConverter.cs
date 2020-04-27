using System;
using System.Windows.Data;

namespace KSoft.WPF.Converters
{
	[ValueConversion(typeof(string), typeof(bool))]
	public class StringIsNotNullOrEmptyConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return false;

			if (value is string str)
			{
				return str.Length != 0;
			}

			throw new InvalidOperationException("The value must be a string, not a " + value.GetType());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	};
}
