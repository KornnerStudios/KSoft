using System;

namespace KSoft.Security;

public static class Program
{
	public static void Initialize()
		=> KSoft.Program.RegisterTraceSources(DebugTraceClass);

	public static void Dispose()
	{
	}

	public static Type DebugTraceClass => typeof(Debug.Trace);
}
