using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class KSoftFileLogTraceListenerTest : BaseTestClass
{
	static void AssertThrowsArgumentNull(Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual("value", exception.ParamName);
	}

	static void AssertThrowsArgumentOutOfRange(Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual("value", exception.ParamName);
	}

	[TestMethod]
	public void Settings_NullValues_ThrowArgumentNullException()
	{
#pragma warning disable CA2000 // The listener's lazy stream is unopened by these validation-only assignments.
		var listener = new KSoftFileLogTraceListener();
#pragma warning restore CA2000

		AssertThrowsArgumentNull(() => listener.BaseFileName = null!);
		AssertThrowsArgumentNull(() => listener.Encoding = null!);
	}

	[TestMethod]
	public void Settings_OutOfRangeValues_ThrowArgumentOutOfRangeException()
	{
#pragma warning disable CA2000 // The listener's lazy stream is unopened by these validation-only assignments.
		var listener = new KSoftFileLogTraceListener();
#pragma warning restore CA2000

		AssertThrowsArgumentOutOfRange(() => listener.MaxFileSize = 1000);
		AssertThrowsArgumentOutOfRange(() => listener.ReserveDiskSpace = -1);
	}
}
