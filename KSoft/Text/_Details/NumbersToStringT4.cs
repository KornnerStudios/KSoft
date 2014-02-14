using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using StringBuilder = System.Text.StringBuilder;

namespace KSoft
{
	partial class Numbers
	{
		#region UInt32
		static void ToStringBuilder(StringBuilder sb, uint value, int radix, int startIndex, string digits)
		{
			var radix_in_word = (uint)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Insert(startIndex, digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, uint value, int radix, string digits)
		{
			int start_index = sb.Count;

			var radix_in_word = (uint)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Add(digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);

			sb.Reverse(start_index, sb.Count-start_index);
		}
		static string ToStringImpl(uint value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(uint value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(uint value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, uint value, NumeralBase radix = NumeralBase.Decimal, int startIndex = -1, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, uint value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region Int32
		static void ToStringBuilder(StringBuilder sb, int value, int radix, int startIndex, string digits)
		{
			// Sign support only exist for decimal and lower bases
			if(radix <= kBase10 && value < 0)
			{
				sb.Append('-');
				++startIndex;
				value = -value; // change the value to positive
			}

			var radix_in_word = (int)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Insert(startIndex, digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, int value, int radix, string digits)
		{
			int start_index = sb.Count;

			bool signed = false;
			// Sign support only exist for decimal and lower bases
			if(radix <= kBase10 && value < 0)
			{
				signed = true;
				value = -value; // change the value to positive
			}

			var radix_in_word = (int)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Add(digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);

			if(signed)
				sb.Add('-');

			sb.Reverse(start_index, sb.Count-start_index);
		}
		static string ToStringImpl(int value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(int value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(int value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, int value, NumeralBase radix = NumeralBase.Decimal, int startIndex = -1, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, int value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region UInt64
		static void ToStringBuilder(StringBuilder sb, ulong value, int radix, int startIndex, string digits)
		{
			var radix_in_word = (ulong)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Insert(startIndex, digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, ulong value, int radix, string digits)
		{
			int start_index = sb.Count;

			var radix_in_word = (ulong)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Add(digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);

			sb.Reverse(start_index, sb.Count-start_index);
		}
		static string ToStringImpl(ulong value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(ulong value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(ulong value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, ulong value, NumeralBase radix = NumeralBase.Decimal, int startIndex = -1, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, ulong value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion
		#region Int64
		static void ToStringBuilder(StringBuilder sb, long value, int radix, int startIndex, string digits)
		{
			// Sign support only exist for decimal and lower bases
			if(radix <= kBase10 && value < 0)
			{
				sb.Append('-');
				++startIndex;
				value = -value; // change the value to positive
			}

			var radix_in_word = (long)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Insert(startIndex, digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);
		}
		// List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
		// We use additional memory (List<>.ToArrray allocates a new array) but have less computational complexity
		static void ToStringBuilder(List<char> sb, long value, int radix, string digits)
		{
			int start_index = sb.Count;

			bool signed = false;
			// Sign support only exist for decimal and lower bases
			if(radix <= kBase10 && value < 0)
			{
				signed = true;
				value = -value; // change the value to positive
			}

			var radix_in_word = (long)radix;
			do {
				int digit_index = (int)(value % radix_in_word);
				sb.Add(digits[digit_index]);
				value /= radix_in_word;
			} while (value > 0);

			if(signed)
				sb.Add('-');

			sb.Reverse(start_index, sb.Count-start_index);
		}
		static string ToStringImpl(long value, int radix, string digits)
		{
			var sb = new List<char>();
			ToStringBuilder(sb, value, radix, digits);

			return new string(sb.ToArray());
		}
		public static string ToString(long value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(long value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		public static StringBuilder ToStringBuilder(StringBuilder sb, long value, NumeralBase radix = NumeralBase.Decimal, int startIndex = -1, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires(startIndex.IsNoneOrPositive());
			if(startIndex.IsNone())
				startIndex = sb.Length;

			ToStringBuilder(sb, value, (int)radix, startIndex, digits);
			return sb;
		}
		public static List<char> ToStringBuilder(List<char> sb, long value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(sb != null);
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			ToStringBuilder(sb, value, (int)radix, digits);
			return sb;
		}
		#endregion

		#region Byte
		public static string ToString(byte value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(byte value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region SByte
		public static string ToString(sbyte value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(sbyte value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region UInt16
		public static string ToString(ushort value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(ushort value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion
		#region Int16
		public static string ToString(short value, int radix = kBase10, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);

			return ToStringImpl(value, radix, digits);
		}
		public static string ToString(short value, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));

			return ToStringImpl(value, (int)radix, digits);
		}
		#endregion

		#region ToStringList UInt32
		public static string ToStringList(StringListDesc desc, IEnumerable<uint> values, 
			Predicate<IEnumerable<uint>> writeTerminator = null)
		{
			Contract.Requires(!desc.RequiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needs_separator = false;
			int radix = (int)desc.Radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needs_separator)
					{
						chars.Add(desc.Separator);
					}
					else needs_separator = true;

					ToStringBuilder(chars, value, radix, desc.Digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.Terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList Int32
		public static string ToStringList(StringListDesc desc, IEnumerable<int> values, 
			Predicate<IEnumerable<int>> writeTerminator = null)
		{
			Contract.Requires(!desc.RequiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needs_separator = false;
			int radix = (int)desc.Radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needs_separator)
					{
						chars.Add(desc.Separator);
					}
					else needs_separator = true;

					ToStringBuilder(chars, value, radix, desc.Digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.Terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList UInt64
		public static string ToStringList(StringListDesc desc, IEnumerable<ulong> values, 
			Predicate<IEnumerable<ulong>> writeTerminator = null)
		{
			Contract.Requires(!desc.RequiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needs_separator = false;
			int radix = (int)desc.Radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needs_separator)
					{
						chars.Add(desc.Separator);
					}
					else needs_separator = true;

					ToStringBuilder(chars, value, radix, desc.Digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.Terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
		#region ToStringList Int64
		public static string ToStringList(StringListDesc desc, IEnumerable<long> values, 
			Predicate<IEnumerable<long>> writeTerminator = null)
		{
			Contract.Requires(!desc.RequiresTerminator || writeTerminator != null);
			Contract.Ensures(Contract.Result<string>() != null);

			var chars = new List<char>();

			bool needs_separator = false;
			int radix = (int)desc.Radix;
			if (values != null)
			{
				foreach (var value in values)
				{
					if (needs_separator)
					{
						chars.Add(desc.Separator);
					}
					else needs_separator = true;

					ToStringBuilder(chars, value, radix, desc.Digits);
				}

				if (writeTerminator != null && writeTerminator(values))
					chars.Add(desc.Terminator);
			}

			return new string(chars.ToArray());
		}
		#endregion
	};
}