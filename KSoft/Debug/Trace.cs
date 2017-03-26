using System;
using Diag = System.Diagnostics;

namespace KSoft.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kKSoftSource
			, kIoSource
			, kLowLevelSource
			, kReflectionSource
			, kTextSource
			, kUtilSource
			;

		static Trace()
		{
			kKSoftSource = new			Diag.TraceSource("KSoft",				Diag.SourceLevels.All);
			kIoSource = new				Diag.TraceSource("KSoft.IO",			Diag.SourceLevels.All);
			kLowLevelSource = new		Diag.TraceSource("KSoft.LowLevel",		Diag.SourceLevels.All);
			kReflectionSource = new		Diag.TraceSource("KSoft.Reflection",	Diag.SourceLevels.All);
			kTextSource = new			Diag.TraceSource("KSoft.Text",			Diag.SourceLevels.All);
			kUtilSource = new			Diag.TraceSource("KSoft.Util",			Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="KSoft"/> namespace</summary>
		public static Diag.TraceSource KSoft		{ get { return kKSoftSource; } }
		/// <summary>Tracer for the <see cref="KSoft.IO"/> namespace</summary>
		public static Diag.TraceSource IO			{ get { return kIoSource; } }
		/// <summary>Tracer for the <see cref="KSoft.LowLevel"/> namespace</summary>
		public static Diag.TraceSource LowLevel		{ get { return kLowLevelSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Reflection"/> namespace</summary>
		public static Diag.TraceSource Reflection	{ get { return kReflectionSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Text"/> namespace</summary>
		public static Diag.TraceSource Text			{ get { return kTextSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Util"/> namespace</summary>
		public static Diag.TraceSource Util			{ get { return kUtilSource; } }
	};
}