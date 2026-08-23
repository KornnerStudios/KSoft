using System;
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
		static NumericTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox),
				new FrameworkPropertyMetadata(typeof(NumericTextBox)));
		}

		#region Value
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value), typeof(double), typeof(NumericTextBox),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, OnCoerceValue));

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
		private static object OnCoerceValue(DependencyObject d, object basevalue)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
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
			var textBlock = Template.FindName("TextBlock", this) as TextBlock
				?? throw new InvalidOperationException("NumericTextBox template must contain a TextBlock named \"TextBlock\".");
			var textBox = Template.FindName("TextBox", this) as TextBox
				?? throw new InvalidOperationException("NumericTextBox template must contain a TextBox named \"TextBox\".");

			var originalPosition = new Point();
			double originalValue = 0;
			var mouseMoved = false;

			textBlock.MouseDown += (sender, e) =>
			{
				originalPosition = e.GetPosition(textBlock);
				originalValue = Value;
				textBlock.CaptureMouse();
				mouseMoved = false;
			};
			textBlock.MouseMove += (sender, e) =>
			{
				if (!textBlock.IsMouseCaptured)
				{
					return;
				}

				mouseMoved = true;

				var newPosition = e.GetPosition(textBlock);
				Value = CoerceValue(originalValue + (newPosition.X - originalPosition.X) / 50.0);
			};
			textBlock.MouseUp += (sender, e) =>
			{
				if (textBlock.IsMouseCaptured)
				{
					textBlock.ReleaseMouseCapture();
				}

				if (!mouseMoved)
				{
					Mode = NumericTextBoxMode.TextBox;
					textBox.SelectAll();
					textBox.Focus();
				}
			};
			textBox.KeyUp += (sender, e) =>
			{
				if (e.Key == Key.Escape || e.Key == Key.Enter)
				{
					Mode = NumericTextBoxMode.Normal;
				}
			};
			textBox.LostFocus += (sender, e) => Mode = NumericTextBoxMode.Normal;

			base.OnApplyTemplate();
		}

		private double CoerceValue(double newValue)
		{
			if (Minimum != null && newValue < Minimum.Value)
			{
				return Minimum.Value;
			}

			if (Maximum != null && newValue > Maximum.Value)
			{
				return Maximum.Value;
			}

			return newValue;
		}
	};
}
