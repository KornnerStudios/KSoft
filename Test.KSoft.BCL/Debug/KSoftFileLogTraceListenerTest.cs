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
		var listener = new KSoftFileLogTraceListener();

		AssertThrowsArgumentNull(() => listener.BaseFileName = null!);
		AssertThrowsArgumentNull(() => listener.Encoding = null!);
	}

	[TestMethod]
	public void Settings_OutOfRangeValues_ThrowArgumentOutOfRangeException()
	{
		var listener = new KSoftFileLogTraceListener();

		AssertThrowsArgumentOutOfRange(() => listener.MaxFileSize = 1000);
		AssertThrowsArgumentOutOfRange(() => listener.ReserveDiskSpace = -1);
	}
}
