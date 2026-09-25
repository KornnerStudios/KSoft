using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

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

		static readonly object gInitializationLock = new();
		static readonly Debug.TraceSourceRegistry gTraceSources = new();
		static bool gInitialized;
		static Debug.TraceSourceLoggerProvider? gLoggerProvider;

		public static void Initialize() => Initialize(System.Diagnostics.TraceConfiguration.Register);

		internal static void Initialize(Action registerTraceConfiguration)
		{
			ArgumentNullException.ThrowIfNull(registerTraceConfiguration);

			lock (gInitializationLock)
			{
				if (gInitialized)
					return;

				registerTraceConfiguration();
				gTraceSources.Register(
					Debug.AssemblyTraceSourcesCollector.FromClass(DebugTraceClass));
				gLoggerProvider = new Debug.TraceSourceLoggerProvider(gTraceSources);
				gInitialized = true;
			}
		}

		public static void Dispose()
		{
			lock (gInitializationLock)
			{
				if (!gInitialized)
					return;

				try
				{
					gLoggerProvider!.Dispose();
					System.Diagnostics.Trace.Flush();
				}
				finally
				{
					gLoggerProvider = null;
					gInitialized = false;
				}
			}
		}

		/// <summary>Creates a logger whose category resolves to a registered canonical trace source when used.</summary>
		/// <remarks>
		/// Categories resolve by exact trace-source name and then by dot-delimited parent.
		/// This intentionally lets source-less KSoft assemblies inherit the registered <c>KSoft</c> source.
		/// </remarks>
		public static ILogger CreateLogger(string categoryName)
		{
			lock (gInitializationLock)
				return GetLoggerProvider().CreateLogger(categoryName);
		}

		/// <summary>Creates a logger using the full name of <typeparamref name="T"/> as its category.</summary>
		public static ILogger<T> CreateLogger<T>()
		{
			lock (gInitializationLock)
				return GetLoggerProvider().CreateLogger<T>();
		}

		/// <summary>Registers trace sources exposed by the public static properties of trace holder classes.</summary>
		public static void RegisterTraceSources(params Type[] debugTraceClasses)
		{
			ArgumentNullException.ThrowIfNull(debugTraceClasses);
			RegisterTraceSources(Debug.AssemblyTraceSourcesCollector.FromClasses(null, debugTraceClasses));
		}

		/// <summary>Registers canonical trace source instances.</summary>
		public static void RegisterTraceSources(params TraceSource[] traceSources)
			=> RegisterTraceSources((IEnumerable<TraceSource>)traceSources);

		/// <summary>Registers canonical trace source instances.</summary>
		public static void RegisterTraceSources(IEnumerable<TraceSource> traceSources)
		{
			ArgumentNullException.ThrowIfNull(traceSources);

			lock (gInitializationLock)
			{
				if (!gInitialized)
					throw new InvalidOperationException("KSoft.Program must be initialized before registering trace sources.");

				gTraceSources.Register(traceSources);
			}
		}

		private static Debug.TraceSourceLoggerProvider GetLoggerProvider()
			=> gLoggerProvider ?? throw new InvalidOperationException("KSoft.Program is not initialized.");

		public static Type DebugTraceClass { get { return typeof(Debug.Trace); } }
	};
}