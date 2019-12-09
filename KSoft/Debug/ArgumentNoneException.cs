
namespace KSoft
{
	/// <summary>Exception thrown when a handle value is NONE where it is not a supported case</summary>
	public class ArgumentNoneException : System.ArgumentException
	{
		public ArgumentNoneException() : base()
		{
		}

		public ArgumentNoneException(string paramName) : base("Parameter cannot be NONE", paramName) {}
		public ArgumentNoneException(string paramName, System.Exception innerException) : base("Parameter cannot be NONE", paramName, innerException) {}

		public ArgumentNoneException(string msg, string paramName) : base(msg, paramName) {}
		public ArgumentNoneException(string msg, string paramName, System.Exception innerException) : base(msg, paramName, innerException) {}
	};
}