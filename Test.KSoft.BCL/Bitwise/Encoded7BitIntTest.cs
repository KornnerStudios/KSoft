using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test;

[TestClass]
public sealed class Encoded7BitIntTest : BaseTestClass
{

	static void AssertEncoding(int value, byte[] expectedBytes)
	{
		var buffer = new byte[5];
		Array.Fill(buffer, (byte)0xCC);

		int bytesWritten = Encoded7BitInt.Write(buffer, value);

		Assert.AreEqual(expectedBytes.Length, Encoded7BitInt.CalculateSize(value));
		Assert.AreEqual(expectedBytes.Length, bytesWritten);
		CollectionAssert.AreEqual(expectedBytes, buffer[..bytesWritten]);
		Assert.IsFalse(buffer.AsSpan(bytesWritten).ContainsAnyExcept((byte)0xCC));

		using var referenceStream = new MemoryStream();
		using var writer = new BinaryWriter(referenceStream);
		writer.Write7BitEncodedInt(value);
		CollectionAssert.AreEqual(expectedBytes, referenceStream.ToArray());

		using var reader = new BinaryReader(new MemoryStream(buffer, 0, bytesWritten));
		Assert.AreEqual(value, reader.Read7BitEncodedInt());
		Assert.AreEqual((long)bytesWritten, reader.BaseStream.Position);
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
		AssertEncoding(Encoded7BitInt.kMaxValue4Bytes + 1, [0x80, 0x80, 0x80, 0x80, 0x01]);
		AssertEncoding(int.MaxValue, [0xFF, 0xFF, 0xFF, 0xFF, 0x07]);
		AssertEncoding(int.MinValue, [0x80, 0x80, 0x80, 0x80, 0x08]);
		AssertEncoding(-1, [0xFF, 0xFF, 0xFF, 0xFF, 0x0F]);
	}

	[TestMethod]
	[DataRow("8080808000", 0)]
	[DataRow("8180808000", 1)]
	[DataRow("8080808008", int.MinValue)]
	[DataRow("FFFFFFFF0F", -1)]
	public void Read_FiveBytePrefixes_MatchesBinaryReaderAndReturnsRelativeByteCount(string hex, int expectedValue)
	{
		byte[] prefix = Convert.FromHexString(hex);
		// Preserve Read's existing payload-capacity check without allocating a large payload.
		int payloadLength = Math.Max(expectedValue, 0);
		var buffer = new byte[2 + prefix.Length + payloadLength + 1];
		Array.Fill(buffer, (byte)0xCC);
		prefix.CopyTo(buffer, 2);
		byte[] original = (byte[])buffer.Clone();

		int value = Encoded7BitInt.Read(buffer.AsSpan(2, prefix.Length + payloadLength), out int bytesRead);

		Assert.AreEqual(expectedValue, value);
		Assert.AreEqual(prefix.Length, bytesRead);
		CollectionAssert.AreEqual(original, buffer);
		using var reader = new BinaryReader(new MemoryStream(prefix));
		Assert.AreEqual(reader.Read7BitEncodedInt(), value);
		Assert.AreEqual(reader.BaseStream.Position, bytesRead);
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
		AssertReadFailure([0x80, 0x80, 0x80, 0x80, 0x80]);
	}

	[TestMethod]
	[DataRow((byte)0x10)]
	[DataRow((byte)0x7F)]
	[DataRow((byte)0x80)]
	[DataRow((byte)0xFF)]
	public void Read_InvalidFifthByte_RejectsTheSameOverflowAsBinaryReader(byte fifthByte)
	{
		byte[] buffer = [0x80, 0x80, 0x80, 0x80, fifthByte];

		AssertReadFailure(buffer);
		using var reader = new BinaryReader(new MemoryStream(buffer));
		Assert.ThrowsExactly<FormatException>(() => reader.Read7BitEncodedInt());
		Assert.AreEqual(5L, reader.BaseStream.Position);
	}

	[TestMethod]
	public void Read_DecodedPayloadDoesNotFitRemainingSpan_ReturnsBothFailureSentinels()
	{
		AssertReadFailure([0x04, 0xAA, 0xBB, 0xCC]);

		byte[] buffer = [0xEE, 0x03, 0xAA, 0xBB, 0xCC, 0xFF];
		AssertReadFailure(buffer.AsSpan(1, 3));
		AssertReadFailure([0x80, 0x80, 0x80, 0x80, 0x01]);
		AssertReadFailure([0xFF, 0xFF, 0xFF, 0xFF, 0x07]);
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
	public void Write_FiveByteValueInCallerSlice_PreservesSentinels()
	{
		var buffer = new byte[9];
		Array.Fill(buffer, (byte)0xCC);

		int bytesWritten = Encoded7BitInt.Write(buffer.AsSpan(2, 5), 0x10000000);

		Assert.AreEqual(5, bytesWritten);
		CollectionAssert.AreEqual(
			new byte[] { 0xCC, 0xCC, 0x80, 0x80, 0x80, 0x80, 0x01, 0xCC, 0xCC }, buffer);
	}

	[TestMethod]
	[DataRow(0x10000000)]
	[DataRow(int.MaxValue)]
	[DataRow(int.MinValue)]
	[DataRow(-1)]
	public void Write_FiveByteValuesWithShortDestination_ThrowBeforeMutation(int value)
	{
		var buffer = new byte[4];
		Array.Fill(buffer, (byte)0xCC);
		byte[] original = (byte[])buffer.Clone();

		AssertThrowsArgument("buffer", () => Encoded7BitInt.Write(buffer, value));

		CollectionAssert.AreEqual(original, buffer);
	}
}
