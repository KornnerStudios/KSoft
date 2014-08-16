using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	internal struct TextStreamReadErrorState
	{
		readonly IKSoftStream mStream;
		Text.ITextLineInfo mReadLineInfo;

		public TextStreamReadErrorState(IKSoftStream textStream)
		{
			Contract.Requires<ArgumentNullException>(textStream != null);

			mStream = textStream;
			mReadLineInfo = null;
		}

		/// <summary>Line info of the last read that took place</summary>
		/// <remarks>Rather, about to take place. Should be set before a read with a possible error executes</remarks>
		public Text.ITextLineInfo LastReadLineInfo
		{
			get { return mReadLineInfo; }
			set { mReadLineInfo = value; }
		}

		const string kReadLineInfoIsNullMsg = 
			"A Text stream reader implementation failed to set the LastReadLineInfo before a read took place. " +
			"Guess what? Said read just failed";

		public Exception GetLineInfoException()
		{
			Contract.Assert(mReadLineInfo != null, kReadLineInfoIsNullMsg);

			return new Text.TextLineInfoException(mReadLineInfo, mStream.StreamName);
		}

		/// <summary>Throws a <see cref="Text.TextLineInfoException"/> using <see cref="LastReadLineInfo"/></summary>
		/// <param name="detailsException">The details (inner) exception of what went wrong</param>
		public void ThrowReadExeception(Exception detailsException)
		{
			Contract.Assert(mReadLineInfo != null, kReadLineInfoIsNullMsg);

			throw new Text.TextLineInfoException(detailsException, mReadLineInfo, mStream.StreamName);
		}
	};
}