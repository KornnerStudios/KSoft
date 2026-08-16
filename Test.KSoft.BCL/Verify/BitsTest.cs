using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class VerifyBitsTest : BaseTestClass
{
	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void AtMost_ThrowsExpectedExceptions()
	{
		int bitCount = Bits.kByteBitCount + 1;
		int countBitSize = Bits.kInt32BitCount + 1;

		AssertThrowsArgumentOutOfRange(() => Verify.Bits.AtMost(bitCount, Bits.kByteBitCount), nameof(bitCount));
		AssertThrowsArgumentOutOfRange(
			() => Verify.Bits.AtMost(countBitSize, Bits.kInt32BitCount),
			nameof(countBitSize));

		Verify.Bits.AtMost(Bits.kByteBitCount, Bits.kByteBitCount);
		Verify.Bits.AtMost(-1, Bits.kByteBitCount);
	}
}
