using System;

namespace KSoft.Debug;

internal sealed class NullTraceScope : IDisposable
{
	public static NullTraceScope Instance { get; } = new();

	private NullTraceScope()
	{
	}

	public void Dispose()
	{
	}
}
