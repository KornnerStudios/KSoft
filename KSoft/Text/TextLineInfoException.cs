using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Text
{
	/// <summary>Exception for use as an inner exception when processing text files and there's line/column information available</summary>
	public class TextLineInfoException : Exception, ITextLineInfo
	{
		readonly string mStreamName;
		// Use TextLineInfo, instead of ITextLineInfo, as it is a value type, thus just an explicit copy of the line info
		readonly TextLineInfo mLineInfo;

		public string StreamName { get { return mStreamName; } }

		public TextLineInfoException(ITextLineInfo lineInfo, string streamName = null)
		{
			Contract.Requires<ArgumentNullException>(lineInfo != null);

			if(string.IsNullOrEmpty(streamName))
				streamName = "<unknown text stream>";

			mStreamName = streamName;
			mLineInfo = new TextLineInfo(lineInfo);
		}

		public override string Message { get {
			return string.Format("{0} ({1})",
				mStreamName, mLineInfo.ToString());
		} }

		#region ITextLineInfo Members
		public bool HasLineInfo	{ get { return mLineInfo.HasLineInfo; } }
		public int LineNumber	{ get { return mLineInfo.LineNumber; } }
		public int LinePosition	{ get { return mLineInfo.LinePosition; } }
		#endregion
	};
}