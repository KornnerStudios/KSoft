using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace KSoft.Debug;

internal sealed class TraceSourceLoggerProvider : ILoggerProvider
{
	private readonly TraceSourceRegistry mRegistry;
	private int mDisposed;

	public TraceSourceLoggerProvider(TraceSourceRegistry registry)
	{
		ArgumentNullException.ThrowIfNull(registry);
		mRegistry = registry;
	}

	public ILogger CreateLogger(string categoryName)
	{
		ObjectDisposedException.ThrowIf(Volatile.Read(ref mDisposed) != 0, this);
		return new TraceSourceLogger(this, categoryName);
	}

	public ILogger<T> CreateLogger<T>()
	{
		ObjectDisposedException.ThrowIf(Volatile.Read(ref mDisposed) != 0, this);
		return new TraceSourceLogger<T>(this);
	}

	public void Dispose()
		=> Interlocked.Exchange(ref mDisposed, 1);

	internal TraceSource Resolve(string categoryName)
	{
		ObjectDisposedException.ThrowIf(Volatile.Read(ref mDisposed) != 0, this);
		return mRegistry.Resolve(categoryName);
	}
}
