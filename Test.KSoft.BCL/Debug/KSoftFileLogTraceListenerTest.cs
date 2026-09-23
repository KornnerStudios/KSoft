using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class KSoftFileLogTraceListenerTest : BaseTestClass
{

	[TestMethod]
	public void Settings_NullValues_ThrowArgumentNullException()
	{
#pragma warning disable CA2000 // The listener's lazy stream is unopened by these validation-only assignments.
		var listener = new KSoftFileLogTraceListener();
#pragma warning restore CA2000

		AssertThrowsArgumentNull("value", () => listener.BaseFileName = null!);
		AssertThrowsArgumentNull("value", () => listener.Encoding = null!);
	}

	[TestMethod]
	public void Settings_OutOfRangeValues_ThrowArgumentOutOfRangeException()
	{
#pragma warning disable CA2000 // The listener's lazy stream is unopened by these validation-only assignments.
		var listener = new KSoftFileLogTraceListener();
#pragma warning restore CA2000

		AssertThrowsArgumentOutOfRange("value", () => listener.MaxFileSize = 1000);
		AssertThrowsArgumentOutOfRange("value", () => listener.ReserveDiskSpace = -1);
	}

	[TestMethod]
	public void Settings_ReserveDiskSpaceAttributes_SupportCanonicalAndLegacyNames()
	{
		using var canonicalListener = new KSoftFileLogTraceListener();
		canonicalListener.Attributes[nameof(KSoftFileLogTraceListener.ReserveDiskSpace)] = "1234";
		Assert.AreEqual(1234, canonicalListener.ReserveDiskSpace);

		using var legacyListener = new KSoftFileLogTraceListener();
		legacyListener.Attributes["ReservedDiskSpace"] = "5678";
		Assert.AreEqual(5678, legacyListener.ReserveDiskSpace);
	}

	[TestMethod]
	public void DefaultBaseFileName_UsesEntryApplicationIdentity()
	{
		using var listener = new KSoftFileLogTraceListener();
		Assert.AreEqual(Assembly.GetEntryAssembly()!.GetName().Name, listener.BaseFileName);
	}

	[TestMethod]
	public void Write_NullMessages_WritesAnEmptyLine()
	{
		string testDirectory = CreateTestDirectory();
		try
		{
			using (var listener = CreateListener(testDirectory, "null-message"))
			{
				listener.Write(null);
				listener.WriteLine(null);
			}

			string contents = File.ReadAllText(Path.Combine(testDirectory, "null-message.log"), Encoding.UTF8);
			Assert.AreEqual(Environment.NewLine, contents);
		}
		finally
		{
			Directory.Delete(testDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void TraceData_Exception_PreservesMessageAndStackTrace()
	{
		string testDirectory = CreateTestDirectory();
		try
		{
			using (var listener = CreateListener(testDirectory, "exception"))
			{
				Exception exception;
				try
				{
					throw new InvalidOperationException("expected failure");
				}
				catch (Exception ex)
				{
					exception = ex;
				}

				listener.TraceData(null, "TestSource", TraceEventType.Error, 17, exception);
			}

			string contents = File.ReadAllText(Path.Combine(testDirectory, "exception.log"), Encoding.UTF8);
			StringAssert.Contains(contents, "TestSource Error: 17 : System.InvalidOperationException: expected failure");
			StringAssert.Contains(contents, nameof(TraceData_Exception_PreservesMessageAndStackTrace));
		}
		finally
		{
			Directory.Delete(testDirectory, recursive: true);
		}
	}

	private static KSoftFileLogTraceListener CreateListener(string directory, string baseFileName)
	{
		return new KSoftFileLogTraceListener
		{
			Append = false,
			AutoFlush = true,
			BaseFileName = baseFileName,
			CustomLocation = directory,
			MaxFileSize = 1024 * 1024,
			ReserveDiskSpace = 0,
		};
	}

	private static string CreateTestDirectory()
	{
		string path = Path.Combine(Path.GetTempPath(), $"KSoftFileLogTraceListener-{Guid.NewGuid():N}");
		Directory.CreateDirectory(path);
		return path;
	}
}
