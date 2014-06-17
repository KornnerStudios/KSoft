using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	partial class Numbers
	{
		#region UInt32
		static bool TryParseImpl(string s, ref uint result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;



			for(var radix_in_word = (uint)radix; success && pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (uint)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
					success = false;
			}


			return success;
		}
		public static bool TryParse(string s, out uint result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out uint result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out uint result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int32
		static bool TryParseImpl(string s, ref int result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;


			bool negate = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= kBase10 && pos < end)
			{
				char sign = s[pos];

				negate = sign == '-';
				// Skip the sign character
				if (negate || sign == '+')
					++pos;
			}

			for(var radix_in_word = (int)radix; success && pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (int)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
					success = false;
			}

			// Negate the result if anything was processed 
			if (negate)
				result = -result;

			return success;
		}
		public static bool TryParse(string s, out int result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out int result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out int result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region UInt64
		static bool TryParseImpl(string s, ref ulong result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;



			for(var radix_in_word = (ulong)radix; success && pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (ulong)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
					success = false;
			}


			return success;
		}
		public static bool TryParse(string s, out ulong result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out ulong result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out ulong result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int64
		static bool TryParseImpl(string s, ref long result, int radix, int startIndex, int length, string digits)
		{
			int pos = startIndex;
			int end = startIndex+length;
			bool success = true;

			// Skip any starting whitespace, avoids s.Trim() allocations
			while(pos < end && char.IsWhiteSpace(s[pos]))
				++pos;


			bool negate = false;
			// Sign support only exist for decimal and lower bases
			if (radix <= kBase10 && pos < end)
			{
				char sign = s[pos];

				negate = sign == '-';
				// Skip the sign character
				if (negate || sign == '+')
					++pos;
			}

			for(var radix_in_word = (long)radix; success && pos < end && !char.IsWhiteSpace(s[pos]); ++pos)
			{
				char digit = s[pos];


				int x = digits.IndexOf(digit);
				if (x >= 0 && x < radix)
				{
					result *= radix_in_word;
					result += (long)x;
				}
				else // Character wasn't found in the look-up table, it is invalid
					success = false;
			}

			// Negate the result if anything was processed 
			if (negate)
				result = -result;

			return success;
		}
		public static bool TryParse(string s, out long result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out long result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out long result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion

		#region Byte
		static bool TryParseImpl(string s, ref byte result, int radix, int startIndex, int length, string digits)
		{
			uint word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= byte.MinValue && word <= byte.MaxValue)
			{
				result = (byte)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out byte result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out byte result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out byte result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region SByte
		static bool TryParseImpl(string s, ref sbyte result, int radix, int startIndex, int length, string digits)
		{
			int word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= sbyte.MinValue && word <= sbyte.MaxValue)
			{
				result = (sbyte)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out sbyte result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out sbyte result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out sbyte result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region UInt16
		static bool TryParseImpl(string s, ref ushort result, int radix, int startIndex, int length, string digits)
		{
			uint word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= ushort.MinValue && word <= ushort.MaxValue)
			{
				result = (ushort)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out ushort result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out ushort result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out ushort result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion
		#region Int16
		static bool TryParseImpl(string s, ref short result, int radix, int startIndex, int length, string digits)
		{
			int word = 0;
			bool success = false;
			if (TryParseImpl(s, ref word, radix, startIndex, length, digits) &&
				word >= short.MinValue && word <= short.MaxValue)
			{
				result = (short)word;
				success = true;
			}

			return success;
		}
		public static bool TryParse(string s, out short result, int radix = kBase10, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(radix >= 2 && radix <= digits.Length);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, radix, startIndex, s.Length, digits);
		}
		public static bool TryParse(string s, out short result, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			result = 0;

			return  s != null && startIndex < s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, s.Length, digits);
		}
		public static bool TryParseRange(string s, out short result, int startIndex, int length, NumeralBase radix = NumeralBase.Decimal, string digits = kBase64Digits)
		{
			Contract.Requires(!string.IsNullOrEmpty(digits));
			Contract.Requires(IsValidLookupTable(radix, digits));
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			result = 0;

			return s != null && startIndex+length <= s.Length && 
				TryParseImpl(s, ref result, (int)radix, startIndex, length, digits);
		}
		#endregion

		#region ParseString Byte
		static bool ParseStringImpl(string s, ref byte value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref byte result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString SByte
		static bool ParseStringImpl(string s, ref sbyte value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref sbyte result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString UInt16
		static bool ParseStringImpl(string s, ref ushort value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref ushort result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString Int16
		static bool ParseStringImpl(string s, ref short value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref short result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString UInt32
		static bool ParseStringImpl(string s, ref uint value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref uint result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString Int32
		static bool ParseStringImpl(string s, ref int value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref int result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString UInt64
		static bool ParseStringImpl(string s, ref ulong value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref ulong result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion
		#region ParseString Int64
		static bool ParseStringImpl(string s, ref long value, bool noThrow, int radix, int startIndex,
			Func<Exception> getInnerException)
		{
			var result = string.IsNullOrEmpty(s) 
				? ParseErrorType.NoInput 
				: ParseErrorType.None;

			if (result != ParseErrorType.NoInput && (startIndex < 0 || startIndex >= s.Length))
				result = ParseErrorType.InvalidStartIndex;

			if (result == ParseErrorType.None)
				result = TryParse(s, out value, radix, startIndex, kBase64Digits) 
					? ParseErrorType.None 
					: ParseErrorType.InvalidValue;

			if (noThrow)
				return result == ParseErrorType.None;

			return HandleParseError(result, s, startIndex, getInnerException);
		}
		public static bool ParseString(string s, ref long result, bool noThrow, NumeralBase radix = NumeralBase.Decimal, int startIndex = 0,
			Func<Exception> getInnerException = null)
		{
			Contract.Requires(IsValidLookupTable(radix, kBase64Digits));

			return ParseStringImpl(s, ref result, noThrow, (int)radix, startIndex, getInnerException);
		}
		#endregion

		#region TryParse list Byte
		static readonly Lazy<byte?[]> kTryParseListByteEmptyResult = 
			new Lazy<byte?[]>(() => new byte?[0]);

		static byte? TryParseListTaskByte(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			byte result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (byte?)null;
		}
		public static IEnumerable<byte?> TryParseByte(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListByteEmptyResult.Value;

			var tasks_list = new List<Task<byte?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<byte?>.Factory.StartNew(TryParseListTaskByte,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListByteEmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list SByte
		static readonly Lazy<sbyte?[]> kTryParseListSByteEmptyResult = 
			new Lazy<sbyte?[]>(() => new sbyte?[0]);

		static sbyte? TryParseListTaskSByte(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			sbyte result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (sbyte?)null;
		}
		public static IEnumerable<sbyte?> TryParseSByte(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListSByteEmptyResult.Value;

			var tasks_list = new List<Task<sbyte?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<sbyte?>.Factory.StartNew(TryParseListTaskSByte,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListSByteEmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list UInt16
		static readonly Lazy<ushort?[]> kTryParseListUInt16EmptyResult = 
			new Lazy<ushort?[]>(() => new ushort?[0]);

		static ushort? TryParseListTaskUInt16(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			ushort result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (ushort?)null;
		}
		public static IEnumerable<ushort?> TryParseUInt16(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListUInt16EmptyResult.Value;

			var tasks_list = new List<Task<ushort?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<ushort?>.Factory.StartNew(TryParseListTaskUInt16,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListUInt16EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list Int16
		static readonly Lazy<short?[]> kTryParseListInt16EmptyResult = 
			new Lazy<short?[]>(() => new short?[0]);

		static short? TryParseListTaskInt16(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			short result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (short?)null;
		}
		public static IEnumerable<short?> TryParseInt16(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListInt16EmptyResult.Value;

			var tasks_list = new List<Task<short?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<short?>.Factory.StartNew(TryParseListTaskInt16,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListInt16EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list UInt32
		static readonly Lazy<uint?[]> kTryParseListUInt32EmptyResult = 
			new Lazy<uint?[]>(() => new uint?[0]);

		static uint? TryParseListTaskUInt32(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			uint result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (uint?)null;
		}
		public static IEnumerable<uint?> TryParseUInt32(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListUInt32EmptyResult.Value;

			var tasks_list = new List<Task<uint?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<uint?>.Factory.StartNew(TryParseListTaskUInt32,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListUInt32EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list Int32
		static readonly Lazy<int?[]> kTryParseListInt32EmptyResult = 
			new Lazy<int?[]>(() => new int?[0]);

		static int? TryParseListTaskInt32(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			int result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (int?)null;
		}
		public static IEnumerable<int?> TryParseInt32(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListInt32EmptyResult.Value;

			var tasks_list = new List<Task<int?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<int?>.Factory.StartNew(TryParseListTaskInt32,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListInt32EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list UInt64
		static readonly Lazy<ulong?[]> kTryParseListUInt64EmptyResult = 
			new Lazy<ulong?[]>(() => new ulong?[0]);

		static ulong? TryParseListTaskUInt64(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			ulong result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (ulong?)null;
		}
		public static IEnumerable<ulong?> TryParseUInt64(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListUInt64EmptyResult.Value;

			var tasks_list = new List<Task<ulong?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<ulong?>.Factory.StartNew(TryParseListTaskUInt64,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListUInt64EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
		#region TryParse list Int64
		static readonly Lazy<long?[]> kTryParseListInt64EmptyResult = 
			new Lazy<long?[]>(() => new long?[0]);

		static long? TryParseListTaskInt64(object state)
		{
			var args = (Tuple<StringListDesc, string, int, int>)state;
			var desc = args.Item1;
			long result = 0;
			bool success = TryParseImpl(args.Item2, ref result, (int)desc.Radix, args.Item3, args.Item4, desc.Digits);
			return success ? result : (long?)null;
		}
		public static IEnumerable<long?> TryParseInt64(StringListDesc desc, string values)
		{
			if(values == null)
				return kTryParseListInt64EmptyResult.Value;

			var tasks_list = new List<Task<long?>>();

			char c;
			bool found_terminator = false;
			for (int start = 0; !found_terminator && start < values.Length; )
			{
				// Skip any starting whitespace
				while (start < values.Length && char.IsWhiteSpace(values[start]))
					++start;

				int end = start;
				int length = 0;
				while (end < values.Length)
				{
					c = values[end];
					found_terminator = c == desc.Terminator;
					if (c == desc.Separator || found_terminator)
						break;

					++end;
					++length;
				}

				if (length > 0)
					tasks_list.Add(Task<long?>.Factory.StartNew(TryParseListTaskInt64,
						new Tuple<StringListDesc, string, int, int>(desc, values, start, length))
						);

				start = end + 1;
			}


			if(tasks_list.Count == 0)
				return kTryParseListInt64EmptyResult.Value;

			var tasks = tasks_list.ToArray();
			tasks_list = null;
			Task.WaitAll(tasks);

			var results =	from task in tasks
							select task.Result;

			return results;
		}
		#endregion
	};
}