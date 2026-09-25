using Diag = System.Diagnostics;

namespace KSoft.Security.Debug;

internal static class Trace
{
	public static Diag.TraceSource Security { get; } = new("KSoft.Security", Diag.SourceLevels.All);
}
