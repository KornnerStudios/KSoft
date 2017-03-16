﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class TagElementTextStreamUtils
	{
		enum ParseErrorType
		{
			None,
			/// <summary>Input string is null or empty</summary>
			NoInput,
			/// <summary>The input can't be parsed as-is</summary>
			InvalidValue,
		};
		static ParseErrorType ParseVerifyInput(string input)
		{
			return string.IsNullOrEmpty(input)
				? ParseErrorType.NoInput
				: ParseErrorType.None;
		}
		static ParseErrorType ParseVerifyResult(ParseErrorType result, bool parseResult)
		{
			return parseResult
				? ParseErrorType.None
				: ParseErrorType.InvalidValue;
		}

		/// <summary></summary>
		/// <param name="type"></param>
		/// <param name="noThrow">Does the caller want exceptions to be thrown on errors?</param>
		/// <param name="input"></param>
		/// <param name="errorState"></param>
		/// <returns>True if no error handling was needed. Else, an exception is throw (if allowed)</returns>
		static bool ParseHandleError(ParseErrorType type, bool noThrow, string input
			, TextStreamReadErrorState errorState)
		{
			Exception detailsException = null;
			switch (type)
			{
			case ParseErrorType.NoInput:
				if (noThrow)
					return false;

				detailsException = new ArgumentException
					("Input null or empty", "input");
				break;

			case ParseErrorType.InvalidValue:
				detailsException = new ArgumentException(string.Format
					("Couldn't parse \"{0}\"", input), "input");
				break;

			default:
				return true;
			}

			if (noThrow == false)
				errorState.ThrowReadExeception(detailsException);

			errorState.LogReadExceptionWarning(detailsException);
			return true;
		}

		public static bool ParseString(string input, ref char value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				value = input[0];

			return ParseHandleError(result, noThrow, input, errorState);
		}
		public static bool ParseString(string input, ref bool value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				value = Text.Util.ParseBooleanLazy(input);

			return ParseHandleError(result, noThrow, input, errorState);
		}

		#region Real
		public static bool ParseString(string input, ref float value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
			{
				// #HACK Fucking HaloWars data has floats with C-based 'f' suffix
				char last_char = input[input.Length - 1];
				if (last_char == 'f' || last_char == 'F')
					input = input.Substring(0, input.Length - 1);

				result = ParseVerifyResult(result, float.TryParse(input, out value));
			}

			return ParseHandleError(result, noThrow, input, errorState);
		}
		public static bool ParseString(string input, ref double value, bool noThrow
			, TextStreamReadErrorState errorState)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				result = ParseVerifyResult(result, double.TryParse(input, out value));

			return ParseHandleError(result, noThrow, input, errorState);
		}
		#endregion
	};

	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region Parse Util
		TextStreamReadErrorState mReadErrorState;

		/// <summary>Sets the node that the current read operation is querying</summary>
		/// <remarks>It should be assumed that this isn't reset after the current read successfully finishes</remarks>
		protected object ReadErrorNode { set {
			// and in fact, this property can't be used to set mReadErrorNode to null with this assert.
			// TextStream implementations should only ever need to set the node and forget
			Contract.Assert(value is Text.ITextLineInfo);

			mReadErrorState.LastReadLineInfo = (Text.ITextLineInfo)value;
		} }
		/// <summary>Throws a <see cref="Text.TextLineInfoException"/></summary>
		/// <param name="detailsException">The details (inner) exception of what went wrong</param>
		public sealed override void ThrowReadException(Exception detailsException)
		{
			mReadErrorState.ThrowReadExeception(detailsException);
		}
		public Text.TextLineInfo TryGetLastReadLineInfo()
		{
			return IsReading && mReadErrorState.LastReadLineInfo != null
				? new Text.TextLineInfo(mReadErrorState.LastReadLineInfo)
				: Text.TextLineInfo.Empty;
		}

		/// <summary>Argument value for noThrow to throw exceptions</summary>
		const bool kThrowExcept = false;
		/// <summary>Argument value for noThrow to not throw exceptions</summary>
		const bool kNoExcept = true;

		protected bool ReadEnumInternal<TEnum>(string enumString, ref TEnum enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var result = TagElementStreamParseEnumUtil.Parse(IgnoreCaseOnEnums,
				enumString, ref enumValue);

			if (result == TagElementStreamParseEnumResult.FailedMemberNotFound &&
				ExceptionOnEnumParseFail)
			{
				ThrowReadException(new System.IO.InvalidDataException(string.Format(
					"'{0}' is not a member of {1}",
					enumString, typeof(TEnum) )));
			}

			return result == TagElementStreamParseEnumResult.Success;
		}
		protected bool ReadEnumInternal<TEnum>(string enumString, ref int enumValue)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var result = TagElementStreamParseEnumUtil.Parse<TEnum>(IgnoreCaseOnEnums,
				enumString, ref enumValue);

			if (result == TagElementStreamParseEnumResult.FailedMemberNotFound &&
				ExceptionOnEnumParseFail)
			{
				ThrowReadException(new System.IO.InvalidDataException(string.Format(
					"'{0}' is not a member of {1}",
					enumString, typeof(TEnum))));
			}

			return result == TagElementStreamParseEnumResult.Success;
		}
		#endregion

		#region ReadElement impl
		protected abstract string GetInnerText(TCursor n);

		protected override void ReadElementEnum<TEnum>(TCursor n, ref TEnum enumValue)
		{
			ReadEnumInternal(GetInnerText(n), ref enumValue);
		}
		protected override void ReadElementEnum<TEnum>(TCursor n, ref int enumValue)
		{
			ReadEnumInternal<TEnum>(GetInnerText(n), ref enumValue);
		}

		protected override void ReadElement(TCursor n, ref Values.KGuid value)
		{
			value = Values.KGuid.ParseExact(GetInnerText(n), mGuidFormatString);
		}
		#endregion

		/// <summary>Interpret the Name of <see cref="Cursor"/> as a member of <typeparamref name="TEnum"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public override void ReadCursorName<TEnum>(ref TEnum enumValue)
		{
			ReadEnumInternal(CursorName, ref enumValue);
		}

		#region ReadAttribute
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected abstract string ReadAttribute(string name);

		public override void ReadAttributeEnum<TEnum>(string name, ref TEnum enumValue)
		{
			ReadEnumInternal(ReadAttribute(name), ref enumValue);
		}
		public override void ReadAttributeEnum<TEnum>(string name, ref int enumValue)
		{
			ReadEnumInternal<TEnum>(ReadAttribute(name), ref enumValue);
		}

		public override void ReadAttribute(string name, ref Values.KGuid value)
		{
			value = Values.KGuid.ParseExact(ReadAttribute(name), mGuidFormatString);
		}
		#endregion

		#region ReadElementOpt
		/// <summary>Streams out the InnerText of element <paramref name="name"/></summary>
		/// <param name="name">Element name</param>
		/// <returns></returns>
		protected abstract string ReadElementOpt(string name);

		public override bool ReadElementEnumOpt<TEnum>(string name, ref TEnum enumValue)
		{
			string str = ReadElementOpt(name);
			return !string.IsNullOrEmpty(str)
				&& ReadEnumInternal(str, ref enumValue);
		}
		public override bool ReadElementEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = ReadElementOpt(name);
			return !string.IsNullOrEmpty(str)
				&& ReadEnumInternal<TEnum>(str, ref enumValue);
		}

		public override bool ReadElementOpt(string name, ref Values.KGuid value)
		{
			string str = ReadElementOpt(name);
			return !string.IsNullOrEmpty(str) &&
				Values.KGuid.TryParseExactHyphenated(str, out value);
		}
		#endregion

		#region ReadAttributeOpt
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected abstract string ReadAttributeOpt(string name);

		public override bool ReadAttributeEnumOpt<TEnum>(string name, ref TEnum enumValue)
		{
			string str = ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str)
				&& ReadEnumInternal(str, ref enumValue);
		}
		public override bool ReadAttributeEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str)
				&& ReadEnumInternal<TEnum>(str, ref enumValue);
		}

		public override bool ReadAttributeOpt(string name, ref Values.KGuid value)
		{
			string str = ReadAttributeOpt(name);
			return !string.IsNullOrEmpty(str) &&
				Values.KGuid.TryParseExactHyphenated(str, out value);
		}
		#endregion
	};
}