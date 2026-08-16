using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class BuffersTest : BaseTestClass
{
	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void CountWithinLength_ThrowsExpectedExceptions()
	{
		byte[] buffer = null!;
		byte[] value = null!;
		byte[] valid = new byte[1];
		int negativeCount = -1;
		int tooLargeCount = 2;

		AssertThrowsArgumentNull(() => Verify.Buffers.CountWithinLength(buffer, 0), nameof(buffer));
		AssertThrowsArgumentNull(() => Verify.Buffers.CountWithinLength(value, 0), nameof(value));
		AssertThrowsArgumentOutOfRange(() => Verify.Buffers.CountWithinLength(valid, negativeCount), nameof(negativeCount));
		AssertThrowsArgumentOutOfRange(() => Verify.Buffers.CountWithinLength(valid, tooLargeCount), nameof(tooLargeCount));

		Verify.Buffers.CountWithinLength(valid, valid.Length);
	}
}
