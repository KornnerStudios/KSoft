
namespace KSoft.Debug
{
	/// <summary>Exception thrown in supposedly unreachable cases (ie, switch or if-else)</summary>
	public class UnreachableException : System.Exception
	{
		public UnreachableException(
#if DEBUG
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNum = -1
#endif
			) : base(
#if DEBUG
				string.Format("Unreachable: {0}({1}", sourceFile, sourceLineNum)
#endif
				)
		{
		}

		public UnreachableException(string msg) : base(msg) {}
		public UnreachableException(string msg, System.Exception innerException) : base(msg, innerException) {}
	};
}