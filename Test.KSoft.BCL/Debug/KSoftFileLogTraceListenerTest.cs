using System;
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
}
