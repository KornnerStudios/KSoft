using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KSoft.Debug;

internal sealed class TraceSourceRegistry
{
	private readonly object mLock = new();
	private readonly Dictionary<string, TraceSource> mSources = new(StringComparer.Ordinal);

	public void Register(params TraceSource[] traceSources)
		=> Register((IEnumerable<TraceSource>)traceSources);

	public void Register(IEnumerable<TraceSource> traceSources)
	{
		ArgumentNullException.ThrowIfNull(traceSources);

		TraceSource[] sources = traceSources.ToArray();
		if (sources.Any(source => source == null))
			throw new ArgumentException("Trace source collections cannot contain null entries.", nameof(traceSources));

		lock (mLock)
		{
			var batchSources = new Dictionary<string, TraceSource>(StringComparer.Ordinal);
			foreach (TraceSource source in sources)
			{
				if (mSources.TryGetValue(source.Name, out TraceSource? registeredSource)
					&& !ReferenceEquals(registeredSource, source))
				{
					throw new InvalidOperationException(
						$"Trace source '{source.Name}' is already registered by a different instance.");
				}

				if (batchSources.TryGetValue(source.Name, out TraceSource? batchSource)
					&& !ReferenceEquals(batchSource, source))
				{
					throw new InvalidOperationException(
						$"Trace source batch contains different instances named '{source.Name}'.");
				}

				batchSources[source.Name] = source;
			}

			foreach ((string name, TraceSource source) in batchSources)
				mSources[name] = source;
		}
	}

	public TraceSource Resolve(string categoryName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);

		lock (mLock)
		{
			string candidate = categoryName;
			while (true)
			{
				if (mSources.TryGetValue(candidate, out TraceSource? source))
					return source;

				int separatorIndex = candidate.LastIndexOf('.');
				if (separatorIndex < 0)
					break;

				candidate = candidate[..separatorIndex];
			}
		}

		throw new InvalidOperationException(
			$"No trace source is registered for logger category '{categoryName}'.");
	}
}
