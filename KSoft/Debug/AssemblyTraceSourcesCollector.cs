using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#nullable enable

namespace KSoft.Debug
{
	public static class AssemblyTraceSourcesCollector
	{
		public static List<TraceSource> FromClass(Type debugTraceClass, List<TraceSource>? sources = null)
		{
			ArgumentNullException.ThrowIfNull(debugTraceClass);

			if (sources == null)
			{
				sources = [];
			}

			var properties = debugTraceClass.GetProperties(BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public);

			foreach (var prop in properties)
			{
				if (prop.PropertyType != typeof(TraceSource))
				{
					continue;
				}

				TraceSource? trace_source = (TraceSource?)prop.GetValue(null);
				if (trace_source == null)
				{
					throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
						"TraceSource property '{0}' returned null.",
						prop.Name));
				}

				sources.Add(trace_source);
			}

			return sources;
		}

		public static List<TraceSource> FromClasses(List<TraceSource>? sources, params Type[] debugTraceClasses)
		{
			ArgumentNullException.ThrowIfNull(debugTraceClasses);

			if (sources == null)
			{
				sources = [];
			}

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
