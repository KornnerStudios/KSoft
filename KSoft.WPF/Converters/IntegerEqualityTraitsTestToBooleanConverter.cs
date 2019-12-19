using System;
using System.Windows.Data;

namespace KSoft.WPF.Converters
{
	[ValueConversion(typeof(sbyte), typeof(bool))]
	[ValueConversion(typeof(short), typeof(bool))]
	[ValueConversion(typeof(int), typeof(bool))]
	[ValueConversion(typeof(long), typeof(bool))]
	public class IntegerEqualityTraitsTestToBooleanConverter
		: IValueConverter
	{
		#region EqualsZero
		private static IntegerEqualityTraitsTestToBooleanConverter gEqualsZero;
		public static IntegerEqualityTraitsTestToBooleanConverter EqualsZero { get {
			if (gEqualsZero == null)
				gEqualsZero = new IntegerEqualityTraitsTestToBooleanConverter(Values.EqualityTraits.Equal, 0);

			return gEqualsZero;
		} }
		#endregion

		#region GreaterThanZero
		private static IntegerEqualityTraitsTestToBooleanConverter gGreaterThanZero;
		public static IntegerEqualityTraitsTestToBooleanConverter GreaterThanZero
		{
			get
			{
				if (gGreaterThanZero == null)
					gGreaterThanZero = new IntegerEqualityTraitsTestToBooleanConverter(Values.EqualityTraits.GreaterThan, 0);

				return gGreaterThanZero;
			}
		}
		#endregion

		public Values.EqualityTraits EqualityTraitsForTest { get; private set; }
		public long IntegerForTestInt64 { get; private set; }

		public IntegerEqualityTraitsTestToBooleanConverter(Values.EqualityTraits equalityTraits, long rhs)
		{
			EqualityTraitsForTest = equalityTraits;
			IntegerForTestInt64 = rhs;
		}

		bool PerformTest(long lhs)
		{
			if (EqualityTraitsForTest.IsNotEqual())
			{
				return lhs != IntegerForTestInt64;
			}
			if (EqualityTraitsForTest.IsEqual())
			{
				return lhs == IntegerForTestInt64;
			}
			if (EqualityTraitsForTest.IsLessThan())
			{
				return lhs < IntegerForTestInt64;
			}
			if (EqualityTraitsForTest.IsGreaterThan())
			{
				return lhs > IntegerForTestInt64;
			}

			throw new Debug.UnreachableException();
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value.TryGetTypeCode().IsSigned())
			{
				long lhs = System.Convert.ToInt64(value);
				bool test = PerformTest(lhs);
				return test;
			}

			throw new InvalidOperationException("The value must be a signed integer, not a " + targetType);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	};
}