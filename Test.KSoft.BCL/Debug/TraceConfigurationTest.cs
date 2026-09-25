using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class TraceConfigurationTest
{
	[TestMethod]
	public void Initialize_ConcurrentCallerWaitsForRegistration()
	{
		string hostPath = GetHostPath();
		var startInfo = CreateHostStartInfo(hostPath);
		startInfo.ArgumentList.Add("--verify-initialize-lock");

		using var process = Process.Start(startInfo);
		Assert.IsNotNull(process);

		string output = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		Assert.AreEqual(0, process.ExitCode, error);
		StringAssert.Contains(output, "Concurrent initialization waited for completed registration.");
	}

	[TestMethod]
	public void AppConfig_RegistersRefreshesAndWritesConfiguredListeners()
	{
		string testDirectory = Path.Combine(Path.GetTempPath(), $"KSoftTraceConfiguration-{Guid.NewGuid():N}");
		Directory.CreateDirectory(testDirectory);

		try
		{
			string configPath = Path.Combine(testDirectory, "trace-host.dll.config");
			string escapedDirectory = SecurityElement.Escape(testDirectory)!;
			File.WriteAllText(configPath, $"""
				<?xml version="1.0" encoding="utf-8"?>
				<configuration>
				  <system.diagnostics>
				    <switches>
				      <add name="HostSwitch" value="Warning" />
				    </switches>
				    <sharedListeners>
				      <add name="File"
				        type="KSoft.Debug.KSoftFileLogTraceListener, KSoft"
				        Location="Custom"
				        CustomLocation="{escapedDirectory}"
				        BaseFileName="trace-config"
				        Append="false"
				        AutoFlush="true"
				        DoNotIncludeEventId="true"
				        ReserveDiskSpace="0"
				        MaxFileSize="1048576" />
				    </sharedListeners>
				    <sources>
				      <source name="ConfiguredSource" switchName="HostSwitch">
				        <listeners>
				          <clear />
				          <add name="File" />
				        </listeners>
				      </source>
				    </sources>
				    <trace autoflush="true">
				      <listeners>
				        <clear />
				        <add name="File" />
				      </listeners>
				    </trace>
				  </system.diagnostics>
				</configuration>
				""");

			string hostPath = GetHostPath();
			var startInfo = CreateHostStartInfo(hostPath);
			startInfo.ArgumentList.Add(configPath);

			using var process = Process.Start(startInfo);
			Assert.IsNotNull(process);

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
			process.WaitForExit();

			Assert.AreEqual(0, process.ExitCode, error);
			StringAssert.Contains(output, "BeforeLevel=Warning");
			StringAssert.Contains(output, "AfterLevel=All");
			StringAssert.Contains(output, "Listener=KSoft.Debug.KSoftFileLogTraceListener");
			StringAssert.Contains(output, "ReserveDiskSpace=0");
			StringAssert.Contains(output, "DoNotIncludeEventId=True");

			string[] logPaths = Directory.GetFiles(testDirectory, "trace-config*.log");
			Assert.IsNotEmpty(logPaths);
			string log = string.Join(Environment.NewLine, logPaths.Select(File.ReadAllText));
			StringAssert.Contains(log, "global-message", output);
			StringAssert.Contains(log, "ConfiguredSource Information: source-message");
		}
		finally
		{
			Directory.Delete(testDirectory, recursive: true);
		}
	}

	private static ProcessStartInfo CreateHostStartInfo(string hostPath)
	{
		var startInfo = new ProcessStartInfo("dotnet")
		{
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};
		startInfo.ArgumentList.Add(hostPath);
		return startInfo;
	}

	private static string GetHostPath()
	{
		string hostPath = Path.Combine(
			AppContext.BaseDirectory,
			"TraceConfigHost",
			"Test.KSoft.TraceConfigHost.dll");
		Assert.IsTrue(File.Exists(hostPath), $"Trace configuration host was not built at '{hostPath}'.");
		return hostPath;
	}
}
