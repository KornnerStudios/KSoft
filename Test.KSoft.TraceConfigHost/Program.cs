using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test.KSoft.TraceConfigHost;

internal static class Program
{
	private static int Main(string[] args)
	{
		if (args is ["--verify-initialize-lock"])
			return VerifyInitializeLock();

		if (args.Length > 1)
		{
			Console.Error.WriteLine("Expected zero arguments or a configuration file path.");
			return 2;
		}

		string? configPath = null;
		if (args.Length == 1)
		{
			configPath = Path.GetFullPath(args[0]);
			AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
		}

		global::KSoft.Program.Initialize();
		var source = new TraceSource("ConfiguredSource", SourceLevels.Off);
		try
		{
			var listener = source.Listeners.OfType<global::KSoft.Debug.KSoftFileLogTraceListener>().Single();

			Console.WriteLine($"BeforeLevel={source.Switch.Level}");
			Console.WriteLine($"Listener={listener.GetType().FullName}");
			Console.WriteLine($"ReserveDiskSpace={listener.ReserveDiskSpace}");
			Console.WriteLine($"DoNotIncludeEventId={listener.DoNotIncludeEventId}");
			Console.WriteLine($"GlobalListenersBefore={string.Join(",", Trace.Listeners.Cast<TraceListener>().Select(x => x.Name))}");
			Trace.WriteLine("global-message");
			Trace.Flush();

			if (configPath != null)
			{
				string config = File.ReadAllText(configPath);
				File.WriteAllText(configPath, config.Replace("value=\"Warning\"", "value=\"All\"", StringComparison.Ordinal));
				Trace.Refresh();
			}

			Console.WriteLine($"AfterLevel={source.Switch.Level}");
			Console.WriteLine($"GlobalListenersAfter={string.Join(",", Trace.Listeners.Cast<TraceListener>().Select(x => x.Name))}");

			source.TraceEvent(TraceEventType.Information, 7, "source-message");
			source.Flush();
			Trace.Flush();

			Console.WriteLine($"LogFile={listener.FullLogFileName}");
			return 0;
		}
		finally
		{
			source.Close();
			global::KSoft.Program.Dispose();
		}
	}

	private static int VerifyInitializeLock()
	{
		using var registrationEntered = new ManualResetEventSlim();
		using var releaseRegistration = new ManualResetEventSlim();
		int registrationCount = 0;

		Task firstInitialize = Task.Run(() => global::KSoft.Program.Initialize(() =>
		{
			Interlocked.Increment(ref registrationCount);
			registrationEntered.Set();
			releaseRegistration.Wait();
		}));

		if (!registrationEntered.Wait(TimeSpan.FromSeconds(5)))
			return 3;

		Task secondInitialize = Task.Run(() => global::KSoft.Program.Initialize(
			() => Interlocked.Increment(ref registrationCount)));

		try
		{
			if (secondInitialize.Wait(TimeSpan.FromMilliseconds(250)))
			{
				Console.Error.WriteLine("Concurrent Initialize returned before registration completed.");
				return 4;
			}
		}
		finally
		{
			releaseRegistration.Set();
		}

		Task.WaitAll(firstInitialize, secondInitialize);
		global::KSoft.Program.Dispose();

		if (registrationCount != 1)
		{
			Console.Error.WriteLine($"Registration executed {registrationCount} times.");
			return 5;
		}

		Console.WriteLine("Concurrent initialization waited for completed registration.");
		return 0;
	}
}
