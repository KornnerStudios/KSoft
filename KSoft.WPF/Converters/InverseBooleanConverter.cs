using System;
using System.Windows.Data;

namespace KSoft.WPF.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public sealed class InverseBooleanConverter
		: IValueConverter
	{
		private static InverseBooleanConverter gInstance;
		public static InverseBooleanConverter Instance { get {
			if (gInstance == null)
				gInstance = new InverseBooleanConverter
				{
				};

			return gInstance;
		} }

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				throw new InvalidOperationException("The target must be a boolean");

			var boolean = (bool)value;
			return !boolean;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(bool))
				throw new InvalidOperationException("The target must be a boolean");

			var boolean = (bool)value;
			return !boolean;
		}
	};
}
