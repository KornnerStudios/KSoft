using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test;

[TestClass]
public sealed class Encoded7BitIntTest : BaseTestClass
{

	static void AssertRead(byte[] buffer, int startIndex, int maxCount, int expectedValue, int expectedEndingIndex)
	{
		int value = Encoded7BitInt.Read(buffer, startIndex, maxCount, out int endingIndex);

		Assert.AreEqual(expectedValue, value);
		Assert.AreEqual(expectedEndingIndex, endingIndex);
	}

	[TestMethod]
	public void CalculateSize_BoundaryValues_ReturnsExpectedByteCount()
	{
		Assert.AreEqual(1, Encoded7BitInt.CalculateSize(0));
		Assert.AreEqual(1, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue1Bytes));
		Assert.AreEqual(2, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue1Bytes + 1));
		Assert.AreEqual(2, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue2Bytes));
		Assert.AreEqual(3, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue2Bytes + 1));
		Assert.AreEqual(3, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue3Bytes));
		Assert.AreEqual(4, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue3Bytes + 1));
		Assert.AreEqual(4, Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue4Bytes));
	}

	[TestMethod]
	public void CalculateSize_InvalidValuesThrowExpectedExceptions()
	{
		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.CalculateSize(-1));
		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue4Bytes + 1));
	}

	[TestMethod]
	public void Read_ValidBuffers_ReturnsDecodedValueAndEndingIndex()
	{
		AssertRead([0x00], 0, 1, 0, 1);
		AssertRead([0x03, 0xAA, 0xBB, 0xCC], 0, 4, 3, 1);
		AssertRead([0xEE, 0x03, 0xAA, 0xBB, 0xCC], 1, 4, 3, 2);

		var buffer = new byte[130];
		buffer[0] = 0x80;
		buffer[1] = 0x01;
		AssertRead(buffer, 0, buffer.Length, 128, 2);
	}

	[TestMethod]
	public void Read_InvalidArgumentsThrowExpectedExceptions()
	{
		AssertThrowsArgumentNull("buffer", () => Encoded7BitInt.Read(null!, 0, 1, out _));
		AssertThrowsArgumentOutOfRange("startIndex", () => Encoded7BitInt.Read([0], -1, 1, out _));
		AssertThrowsArgumentOutOfRange("startIndex", () => Encoded7BitInt.Read([0], 2, 1, out _));
		AssertThrowsArgumentOutOfRange("maxCount", () => Encoded7BitInt.Read([0], 0, 0, out _));
		AssertThrowsArgumentOutOfRange("maxCount", () => Encoded7BitInt.Read([0], 0, 2, out _));
	}

	[TestMethod]
	public void Read_IncompleteOrInvalidData_ReturnsNone()
	{
		AssertRead([0x80], 0, 1, TypeExtensions.kNone, TypeExtensions.kNone);
		AssertRead([0x04, 0xAA, 0xBB, 0xCC], 0, 4, TypeExtensions.kNone, TypeExtensions.kNone);
		AssertRead([0x80, 0x80, 0x80, 0x80, 0x00], 0, 5, TypeExtensions.kNone, TypeExtensions.kNone);
	}

	[TestMethod]
	public void Write_ValidValuesWritesExpectedBytesAndReturnsEndingIndex()
	{
		var buffer = new byte[8];

		Assert.AreEqual(1, Encoded7BitInt.Write(buffer, 0, 0));
		Assert.AreEqual((byte)0x00, buffer[0]);

		Array.Clear(buffer);
		Assert.AreEqual(1, Encoded7BitInt.Write(buffer, 0, Encoded7BitInt.kMaxValue1Bytes));
		Assert.AreEqual((byte)0x7F, buffer[0]);

		Array.Clear(buffer);
		Assert.AreEqual(2, Encoded7BitInt.Write(buffer, 0, Encoded7BitInt.kMaxValue1Bytes + 1));
		CollectionAssert.AreEqual(new byte[] { 0x80, 0x01 }, buffer[..2]);

		Array.Clear(buffer);
		Assert.AreEqual(6, Encoded7BitInt.Write(buffer, 2, Encoded7BitInt.kMaxValue4Bytes));
		CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, buffer[2..6]);
	}

	[TestMethod]
	public void Write_InvalidArgumentsThrowExpectedExceptions()
	{
		AssertThrowsArgumentNull("buffer", () => Encoded7BitInt.Write(null!, 0, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => Encoded7BitInt.Write(new byte[1], -1, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => Encoded7BitInt.Write(new byte[1], 2, 0));
		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.Write(new byte[1], 0, -1));
		AssertThrowsArgumentOutOfRange("value",
			() => Encoded7BitInt.Write(new byte[4], 0, Encoded7BitInt.kMaxValue4Bytes + 1));
		AssertThrowsArgument("buffer", () => Encoded7BitInt.Write(new byte[1], 0, Encoded7BitInt.kMaxValue1Bytes + 1));
	}
}
