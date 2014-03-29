using System.IO;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public partial class VersionMismatchException : System.Exception
	{
		const string kFormat = "Invalid version! @{0} Expected '{1}', got '{2}' ({3} data)";
		const string kDescFormat = "Invalid '{0}' version! Expected '{1}', got '{2}' ({3} data)";

		static string VersionCompareDesc<T>(T expected, T found)
			where T : struct, System.IComparable<T>
		{
			if (found.CompareTo(expected) > 0)
				return "newer";

			return "older";
		}

		VersionMismatchException(long pos, string cmp, string expected, string found) :
			base(string.Format(kFormat, pos.ToString("X8"), expected, found, cmp))
		{
		}
	};
}