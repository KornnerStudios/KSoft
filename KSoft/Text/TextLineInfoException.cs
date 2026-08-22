using System;

namespace KSoft.Text
{
	/// <summary>Exception for use as an inner exception when processing text files and there's line/column information available</summary>
	public class TextLineInfoException
		: Exception
		, ITextLineInfo
	{
		readonly string mStreamName;
		// Use TextLineInfo, instead of ITextLineInfo, as it is a value type, thus just an explicit copy of the line info
		readonly TextLineInfo mLineInfo;

		public string StreamName { get => mStreamName; }

		public TextLineInfoException(Exception? innerException, ITextLineInfo lineInfo, string? streamName = null)
			: base("Text stream error", innerException)
		{
			ArgumentNullException.ThrowIfNull(lineInfo);

			if (string.IsNullOrEmpty(streamName))
			{
				streamName = "<unknown text stream>";
			}

			mStreamName = streamName;
			mLineInfo = new TextLineInfo(lineInfo);
		}
		public TextLineInfoException(ITextLineInfo lineInfo, string? streamName = null)
			: this(null, lineInfo, streamName)
		{
		}

		public override string Message { get => string.Format(KSoft.Util.InvariantCultureInfo,
			"{0} ({1})",
			mStreamName, mLineInfo.ToString());
		}

		#region ITextLineInfo Members
		public bool HasLineInfo	{ get => mLineInfo.HasLineInfo; }
		public int LineNumber	{ get => mLineInfo.LineNumber; }
		public int LinePosition	{ get => mLineInfo.LinePosition; }
		#endregion
	};
}
