using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test;

[TestClass]
public sealed class BitFieldTraitsTest : BaseTestClass
{
	static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	static void AssertThrowsArgument(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	[TestMethod]
	public void Constructor_ValidBitCount_InitializesZeroBasedField()
	{
		var traits = new BitFieldTraits(5);

		Assert.AreEqual(5, traits.BitCount);
		Assert.AreEqual(0, traits.BitIndex);
		Assert.AreEqual(5, traits.NextFieldBitIndex);
		Assert.AreEqual(5, traits.FieldsBitCount);
		Assert.IsTrue(traits.Is32Bit);
		Assert.AreEqual(0b1_1111U, traits.Bitmask32);
	}

	[TestMethod]
	public void Constructor_ValidBitCountAndIndex_InitializesOffsetField()
	{
		var traits = new BitFieldTraits(4, 60);

		Assert.AreEqual(4, traits.BitCount);
		Assert.AreEqual(60, traits.BitIndex);
		Assert.AreEqual(64, traits.NextFieldBitIndex);
		Assert.AreEqual(64, traits.FieldsBitCount);
		Assert.IsTrue(traits.Is32Bit);
		Assert.AreEqual(0b1111U, traits.Bitmask32);
	}

	[TestMethod]
	public void Constructor_PreviousTraits_InitializesNextField()
	{
		var first = new BitFieldTraits(6, 8);
		var second = new BitFieldTraits(10, first);

		Assert.AreEqual(10, second.BitCount);
		Assert.AreEqual(14, second.BitIndex);
		Assert.AreEqual(24, second.NextFieldBitIndex);
	}

	[TestMethod]
	public void Constructor_InvalidBitCount_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("bitCount", () => new BitFieldTraits(0));
		AssertThrowsArgumentOutOfRange("bitCount", () => new BitFieldTraits(BitFieldTraits.kMaxBitCount + 1));
	}

	[TestMethod]
	public void Constructor_InvalidBitIndex_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("bitIndex", () => new BitFieldTraits(1, -1));
		AssertThrowsArgumentOutOfRange("bitIndex", () => new BitFieldTraits(1, BitFieldTraits.kMaxBitCount));
	}

	[TestMethod]
	public void Constructor_RangePastMaxBitCount_ThrowsArgumentException()
	{
		var previous = new BitFieldTraits(BitFieldTraits.kMaxBitCount);

		AssertThrowsArgument("bitCount", () => new BitFieldTraits(2, BitFieldTraits.kMaxBitCount - 1));
		AssertThrowsArgument("bitCount", () => new BitFieldTraits(1, previous));
	}
}
