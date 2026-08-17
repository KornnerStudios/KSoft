using System;

namespace KSoft.Text
{
	public interface ITextLineInfo
	{
		bool HasLineInfo { get; }

		int LineNumber { get; }
		int LinePosition { get; }
	};

	public readonly struct TextLineInfo
		: ITextLineInfo
		, IComparable<ITextLineInfo>
		, IEquatable<ITextLineInfo>
	{
		public static readonly TextLineInfo Empty = new();

		readonly int mLineNumber, mLinePosition;

		public bool HasLineInfo		=> mLineNumber > 0;

		public int LineNumber		=> mLineNumber;
		public int LinePosition		=> mLinePosition;

		public TextLineInfo(int lineNumber, int linePosition)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineNumber);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(linePosition);

			mLineNumber = lineNumber;
			mLinePosition = linePosition;
		}
		public TextLineInfo(ITextLineInfo otherLineInfo)
			: this(KSoft.Util.ThrowIfNull(otherLineInfo).LineNumber, otherLineInfo.LinePosition)
		{
		}

		public bool IsEmpty => LineNumber == 0 && LinePosition == 0;

		public int CompareTo(ITextLineInfo other)
		{
			if (LineNumber == other.LineNumber)
			{
				return LinePosition - other.LinePosition;
			}
			else
			{
				return LineNumber - other.LineNumber;
			}
		}

		public bool Equals(ITextLineInfo other) =>
			LineNumber == other.LineNumber &&
			LinePosition == other.LinePosition;

		public override bool Equals(object obj) =>
			obj is ITextLineInfo other && Equals(other);

		public override int GetHashCode() =>
			LineNumber.GetHashCode() ^ LinePosition.GetHashCode();

		#region ToString
		/// <summary>Returns a verbose string of the line/column values</summary>
		/// <returns></returns>
		public override string ToString() => ToString(this, true);

		const string kNoLineInfoString = "<no line info>";

		public static string ToStringLineOnly<T>(T lineInfo, bool verboseString)
			where T : Text.ITextLineInfo
		{
			const string k_format_string =
				"{0}";
			const string k_format_string_verbose =
				"Ln {0}";

			if (!lineInfo.HasLineInfo)
			{
				return kNoLineInfoString;
			}

			return string.Format(KSoft.Util.InvariantCultureInfo,
				verboseString ? k_format_string_verbose : k_format_string,
				lineInfo.LineNumber.ToString(KSoft.Util.InvariantCultureInfo));
		}
		public static string ToString<T>(T lineInfo, bool verboseString)
			where T : Text.ITextLineInfo
		{
			const string k_format_string =
				"{0}, {1}";
			const string k_format_string_verbose =
				"Ln {0}, Col {1}";

			if (!lineInfo.HasLineInfo)
			{
				return kNoLineInfoString;
			}

			return string.Format(KSoft.Util.InvariantCultureInfo,
				verboseString ? k_format_string_verbose : k_format_string,
				lineInfo.LineNumber.ToString(KSoft.Util.InvariantCultureInfo),
				lineInfo.LinePosition.ToString(KSoft.Util.InvariantCultureInfo));
		}
		#endregion

		#region Operators
		public static bool operator ==(TextLineInfo a, TextLineInfo b) => a.Equals(b);
		public static bool operator !=(TextLineInfo a, TextLineInfo b) => !(a == b);
		#endregion
	};
}
