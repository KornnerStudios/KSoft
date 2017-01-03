using System;
using System.Windows;
using System.Windows.Data;

namespace KSoft.WPF
{
	public sealed class BoolToVisiblityConverter : IValueConverter
	{
		private static BoolToVisiblityConverter gVisibleOrHidden;
		public static BoolToVisiblityConverter VisibleOrHidden { get {
			if (gVisibleOrHidden == null)
				gVisibleOrHidden = new BoolToVisiblityConverter
				{
				};

			return gVisibleOrHidden;
		} }
		private static BoolToVisiblityConverter gVisibleOrHiddenInverted;
		public static BoolToVisiblityConverter VisibleOrHiddenInverted { get {
			if (gVisibleOrHiddenInverted == null)
				gVisibleOrHiddenInverted = new BoolToVisiblityConverter
				{
					VisibleFlag = false,
				};

			return gVisibleOrHiddenInverted;
		} }

		private static BoolToVisiblityConverter gVisibleOrCollapsed;
		public static BoolToVisiblityConverter VisibleOrCollapsed { get {
			if (gVisibleOrCollapsed == null)
				gVisibleOrCollapsed = new BoolToVisiblityConverter
				{
					Collapse = true,
				};

			return gVisibleOrCollapsed;
		} }
		private static BoolToVisiblityConverter gVisibleOrCollapsedInverted;
		public static BoolToVisiblityConverter VisibleOrCollapseInverted { get {
			if (gVisibleOrCollapsedInverted == null)
				gVisibleOrCollapsedInverted = new BoolToVisiblityConverter
				{
					Collapse = true,
					VisibleFlag = false,
				};

			return gVisibleOrCollapsedInverted;
		} }

		public bool Collapse { get; set; }
		public bool VisibleFlag { get; set; }

		private BoolToVisiblityConverter()
		{
			VisibleFlag = true;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool is_visible = (bool)value;
			var visibility = Visibility.Visible;

			if (is_visible != VisibleFlag)
			{
				visibility = Collapse
					? Visibility.Collapsed
					: Visibility.Hidden;
			}

			return visibility;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var visibility = (Visibility)value;

			if (visibility == Visibility.Visible)
				return VisibleFlag;
			else
				return !VisibleFlag;
		}
	};
}
