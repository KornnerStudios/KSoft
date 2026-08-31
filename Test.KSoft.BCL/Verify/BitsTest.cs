using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class VerifyBitsTest : BaseTestClass
{

	[TestMethod]
	public void AtMost_ThrowsExpectedExceptions()
	{
		int bitCount = Bits.kByteBitCount + 1;
		int countBitSize = Bits.kInt32BitCount + 1;

		AssertThrowsArgumentOutOfRange(nameof(bitCount), () => Verify.Bits.AtMost(bitCount, Bits.kByteBitCount));
		AssertThrowsArgumentOutOfRange(nameof(countBitSize),
			() => Verify.Bits.AtMost(countBitSize, Bits.kInt32BitCount));

		Verify.Bits.AtMost(Bits.kByteBitCount, Bits.kByteBitCount);
		Verify.Bits.AtMost(-1, Bits.kByteBitCount);
	}
}
