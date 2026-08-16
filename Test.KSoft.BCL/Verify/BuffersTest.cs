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

	[TestMethod]
	public void StartIndexWithinLength_ThrowsExpectedExceptions()
	{
		byte[] buffer = null!;
		byte[] valid = new byte[1];
		int startIndex = -1;
		int tooLargeStartIndex = 2;

		AssertThrowsArgumentNull(() => Verify.Buffers.StartIndexWithinLength(buffer, 0), nameof(buffer));
		AssertThrowsArgumentOutOfRange(() => Verify.Buffers.StartIndexWithinLength(valid, startIndex), nameof(startIndex));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Buffers.StartIndexWithinLength(valid, tooLargeStartIndex),
			nameof(tooLargeStartIndex));

		Verify.Buffers.StartIndexWithinLength(valid, valid.Length);
	}

	[TestMethod]
	public void OffsetAndLengthWithinLength_ThrowsExpectedExceptions()
	{
		byte[] buffer = null!;
		byte[] valid = new byte[1];
		int offset = -1;
		int length = -1;
		int tooLargeOffset = 2;
		int tooLargeLength = 2;

		AssertThrowsArgumentNull(() => Verify.Buffers.OffsetAndLengthWithinLength(buffer, 0, 0), nameof(buffer));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, offset, 0),
			nameof(offset));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, 0, length),
			nameof(length));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, tooLargeOffset, 0),
			nameof(tooLargeOffset));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, 0, tooLargeLength),
			nameof(tooLargeLength));

		Verify.Buffers.OffsetAndLengthWithinLength(valid, valid.Length, 0);
	}
}
