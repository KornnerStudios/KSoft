using System;
using System.Threading;

namespace KSoft
{
	internal static class KSoftConstants
	{
		/// <summary>Applied to enumeration members which act as padding. E.g., for enums which are streamed or bit-encoded and max be extended in the future so they have reserved members</summary>
		public const string kReservedMsg = "Reserved member. Don't use.";
		/// <summary>Applied to enumeration members which aren't currently supported yet in production code</summary>
		public const string kUnsupportedMsg = "Currently unsupported. Don't use.";
	};

	public static class Program
	{
		// Since static ctors in structs are pretty fucked (http://stackoverflow.com/a/3246817/444977)
		// we instead opt for explicit startup/shutdown

		private static int gInitialized;

		public static void Initialize()
		{
			if (Interlocked.CompareExchange(ref gInitialized, 1, 0) != 0)
				return;

			try
			{
				System.Diagnostics.TraceConfiguration.Register();
			}
			catch
			{
				Volatile.Write(ref gInitialized, 0);
				throw;
			}
		}

		public static void Dispose()
		{
			if (Interlocked.Exchange(ref gInitialized, 0) == 0)
				return;

			System.Diagnostics.Trace.Flush();
		}

		public static Type DebugTraceClass { get { return typeof(Debug.Trace); } }
	};
}