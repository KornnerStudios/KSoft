
namespace KSoft.Debug
{
	// TODO: caller infomation

	/// <summary>Exception thrown in supposedly unreachable cases (ie, switch or if-else)</summary>
	public class UnreachableException : System.Exception
	{
		public UnreachableException() {}
		public UnreachableException(string msg) : base(msg) {}
		public UnreachableException(string msg, System.Exception innerException) : base(msg, innerException) {}
	};
}