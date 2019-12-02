using Diag = System.Diagnostics;

namespace KSoft.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		/// <summary>Tracer for the <see cref="KSoft"/> namespace</summary>
		public static Diag.TraceSource KSoft		{ get; } = new	Diag.TraceSource("KSoft",				Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.IO"/> namespace</summary>
		public static Diag.TraceSource IO			{ get; } = new	Diag.TraceSource("KSoft.IO",			Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.LowLevel"/> namespace</summary>
		public static Diag.TraceSource LowLevel		{ get; } = new	Diag.TraceSource("KSoft.LowLevel",		Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Reflection"/> namespace</summary>
		public static Diag.TraceSource Reflection	{ get; } = new	Diag.TraceSource("KSoft.Reflection",	Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Text"/> namespace</summary>
		public static Diag.TraceSource Text			{ get; } = new	Diag.TraceSource("KSoft.Text",			Diag.SourceLevels.All);
		/// <summary>Tracer for the <see cref="KSoft.Util"/> namespace</summary>
		public static Diag.TraceSource Util			{ get; } = new	Diag.TraceSource("KSoft.Util",			Diag.SourceLevels.All);
	};
}