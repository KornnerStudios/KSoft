using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	// Getting matching results with radix <= 63:
	// http://www.pgregg.com/projects/php/base_conversion/base_conversion.php

	public static partial class Numbers
	{
		enum ParseErrorType
		{
			None,
			/// <summary>Input string is null or empty</summary>
			NoInput,
			/// <summary>The input can't be parsed as-is</summary>
			InvalidValue,
			InvalidStartIndex,
		};

		public const int kBase10 = 10;
		public const int kBase36 = 36;
		public const int kBase64 = 64;
		public const string kBase64Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/";
		public const string kBase64DigitsRfc4648 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		static bool HandleParseError(ParseErrorType errorType, string s, int startIndex)
		{
			switch (errorType)
			{
				case ParseErrorType.NoInput: throw new ArgumentException("Input null or empty", "s");
				case ParseErrorType.InvalidValue: throw new ArgumentException(string.Format("Couldn't parse '{0}'", s), "s");
				case ParseErrorType.InvalidStartIndex: throw new ArgumentOutOfRangeException(string.Format(
					"'{0}' is out of range of the input length of '{1}'", startIndex, s.Length), "startIndex");
				default: return true;
			}
		}

		[Contracts.Pure]
		public static bool IsValidLookupTable(NumeralBase radix, string digits)
		{
			return radix >= NumeralBase.Binary && (int)radix <= digits.Length;
		}
		[Contracts.Pure]
		public static bool IsValidLookupTable(NumbersRadix radix, string digits)
		{
			return radix >= NumbersRadix.Binary && (int)radix <= digits.Length;
		}


		public struct StringListDesc
		{
			public const char kDefaultSeparator = ',';
			public const char kDefaultTerminator = ';';
			public static readonly StringListDesc kDefault = new StringListDesc(kDefaultSeparator);

			public string Digits;
			public NumbersRadix Radix;
			/// <remarks><b>false</b> by default</remarks>
			public bool RequiresTerminator;

			public char Separator;
			public char Terminator;

			public StringListDesc(char separator, char terminator = kDefaultTerminator,
				NumbersRadix radix = NumbersRadix.Decimal, string digits = kBase64Digits)
			{
				Contract.Requires(!string.IsNullOrEmpty(digits));
				Contract.Requires(IsValidLookupTable(radix, digits));

				Digits = digits;
				Radix = radix;
				RequiresTerminator = false;

				Separator = separator;
				Terminator = terminator;
			}
		};
	};
};