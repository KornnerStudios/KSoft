using System;
using System.Windows;
using System.Windows.Data;

namespace KSoft.WPF
{
	[ValueConversion(typeof(bool), typeof(WindowState))]
	public sealed class BoolToWindowStateConverter : IValueConverter
	{
		private static BoolToWindowStateConverter gNormalOrMaximized;
		public static BoolToWindowStateConverter NormalOrMaximized { get {
			if (gNormalOrMaximized == null)
				gNormalOrMaximized = new BoolToWindowStateConverter
				{
				};

			return gNormalOrMaximized;
		} }
		private static BoolToWindowStateConverter gNormalOrMaximizedInverted;
		public static BoolToWindowStateConverter NormalOrMaximizedInverted { get {
			if (gNormalOrMaximizedInverted == null)
				gNormalOrMaximizedInverted = new BoolToWindowStateConverter
				{
					NormalFlag = false,
				};

			return gNormalOrMaximizedInverted;
		} }

		private static BoolToWindowStateConverter gNormalOrMinimized;
		public static BoolToWindowStateConverter NormalOrMinimized { get {
			if (gNormalOrMinimized == null)
				gNormalOrMinimized = new BoolToWindowStateConverter
				{
					Minimize = true,
				};

			return gNormalOrMinimized;
		} }
		private static BoolToWindowStateConverter gNormalOrMinimizedInverted;
		public static BoolToWindowStateConverter NormalOrMinimizedInverted { get {
			if (gNormalOrMinimizedInverted == null)
				gNormalOrMinimizedInverted = new BoolToWindowStateConverter
				{
					Minimize = true,
					NormalFlag = false,
				};

			return gNormalOrMinimizedInverted;
		} }

		public bool Minimize { get; private set; }
		public bool NormalFlag { get; private set; }

		private BoolToWindowStateConverter()
		{
			NormalFlag = true;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool is_normal = (bool)value;
			var state = WindowState.Normal;

			if (is_normal != NormalFlag)
			{
				state = Minimize
					? WindowState.Minimized
					: WindowState.Maximized;
			}

			return state;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var state = (WindowState)value;

			if (state == WindowState.Normal)
				return NormalFlag;
			else
				return !NormalFlag;
		}
	};
}
