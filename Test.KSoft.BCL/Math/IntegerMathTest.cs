using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public sealed class IntegerMathTest : BaseTestClass
{
	[TestMethod]
	public void Align_ValueIsNotAligned_ReturnsNextAlignedValue()
	{
		Assert.AreEqual(0x18U, IntegerMath.Align(3, 0x11U));
		Assert.AreEqual(0x18, IntegerMath.Align(3, 0x11));
		Assert.AreEqual(0x18UL, IntegerMath.Align(3, 0x11UL));
		Assert.AreEqual(0x18L, IntegerMath.Align(3, 0x11L));
	}

	[TestMethod]
	public void Align_ValueIsAlreadyAligned_ReturnsOriginalValue()
	{
		Assert.AreEqual(0x20U, IntegerMath.Align(4, 0x20U));
		Assert.AreEqual(0x20, IntegerMath.Align(4, 0x20));
		Assert.AreEqual(0x20UL, IntegerMath.Align(4, 0x20UL));
		Assert.AreEqual(0x20L, IntegerMath.Align(4, 0x20L));
	}

	[TestMethod]
	public void PaddingRequired_ValueIsNotAligned_ReturnsMissingByteCount()
	{
		Assert.AreEqual(7, IntegerMath.PaddingRequired(3, 0x11U));
		Assert.AreEqual(7, IntegerMath.PaddingRequired(3, 0x11));
		Assert.AreEqual(7, IntegerMath.PaddingRequired(3, 0x11UL));
		Assert.AreEqual(7, IntegerMath.PaddingRequired(3, 0x11L));
	}

	[TestMethod]
	public void PaddingRequired_ValueIsAlreadyAligned_ReturnsZero()
	{
		Assert.AreEqual(0, IntegerMath.PaddingRequired(4, 0x20U));
		Assert.AreEqual(0, IntegerMath.PaddingRequired(4, 0x20));
		Assert.AreEqual(0, IntegerMath.PaddingRequired(4, 0x20UL));
		Assert.AreEqual(0, IntegerMath.PaddingRequired(4, 0x20L));
	}

	[TestMethod]
	public void Align_AlignmentBitIsTooLarge_ThrowsArgumentOutOfRangeException()
	{
		const int invalidAlignmentBit = IntegerMath.kMaxAlignmentBit + 1;

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(invalidAlignmentBit, 0U));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(invalidAlignmentBit, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(invalidAlignmentBit, 0UL));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(invalidAlignmentBit, 0L));
	}

	[TestMethod]
	public void PaddingRequired_AlignmentBitIsTooLarge_ThrowsArgumentOutOfRangeException()
	{
		const int invalidAlignmentBit = IntegerMath.kMaxAlignmentBit + 1;

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(invalidAlignmentBit, 0U));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(invalidAlignmentBit, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(invalidAlignmentBit, 0UL));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(invalidAlignmentBit, 0L));
	}

	[TestMethod]
	public void Align_SignedValueIsNegative_ThrowsArgumentOutOfRangeException()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(3, -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.Align(3, -1L));
	}

	[TestMethod]
	public void PaddingRequired_SignedValueIsNegative_ThrowsArgumentOutOfRangeException()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(3, -1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => IntegerMath.PaddingRequired(3, -1L));
	}

	[TestMethod]
	public void IsSigned_SignBitIsSet_ReturnsTrue()
	{
		Assert.IsTrue(IntegerMath.IsSigned((byte)0x80));
		Assert.IsTrue(IntegerMath.IsSigned((ushort)0x8000));
		Assert.IsTrue(IntegerMath.IsSigned(0x80000000U));
		Assert.IsTrue(IntegerMath.IsSigned(0x8000000000000000UL));
	}

	[TestMethod]
	public void IsSigned_SignBitIsClear_ReturnsFalse()
	{
		Assert.IsFalse(IntegerMath.IsSigned((byte)0x7F));
		Assert.IsFalse(IntegerMath.IsSigned((ushort)0x7FFF));
		Assert.IsFalse(IntegerMath.IsSigned(0x7FFFFFFFU));
		Assert.IsFalse(IntegerMath.IsSigned(0x7FFFFFFFFFFFFFFFUL));
	}

	[TestMethod]
	public void SetSignBit_SignBitIsClear_ReturnsValueWithSignBitSet()
	{
		Assert.AreEqual((byte)0x81, IntegerMath.SetSignBit((byte)0x01));
		Assert.AreEqual((ushort)0x8001, IntegerMath.SetSignBit((ushort)0x0001));
		Assert.AreEqual(0x80000001U, IntegerMath.SetSignBit(0x00000001U));
		Assert.AreEqual(0x8000000000000001UL, IntegerMath.SetSignBit(0x0000000000000001UL));
	}

	[TestMethod]
	public void SetSignBit_SignBitIsAlreadySet_ReturnsOriginalValue()
	{
		Assert.AreEqual((byte)0x81, IntegerMath.SetSignBit((byte)0x81));
		Assert.AreEqual((ushort)0x8001, IntegerMath.SetSignBit((ushort)0x8001));
		Assert.AreEqual(0x80000001U, IntegerMath.SetSignBit(0x80000001U));
		Assert.AreEqual(0x8000000000000001UL, IntegerMath.SetSignBit(0x8000000000000001UL));
	}
};
