using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test;

[TestClass]
public sealed class BitFieldTraitsTest : BaseTestClass
{
	enum FourValueEnum
	{
		Zero,
		One,
		Two,
		Three,
	};

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
	public void Empty_HasZeroFieldRange()
	{
		var traits = BitFieldTraits.Empty;

		Assert.AreEqual(0, traits.BitCount);
		Assert.AreEqual(0, traits.BitIndex);
		Assert.AreEqual(0, traits.NextFieldBitIndex);
		Assert.AreEqual(0, traits.FieldsBitCount);
		Assert.IsTrue(traits.IsEmpty);
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

	[TestMethod]
	public void For_ValidEnumEncoder_InitializesFromEncoderBitCount()
	{
		var enumEncoder = new EnumBitEncoder32<FourValueEnum>();
		var previous = new BitFieldTraits(3);

		var traits = BitFieldTraits.For(enumEncoder);
		var nextTraits = BitFieldTraits.For(enumEncoder, previous);

		Assert.AreEqual(enumEncoder.BitCountTrait, traits.BitCount);
		Assert.AreEqual(0, traits.BitIndex);
		Assert.AreEqual(enumEncoder.BitCountTrait, nextTraits.BitCount);
		Assert.AreEqual(previous.NextFieldBitIndex, nextTraits.BitIndex);
	}

	[TestMethod]
	public void For_NullEnumEncoder_ThrowsArgumentNullException()
	{
		var previous = new BitFieldTraits(3);

		var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
			BitFieldTraits.For<uint>(null!));

		Assert.AreEqual("enumEncoder", exception.ParamName);

		exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
			BitFieldTraits.For<uint>(null!, previous));

		Assert.AreEqual("enumEncoder", exception.ParamName);
	}
}
