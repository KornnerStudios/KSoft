using System;

namespace KSoft.Text
{
	public interface IHandleTextParseError
	{
		void ThrowReadExeception(Exception detailsException);

		void LogReadExceptionWarning(Exception detailsException);
	};

	internal sealed class DefaultTextParseErrorHandler
		: IHandleTextParseError
	{
		public void ThrowReadExeception(Exception detailsException)
		{
			throw detailsException;
		}

		public void LogReadExceptionWarning(Exception detailsException)
		{
#if false
			Debug.Trace.Text.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Failed to parse value: {0}",
				detailsException);
#endif
		}
	};
};