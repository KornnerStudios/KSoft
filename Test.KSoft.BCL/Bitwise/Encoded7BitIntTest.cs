using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test;

[TestClass]
public sealed class Encoded7BitIntTest : BaseTestClass
{

	static void AssertEncoding(int value, byte[] expectedBytes)
	{
		var buffer = new byte[4];
		Array.Fill(buffer, (byte)0xCC);

		int bytesWritten = Encoded7BitInt.Write(buffer, value);

		Assert.AreEqual(expectedBytes.Length, Encoded7BitInt.CalculateSize(value));
		Assert.AreEqual(expectedBytes.Length, bytesWritten);
		CollectionAssert.AreEqual(expectedBytes, buffer[..bytesWritten]);
	}

	static void AssertReadFailure(ReadOnlySpan<byte> buffer)
	{
		int value = Encoded7BitInt.Read(buffer, out int bytesRead);

		Assert.AreEqual(TypeExtensions.kNone, value);
		Assert.AreEqual(TypeExtensions.kNone, bytesRead);
	}

	[TestMethod]
	public void CalculateSizeAndWrite_BoundaryValues_ReturnExpectedSizesAndBytes()
	{
		AssertEncoding(0, [0x00]);
		AssertEncoding(Encoded7BitInt.kMaxValue1Bytes, [0x7F]);
		AssertEncoding(Encoded7BitInt.kMaxValue1Bytes + 1, [0x80, 0x01]);
		AssertEncoding(Encoded7BitInt.kMaxValue2Bytes, [0xFF, 0x7F]);
		AssertEncoding(Encoded7BitInt.kMaxValue2Bytes + 1, [0x80, 0x80, 0x01]);
		AssertEncoding(Encoded7BitInt.kMaxValue3Bytes, [0xFF, 0xFF, 0x7F]);
		AssertEncoding(Encoded7BitInt.kMaxValue3Bytes + 1, [0x80, 0x80, 0x80, 0x01]);
		AssertEncoding(Encoded7BitInt.kMaxValue4Bytes, [0xFF, 0xFF, 0xFF, 0x7F]);
	}

	[TestMethod]
	public void CalculateSize_InvalidValuesThrowExpectedExceptions()
	{
		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.CalculateSize(-1));
		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.CalculateSize(Encoded7BitInt.kMaxValue4Bytes + 1));
	}

	[TestMethod]
	public void Read_WholeSpan_ReturnsDecodedValueAndRelativeBytesRead()
	{
		byte[] buffer = [0x03, 0xAA, 0xBB, 0xCC];
		byte[] original = (byte[])buffer.Clone();

		int value = Encoded7BitInt.Read(buffer, out int bytesRead);

		Assert.AreEqual(3, value);
		Assert.AreEqual(1, bytesRead);
		CollectionAssert.AreEqual(original, buffer);
	}

	[TestMethod]
	public void Read_NonzeroCallerSlice_ReturnsRelativeBytesReadAndDoesNotMutateSource()
	{
		var buffer = new byte[132];
		Array.Fill(buffer, (byte)0xAA);
		buffer[0] = 0xEE;
		buffer[1] = 0x80;
		buffer[2] = 0x01;
		buffer[^1] = 0xFF;
		byte[] original = (byte[])buffer.Clone();

		int value = Encoded7BitInt.Read(buffer.AsSpan(1, 130), out int bytesRead);

		Assert.AreEqual(128, value);
		Assert.AreEqual(2, bytesRead);
		CollectionAssert.AreEqual(original, buffer);
	}

	[TestMethod]
	public void Read_EmptyIncompleteOrCorruptData_ReturnsBothFailureSentinels()
	{
		AssertReadFailure([]);
		AssertReadFailure([0x80]);
		AssertReadFailure([0x80, 0x80]);
		AssertReadFailure([0x80, 0x80, 0x80]);
		AssertReadFailure([0x80, 0x80, 0x80, 0x80]);
		AssertReadFailure([0x80, 0x80, 0x80, 0x80, 0x00]);
	}

	[TestMethod]
	public void Read_DecodedPayloadDoesNotFitRemainingSpan_ReturnsBothFailureSentinels()
	{
		AssertReadFailure([0x04, 0xAA, 0xBB, 0xCC]);

		byte[] buffer = [0xEE, 0x03, 0xAA, 0xBB, 0xCC, 0xFF];
		AssertReadFailure(buffer.AsSpan(1, 3));
	}

	[TestMethod]
	public void Write_NonzeroCallerSlice_ReturnsRelativeByteCountAndPreservesSentinels()
	{
		var buffer = new byte[8];
		Array.Fill(buffer, (byte)0xCC);

		int bytesWritten = Encoded7BitInt.Write(buffer.AsSpan(2, 4), Encoded7BitInt.kMaxValue4Bytes);

		Assert.AreEqual(4, bytesWritten);
		CollectionAssert.AreEqual(
			new byte[] { 0xCC, 0xCC, 0xFF, 0xFF, 0xFF, 0x7F, 0xCC, 0xCC },
			buffer);
	}

	[TestMethod]
	public void Write_InsufficientDestination_ThrowsBeforeMutation()
	{
		var buffer = new byte[4];
		Array.Fill(buffer, (byte)0xCC);
		byte[] original = (byte[])buffer.Clone();

		AssertThrowsArgument("buffer",
			() => Encoded7BitInt.Write(buffer.AsSpan(1, 1), Encoded7BitInt.kMaxValue1Bytes + 1));

		CollectionAssert.AreEqual(original, buffer);
	}

	[TestMethod]
	public void Write_InvalidValuesThrowBeforeMutation()
	{
		var buffer = new byte[4];
		Array.Fill(buffer, (byte)0xCC);
		byte[] original = (byte[])buffer.Clone();

		AssertThrowsArgumentOutOfRange("value", () => Encoded7BitInt.Write(buffer, -1));
		AssertThrowsArgumentOutOfRange("value",
			() => Encoded7BitInt.Write(buffer, Encoded7BitInt.kMaxValue4Bytes + 1));

		CollectionAssert.AreEqual(original, buffer);
	}
}
