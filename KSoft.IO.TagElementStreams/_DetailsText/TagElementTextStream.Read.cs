using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region Parse Util
		/// <summary>Argument value for noThrow to throw exceptions</summary>
		const bool kThrowExcept = false;
		/// <summary>Argument value for noThrow to not throw exceptions</summary>
		const bool kNoExcept = true;

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
			return string.IsNullOrEmpty(input) ? ParseErrorType.NoInput : ParseErrorType.None;
		}
		static ParseErrorType ParseVerifyResult(ParseErrorType result, bool parseResult)
		{
			return parseResult ? ParseErrorType.None : ParseErrorType.InvalidValue;
		}
		/// <summary></summary>
		/// <param name="type"></param>
		/// <param name="noThrow">Does the caller want exceptions to be thrown on errors?</param>
		/// <param name="input"></param>
		/// <returns>True if no error handling was needed. Else, an exception is throw (if allowed)</returns>
		static bool ParseHandleError(ParseErrorType type, bool noThrow, string input)
		{
			// If no exceptions are wanted, return whether parsing ran without error
			if (noThrow)
				return type == ParseErrorType.None;

			switch (type)
			{
				case ParseErrorType.NoInput:
					throw new ArgumentException("Input null or empty", "input");
				case ParseErrorType.InvalidValue:
					throw new ArgumentException(string.Format("Couldn't parse \"{0}\"", input), "input");

				default: return true;
			}
		}

		static bool ParseString(string input, ref char value, bool noThrow)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				value = input[0];

			return ParseHandleError(result, noThrow, input);
		}
		static bool ParseString(string input, ref bool value, bool noThrow)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				value = Text.Util.ParseBooleanLazy(input);

			return ParseHandleError(result, noThrow, input);
		}

		#region Real
		static bool ParseString(string input, ref float value, bool noThrow)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				result = ParseVerifyResult(result, float.TryParse(input, out value));

			return ParseHandleError(result, noThrow, input);
		}
		static bool ParseString(string input, ref double value, bool noThrow)
		{
			var result = ParseVerifyInput(input);
			if (result == ParseErrorType.None)
				result = ParseVerifyResult(result, double.TryParse(input, out value));

			return ParseHandleError(result, noThrow, input);
		}
		#endregion
		#endregion

		#region ReadElement impl
		protected abstract string GetInnerText(TCursor n);

		protected override void ReadElementEnum<TEnum>(TCursor n, ref TEnum enumValue)
		{
			ParseEnum<TEnum>(GetInnerText(n), out enumValue);
		}
		protected override void ReadElementEnum<TEnum>(TCursor n, ref int enumValue)
		{
			ParseEnum<TEnum>(GetInnerText(n), out enumValue);
		}

		protected override void ReadElement(TCursor n, ref Values.KGuid value)
		{
			value = Values.KGuid.ParseExactNoStyle(GetInnerText(n));
		}
		#endregion

		/// <summary>Interpret the Name of <see cref="Cursor"/> as a member of <typeparamref name="TEnum"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="enumValue">value to receive the data</param>
		public override void ReadCursorName<TEnum>(ref TEnum enumValue)
		{
			ParseEnum<TEnum>(CursorName, out enumValue);
		}

		#region ReadAttribute
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected abstract string ReadAttribute(string name);

		public override void ReadAttributeEnum<TEnum>(string name, ref TEnum enumValue)
		{
			ParseEnum(ReadAttribute(name), out enumValue);
		}
		public override void ReadAttributeEnum<TEnum>(string name, ref int enumValue)
		{
			ParseEnum(ReadAttribute(name), out enumValue);
		}

		public override void ReadAttribute(string name, ref Values.KGuid value)
		{
			value = Values.KGuid.ParseExactNoStyle(ReadAttribute(name));
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
			if (string.IsNullOrEmpty(str))
				return false;

			return ParseEnumOpt(str, out enumValue);
		}
		public override bool ReadElementEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = ReadElementOpt(name);
			if (string.IsNullOrEmpty(str))
				return false;

			return ParseEnumOpt(str, out enumValue);
		}

		public override bool ReadElementOpt(string name, ref Values.KGuid value)
		{
			string str = ReadElementOpt(name);
			if (string.IsNullOrEmpty(str))
				return false;

			return Values.KGuid.TryParseExactNoStyle(str, out value);
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
			if (string.IsNullOrEmpty(str))
				return false;

			return ParseEnumOpt(str, out enumValue);
		}
		public override bool ReadAttributeEnumOpt<TEnum>(string name, ref int enumValue)
		{
			string str = ReadAttributeOpt(name);
			if (string.IsNullOrEmpty(str))
				return false;

			return ParseEnumOpt(str, out enumValue);
		}

		public override bool ReadAttributeOpt(string name, ref Values.KGuid value)
		{
			string str = ReadAttributeOpt(name);
			if (string.IsNullOrEmpty(str))
				return false;

			return Values.KGuid.TryParseExactNoStyle(str, out value);
		}
		#endregion
	};
}