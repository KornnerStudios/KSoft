using System;
using System.Collections.Generic;
using System.Globalization;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	// Getting matching results with radix <= 63:
	// http://www.pgregg.com/projects/php/base_conversion/base_conversion.php

	// http://bitplane.net/2010/08/java-float-fast-parser/
	// http://tinodidriksen.com/2011/05/28/cpp-convert-string-to-double-speed/

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

		[Contracts.Pure]
		static bool HandleParseError(ParseErrorType errorType, bool noThrow, string s, int startIndex
			, Text.IHandleTextParseError handler = null)
		{
			Exception detailsException = null;

			switch (errorType)
			{
			case ParseErrorType.NoInput:
				if (noThrow)
					return false;

				detailsException = new ArgumentException
					("Input null or empty", "s");
				break;

			case ParseErrorType.InvalidValue:
				detailsException = new ArgumentException(string.Format
					("Couldn't parse '{0}'", s), "s");
				break;

			case ParseErrorType.InvalidStartIndex:
				detailsException = new ArgumentOutOfRangeException(string.Format
					("'{0}' is out of range of the input length of '{1}'", startIndex, s.Length));
				break;

			default:
				return true;
			}

			if (handler == null)
				handler = Text.Util.DefaultTextParseErrorHandler;

			if (noThrow == false)
				handler.ThrowReadExeception(detailsException);

			handler.LogReadExceptionWarning(detailsException);
			return true;
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
			public static StringListDesc Default { get {
				return new StringListDesc(kDefaultSeparator);
			} }

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

			[Contracts.Pure]
			internal int PredictedCount(string values)
			{
				Contract.Assume(values != null);

				int count = 1;

				// using StringSegment and its Enumerator won't allocate any reference types
				var sseg = new Collections.StringSegment(values);
				foreach(char c in sseg)
				{
					if (c == Separator)
						count++;
					else if (c == Terminator)
						break;
				}

				return count;
			}
		};

		// TODO: IsWhiteSpace can be rather expensive, and it is used in TryParseImpl. Perhaps we can make a variant
		// that can safely assume all characters are non-ws, and have TryParseList impls call it instead?
		// The TryParse() below would need to be updated to catch trailing ws

		// TODO: add an option to just flat out skip unsuccessful items?

		abstract class TryParseNumberListBase<T, TListItem>
			where T : struct
		{
			protected readonly StringListDesc mDesc;
			protected readonly string mValues;
			protected List<TListItem> mList;

			protected TryParseNumberListBase(StringListDesc desc, string values)
			{
				mDesc = desc;
				mValues = values;
			}

			protected abstract IEnumerable<T?> EmptyResult { get; }

			void InitializeList()
			{
				// ReSharper disable once ImpureMethodCallOnReadonlyValueField - yes IT IS fucking Pure you POS
				int predicated_count = mDesc.PredictedCount(mValues);
				mList = new List<TListItem>(predicated_count);
			}

			protected abstract TListItem CreateItem(int start, int length);

			protected abstract IEnumerable<T?> CreateResult();

			public IEnumerable<T?> TryParse()
			{
				if (mValues == null)
					return EmptyResult;

				InitializeList();

				bool found_terminator = false;
				int value_length = mValues.Length;
				for (int start = 0; !found_terminator && start < value_length; )
				{
					// Skip any starting whitespace
					while (start < value_length && char.IsWhiteSpace(mValues[start]))
						++start;

					int end = start;
					int length = 0;
					while (end < value_length)
					{
						char c = mValues[end];
						found_terminator = c == mDesc.Terminator;
						// NOTE: TryParseImpl actually handles leading and trailing whitespace
						if (c == mDesc.Separator || found_terminator)
							break;

						// NOTE: we wouldn't want to update length if we hit ws before the separator and the TryParseImpl assumes no ws
						++length;
						++end;
					}

					if (length > 0)
						mList.Add(CreateItem(start, length));

					start = end + 1;
				}

				// TODO: should we add support for throwing an exception or such when a terminator isn't encountered?

				return mList.Count == 0
					? EmptyResult
					: CreateResult();
			}
		};

		// based on the reference source, this is what the default number styles are
		public const NumberStyles kFloatTryParseNumberStyles = 0
			| NumberStyles.Float
			| NumberStyles.AllowThousands;
		public static bool FloatTryParseInvariant(string s, out float result)
		{
			return float.TryParse(s, kFloatTryParseNumberStyles, CultureInfo.InvariantCulture, out result);
		}

		// based on the reference source, this is what the default number styles are
		public const NumberStyles kDoubleTryParseNumberStyles = kFloatTryParseNumberStyles;
		public static bool DoubleTryParseInvariant(string s, out double result)
		{
			return double.TryParse(s, kFloatTryParseNumberStyles, CultureInfo.InvariantCulture, out result);
		}
	};
};