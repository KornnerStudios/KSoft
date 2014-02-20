using System;
using System.Collections.Generic;

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
			this Microsoft.VisualStudio.TextTemplating.TextTransformation ttFile,
			TextTransformationCodeBlockType type = TextTransformationCodeBlockType.NoBrackets)
		{
			var bookmark = new TextTransformationCodeBlockBookmark(ttFile, type);
			bookmark.Enter();

			return bookmark;
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