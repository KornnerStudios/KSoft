using System;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public static class UtilT4
	{
		public static System.Globalization.CultureInfo InvariantCultureInfo { get => System.Globalization.CultureInfo.InvariantCulture; }

		public static string EnumConstraintsCode()
		{
			var sb = new System.Text.StringBuilder("struct");

			sb.AppendFormat(InvariantCultureInfo, ", {0}", typeof(IComparable).Name);
			sb.AppendFormat(InvariantCultureInfo, ", {0}", typeof(IFormattable).Name);
			sb.AppendFormat(InvariantCultureInfo, ", {0}", typeof(IConvertible).Name);

			return sb.ToString();
		}

		public static NumberCodeDefinition TryGetSignedDefinition(this NumberCodeDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			switch (def.Code)
			{
				case TypeCode.Byte:
					return PrimitiveDefinitions.kSByte;

				case TypeCode.UInt16:
					return PrimitiveDefinitions.kInt16;

				case TypeCode.UInt32:
					return PrimitiveDefinitions.kInt32;

				case TypeCode.UInt64:
					return PrimitiveDefinitions.kInt64;

				default:
					return null;
			}
		}

		internal static TextTransformationCodeBlockBookmark EnterCodeBlock(
			this TextTemplating.TextTransformation ttFile,
			TextTransformationCodeBlockType type = TextTransformationCodeBlockType.NoBrackets, int indentCount = 1)
		{
			var bookmark = new TextTransformationCodeBlockBookmark(ttFile, type, indentCount);
			bookmark.Enter();

			return bookmark;
		}

		internal static void NewLine(this TextTemplating.TextTransformation ttFile)
		{
			ttFile.WriteLine("");
		}
		internal static void EndStmt(this TextTemplating.TextTransformation ttFile)
		{
			ttFile.WriteLine(";");
		}

		static void WriteXmlDocLine(TextTemplating.TextTransformation ttFile,
			string xmlTag, string attributeText, string text)
		{
			ttFile.Write("/// ");

			ttFile.Write("<{0}", xmlTag);
			if (!string.IsNullOrEmpty(attributeText))
			{
				ttFile.Write(" ");
				ttFile.Write(attributeText);
			}
			ttFile.Write(">");

			ttFile.Write(text);

			ttFile.Write("</{0}>", xmlTag);

			ttFile.WriteLine("");
		}
		internal static void WriteXmlDocSummary(this TextTemplating.TextTransformation ttFile,
			string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "summary", null,
				string.Format(InvariantCultureInfo, format, args));
		}
		internal static void WriteXmlDocParameter(this TextTemplating.TextTransformation ttFile,
			string paramName, string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "param",
				string.Format(InvariantCultureInfo, "name=\"{0}\"", paramName),
				string.Format(InvariantCultureInfo, format, args));
		}
		internal static void WriteXmlDocReturns(this TextTemplating.TextTransformation ttFile,
			string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "returns", null,
				string.Format(InvariantCultureInfo, format, args));
		}
		internal static void WriteXmlDocRemarks(this TextTemplating.TextTransformation ttFile,
			string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "remarks", null,
				string.Format(InvariantCultureInfo, format, args));
		}

		internal static string ToValueKeyword(this bool condition)
		{
			return condition ? "true" : "false";
		}
		internal static string UseStringOrEmpty(this bool condition, string trueString)
		{
			return condition ? trueString : string.Empty;
		}
		internal static string UseStringOrEmpty(this bool condition, string trueStringFormat, params object[] args)
		{
			return condition ? string.Format(InvariantCultureInfo, trueStringFormat, args) : string.Empty;
		}

		class NullDisposableImpl
			: IDisposable
		{
			public void Dispose() { }
		};
		/// <summary>Object which can be disposed of without limit and is thread safe</summary>
		public static readonly IDisposable NullDisposable = new NullDisposableImpl();
	};
}
