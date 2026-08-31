using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class BuffersTest : BaseTestClass
{

	[TestMethod]
	public void CountWithinLength_ThrowsExpectedExceptions()
	{
		byte[] buffer = null!;
		byte[] value = null!;
		byte[] valid = new byte[1];
		int negativeCount = -1;
		int tooLargeCount = 2;

		AssertThrowsArgumentNull(nameof(buffer), () => Verify.Buffers.CountWithinLength(buffer, 0));
		AssertThrowsArgumentNull(nameof(value), () => Verify.Buffers.CountWithinLength(value, 0));
		AssertThrowsArgumentOutOfRange(nameof(negativeCount), () => Verify.Buffers.CountWithinLength(valid, negativeCount));
		AssertThrowsArgumentOutOfRange(nameof(tooLargeCount), () => Verify.Buffers.CountWithinLength(valid, tooLargeCount));

		Verify.Buffers.CountWithinLength(valid, valid.Length);
	}

	[TestMethod]
	public void StartIndexWithinLength_ThrowsExpectedExceptions()
	{
		byte[] buffer = null!;
		byte[] valid = new byte[1];
		int startIndex = -1;
		int tooLargeStartIndex = 2;

		AssertThrowsArgumentNull(nameof(buffer), () => Verify.Buffers.StartIndexWithinLength(buffer, 0));
		AssertThrowsArgumentOutOfRange(nameof(startIndex), () => Verify.Buffers.StartIndexWithinLength(valid, startIndex));
		AssertThrowsArgumentOutOfRange(nameof(tooLargeStartIndex),
			() => Verify.Buffers.StartIndexWithinLength(valid, tooLargeStartIndex));

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

		AssertThrowsArgumentNull(nameof(buffer), () => Verify.Buffers.OffsetAndLengthWithinLength(buffer, 0, 0));
		AssertThrowsArgumentOutOfRange(nameof(offset),
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, offset, 0));
		AssertThrowsArgumentOutOfRange(nameof(length),
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, 0, length));
		AssertThrowsArgumentOutOfRange(nameof(tooLargeOffset),
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, tooLargeOffset, 0));
		AssertThrowsArgumentOutOfRange(nameof(tooLargeLength),
			() => Verify.Buffers.OffsetAndLengthWithinLength(valid, 0, tooLargeLength));

		Verify.Buffers.OffsetAndLengthWithinLength(valid, valid.Length, 0);
	}
}
