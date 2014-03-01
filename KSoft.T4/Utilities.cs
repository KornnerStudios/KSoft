using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public static class UtilT4
	{
		public static string EnumConstraintsCode()
		{
			var sb = new System.Text.StringBuilder("struct");

			sb.AppendFormat(", {0}", typeof(IComparable).Name);
			sb.AppendFormat(", {0}", typeof(IFormattable).Name);
			sb.AppendFormat(", {0}", typeof(IConvertible).Name);

			return sb.ToString();
		}

		internal static TextTransformationCodeBlockBookmark EnterCodeBlock(
			this TextTemplating.TextTransformation ttFile,
			TextTransformationCodeBlockType type = TextTransformationCodeBlockType.NoBrackets)
		{
			var bookmark = new TextTransformationCodeBlockBookmark(ttFile, type);
			bookmark.Enter();

			return bookmark;
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
				string.Format(format, args));
		}
		internal static void WriteXmlDocParameter(this TextTemplating.TextTransformation ttFile,
			string paramName, string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "param",
				string.Format("name=\"{0}\"", paramName),
				string.Format(format, args));
		}
		internal static void WriteXmlDocReturns(this TextTemplating.TextTransformation ttFile,
			string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "returns", null,
				string.Format(format, args));
		}
		internal static void WriteXmlDocRemarks(this TextTemplating.TextTransformation ttFile,
			string format, params object[] args)
		{
			WriteXmlDocLine(ttFile, "remarks", null,
				string.Format(format, args));
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
			return condition ? string.Format(trueStringFormat, args) : string.Empty;
		}
	};
}