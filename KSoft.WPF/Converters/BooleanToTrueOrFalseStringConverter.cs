using System;
using System.Windows.Data;

namespace KSoft.WPF.Converters
{
	public sealed class TrueOrFalseString
	{
		public string TrueString { get; set; }
		public string FalseString { get; set; }
	};

	[ValueConversion(typeof(bool), typeof(string))]
	public class BooleanToTrueOrFalseStringConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool && parameter is TrueOrFalseString)
			{
				var str = parameter as TrueOrFalseString;
				if ((bool)value)
					return str.TrueString;
				else
					return str.FalseString;
			}

			throw new InvalidOperationException("The value must be a boolean and parameter must be a " + nameof(TrueOrFalseString));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	};
}
