using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test;

using Memory.Strings;

[TestClass]
public sealed class StringStorageContractTest : BaseTestClass
{
	static IO.BitStream CreateBitReader(byte[] bytes) =>
		new(new MemoryStream(bytes), FileAccess.Read) { StreamMode = FileAccess.Read };

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int7)]
	[DataRow(StringStorageLengthPrefix.Int8)]
	[DataRow(StringStorageLengthPrefix.Int16)]
	[DataRow(StringStorageLengthPrefix.Int32)]
	public void PascalConstructor_VariableWidth_RejectsDescriptor(StringStorageLengthPrefix prefix)
	{
		var error = Assert.ThrowsExactly<ArgumentException>(() =>
			new StringStorage(StringStorageWidthType.UTF8, prefix));

		Assert.AreEqual("widthType", error.ParamName);
		Assert.Contains("characters, not bytes", error.Message);
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii)]
	[DataRow(StringStorageWidthType.Unicode)]
	[DataRow(StringStorageWidthType.UTF32)]
	public void PascalConstructor_FixedWidths_AcceptAllPrefixes(StringStorageWidthType width)
	{
		foreach (var prefix in new[] { StringStorageLengthPrefix.Int7, StringStorageLengthPrefix.Int8,
			StringStorageLengthPrefix.Int16, StringStorageLengthPrefix.Int32 })
		{
			var storage = new StringStorage(width, prefix);
			var encoding = StringStorageEncoding.TryAndGetStaticEncoding(storage, Shell.EndianFormat.Little);
			Assert.AreEqual(storage, encoding.Storage);
			Assert.AreEqual(Shell.EndianFormat.Little, encoding.ByteOrder);
		}
	}

	[TestMethod]
	public void StringStorageEncoding_IdentityIncludesByteOrder()
	{
		var storage = StringStorage.CStringUnicode;
		var little = StringStorageEncoding.TryAndGetStaticEncoding(storage, Shell.EndianFormat.Little);
		var sameLittle = StringStorageEncoding.TryAndGetStaticEncoding(storage, Shell.EndianFormat.Little);
		var big = StringStorageEncoding.TryAndGetStaticEncoding(storage, Shell.EndianFormat.Big);

		Assert.AreSame(little, sameLittle);
		Assert.AreNotSame(little, big);
		Assert.AreNotEqual(little, big);
		Assert.AreNotEqual(little.GetHashCode(), big.GetHashCode());
		Assert.AreNotEqual(0, little.CompareTo(big));
		Assert.AreEqual(storage, little.Storage);
		Assert.AreEqual(storage, big.Storage);
		Assert.AreEqual(Shell.EndianFormat.Little, little.ByteOrder);
		Assert.AreEqual(Shell.EndianFormat.Big, big.ByteOrder);
	}

	[TestMethod]
	public void StringStorageEncoding_StaticByteCount_IsIndependentOfByteOrder()
	{
		var storage = new StringStorage(
			StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32);
		const string text = "A\u00E9";

		Assert.AreEqual(8, StringStorageEncoding.GetByteCount(storage, text));
		Assert.AreEqual(
			new StringStorageEncoding(storage, Shell.EndianFormat.Little).GetByteCount(text),
			new StringStorageEncoding(storage, Shell.EndianFormat.Big).GetByteCount(text));
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int8, Shell.EndianFormat.Little, "AB", "024142")]
	[DataRow(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int16, Shell.EndianFormat.Little, "AB", "02004142")]
	[DataRow(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big, "AB", "000000024142")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int7, Shell.EndianFormat.Little, "A\u00E9", "024100E900")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int8, Shell.EndianFormat.Little, "A\u00E9", "024100E900")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int16, Shell.EndianFormat.Little, "A\u00E9", "02004100E900")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int16, Shell.EndianFormat.Big, "A\u00E9", "0002004100E9")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Little, "A\u00E9", "020000004100E900")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big, "A\u00E9", "00000002004100E9")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int8, Shell.EndianFormat.Big, "\U0001F642", "02D83DDE42")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int8, Shell.EndianFormat.Little, "A", "0141000000")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int16, Shell.EndianFormat.Big, "A", "000100000041")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int7, Shell.EndianFormat.Big, "\U0001F642", "010001F642")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int8, Shell.EndianFormat.Little, "\U0001F642", "0142F60100")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int16, Shell.EndianFormat.Big, "\U0001F642", "00010001F642")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big, "A\U0001F642", "00000002000000410001F642")]
	[DataRow(StringStorageWidthType.UTF32, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Little, "", "00000000")]
	[DataRow(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big, "", "00000000")]
	public void Pascal_FixedWidthRecords_UseCharacterCountsAndEncodingOrder(
		StringStorageWidthType width, StringStorageLengthPrefix prefix, Shell.EndianFormat order,
		string text, string expectedHex)
	{
		AssertPascalRecord(new StringStorage(width, prefix), order, text, Convert.FromHexString(expectedHex));
	}

	[TestMethod]
	[DataRow(0, "00")]
	[DataRow(1, "01")]
	[DataRow(63, "3F")]
	[DataRow(64, "40")]
	[DataRow(127, "7F")]
	[DataRow(128, "8001")]
	[DataRow(16383, "FF7F")]
	[DataRow(16384, "808001")]
	public void PascalInt7_UnicodeBoundaryCounts_DoNotSizePrefixFromPayloadBytes(int count, string prefixHex)
	{
		byte[] prefix = Convert.FromHexString(prefixHex);
		var expected = new byte[prefix.Length + count * 2];
		prefix.CopyTo(expected, 0);
		// Each ASCII-range UTF-16 code unit is 00 41 in the big-endian payload.
		for (int index = prefix.Length + 1; index < expected.Length; index += 2)
			expected[index] = 0x41;

		AssertPascalRecord(new StringStorage(StringStorageWidthType.Unicode,
			StringStorageLengthPrefix.Int7), Shell.EndianFormat.Big, new string('A', count), expected);
	}

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int7, 127, "7F")]
	[DataRow(StringStorageLengthPrefix.Int7, 128, "8001")]
	[DataRow(StringStorageLengthPrefix.Int8, 128, "80")]
	[DataRow(StringStorageLengthPrefix.Int8, 255, "FF")]
	public void PascalUtf32_PrefixCountsSerializedScalars(StringStorageLengthPrefix prefixType, int count, string prefixHex)
	{
		byte[] prefix = Convert.FromHexString(prefixHex);
		ReadOnlySpan<byte> scalarBytes = [0x00, 0x01, 0xF6, 0x42];
		var expected = new byte[prefix.Length + count * scalarBytes.Length];
		prefix.CopyTo(expected, 0);
		var chars = new char[count * 2];
		for (int index = 0; index < count; index++)
		{
			chars[index * 2] = '\uD83D';
			chars[index * 2 + 1] = '\uDE42';
			scalarBytes.CopyTo(expected.AsSpan(prefix.Length + index * scalarBytes.Length));
		}

		AssertPascalRecord(new StringStorage(StringStorageWidthType.UTF32, prefixType),
			Shell.EndianFormat.Big, new string(chars), expected);
	}

	static void AssertPascalRecord(
		StringStorage storage, Shell.EndianFormat byteOrder, string text, byte[] expected)
	{
		var encoding = StringStorageEncoding.TryAndGetStaticEncoding(storage, byteOrder);
		char[] chars = text.ToCharArray();
		Assert.AreEqual(expected.Length, encoding.GetByteCount(chars, 0, chars.Length));
		Assert.IsGreaterThanOrEqualTo(expected.Length, encoding.GetMaxByteCount(chars.Length));
		CollectionAssert.AreEqual(expected, encoding.GetBytes(text));
		Assert.AreEqual(expected.Length, encoding.GetEncoder().GetByteCount(chars, 0, chars.Length, flush: true));

		foreach (bool stateful in new[] { false, true })
		{
			var destination = new byte[expected.Length + 2];
			Array.Fill(destination, (byte)0xCC);
			int count = stateful
				? encoding.GetEncoder().GetBytes(chars, 0, chars.Length, destination, 1, flush: true)
				: encoding.GetBytes(chars, 0, chars.Length, destination, 1);
			Assert.AreEqual(expected.Length, count);
			CollectionAssert.AreEqual(expected, destination[1..^1]);
			Assert.AreEqual((byte)0xCC, destination[0]);
			Assert.AreEqual((byte)0xCC, destination[^1]);
			Assert.AreEqual(text, encoding.GetString(destination, 1, count));
			Assert.AreEqual(text.Length, encoding.GetCharCount(destination, 1, count));
			var decoded = new char[text.Length + 2];
			Array.Fill(decoded, '!');
			var decoder = encoding.GetDecoder();
			Assert.AreEqual(text.Length, decoder.GetCharCount(destination, 1, count, flush: true));
			int decodedCount = stateful
				? decoder.GetChars(destination, 1, count, decoded, 1, flush: true)
				: encoding.GetChars(destination, 1, count, decoded, 1);
			Assert.AreEqual(text.Length, decodedCount);
			Assert.AreEqual(text, new string(decoded, 1, decodedCount));
			Assert.AreEqual('!', decoded[0]);
			Assert.AreEqual('!', decoded[^1]);
		}

		using var stream = new MemoryStream();
		using var writer = new IO.EndianWriter(stream,
			byteOrder == Shell.EndianFormat.Big ? Shell.EndianFormat.Little : Shell.EndianFormat.Big)
		{ BaseStreamOwner = false };
		writer.Write(text.AsSpan(), encoding);
		CollectionAssert.AreEqual(expected, stream.ToArray());
		writer.Write((byte)0xCC);
		stream.Position = 0;
		using var reader = new IO.EndianReader(stream, writer.ByteOrder) { BaseStreamOwner = false };
		Assert.AreEqual(text, reader.ReadString(encoding));
		Assert.AreEqual((long)expected.Length, stream.Position);
		Assert.AreEqual((byte)0xCC, reader.ReadByte());

		using var bitOutput = new MemoryStream();
		using (var bits = new IO.BitStream(bitOutput, FileAccess.Write) { StreamMode = FileAccess.Write })
		{
			bits.Write(text, encoding);
		}
		CollectionAssert.AreEqual(expected, bitOutput.ToArray());
	}

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int8, 255)]
	[DataRow(StringStorageLengthPrefix.Int16, 32767)]
	public void PascalWrite_PrefixOverflow_RejectsBeforeWriting(StringStorageLengthPrefix prefix, int maxCount)
	{
		var encoding = new StringStorageEncoding(
			new StringStorage(StringStorageWidthType.Ascii, prefix), Shell.EndianFormat.Little);
		char[] chars = new string('A', maxCount + 1).ToCharArray();
		var destination = new byte[chars.Length + 8];
		Array.Fill(destination, (byte)0xCC);

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => encoding.GetByteCount(chars, 0, chars.Length));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			encoding.GetEncoder().GetByteCount(chars, 0, chars.Length, flush: true));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => encoding.GetBytes(chars, 0, chars.Length, destination, 0));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			encoding.GetEncoder().GetBytes(chars, 0, chars.Length, destination, 0, flush: true));
		Assert.IsFalse(destination.AsSpan().ContainsAnyExcept((byte)0xCC));
		using var stream = new MemoryStream();
		using var writer = new IO.EndianWriter(stream) { BaseStreamOwner = false };
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => writer.Write(chars.AsSpan(), encoding));
		Assert.AreEqual(0L, stream.Length);
		using (var bits = new IO.BitStream(stream, FileAccess.Write) { StreamMode = FileAccess.Write })
		{
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Write(new string(chars), encoding));
		}
		Assert.AreEqual(0, stream.ToArray().Length);

		char[] accepted = chars[..maxCount];
		Assert.AreEqual(maxCount + (prefix == StringStorageLengthPrefix.Int8 ? 1 : 2),
			encoding.GetBytes(accepted).Length);
	}

	[TestMethod]
	[DataRow(StringStorageType.CString, "41424300")]
	[DataRow(StringStorageType.CharArray, "41424344")]
	public void Encoder_FixedFields_ClampsToPayloadCapacity(StringStorageType type, string expectedHex)
	{
		var encoding = new StringStorageEncoding(
			new StringStorage(StringStorageWidthType.Ascii, type, 4), Shell.EndianFormat.Little);
		char[] chars = "ABCDE".ToCharArray();
		var encoder = encoding.GetEncoder();
		Assert.AreEqual(4, encoder.GetByteCount(chars, 0, chars.Length, flush: true));
		byte[] destination = [0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC];
		Assert.AreEqual(4, encoder.GetBytes(chars, 0, chars.Length, destination, 1, flush: true));
		CollectionAssert.AreEqual(Convert.FromHexString(expectedHex), destination[1..^1]);
		Assert.AreEqual((byte)0xCC, destination[0]);
		Assert.AreEqual((byte)0xCC, destination[^1]);
	}

	[TestMethod]
	public void EncodeStringSpan_ClearsFixedFieldTailAndReportsPayloadAndExtent()
	{
		var storage = new StringStorage(StringStorageWidthType.Unicode, StringStorageType.CString, 4);
		var encoding = new StringStorageEncoding(storage, Shell.EndianFormat.Big);
		var destination = new byte[10];
		Array.Fill(destination, (byte)0xCC);

		int extent = encoding.EncodeString("A", destination.AsSpan(1, 8), out int payloadByteCount);

		Assert.AreEqual(2, payloadByteCount);
		Assert.AreEqual(8, extent);
		CollectionAssert.AreEqual(
			Convert.FromHexString("CC0041000000000000CC"),
			destination);
	}

	[TestMethod]
	[DataRow((short)4)]
	[DataRow((short)129)]
	public void StreamWrites_ClearCompleteFixedFieldAcrossBufferStrategies(short capacity)
	{
		var storage = new StringStorage(StringStorageWidthType.Unicode, StringStorageType.CString, capacity);
		var expected = new byte[capacity * sizeof(char)];
		expected[1] = 0x41;

		using var endianOutput = new MemoryStream();
		using (var writer = new IO.EndianWriter(endianOutput, Shell.EndianFormat.Big)
			{ BaseStreamOwner = false })
		{
			writer.Write("A".AsSpan(), storage);
		}
		CollectionAssert.AreEqual(expected, endianOutput.ToArray());

		using var bitOutput = new MemoryStream();
		using (var bits = new IO.BitStream(bitOutput, FileAccess.Write) { StreamMode = FileAccess.Write })
		{
			bits.Write("A", storage, Shell.EndianFormat.Big);
			bits.Flush();
		}
		CollectionAssert.AreEqual(expected, bitOutput.ToArray());
	}

	[TestMethod]
	[DataRow((short)16)]
	[DataRow((short)128)]
	[DataRow((short)129)]
	public void KnownLengthReads_PreserveRecordsAcrossBufferStrategies(short capacity)
	{
		string fullText = new('A', capacity);
		string paddedText = new('A', capacity / 2);
		var cases = new[] {
			(Storage: new StringStorage(StringStorageWidthType.Unicode, StringStorageType.CString,
				checked((short)(capacity + 1))), Text: paddedText, ExplicitLength: TypeExtensions.kNone),
			(Storage: StringStorage.CStringUnicode, Text: fullText, ExplicitLength: fullText.Length),
			(Storage: new StringStorage(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int32),
				Text: fullText, ExplicitLength: TypeExtensions.kNone),
			(Storage: new StringStorage(StringStorageWidthType.Unicode, StringStorageType.CharArray, capacity),
				Text: paddedText, ExplicitLength: TypeExtensions.kNone),
		};

		foreach (var testCase in cases)
		{
			var encoding = new StringStorageEncoding(testCase.Storage, Shell.EndianFormat.Big);
			byte[] record = encoding.GetBytes(testCase.Text);
			byte[] input = [.. record, 0xCC];

			using (var reader = new IO.EndianReader(
				new MemoryStream(input), Shell.EndianFormat.Big))
			{
				string actual = testCase.ExplicitLength.IsNone()
					? reader.ReadString(encoding)
					: reader.ReadString(encoding, testCase.ExplicitLength);
				Assert.AreEqual(testCase.Text, actual);
				Assert.AreEqual((long)record.Length, reader.BaseStream.Position);
				Assert.AreEqual((byte)0xCC, reader.ReadByte());
			}

			using var bits = CreateBitReader(input);
			string bitActual = testCase.ExplicitLength.IsNone()
				? bits.ReadString(encoding)
				: bits.ReadString(encoding, testCase.ExplicitLength);
			Assert.AreEqual(testCase.Text, bitActual);
			Assert.AreEqual((byte)0xCC, bits.ReadByte());
		}
	}

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int8, "02", 1)]
	[DataRow(StringStorageLengthPrefix.Int16, "0002", 2)]
	[DataRow(StringStorageLengthPrefix.Int32, "00000002", 4)]
	public void PascalDecode_TruncatedSuppliedRange_DoesNotReadOutsideIt(
		StringStorageLengthPrefix prefix, string prefixHex, int prefixBytes)
	{
		byte[] bytes = [.. Convert.FromHexString(prefixHex), 0x41, 0x42];
		var encoding = new StringStorageEncoding(
			new StringStorage(StringStorageWidthType.Ascii, prefix), Shell.EndianFormat.Big);

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => encoding.GetString(bytes, 0, prefixBytes + 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			encoding.GetDecoder().GetCharCount(bytes, 0, prefixBytes + 1, flush: true));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => encoding.GetString(bytes, 0, prefixBytes - 1));
	}

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int16, StringStorageWidthType.Ascii, "FFFF")]
	[DataRow(StringStorageLengthPrefix.Int32, StringStorageWidthType.Ascii, "FFFFFFFF")]
	[DataRow(StringStorageLengthPrefix.Int32, StringStorageWidthType.Unicode, "40000000")]
	[DataRow(StringStorageLengthPrefix.Int32, StringStorageWidthType.UTF32, "20000000")]
	[DataRow(StringStorageLengthPrefix.Int7, StringStorageWidthType.Ascii, "FFFFFFFF0F")]
	public void PascalRead_InvalidOrOverflowingCount_RejectsHeader(
		StringStorageLengthPrefix prefix, StringStorageWidthType width, string hex)
	{
		byte[] bytes = Convert.FromHexString(hex);
		var encoding = new StringStorageEncoding(new StringStorage(width, prefix), Shell.EndianFormat.Big);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => encoding.GetString(bytes));
		using var stream = new MemoryStream(bytes);
		using var reader = new IO.EndianReader(stream);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => reader.ReadString(encoding));
		Assert.AreEqual((long)bytes.Length, stream.Position);
	}

	[TestMethod]
	public void PascalInt7_StreamPrefixUsesBinaryReaderSemantics()
	{
		var storage = new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int7);
		using (var empty = new IO.EndianReader(new MemoryStream([0x80, 0x80, 0x80, 0x80, 0x00, 0xCC])))
		{
			Assert.AreEqual(string.Empty, empty.ReadString(storage));
			Assert.AreEqual((byte)0xCC, empty.ReadByte());
		}
		using var oversized = new IO.EndianReader(new MemoryStream([0x80, 0x80, 0x80, 0x80, 0x01]));
		Assert.ThrowsExactly<EndOfStreamException>(() => oversized.ReadString(storage));
		Assert.AreEqual(5L, oversized.BaseStream.Position);
		using var malformed = new IO.EndianReader(new MemoryStream([0x80, 0x80, 0x80, 0x80, 0x10]));
		Assert.ThrowsExactly<FormatException>(() => malformed.ReadString(storage));
		Assert.AreEqual(5L, malformed.BaseStream.Position);
	}

	[TestMethod]
	[DataRow(StringStorageType.CString, (short)2, "\U0001F642A", "\U0001F642", "0001F64200000000")]
	[DataRow(StringStorageType.CString, (short)3, "A\U0001F642B", "A\U0001F642", "000000410001F64200000000")]
	[DataRow(StringStorageType.CString, (short)1, "\U0001F642", "", "00000000")]
	[DataRow(StringStorageType.CharArray, (short)2, "\U0001F642AB", "\U0001F642A", "0001F64200000041")]
	[DataRow(StringStorageType.CharArray, (short)2, "\U0001F642", "\U0001F642", "0001F64200000000")]
	[DataRow(StringStorageType.CharArray, (short)1, "\U0001F642A", "\U0001F642", "0001F642")]
	public void Utf32_FixedFieldCapacity_CountsSerializedScalars(
		StringStorageType type, short capacity, string input, string expectedText, string expectedHex)
	{
		var storage = new StringStorage(StringStorageWidthType.UTF32, type, capacity);
		var encoding = new StringStorageEncoding(storage, Shell.EndianFormat.Big);
		byte[] expected = Convert.FromHexString(expectedHex);
		Assert.AreEqual(expected.Length, encoding.GetByteCount(input));
		CollectionAssert.AreEqual(expected, encoding.GetBytes(input));

		char[] chars = input.ToCharArray();
		var encoder = encoding.GetEncoder();
		Assert.AreEqual(expected.Length, encoder.GetByteCount(chars, 0, chars.Length, flush: true));
		// Caller-owned buffers still require initialized padding when using the direct Encoder API.
		var destination = new byte[expected.Length];
		int written = encoder.GetBytes(chars, 0, chars.Length, destination, 0, flush: true);
		int payloadBytes = new System.Text.UTF32Encoding(true, false).GetByteCount(expectedText);
		Assert.AreEqual(payloadBytes + (type == StringStorageType.CString ? 4 : 0), written);
		CollectionAssert.AreEqual(expected, destination);

		using var output = new MemoryStream();
		using var writer = new IO.EndianWriter(output, Shell.EndianFormat.Big) { BaseStreamOwner = false };
		writer.Write(input.AsSpan(), storage);
		CollectionAssert.AreEqual(expected, output.ToArray());
		using var reader = new IO.EndianReader(new MemoryStream([.. expected, 0xCC]), Shell.EndianFormat.Big);
		Assert.AreEqual(expectedText, reader.ReadString(storage));
		Assert.AreEqual((long)expected.Length, reader.BaseStream.Position);
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
		using var bitOutput = new MemoryStream();
		using (var bits = new IO.BitStream(bitOutput, FileAccess.Write) { StreamMode = FileAccess.Write })
			bits.Write(input, storage, Shell.EndianFormat.Big);
		CollectionAssert.AreEqual(expected, bitOutput.ToArray());
		using var bitReader = CreateBitReader([.. expected, 0xCC]);
		Assert.AreEqual(expectedText, bitReader.ReadString(storage, Shell.EndianFormat.Big));
		Assert.AreEqual((byte)0xCC, bitReader.ReadByte());
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii, "4100")]
	[DataRow(StringStorageWidthType.Unicode, "00410000")]
	[DataRow(StringStorageWidthType.UTF32, "0000004100000000")]
	public void CharArray_Padding_PreservesTheFirstPayloadUnit(StringStorageWidthType width, string hex)
	{
		var storage = new StringStorage(width, StringStorageType.CharArray, 2);
		byte[] bytes = [.. Convert.FromHexString(hex), 0xCC];
		using var reader = new IO.EndianReader(new MemoryStream(bytes), Shell.EndianFormat.Big);
		Assert.AreEqual("A", reader.ReadString(storage));
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
		using var bits = CreateBitReader(bytes);
		Assert.AreEqual("A", bits.ReadString(storage, Shell.EndianFormat.Big));
		Assert.AreEqual((byte)0xCC, bits.ReadByte());
	}

	[TestMethod]
	public void Utf32_CStringBitLimit_CountsSerializedScalars()
	{
		var storage = new StringStorage(StringStorageWidthType.UTF32, StringStorageType.CString);
		byte[] expected = [0x00, 0x01, 0xF6, 0x42, 0x00, 0x00, 0x00, 0x00];
		using var output = new MemoryStream();
		using (var bits = new IO.BitStream(output, FileAccess.Write) { StreamMode = FileAccess.Write })
			bits.Write("\U0001F642A", storage, Shell.EndianFormat.Big, maxLength: 1);
		CollectionAssert.AreEqual(expected, output.ToArray());
		using var reader = CreateBitReader([.. expected, 0xCC]);
		Assert.AreEqual("\U0001F642",
			reader.ReadString(storage, Shell.EndianFormat.Big, maxLength: 1));
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii, "AB", "A", "41000000")]
	[DataRow(StringStorageWidthType.Unicode, "AB", "A", "0041000000000000")]
	[DataRow(StringStorageWidthType.UTF32, "\U0001F642A", "\U0001F642", "0001F642000000000000000000000000")]
	public void BitStream_FixedCStringLimit_PreservesFieldExtent(
		StringStorageWidthType width, string input, string expectedText, string expectedHex)
	{
		var storage = new StringStorage(width, StringStorageType.CString, 4);
		byte[] expected = Convert.FromHexString(expectedHex);
		using var output = new MemoryStream();
		using (var writer = new IO.BitStream(output, FileAccess.Write) { StreamMode = FileAccess.Write })
			writer.Write(input, storage, Shell.EndianFormat.Big, maxLength: 1);
		CollectionAssert.AreEqual(expected, output.ToArray());

		using var reader = CreateBitReader([.. expected, 0xCC]);
		Assert.AreEqual(expectedText,
			reader.ReadString(storage, Shell.EndianFormat.Big, maxLength: 1));
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
	}

	[TestMethod]
	public void Utf32_UnfixedCString_SupplementaryScalarRemainsSupported()
	{
		var storage = new StringStorage(StringStorageWidthType.UTF32, StringStorageType.CString);
		var encoding = new StringStorageEncoding(storage, Shell.EndianFormat.Big);
		byte[] expected = [0x00, 0x01, 0xF6, 0x42, 0x00, 0x00, 0x00, 0x00];
		CollectionAssert.AreEqual(expected, encoding.GetBytes("\U0001F642"));
		using var reader = new IO.EndianReader(new MemoryStream(expected), Shell.EndianFormat.Big);
		Assert.AreEqual("\U0001F642", reader.ReadString(storage));
	}

	[TestMethod]
	public void Utf8_CharacterCountedReads_RejectBeforeConsumingInput()
	{
		byte[] bytes = [0xC3, 0xA9, 0x00];
		foreach (var storage in new[] { StringStorage.CStringUtf8, StringStorage.Utf8String })
		{
			using var stream = new MemoryStream(bytes);
			using var reader = new IO.EndianReader(stream) { BaseStreamOwner = false };
			Assert.ThrowsExactly<NotSupportedException>(() => reader.ReadString(storage, 1));
			Assert.AreEqual(0L, stream.Position);
			using var bits = new IO.BitStream(stream, FileAccess.Read) { StreamMode = FileAccess.Read };
			Assert.ThrowsExactly<NotSupportedException>(() =>
				bits.ReadString(storage, Shell.EndianFormat.Little, length: 1));
			Assert.AreEqual((byte)0xC3, bits.ReadByte());
		}
		using var scanReader = new IO.EndianReader(new MemoryStream(bytes));
		Assert.AreEqual("\u00E9", scanReader.ReadString(StringStorage.CStringUtf8));
		Assert.AreEqual("\u00E9",
			new StringStorageEncoding(StringStorage.Utf8String, Shell.EndianFormat.Little).GetString(bytes, 0, 2));
	}

	[TestMethod]
	public void BitStream_UnsupportedReadCapabilities_RejectBeforeConsumingInput()
	{
		var storages = new[] {
			new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int7),
			new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int16),
			new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int32),
		};
		foreach (var storage in storages)
		{
			using var bits = CreateBitReader([0xCC, 0x00, 0x00, 0x00]);
			Assert.ThrowsExactly<NotSupportedException>(() =>
				bits.ReadString(storage, Shell.EndianFormat.Little));
			Assert.AreEqual((byte)0xCC, bits.ReadByte());
		}
		using var utf8 = CreateBitReader([0xC3, 0xA9, 0x00]);
		Assert.ThrowsExactly<NotSupportedException>(() =>
			utf8.ReadString(StringStorage.CStringUtf8, Shell.EndianFormat.Little, maxLength: 1));
		Assert.AreEqual((byte)0xC3, utf8.ReadByte());
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Unicode, StringStorageType.CString)]
	[DataRow(StringStorageWidthType.UTF32, StringStorageType.CharArray)]
	public void BitStream_ByteOrderSensitivePayloads_RequireExplicitOrder(
		StringStorageWidthType width, StringStorageType type)
	{
		var storage = new StringStorage(width, type, type == StringStorageType.CharArray ? (short)1 : (short)0);
		using var stream = new MemoryStream(new byte[8]);
		using var bits = new IO.BitStream(stream, FileAccess.Read) { StreamMode = FileAccess.Read };
		long streamPosition = stream.Position;

		Assert.ThrowsExactly<NotSupportedException>(() => bits.ReadString(storage));
		Assert.AreEqual(streamPosition, stream.Position);

		using var output = new MemoryStream();
		using var writer = new IO.BitStream(output, FileAccess.Write) { StreamMode = FileAccess.Write };
		Assert.ThrowsExactly<NotSupportedException>(() => writer.Write("A", storage));
		Assert.AreEqual(0L, output.Length);
	}

	[TestMethod]
	[DataRow(StringStorageLengthPrefix.Int8, "024142")]
	[DataRow(StringStorageLengthPrefix.Int16, "00024142")]
	[DataRow(StringStorageLengthPrefix.Int32, "000000024142")]
	public void BitStream_SupportedPascalPrefixes_PreserveNextField(StringStorageLengthPrefix prefix, string hex)
	{
		var storage = new StringStorage(StringStorageWidthType.Ascii, prefix);
		using var bits = CreateBitReader([.. Convert.FromHexString(hex), 0xCC]);
		Assert.AreEqual("AB", bits.ReadString(storage, Shell.EndianFormat.Big));
		Assert.AreEqual((byte)0xCC, bits.ReadByte());
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(17)]
	public void BitStream_InvalidPrefixBitCount_RejectsBeforeConsumingInput(int bitCount)
	{
		var storage = new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int16);
		using var bits = CreateBitReader([0xCC, 0x00, 0x00, 0x00]);
		AssertThrowsArgumentOutOfRange("prefixBitLength", () =>
			bits.ReadString(storage, Shell.EndianFormat.Big, prefixBitLength: bitCount));
		Assert.AreEqual((byte)0xCC, bits.ReadByte());
	}

	[TestMethod]
	[DataRow(4, "24142CC0")]
	[DataRow(8, "024142CC")]
	public void BitStream_SubByteAndBytePrefixes_DoNotRequireBigEndian(int bitCount, string hex)
	{
		var storage = new StringStorage(StringStorageWidthType.Ascii, StringStorageLengthPrefix.Int16);
		using var bits = CreateBitReader(Convert.FromHexString(hex));
		Assert.AreEqual("AB",
			bits.ReadString(storage, Shell.EndianFormat.Little, prefixBitLength: bitCount));
		Assert.AreEqual((byte)0xCC, bits.ReadByte());
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "41004200", false)]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "41004200", true)]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "00410042", false)]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "00410042", true)]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "4100000042000000", false)]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "4100000042000000", true)]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "0000004100000042", false)]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "0000004100000042", true)]
	public void CString_ExplicitEncodingOrder_OverridesReaderOrder(
		StringStorageWidthType width, Shell.EndianFormat encodingOrder, Shell.EndianFormat readerOrder,
		string payloadHex, bool fixedField)
	{
		byte[] payload = Convert.FromHexString(payloadHex);
		int unitSize = payload.Length / 2;
		int recordSize = payload.Length + unitSize * (fixedField ? 2 : 1);
		var bytes = new byte[recordSize + 1];
		payload.CopyTo(bytes, 0);
		bytes[^1] = 0xCC;
		byte[] original = (byte[])bytes.Clone();
		var storage = new StringStorage(width, StringStorageType.CString, (short)(fixedField ? 4 : 0));
		var encoding = new StringStorageEncoding(storage, encodingOrder);
		using var reader = new IO.EndianReader(new MemoryStream(bytes), readerOrder);

		Assert.AreEqual("AB", reader.ReadString(encoding));
		Assert.AreEqual((long)recordSize, reader.BaseStream.Position);
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
		CollectionAssert.AreEqual(original, bytes);
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "410042000000")]
	[DataRow(StringStorageWidthType.Unicode, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "004100420000")]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Little, Shell.EndianFormat.Big, "410000004200000000000000")]
	[DataRow(StringStorageWidthType.UTF32, Shell.EndianFormat.Big, Shell.EndianFormat.Little, "000000410000004200000000")]
	public void CharArray_ExplicitEncodingOrder_DecodesEveryPayloadUnitConsistently(
		StringStorageWidthType width, Shell.EndianFormat encodingOrder, Shell.EndianFormat readerOrder, string fieldHex)
	{
		byte[] field = Convert.FromHexString(fieldHex);
		byte[] bytes = [.. field, 0xCC];
		byte[] original = (byte[])bytes.Clone();
		var storage = new StringStorage(width, StringStorageType.CharArray, 3);
		var encoding = new StringStorageEncoding(storage, encodingOrder);
		using var reader = new IO.EndianReader(new MemoryStream(bytes), readerOrder);

		Assert.AreEqual("AB", reader.ReadString(encoding));
		Assert.AreEqual((long)field.Length, reader.BaseStream.Position);
		Assert.AreEqual((byte)0xCC, reader.ReadByte());
		CollectionAssert.AreEqual(original, bytes);
	}

	[TestMethod]
	public void EndianReader_ShortReads_FillPayloadAndRejectIncompleteRecords()
	{
		var pascal = new StringStorage(StringStorageWidthType.Unicode, StringStorageLengthPrefix.Int16);
		using (var reader = new IO.EndianReader(
			new ShortReadStream([0x00, 0x02, 0x00, 0x41, 0x00, 0x42, 0xCC]), Shell.EndianFormat.Big))
		{
			Assert.AreEqual("AB", reader.ReadString(pascal));
			Assert.AreEqual((byte)0xCC, reader.ReadByte());
		}
		using (var reader = new IO.EndianReader(
			new ShortReadStream([0x00, 0x02, 0x00, 0x41, 0x00]), Shell.EndianFormat.Big))
			Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadString(pascal));

		using (var reader = new IO.EndianReader(new ShortReadStream([0x00, 0x41, 0x00, 0x00, 0xCC]), Shell.EndianFormat.Big))
		{
			Assert.AreEqual("A", reader.ReadString(StringStorage.CStringUnicode));
			Assert.AreEqual((byte)0xCC, reader.ReadByte());
		}
		using (var reader = new IO.EndianReader(new ShortReadStream([0x00, 0x41, 0x00]), Shell.EndianFormat.Big))
			Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadString(StringStorage.CStringUnicode));

		var fixedStorage = new StringStorage(StringStorageWidthType.Ascii, StringStorageType.CString, 4);
		using (var reader = new IO.EndianReader(new ShortReadStream([0x41, 0x00])))
			Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadString(fixedStorage));
	}

	[TestMethod]
	public void CString_ExplicitLength_RequiresTheTerminator()
	{
		using (var reader = new IO.EndianReader(new MemoryStream([0x41])))
		{
			Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadString(StringStorage.CStringAscii, 1));
			Assert.AreEqual(1L, reader.BaseStream.Position);
		}
		using (var reader = new IO.EndianReader(new MemoryStream([0x41, 0x42])))
		{
			Assert.ThrowsExactly<InvalidDataException>(() => reader.ReadString(StringStorage.CStringAscii, 1));
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}
		using var bits = CreateBitReader([0x41, 0x42]);
		Assert.ThrowsExactly<InvalidDataException>(() =>
			bits.ReadString(StringStorage.CStringAscii, Shell.EndianFormat.Little, length: 1));
		Assert.AreEqual(16L, bits.BitPosition);
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii, "4142")]
	[DataRow(StringStorageWidthType.Unicode, "00410042")]
	[DataRow(StringStorageWidthType.UTF32, "0000004100000042")]
	public void CString_FixedField_RejectsMissingTerminator(StringStorageWidthType width, string hex)
	{
		var storage = new StringStorage(width, StringStorageType.CString, 2);
		byte[] bytes = Convert.FromHexString(hex);
		var encoding = new StringStorageEncoding(storage, Shell.EndianFormat.Big);
		Assert.ThrowsExactly<InvalidDataException>(() => encoding.GetString(bytes));
		using var reader = new IO.EndianReader(new MemoryStream(bytes), Shell.EndianFormat.Big);
		Assert.ThrowsExactly<InvalidDataException>(() => reader.ReadString(storage));
		using var bits = CreateBitReader(bytes);
		Assert.ThrowsExactly<InvalidDataException>(() =>
			bits.ReadString(storage, Shell.EndianFormat.Big));
	}

	[TestMethod]
	[DataRow(StringStorageWidthType.Ascii, "414200", "4100")]
	[DataRow(StringStorageWidthType.Unicode, "004100420000", "00410000")]
	public void BitStream_CStringMaximum_RequiresTerminationWithinLimit(
		StringStorageWidthType width, string oversizedHex, string validHex)
	{
		var storage = new StringStorage(width, StringStorageType.CString);
		using (var bits = CreateBitReader(Convert.FromHexString(oversizedHex)))
			Assert.ThrowsExactly<InvalidDataException>(() =>
				bits.ReadString(storage, Shell.EndianFormat.Big, maxLength: 1));
		using var validBits = CreateBitReader([.. Convert.FromHexString(validHex), 0xCC]);
		Assert.AreEqual("A",
			validBits.ReadString(storage, Shell.EndianFormat.Big, maxLength: 1));
		Assert.AreEqual((byte)0xCC, validBits.ReadByte());
	}

	sealed class ShortReadStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override bool CanSeek => false;
		public override int Read(Span<byte> buffer) => base.Read(buffer[..Math.Min(buffer.Length, 1)]);
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
	}
}
