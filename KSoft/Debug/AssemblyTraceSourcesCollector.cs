using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Debug
{
	public static class AssemblyTraceSourcesCollector
	{
		public static List<TraceSource> FromClass(Type debugTraceClass, List<TraceSource> sources = null)
		{
			Contract.Requires(debugTraceClass != null);

			if (sources == null)
				sources = new List<TraceSource>();

			var properties = debugTraceClass.GetProperties(BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public);

			foreach (var prop in properties)
			{
				if (prop.PropertyType != typeof(TraceSource))
					continue;

				var trace_source = (TraceSource)prop.GetValue(null);
				Contract.Assert(trace_source != null, prop.Name);

				sources.Add(trace_source);
			}

			return sources;
		}

		public static List<TraceSource> FromClasses(List<TraceSource> sources, params Type[] debugTraceClasses)
		{
			Contract.Requires(debugTraceClasses != null);

			if (sources == null)
				sources = new List<TraceSource>();

			foreach (var debugTraceClass in debugTraceClasses)
			{
				FromClass(debugTraceClass, sources);
			}

			return sources;
		}

		public static int CompareTraceSourcesByName(TraceSource x, TraceSource y)
		{
			return string.CompareOrdinal(x.Name, y.Name);
		}
	};
}
