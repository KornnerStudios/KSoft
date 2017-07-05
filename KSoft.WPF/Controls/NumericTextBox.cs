using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KSoft.WPF.Controls
{
	public enum NumericTextBoxMode
	{
		Normal,
		TextBox
	};

	public class NumericTextBox
		: Control
	{
		private TextBlock mTextBlock;
		private TextBox mTextBox;

		static NumericTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox),
				new FrameworkPropertyMetadata(typeof(NumericTextBox)));
		}

		#region Value
		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value), typeof(double), typeof(NumericTextBox),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, OnCoerceValue));

		private static object OnCoerceValue(DependencyObject d, object basevalue)
		{
			return ((NumericTextBox)d).CoerceValue((double)basevalue);
		}
		#endregion

		#region Minimum
		public double? Minimum
		{
			get { return (double?)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			nameof(Minimum), typeof(double?), typeof(NumericTextBox));
		#endregion

		#region Maximum
		public double? Maximum
		{
			get { return (double?)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			nameof(Maximum), typeof(double?), typeof(NumericTextBox));
		#endregion

		#region Mode
		public NumericTextBoxMode Mode
		{
			get { return (NumericTextBoxMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}
		public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
			nameof(Mode), typeof(NumericTextBoxMode), typeof(NumericTextBox),
			new FrameworkPropertyMetadata(NumericTextBoxMode.Normal));
		#endregion

		public override void OnApplyTemplate()
		{
			mTextBlock = (TextBlock)Template.FindName("TextBlock", this);

			var originalPosition = new Point();
			double originalValue = 0;
			var mouseMoved = false;

			mTextBlock.MouseDown += (sender, e) =>
			{
				originalPosition = e.GetPosition(mTextBlock);
				originalValue = Value;
				mTextBlock.CaptureMouse();
				mouseMoved = false;
			};

			mTextBlock.MouseMove += (sender, e) =>
			{
				if (!mTextBlock.IsMouseCaptured)
					return;

				mouseMoved = true;

				var newPosition = e.GetPosition(mTextBlock);
				Value = CoerceValue(originalValue + (newPosition.X - originalPosition.X) / 50.0);
			};

			mTextBlock.MouseUp += (sender, e) =>
			{
				if (mTextBlock.IsMouseCaptured)
					mTextBlock.ReleaseMouseCapture();

				if (!mouseMoved)
				{
					Mode = NumericTextBoxMode.TextBox;
					mTextBox.SelectAll();
					mTextBox.Focus();
				}
			};

			mTextBox = (TextBox)Template.FindName("TextBox", this);
			mTextBox.KeyUp += (sender, e) =>
			{
				if (e.Key == Key.Escape || e.Key == Key.Enter)
					Mode = NumericTextBoxMode.Normal;
			};
			mTextBox.LostFocus += (sender, e) => Mode = NumericTextBoxMode.Normal;

			base.OnApplyTemplate();
		}

		private double CoerceValue(double newValue)
		{
			if (Minimum != null && newValue < Minimum.Value)
				return Minimum.Value;
			if (Maximum != null && newValue > Maximum.Value)
				return Maximum.Value;
			return newValue;
		}
	};
}
