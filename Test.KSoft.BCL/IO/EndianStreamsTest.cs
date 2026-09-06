using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test;

[TestClass]
public class EndianStreamsTest : BaseTestClass
{
	enum TestEnum : byte
	{
		None,
	}

	struct TestStructStreamable : IEndianStreamable
	{
		public void Read(EndianReader s)
		{
		}

		public void Write(EndianWriter s)
		{
		}
	}

	struct TestStructSerializable : IEndianStreamSerializable
	{
		public void Serialize(EndianStream s)
		{
		}
	}

	sealed class TestClassStreamable : IEndianStreamable
	{
		public void Read(EndianReader s)
		{
		}

		public void Write(EndianWriter s)
		{
		}
	}

	sealed class TestClassSerializable : IEndianStreamSerializable
	{
		public void Serialize(EndianStream s)
		{
		}
	}

	sealed class WriteLimitStream : MemoryStream
	{
		int mRemainingBytes;

		public WriteLimitStream(int byteLimit)
		{
			mRemainingBytes = byteLimit;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Write(buffer.AsSpan(offset, count));
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			int count = Math.Min(mRemainingBytes, buffer.Length);
			byte[] bytes = buffer[..count].ToArray();
			base.Write(bytes, 0, bytes.Length);
			mRemainingBytes -= count;

			if (count != buffer.Length)
				throw new IOException("Test stream write limit reached.");
		}

		public override void WriteByte(byte value)
		{
			if (mRemainingBytes == 0)
				throw new IOException("Test stream write limit reached.");

			base.WriteByte(value);
			mRemainingBytes--;
		}
	}

	static void ReadTag32WithUndersizedSpan(EndianReader reader)
	{
		Span<char> tag = stackalloc char[3];
		reader.ReadTag32(tag);
	}

	static void ReadTag64WithUndersizedSpan(EndianReader reader)
	{
		Span<char> tag = stackalloc char[7];
		reader.ReadTag64(tag);
	}

	static void WriteTag32WithUndersizedSpan(EndianWriter writer)
	{
		ReadOnlySpan<char> tag = "ABC";
		writer.WriteTag32(tag);
	}

	static void WriteTag32WithOversizedSpan(EndianWriter writer)
	{
		ReadOnlySpan<char> tag = "ABCDE";
		writer.WriteTag32(tag);
	}

	static EndianStream UnusedStreamArrayValue(ref TestStructSerializable value)
	{
		throw new InvalidOperationException("The zero-count test path should not invoke the stream delegate.");
	}

	[TestMethod]
	public void Constructors_NullEndianReaderInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("input", () => _ = new EndianReader(null!));
		AssertThrowsArgumentNull("input",
			() => _ = new EndianReader(null!, Encoding.UTF8, Shell.EndianFormat.Big));
		AssertThrowsArgumentNull("encoding",
			() => _ = new EndianReader(new MemoryStream(), null!, Shell.EndianFormat.Big));
	}

	[TestMethod]
	public void Constructors_NullEndianWriterInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("output", () => _ = new EndianWriter(null!));
		AssertThrowsArgumentNull("output",
			() => _ = new EndianWriter(null!, Encoding.UTF8, Shell.EndianFormat.Big));
		AssertThrowsArgumentNull("encoding",
			() => _ = new EndianWriter(new MemoryStream(), null!, Shell.EndianFormat.Big));
	}

	[TestMethod]
	public void Constructors_NullEndianStreamInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("baseStream", () => _ = new EndianStream(null!));
		AssertThrowsArgumentNull("baseStream",
			() => _ = new EndianStream(null!, Encoding.UTF8, Shell.EndianFormat.Big));
		AssertThrowsArgumentNull("encoding",
			() => _ = new EndianStream(new MemoryStream(), null!, Shell.EndianFormat.Big));
		AssertThrowsArgumentNull("reader", () => _ = EndianStream.UsingReader(null!));
		AssertThrowsArgumentNull("writer", () => _ = EndianStream.UsingWriter(null!));
	}

	[TestMethod]
	public void ReaderWriterDirectGuards_ThrowExpectedExceptions()
	{
		using var readStream = new MemoryStream(new byte[16]);
		using var reader = new EndianReader(readStream);
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream);

		AssertThrowsArgumentOutOfRange("byteCount", () => reader.Pad(0));
		AssertThrowsArgumentOutOfRange("byteCount", () => reader.Pad(-1));
		AssertThrowsArgumentOutOfRange("byteCount", () => writer.Pad(0));
		AssertThrowsArgumentOutOfRange("byteCount", () => writer.Pad(-1));

		AssertThrowsArgumentNull("encoding", () => _ = reader.ReadString((Text.StringStorageEncoding)null!, 0));
		AssertThrowsArgumentNull("encoding", () => _ = reader.ReadString((Text.StringStorageEncoding)null!));
		AssertThrowsArgumentNull("encoding", () => writer.Write("test".AsSpan(), (Text.StringStorageEncoding)null!));
		Assert.ThrowsExactly<InvalidDataException>(() =>
			reader.ReadString(Memory.Strings.StringStorage.AsciiString, TypeExtensions.kNone));
		AssertThrowsArgumentNull("implementation", () => _ = reader.Read<TestEnum>(null!));
		AssertThrowsArgumentNull("implementation", () => writer.Write(TestEnum.None, null!));
	}

	[TestMethod]
	public void WriterStringStorageSpans_WriteExactBoundedStorageBytes()
	{
		using var stream = new MemoryStream();
		using var writer = new EndianWriter(stream);

		writer.Write(".ABC!".AsSpan(1, 3),
			new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii,
				Memory.Strings.StringStorageType.CharArray));
		writer.Write(".\u00E9!".AsSpan(1, 1), Memory.Strings.StringStorage.CStringUtf8);
		writer.Write(".A\u00E9!".AsSpan(1, 2),
			new Text.StringStorageEncoding(new Memory.Strings.StringStorage(
				Memory.Strings.StringStorageWidthType.UTF8, Memory.Strings.StringStorageLengthPrefix.Int16)));
		writer.Write(".ABCDE!".AsSpan(1, 5),
			new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii,
				Memory.Strings.StringStorageType.CString, fixedLength: 4));

		CollectionAssert.AreEqual(
			new byte[] { 0x41, 0x42, 0x43, 0xC3, 0xA9, 0x00, 0x00, 0x02, 0x41, 0xC3, 0xA9, 0x41, 0x42, 0x43, 0x00 },
			stream.ToArray());
	}

	[TestMethod]
	public void EndianStreamStringStorageWriteBranches_NullValuesWriteEmptyStrings()
	{
		using var stream = new MemoryStream();
		using var writer = new EndianWriter(stream) { BaseStreamOwner = false };
		using var endianStream = EndianStream.UsingWriter(writer);
		string value = null!;
		var storage = Memory.Strings.StringStorage.CStringAscii;
		var encoding = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);

		endianStream.Stream(ref value, storage);
		endianStream.Stream(ref value, storage, length: 32);
		endianStream.Stream(ref value, encoding);
		endianStream.Stream(ref value, encoding, length: 32);

		CollectionAssert.AreEqual(new byte[4], stream.ToArray());
	}

	[TestMethod]
	public void ReaderDirectArrayAndTagSpanHelpers_PopulateExpectedBuffers()
	{
		using (var reader = new EndianReader(new MemoryStream(new byte[] { 1, 2, 3, 4 }), Shell.EndianFormat.Big))
		{
			var byteBuffer = new byte[3];
			Assert.AreEqual(2, reader.Read(byteBuffer.AsSpan(0, 2)));
			CollectionAssert.AreEqual(new byte[] { 1, 2, 0 }, byteBuffer);
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.UTF8.GetBytes("abcd")), Shell.EndianFormat.Big))
		{
			var charBuffer = new char[3];
			Assert.AreEqual(2, reader.Read(charBuffer.AsSpan(0, 2)));
			CollectionAssert.AreEqual(new[] { 'a', 'b', '\0' }, charBuffer);
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.ASCII.GetBytes("ABCD1234")), Shell.EndianFormat.Big))
		{
			var tag32 = new char[5];
			reader.ReadTag32(tag32.AsSpan());
			CollectionAssert.AreEqual(new[] { 'A', 'B', 'C', 'D', '\0' }, tag32);

			Span<char> secondTag32 = stackalloc char[4];
			reader.ReadTag32(secondTag32);
			Assert.AreEqual("1234", new string(secondTag32));
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.ASCII.GetBytes("ABCDEFGH")), Shell.EndianFormat.Big))
		{
			Span<char> tag64 = stackalloc char[8];
			reader.ReadTag64(tag64);
			Assert.AreEqual("ABCDEFGH", new string(tag64));
		}

		using (var reader = new EndianReader(new MemoryStream(new byte[] { 1, 0 }), Shell.EndianFormat.Big))
		{
			var values = new bool[2];
			reader.ReadFixedArray(values.AsSpan());
			CollectionAssert.AreEqual(new[] { true, false }, values);
		}
	}

	[TestMethod]
	public void Stream_SpanByteWholeBuffer_RoundTripsReadAndWrite()
	{
		byte[] source = { 0x11, 0x22, 0x33, 0x44 };
		byte[] written;

		using (var writeStream = new MemoryStream())
		{
			using (var writer = new EndianWriter(writeStream) { BaseStreamOwner = false })
			using (var writeEndianStream = EndianStream.UsingWriter(writer))
			{
				Assert.AreSame(writeEndianStream, writeEndianStream.Stream(new Span<byte>(source)));
			}

			Assert.AreEqual(source.Length, writeStream.Position);
			written = writeStream.ToArray();
		}
		CollectionAssert.AreEqual(source, written);

		var destination = new byte[source.Length];
		using (var reader = new EndianReader(new MemoryStream(written)))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			Assert.AreSame(readEndianStream, readEndianStream.Stream(destination.AsSpan()));
			Assert.AreEqual(source.Length, reader.BaseStream.Position);
		}
		CollectionAssert.AreEqual(source, destination);
	}

	[TestMethod]
	public void Stream_SpanCharWholeBuffer_RoundTripsReadAndWrite()
	{
		char[] source = { 'W', 'X', 'Y', 'Z' };
		byte[] written;

		using (var writeStream = new MemoryStream())
		{
			using (var writer = new EndianWriter(writeStream) { BaseStreamOwner = false })
			using (var writeEndianStream = EndianStream.UsingWriter(writer))
			{
				Assert.AreSame(writeEndianStream, writeEndianStream.Stream(new Span<char>(source)));
			}

			written = writeStream.ToArray();
		}
		CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(source), written);

		var destination = new char[source.Length];
		using (var reader = new EndianReader(new MemoryStream(written)))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			Assert.AreSame(readEndianStream, readEndianStream.Stream(destination.AsSpan()));
			Assert.AreEqual(written.Length, reader.BaseStream.Position);
		}
		CollectionAssert.AreEqual(source, destination);
	}

	[TestMethod]
	public void Stream_SpanBytePrefixSlice_LeavesDestinationSuffixUntouched()
	{
		using (var writeStream = new MemoryStream())
		{
			using (var writer = new EndianWriter(writeStream) { BaseStreamOwner = false })
			using (var writeEndianStream = EndianStream.UsingWriter(writer))
			{
				byte[] source = { 1, 2, 3, 4 };
				Assert.AreSame(writeEndianStream, writeEndianStream.Stream(source.AsSpan(0, 2)));
			}

			CollectionAssert.AreEqual(new byte[] { 1, 2 }, writeStream.ToArray());
		}

		using (var reader = new EndianReader(new MemoryStream(new byte[] { 9, 8, 7 })))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			var destination = new byte[] { 0, 0, 0, 0 };

			Assert.AreSame(readEndianStream, readEndianStream.Stream(destination.AsSpan(0, 2)));

			CollectionAssert.AreEqual(new byte[] { 9, 8, 0, 0 }, destination);
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Stream_SpanCharPrefixSlice_LeavesDestinationSuffixUntouched()
	{
		using (var writeStream = new MemoryStream())
		{
			using (var writer = new EndianWriter(writeStream) { BaseStreamOwner = false })
			using (var writeEndianStream = EndianStream.UsingWriter(writer))
			{
				char[] source = { 'A', 'B', 'C', 'D' };
				Assert.AreSame(writeEndianStream, writeEndianStream.Stream(source.AsSpan(0, 2)));
			}

			CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("AB"), writeStream.ToArray());
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.UTF8.GetBytes("XY"))))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			var destination = new[] { '\0', '\0', '\0', '\0' };

			Assert.AreSame(readEndianStream, readEndianStream.Stream(destination.AsSpan(0, 2)));

			CollectionAssert.AreEqual(new[] { 'X', 'Y', '\0', '\0' }, destination);
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Reader_SpanByteShortStream_ReturnsActualCountAndLeavesSuffixUntouched()
	{
		byte[] available = { 0xAA, 0xBB };
		const byte unreadSentinel = 0xCC;
		const int requestedCount = 4;

		// Inherited three-argument BinaryReader.Read is not an EndianReader-declared overload (no
		// KSoft shim applies) and defines the same partial-read contract as the Span overload.
		using (var inheritedReader = new EndianReader(new MemoryStream(available)))
		{
			var inheritedBuffer = new byte[requestedCount];
			Array.Fill(inheritedBuffer, unreadSentinel);

			int actualInherited = inheritedReader.Read(inheritedBuffer, 0, requestedCount);

			Assert.AreEqual(available.Length, actualInherited);
			CollectionAssert.AreEqual(available, inheritedBuffer[..actualInherited]);
			CollectionAssert.AreEqual(
				new byte[] { unreadSentinel, unreadSentinel }, inheritedBuffer[actualInherited..]);
			Assert.AreEqual(available.Length, inheritedReader.BaseStream.Position);
		}

		// Canonical Span<byte> path: same partial-read contract; the actual count is the direct return value.
		using (var spanReader = new EndianReader(new MemoryStream(available)))
		{
			var spanBuffer = new byte[requestedCount];
			Array.Fill(spanBuffer, unreadSentinel);

			int actualSpan = spanReader.Read(spanBuffer.AsSpan(0, requestedCount));

			Assert.AreEqual(available.Length, actualSpan);
			CollectionAssert.AreEqual(available, spanBuffer[..actualSpan]);
			CollectionAssert.AreEqual(
				new byte[] { unreadSentinel, unreadSentinel }, spanBuffer[actualSpan..]);
			Assert.AreEqual(available.Length, spanReader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Reader_SpanCharShortStream_ReturnsActualCountAndLeavesSuffixUntouched()
	{
		byte[] availableBytes = Encoding.UTF8.GetBytes("AB");
		char[] availableChars = { 'A', 'B' };
		const char unreadSentinel = '\u2603';
		const int requestedCount = 4;

		using (var inheritedReader = new EndianReader(new MemoryStream(availableBytes)))
		{
			var inheritedBuffer = new char[requestedCount];
			Array.Fill(inheritedBuffer, unreadSentinel);

			int actualInherited = inheritedReader.Read(inheritedBuffer, 0, requestedCount);

			Assert.AreEqual(availableChars.Length, actualInherited);
			CollectionAssert.AreEqual(availableChars, inheritedBuffer[..actualInherited]);
			CollectionAssert.AreEqual(
				new[] { unreadSentinel, unreadSentinel }, inheritedBuffer[actualInherited..]);
			Assert.AreEqual(availableBytes.Length, inheritedReader.BaseStream.Position);
		}

		using (var spanReader = new EndianReader(new MemoryStream(availableBytes)))
		{
			var spanBuffer = new char[requestedCount];
			Array.Fill(spanBuffer, unreadSentinel);

			int actualSpan = spanReader.Read(spanBuffer.AsSpan(0, requestedCount));

			Assert.AreEqual(availableChars.Length, actualSpan);
			CollectionAssert.AreEqual(availableChars, spanBuffer[..actualSpan]);
			CollectionAssert.AreEqual(
				new[] { unreadSentinel, unreadSentinel }, spanBuffer[actualSpan..]);
			Assert.AreEqual(availableBytes.Length, spanReader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Stream_EmptyByteSpansAndEndSlice_DoNotAdvanceReader()
	{
		using var readStream = new MemoryStream(new byte[] { 1, 2, 3 });
		using var reader = new EndianReader(readStream);
		using var readEndianStream = EndianStream.UsingReader(reader);

		Assert.AreEqual(0, reader.Read(Array.Empty<byte>().AsSpan()));
		Assert.AreEqual(0L, readStream.Position);

		Assert.AreSame(readEndianStream, readEndianStream.Stream(Array.Empty<byte>().AsSpan()));
		Assert.AreEqual(0L, readStream.Position);

		byte[] buffer = new byte[3];
		Assert.AreSame(readEndianStream, readEndianStream.Stream(buffer.AsSpan(buffer.Length, 0)));
		Assert.AreEqual(0L, readStream.Position);
	}

	[TestMethod]
	public void Stream_EmptyCharSpansAndEndSlice_DoNotAdvanceReader()
	{
		using var readStream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));
		using var reader = new EndianReader(readStream);
		using var readEndianStream = EndianStream.UsingReader(reader);

		Assert.AreEqual(0, reader.Read(Array.Empty<char>().AsSpan()));
		Assert.AreEqual(0L, readStream.Position);

		Assert.AreSame(readEndianStream, readEndianStream.Stream(Array.Empty<char>().AsSpan()));
		Assert.AreEqual(0L, readStream.Position);

		char[] buffer = new char[3];
		Assert.AreSame(readEndianStream, readEndianStream.Stream(buffer.AsSpan(buffer.Length, 0)));
		Assert.AreEqual(0L, readStream.Position);
	}

	[TestMethod]
	public void Stream_ReadWritePositions_AdvanceByActualTransferredBytesForSpanPaths()
	{
		using (var writeStream = new MemoryStream())
		{
			using var writer = new EndianWriter(writeStream) { BaseStreamOwner = false };
			using var writeEndianStream = EndianStream.UsingWriter(writer);

			writeEndianStream.Stream(new byte[] { 1, 2, 3 }.AsSpan());
			Assert.AreEqual(3L, writeStream.Position);

			writeEndianStream.Stream(new byte[] { 4, 5 }.AsSpan());
			Assert.AreEqual(5L, writeStream.Position);
		}

		using (var readStream = new MemoryStream(new byte[] { 9, 8, 7, 6, 5 }))
		using (var reader = new EndianReader(readStream))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			var firstDestination = new byte[3];
			readEndianStream.Stream(firstDestination.AsSpan());
			Assert.AreEqual(3L, readStream.Position);

			var secondDestination = new byte[2];
			readEndianStream.Stream(secondDestination.AsSpan());
			Assert.AreEqual(5L, readStream.Position);
		}
	}

	[TestMethod]
	public void TagSpanOverloads_PreserveEndianOrderingAndDestinationBounds()
	{
		Span<char> tag32 = stackalloc char[5];
		Span<char> tag64 = stackalloc char[9];

		foreach (var (byteOrder, bytes) in new[]
		{
			(Shell.EndianFormat.Big, Encoding.ASCII.GetBytes("ABCDEFGHIJKL")),
			(Shell.EndianFormat.Little, Encoding.ASCII.GetBytes("DCBAHGFELKJI")),
		})
		{
			using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);
			tag32[4] = '\0';
			tag64[8] = '\0';

			reader.ReadTag32(tag32);
			Assert.AreEqual(4L, reader.BaseStream.Position);
			reader.ReadTag64(tag64);
			Assert.AreEqual(12L, reader.BaseStream.Position);

			Assert.AreEqual("ABCD", new string(tag32.Slice(0, 4)));
			Assert.AreEqual('\0', tag32[4]);
			Assert.AreEqual("EFGHIJKL", new string(tag64.Slice(0, 8)));
			Assert.AreEqual('\0', tag64[8]);
		}
	}

	[TestMethod]
	public void TagSpanOverloads_PreserveWriterEndianOrdering()
	{
		foreach (var (byteOrder, expectedBytes) in new[]
		{
			(Shell.EndianFormat.Big, Encoding.ASCII.GetBytes("ABCD")),
			(Shell.EndianFormat.Little, Encoding.ASCII.GetBytes("DCBA")),
		})
		{
			using var stream = new MemoryStream();
			using var writer = new EndianWriter(stream, byteOrder) { BaseStreamOwner = false };

			writer.WriteTag32("ABCD".AsSpan());

			Assert.AreEqual(4L, stream.Position);
			Assert.AreEqual(4L, stream.Length);
			CollectionAssert.AreEqual(expectedBytes, stream.ToArray());
		}
	}

	[TestMethod]
	public void TagSpanOverloads_RejectInvalidWidthsWithoutAdvancingStreams()
	{
		using var readStream = new MemoryStream(new byte[16]);
		using var reader = new EndianReader(readStream);
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream);

		AssertThrowsArgumentOutOfRange("tag", () => ReadTag32WithUndersizedSpan(reader));
		AssertThrowsArgumentOutOfRange("tag", () => ReadTag64WithUndersizedSpan(reader));
		Assert.AreEqual(0L, readStream.Position);

		AssertThrowsArgumentOutOfRange("tag", () => WriteTag32WithUndersizedSpan(writer));
		AssertThrowsArgumentOutOfRange("tag", () => WriteTag32WithOversizedSpan(writer));
		Assert.AreEqual(0L, writeStream.Position);
		Assert.AreEqual(0L, writeStream.Length);
	}

	[TestMethod]
	public void StreamTagBigEndian_PreservesReusableScratchBufferPath()
	{
		const uint k_tag = 0x41424344;

		foreach (var (byteOrder, expectedBytes) in new[]
		{
			(Shell.EndianFormat.Big, Encoding.ASCII.GetBytes("ABCD")),
			(Shell.EndianFormat.Little, Encoding.ASCII.GetBytes("DCBA")),
		})
		{
			using var writeStream = new MemoryStream();
			using (var writer = new EndianWriter(writeStream, byteOrder) { BaseStreamOwner = false })
			using (var endianStream = EndianStream.UsingWriter(writer))
			{
				uint value = k_tag;
				endianStream.StreamTagBigEndian(ref value);
			}

			CollectionAssert.AreEqual(expectedBytes, writeStream.ToArray());

			using var reader = new EndianReader(new MemoryStream(expectedBytes), byteOrder);
			using var readEndianStream = EndianStream.UsingReader(reader);
			uint readValue = 0;

			readEndianStream.StreamTagBigEndian(ref readValue);

			Assert.AreEqual(k_tag, readValue);
		}
	}

	[TestMethod]
	public void EndianStreamFacadeDirectGuards_ThrowExpectedExceptions()
	{
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream) { BaseStreamOwner = false };
		using var endianStream = EndianStream.UsingWriter(writer);
		string text = string.Empty;
		TestEnum enumValue = TestEnum.None;

		AssertThrowsArgumentOutOfRange("byteCount", () => endianStream.Pad(0));
		AssertThrowsArgumentOutOfRange("byteCount", () => endianStream.Pad(-1));

		AssertThrowsArgumentNull("encoding", () => endianStream.Stream(ref text, (Text.StringStorageEncoding)null!));
		AssertThrowsArgumentNull("encoding", () => endianStream.Stream(ref text, (Text.StringStorageEncoding)null!, 0));
		AssertThrowsArgumentNull("implementation",
			() => endianStream.Stream(ref enumValue, (IEnumEndianStreamer<TestEnum>)null!));

		AssertThrowsArgumentNull("signature",
			() => endianStream.StreamSignature(null!, Memory.Strings.StringStorage.CStringAscii));
		AssertThrowsArgument("signature",
			() => endianStream.StreamSignature(string.Empty, Memory.Strings.StringStorage.CStringAscii));
		AssertThrowsArgumentNull("signature",
			() => endianStream.StreamSignature(null!, (Text.StringStorageEncoding)null!));
		AssertThrowsArgumentNull("encoding",
			() => endianStream.StreamSignature("test", (Text.StringStorageEncoding)null!));
	}

	[TestMethod]
	public void EndianStreamObjectAndDelegateGuards_ThrowExpectedExceptions()
	{
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream) { BaseStreamOwner = false };
		using var endianStream = EndianStream.UsingWriter(writer);
		var structValue = new TestStructStreamable();
		TestClassStreamable classValue = new();
		TestClassStreamable nullClassValue = null!;
		TestClassSerializable serializableValue = new();
		TestClassSerializable nullSerializableValue = null!;
		int intValue = 0;
		var context = new object();

		AssertThrowsArgumentNull("initializer",
			() => endianStream.StreamValue(ref structValue, (Func<TestStructStreamable>)null!));
		AssertThrowsArgumentNull("value", () => endianStream.StreamObject((TestClassStreamable)null!));
		AssertThrowsArgumentNull("value",
			() => endianStream.StreamObject(ref nullClassValue, () => new TestClassStreamable()));
		AssertThrowsArgumentNull("initializer",
			() => endianStream.StreamObject(ref classValue, (Func<TestClassStreamable>)null!));
		AssertThrowsArgumentNull("value", () => endianStream.Stream((TestClassSerializable)null!));
		AssertThrowsArgumentNull("value",
			() => endianStream.Stream(ref nullSerializableValue, () => new TestClassSerializable()));
		AssertThrowsArgumentNull("initializer",
			() => endianStream.Stream(ref serializableValue, (Func<TestClassSerializable>)null!));

		AssertThrowsArgumentNull("read",
			() => endianStream.StreamValueMethods(ref intValue, null!, (w, value) => { }));
		AssertThrowsArgumentNull("write",
			() => endianStream.StreamValueMethods(ref intValue, (EndianReader r, out int value) => value = 0, null!));
		AssertThrowsArgumentNull("theObj",
			() => endianStream.StreamObjectMethods<object>(null!, (r, value) => { }, (w, value) => { }));
		AssertThrowsArgumentNull("read",
			() => endianStream.StreamObjectMethods(context, null!, (w, value) => { }));
		AssertThrowsArgumentNull("write",
			() => endianStream.StreamObjectMethods(context, (r, value) => { }, null!));
		AssertThrowsArgumentNull("read", () => endianStream.StreamMethods(null!, w => { }));
		AssertThrowsArgumentNull("write", () => endianStream.StreamMethods(r => { }, null!));
		AssertThrowsArgumentNull("context",
			() => endianStream.StreamMethods<object>(null!, (value, r) => { }, (value, w) => { }));
		AssertThrowsArgumentNull("read",
			() => endianStream.StreamMethods(context, null!, (value, w) => { }));
		AssertThrowsArgumentNull("write",
			() => endianStream.StreamMethods(context, (value, r) => { }, null!));

		using var reader = new EndianReader(new MemoryStream());
		using var readingEndianStream = EndianStream.UsingReader(reader);
		TestClassStreamable readClassValue = null!;
		TestClassSerializable readSerializableValue = null!;

		readingEndianStream.StreamObject(ref readClassValue, () => new TestClassStreamable());
		readingEndianStream.Stream(ref readSerializableValue, () => new TestClassSerializable());

		Assert.IsNotNull(readClassValue);
		Assert.IsNotNull(readSerializableValue);
	}

	[TestMethod]
	public void EndianStreamArrayAndListGuards_ThrowExpectedExceptions()
	{
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream) { BaseStreamOwner = false };
		using var endianStream = EndianStream.UsingWriter(writer);
		TestStructSerializable[] structValues = new TestStructSerializable[1];
		TestStructSerializable[] nullStructValues = null!;
		TestClassSerializable[] classValues = [new TestClassSerializable()];
		TestClassSerializable[] nullClassValues = null!;
		var listValues = new List<TestClassSerializable>();
		EndianStream.StreamArrayValueDelegate<TestStructSerializable> nullStreamFunc = null!;
		EndianStream.ReadArrayDelegate<TestStructSerializable> readArray = (EndianReader r,
			ref TestStructSerializable[] value) => { };
		EndianStream.WriteArrayDelegate<TestStructSerializable> writeArray = (EndianWriter w,
			TestStructSerializable[] value) => { };

		AssertThrowsArgumentNull("values", () => endianStream.StreamArray((TestStructSerializable[])null!));
		AssertThrowsArgumentNull("values", () => endianStream.StreamArrayInt32(ref nullStructValues));
		AssertThrowsArgumentNull("values",
			() => endianStream.StreamArrayInt32(ref nullStructValues, UnusedStreamArrayValue));
		AssertThrowsArgumentNull("streamFunc",
			() => endianStream.StreamArrayInt32(ref structValues, nullStreamFunc));
		Assert.AreEqual(0L, writeStream.Length);

		AssertThrowsArgumentNull("values",
			() => endianStream.StreamArray((TestClassSerializable[])null!, () => new TestClassSerializable()));
		AssertThrowsArgumentNull("initializer",
			() => endianStream.StreamArray(classValues, (Func<TestClassSerializable>)null!));
		AssertThrowsArgumentNull("values",
			() => endianStream.StreamArrayInt32(ref nullClassValues, () => new TestClassSerializable()));
		AssertThrowsArgumentNull("initializer",
			() => endianStream.StreamArrayInt32(ref classValues, (Func<TestClassSerializable>)null!));
		Assert.AreEqual(0L, writeStream.Length);

		AssertThrowsArgumentNull("array",
			() => endianStream.StreamArrayMethods(
				ref nullStructValues,
				readArray,
				writeArray));
		AssertThrowsArgumentNull("read",
			() => endianStream.StreamArrayMethods(
				ref structValues,
				(EndianStream.ReadArrayDelegate<TestStructSerializable>)null!,
				writeArray));
		AssertThrowsArgumentNull("write",
			() => endianStream.StreamArrayMethods(
				ref structValues,
				readArray,
				(EndianStream.WriteArrayDelegate<TestStructSerializable>)null!));

		AssertThrowsArgumentNull("values",
			() => endianStream.StreamListElementsWithClear<TestClassSerializable>(
				null!,
				0,
				() => new TestClassSerializable()));
		AssertThrowsArgumentNull("initializer",
			() => endianStream.StreamListElementsWithClear(
				listValues,
				0,
				(Func<TestClassSerializable>)null!));
	}

	[TestMethod]
	public void ReadArrayInt32_NullArrayWithInitializer_ReturnsInitializedValues()
	{
		static byte[] CreateZeroCountBytes()
		{
			return new byte[] { 0, 0, 0, 0 };
		}

		using (var reader = new EndianReader(new MemoryStream(CreateZeroCountBytes()), Shell.EndianFormat.Big))
		using (var endianStream = EndianStream.UsingReader(reader))
		{
			TestStructSerializable[] values = null!;

			endianStream.StreamArrayInt32(ref values);

			Assert.IsNotNull(values);
			Assert.AreEqual(0, values.Length);
		}

		using (var reader = new EndianReader(new MemoryStream(CreateZeroCountBytes()), Shell.EndianFormat.Big))
		using (var endianStream = EndianStream.UsingReader(reader))
		{
			TestStructSerializable[] values = null!;

			endianStream.StreamArrayInt32(ref values, UnusedStreamArrayValue);

			Assert.IsNotNull(values);
			Assert.AreEqual(0, values.Length);
		}

		using (var reader = new EndianReader(new MemoryStream(CreateZeroCountBytes()), Shell.EndianFormat.Big))
		using (var endianStream = EndianStream.UsingReader(reader))
		{
			TestClassSerializable[] values = null!;

			endianStream.StreamArrayInt32(ref values, () => new TestClassSerializable());

			Assert.IsNotNull(values);
			Assert.AreEqual(0, values.Length);
		}
	}

	[TestMethod]
	public void Write_PrimitiveValues_UsesDeclaredEndianByteOrder()
	{
		CollectionAssert.AreEqual(CreateBigEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void Read_PrimitiveValues_UsesDeclaredEndianByteOrder()
	{
		AssertReadPrimitives(CreateBigEndianPrimitiveBytes(), Shell.EndianFormat.Big);
		AssertReadPrimitives(CreateLittleEndianPrimitiveBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void Read_PrimitivePastEnd_PreservesEndOfStreamBehavior()
	{
		using var reader = new EndianReader(new MemoryStream(new byte[] { 0x12, 0x34, 0x56 }), Shell.EndianFormat.Big);

		Assert.Throws<EndOfStreamException>(() => reader.ReadUInt32());
		Assert.AreEqual(3L, reader.BaseStream.Position);
	}

	[TestMethod]
	public void ReadWrite_EndianSwitch_HonorsCurrentByteOrder()
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			writer.Write(0x12345678U);

			using (writer.BeginEndianSwitch())
			{
				writer.Write(0x12345678U);
			}
		}

		CollectionAssert.AreEqual(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x78, 0x56, 0x34, 0x12 }, ms.ToArray());

		ms.Position = 0;
		using var reader = new EndianReader(ms, Shell.EndianFormat.Big);

		Assert.AreEqual(0x12345678U, reader.ReadUInt32());

		using (reader.BeginEndianSwitch())
		{
			Assert.AreEqual(0x12345678U, reader.ReadUInt32());
		}
	}

	[TestMethod]
	public void ReadWrite_UnknownByteOrder_TreatsAsBigEndian()
	{
		const Shell.EndianFormat unknownByteOrder = (Shell.EndianFormat)2;

		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, unknownByteOrder) { BaseStreamOwner = false })
		{
			// Tag helpers treat non-Little values as Big. Natural-width primitives should stay consistent.
			writer.Write((ushort)0x1234);
		}

		CollectionAssert.AreEqual(new byte[] { 0x12, 0x34 }, ms.ToArray());

		ms.Position = 0;
		using var reader = new EndianReader(ms, unknownByteOrder);
		Assert.AreEqual((ushort)0x1234, reader.ReadUInt16());
	}

	[TestMethod]
	public void WriteFixedArray_DeclaredRange_UsesEndianByteOrder()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void ReadFixedArray_DeclaredRange_UsesEndianByteOrder()
	{
		AssertReadFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void StreamScalar_RepresentativeValues_UsesDeclaredEndianByteOrder()
	{
		CollectionAssert.AreEqual(CreateBigEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Little));

		AssertReadEndianStreamScalars(CreateBigEndianEndianStreamScalarBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamScalars(CreateLittleEndianEndianStreamScalarBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void StreamFixedArray_DeclaredRange_UsesEndianByteOrder()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Little));

		AssertReadEndianStreamFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void FixedArrayPublicApi_ExposesExactCanonicalSpanSurface()
	{
		Type[] elementTypes =
		[
			typeof(bool),
			typeof(byte),
			typeof(sbyte),
			typeof(ushort),
			typeof(short),
			typeof(uint),
			typeof(int),
			typeof(ulong),
			typeof(long),
			typeof(float),
			typeof(double),
		];
		var apiShapes = new[]
		{
			(DeclaringType: typeof(EndianReader), Name: nameof(EndianReader.ReadFixedArray),
				ReturnType: typeof(void), ParameterType: typeof(Span<>)),
			(DeclaringType: typeof(EndianWriter), Name: nameof(EndianWriter.WriteFixedArray),
				ReturnType: typeof(void), ParameterType: typeof(ReadOnlySpan<>)),
			(DeclaringType: typeof(EndianStream), Name: nameof(EndianStream.StreamFixedArray),
				ReturnType: typeof(EndianStream), ParameterType: typeof(Span<>)),
		};
		var fixedArrayMethods = apiShapes
			.SelectMany(static shape => shape.DeclaringType.GetMethods(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
				.Where(method => method.Name == shape.Name))
			.ToArray();

		Assert.AreEqual(33, fixedArrayMethods.Length);
		Assert.IsFalse(fixedArrayMethods.Any(static method =>
			method.GetParameters().Any(static parameter => parameter.ParameterType.IsArray)));

		foreach (var shape in apiShapes)
		{
			var methods = fixedArrayMethods
				.Where(method => method.DeclaringType == shape.DeclaringType && method.Name == shape.Name)
				.ToArray();

			Assert.AreEqual(elementTypes.Length, methods.Length);
			Assert.IsTrue(methods.All(method => method.ReturnType == shape.ReturnType));
			Assert.IsTrue(methods.All(static method => method.GetParameters().Length == 1));
			Assert.IsTrue(methods.All(method =>
				method.GetParameters()[0].ParameterType.IsGenericType &&
				method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == shape.ParameterType));
			CollectionAssert.AreEquivalent(
				elementTypes,
				methods.Select(static method =>
					method.GetParameters()[0].ParameterType.GetGenericArguments()[0]).ToArray());
		}
	}

	[TestMethod]
	public void FixedArray_EmptyAndEndSlices_DoNotPerformIo()
	{
		using var readStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		using var reader = new EndianReader(readStream, Shell.EndianFormat.Big) { BaseStreamOwner = false };
		var readValues = new ushort[2];

		reader.ReadFixedArray(Span<ushort>.Empty);
		reader.ReadFixedArray(readValues.AsSpan(readValues.Length, 0));
		Assert.AreEqual(0L, readStream.Position);

		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream, Shell.EndianFormat.Big) { BaseStreamOwner = false };
		var writeValues = new[] { true, false };

		writer.WriteFixedArray(ReadOnlySpan<bool>.Empty);
		writer.WriteFixedArray(writeValues.AsSpan(writeValues.Length, 0));
		Assert.AreEqual(0L, writeStream.Position);

		using var readEndianStream = EndianStream.UsingReader(reader);
		using var writeEndianStream = EndianStream.UsingWriter(writer);
		Assert.AreSame(readEndianStream, readEndianStream.StreamFixedArray(Span<ushort>.Empty));
		Assert.AreSame(
			readEndianStream,
			readEndianStream.StreamFixedArray(readValues.AsSpan(readValues.Length, 0)));
		Assert.AreSame(writeEndianStream, writeEndianStream.StreamFixedArray(Span<bool>.Empty));
		Assert.AreSame(
			writeEndianStream,
			writeEndianStream.StreamFixedArray(writeValues.AsSpan(writeValues.Length, 0)));
		Assert.AreEqual(0L, readStream.Position);
		Assert.AreEqual(0L, writeStream.Position);
	}

	[TestMethod]
	public void BoolFixedArray_NonzeroInputAndCanonicalOutput_PreserveWireEncoding()
	{
		var values = new bool[3];
		using (var reader = new EndianReader(new MemoryStream(new byte[] { 0, 2, 0xFF })))
		{
			reader.ReadFixedArray(values.AsSpan());
			Assert.AreEqual(3L, reader.BaseStream.Position);
		}
		CollectionAssert.AreEqual(new[] { false, true, true }, values);

		using var stream = new MemoryStream();
		using (var writer = new EndianWriter(stream) { BaseStreamOwner = false })
		{
			writer.WriteFixedArray(values.AsSpan());
		}
		CollectionAssert.AreEqual(new byte[] { 0, 1, 1 }, stream.ToArray());
	}

	[TestMethod]
	public void ReadFixedArray_TruncatedInput_PreservesCompletedElementsAndAdvancement()
	{
		using (var reader = new EndianReader(new MemoryStream(new byte[] { 0x11, 0x22 })))
		{
			var values = new byte[] { 0xCC, 0xCC, 0xCC };

			Assert.ThrowsExactly<EndOfStreamException>(() => reader.ReadFixedArray(values.AsSpan()));

			CollectionAssert.AreEqual(new byte[] { 0x11, 0x22, 0xCC }, values);
			Assert.AreEqual(2L, reader.BaseStream.Position);
		}

		using var readStream = new MemoryStream(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 });
		using var ushortReader = new EndianReader(readStream, Shell.EndianFormat.Big);
		var ushortValues = new ushort[] { 0xAAAA, 0xBBBB, 0xCCCC };

		Assert.ThrowsExactly<EndOfStreamException>(() => ushortReader.ReadFixedArray(ushortValues.AsSpan()));

		CollectionAssert.AreEqual(new ushort[] { 0x1122, 0x3344, 0xCCCC }, ushortValues);
		Assert.AreEqual(5L, readStream.Position);
	}

	[TestMethod]
	public void WriteFixedArray_TruncatedOutput_PreservesCompletedElementsAndAdvancement()
	{
		using var stream = new WriteLimitStream(byteLimit: 3);
		using var writer = new EndianWriter(stream, Shell.EndianFormat.Big) { BaseStreamOwner = false };
		ushort[] values = { 0x1122, 0x3344, 0x5566 };

		Assert.ThrowsExactly<IOException>(() => writer.WriteFixedArray(values.AsSpan()));

		CollectionAssert.AreEqual(new byte[] { 0x11, 0x22, 0x33 }, stream.ToArray());
		Assert.AreEqual(3L, stream.Position);
	}

	[TestMethod]
	public void BaseStateAndTypeExtensions_RepresentativeOperations_UseEndianStreamBehavior()
	{
		var owner = new object();
		using var stream = new MemoryStream();
		using (var writer = new EndianWriter(stream, Shell.EndianFormat.Big, owner, "writer.bin") {
			BaseStreamOwner = false,
		})
		{
			Assert.AreSame(owner, writer.Owner);
			Assert.AreEqual("writer.bin", writer.StreamName);
			Assert.AreEqual(Shell.EndianFormat.Big, writer.ByteOrder);
			Assert.AreEqual(Values.PtrHandle.Null32, writer.BaseAddress);

			TypeExtensions.Write(true, writer);
			TypeExtensions.Write((ushort)0x1234, writer);
			TypeExtensions.Write(0x89ABCDEFU, writer);
			TypeExtensions.Write(BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)), writer);

			writer.Seek32(1);
			Assert.AreEqual(1L, writer.BaseStream.Position);
			writer.Seek(0, SeekOrigin.End);
		}

		CollectionAssert.AreEqual(new byte[] {
			0x01,
			0x12, 0x34,
			0x89, 0xAB, 0xCD, 0xEF,
			0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
		}, stream.ToArray());

		stream.Position = 0;
		using var reader = new EndianReader(stream, Shell.EndianFormat.Big, owner, "reader.bin");

		TypeExtensions.Read(reader, out bool boolValue);
		TypeExtensions.Read(reader, out ushort ushortValue);
		TypeExtensions.Read(reader, out uint uintValue);
		TypeExtensions.Read(reader, out double doubleValue);

		Assert.AreSame(owner, reader.Owner);
		Assert.AreEqual("reader.bin", reader.StreamName);
		Assert.IsTrue(boolValue);
		Assert.AreEqual((ushort)0x1234, ushortValue);
		Assert.AreEqual(0x89ABCDEFU, uintValue);
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValue));
	}

	[TestMethod]
	public void VirtualAddressTranslation_RelativePointers_TranslatesExpectedAddresses()
	{
		using var stream = new MemoryStream();
		using (var writer = new EndianWriter(stream, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			Assert.Throws<InvalidOperationException>(
				() => writer.VirtualAddressTranslationPop());

			writer.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);
			Assert.Throws<InvalidOperationException>(
				() => writer.VirtualAddressTranslationPop());

			writer.VirtualAddressTranslationPush(new Values.PtrHandle(0x1000U));
			writer.WriteVirtualAddress(new Values.PtrHandle(0x1020U));
			writer.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);

			Assert.AreEqual(new Values.PtrHandle(0x1000U), writer.VirtualAddressTranslationPop());
		}

		CollectionAssert.AreEqual(new byte[] {
			0x00, 0x00, 0x00, 0x20,
			0xFF, 0xFF, 0xFF, 0xFF,
		}, stream.ToArray());

		stream.Position = 0;
		using var reader = new EndianReader(stream, Shell.EndianFormat.Big);

		Assert.Throws<InvalidOperationException>(
			() => reader.ReadVirtualAddress());

		reader.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);
		reader.VirtualAddressTranslationPush(new Values.PtrHandle(0x1000U));

		Assert.AreEqual(new Values.PtrHandle(0x1020U), reader.ReadVirtualAddress());
		Assert.AreEqual(Values.PtrHandle.InvalidHandle32, reader.ReadVirtualAddress());
		Assert.AreEqual(new Values.PtrHandle(0x1000U), reader.VirtualAddressTranslationPop());
	}

	static byte[] WritePrimitives(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		{
			WritePrimitiveValues(writer);
		}

		return ms.ToArray();
	}

	static void WritePrimitiveValues(EndianWriter writer)
	{
		// Use signed values and NaN payloads so byte-exact tests catch bit-preservation regressions, not just equality.
		writer.Write((ushort)0x1234);
		writer.Write(unchecked((short)0x89AB));
		writer.Write(0x89ABCDEFU);
		writer.Write(unchecked((int)0x89ABCDEF));
		writer.Write(0x0123456789ABCDEFUL);
		writer.Write(unchecked((long)0xFEDCBA9876543210UL));
		writer.Write(BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)));
		writer.Write(BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)));
	}

	static byte[] WriteFixedArrayRange(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		{
			WriteFixedArrayRangeValues(writer);
		}

		return ms.ToArray();
	}

	static byte[] WriteEndianStreamScalars(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		using (var stream = EndianStream.UsingWriter(writer))
		{
			StreamScalarValues(stream);
		}

		return ms.ToArray();
	}

	static byte[] WriteEndianStreamFixedArrayRange(Shell.EndianFormat byteOrder)
	{
		using var ms = new MemoryStream();
		using (var writer = new EndianWriter(ms, byteOrder) { BaseStreamOwner = false })
		using (var stream = EndianStream.UsingWriter(writer))
		{
			StreamFixedArrayRangeValues(stream);
		}

		return ms.ToArray();
	}

	static void WriteFixedArrayRangeValues(EndianWriter writer)
	{
		writer.WriteFixedArray(new bool[] { true, false, true, false }.AsSpan(1, 2));
		writer.WriteFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }.AsSpan(1, 2));
		writer.WriteFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }.AsSpan(1, 2));
		writer.WriteFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }.AsSpan(1, 2));
		writer.WriteFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }.AsSpan(1, 2));
		writer.WriteFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }.AsSpan(1, 2));
		writer.WriteFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }.AsSpan(1, 2));
		writer.WriteFixedArray(new ulong[] {
			0,
			0x0123456789ABCDEFUL,
			0xFEDCBA9876543210UL,
			0,
		}.AsSpan(1, 2));
		writer.WriteFixedArray(new long[] {
			0,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			0,
		}.AsSpan(1, 2));
		writer.WriteFixedArray(new float[] {
			0,
			BitConverter.Int32BitsToSingle(0x3F800000),
			BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
			BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
			0,
		}.AsSpan(1, 3));
		writer.WriteFixedArray(new double[] {
			0,
			BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
			BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
			BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000UL)),
			0,
		}.AsSpan(1, 3));
	}

	static void StreamScalarValues(EndianStream stream)
	{
		byte byteValue = 0x7A;
		sbyte sbyteValue = unchecked((sbyte)0x85);
		ushort ushortValue = 0x1234;
		short shortValue = unchecked((short)0x89AB);
		uint uintValue = 0x89ABCDEFU;
		int intValue = unchecked((int)0x89ABCDEF);
		ulong ulongValue = 0x0123456789ABCDEFUL;
		long longValue = unchecked((long)0xFEDCBA9876543210UL);
		float floatValue = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001));
		double doubleValue = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL));

		stream.Stream(ref byteValue);
		stream.Stream(ref sbyteValue);
		stream.Stream(ref ushortValue);
		stream.Stream(ref shortValue);
		stream.Stream(ref uintValue);
		stream.Stream(ref intValue);
		stream.Stream(ref ulongValue);
		stream.Stream(ref longValue);
		stream.Stream(ref floatValue);
		stream.Stream(ref doubleValue);
	}

	static void StreamFixedArrayRangeValues(EndianStream stream)
	{
		Assert.AreSame(stream, stream.StreamFixedArray(new bool[] { true, false, true, false }.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }.AsSpan(1, 2)));
		Assert.AreSame(
			stream,
			stream.StreamFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }.AsSpan(1, 2)));
		Assert.AreSame(
			stream,
			stream.StreamFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }.AsSpan(1, 2)));
		Assert.AreSame(
			stream,
			stream.StreamFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new ulong[] {
			0,
			0x0123456789ABCDEFUL,
			0xFEDCBA9876543210UL,
			0,
		}.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new long[] {
				0,
				0x0123456789ABCDEFL,
				unchecked((long)0xFEDCBA9876543210UL),
				0,
			}.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(new float[] {
				0,
				BitConverter.Int32BitsToSingle(0x3F800000),
				BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
				BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
				0,
			}.AsSpan(1, 3)));
		Assert.AreSame(stream, stream.StreamFixedArray(new double[] {
				0,
				BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
				BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
				BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000UL)),
				0,
			}.AsSpan(1, 3)));
	}

	static void AssertReadPrimitives(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);

		Assert.AreEqual((ushort)0x1234, reader.ReadUInt16());
		Assert.AreEqual(unchecked((short)0x89AB), reader.ReadInt16());
		Assert.AreEqual(0x89ABCDEFU, reader.ReadUInt32());
		Assert.AreEqual(unchecked((int)0x89ABCDEF), reader.ReadInt32());
		Assert.AreEqual(0x0123456789ABCDEFUL, reader.ReadUInt64());
		Assert.AreEqual(unchecked((long)0xFEDCBA9876543210UL), reader.ReadInt64());
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(reader.ReadSingle()));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(reader.ReadDouble()));
	}

	static void AssertReadFixedArrayRange(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);

		AssertReadFixedArrayRangeValues(reader);
	}

	static void AssertReadEndianStreamScalars(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);
		using var stream = EndianStream.UsingReader(reader);

		byte byteValue = 0;
		sbyte sbyteValue = 0;
		ushort ushortValue = 0;
		short shortValue = 0;
		uint uintValue = 0;
		int intValue = 0;
		ulong ulongValue = 0;
		long longValue = 0;
		float floatValue = 0;
		double doubleValue = 0;

		stream.Stream(ref byteValue);
		stream.Stream(ref sbyteValue);
		stream.Stream(ref ushortValue);
		stream.Stream(ref shortValue);
		stream.Stream(ref uintValue);
		stream.Stream(ref intValue);
		stream.Stream(ref ulongValue);
		stream.Stream(ref longValue);
		stream.Stream(ref floatValue);
		stream.Stream(ref doubleValue);

		Assert.AreEqual((byte)0x7A, byteValue);
		Assert.AreEqual(unchecked((sbyte)0x85), sbyteValue);
		Assert.AreEqual((ushort)0x1234, ushortValue);
		Assert.AreEqual(unchecked((short)0x89AB), shortValue);
		Assert.AreEqual(0x89ABCDEFU, uintValue);
		Assert.AreEqual(unchecked((int)0x89ABCDEF), intValue);
		Assert.AreEqual(0x0123456789ABCDEFUL, ulongValue);
		Assert.AreEqual(unchecked((long)0xFEDCBA9876543210UL), longValue);
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(floatValue));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValue));
	}

	static void AssertReadEndianStreamFixedArrayRange(byte[] bytes, Shell.EndianFormat byteOrder)
	{
		using var reader = new EndianReader(new MemoryStream(bytes), byteOrder);
		using var stream = EndianStream.UsingReader(reader);

		AssertReadStreamFixedArrayRangeValues(stream);
	}

	static void AssertReadFixedArrayRangeValues(EndianReader reader)
	{
		var boolValues = new[] { true, true, true, false };
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, 0, -2 };

		reader.ReadFixedArray(boolValues.AsSpan(1, 2));
		reader.ReadFixedArray(byteValues.AsSpan(1, 2));
		reader.ReadFixedArray(sbyteValues.AsSpan(1, 2));
		reader.ReadFixedArray(ushortValues.AsSpan(1, 2));
		reader.ReadFixedArray(shortValues.AsSpan(1, 2));
		reader.ReadFixedArray(uintValues.AsSpan(1, 2));
		reader.ReadFixedArray(intValues.AsSpan(1, 2));
		reader.ReadFixedArray(ulongValues.AsSpan(1, 2));
		reader.ReadFixedArray(longValues.AsSpan(1, 2));
		reader.ReadFixedArray(floatValues.AsSpan(1, 3));
		reader.ReadFixedArray(doubleValues.AsSpan(1, 3));

		AssertFixedArrayRangeValues(
			boolValues,
			byteValues,
			sbyteValues,
			ushortValues,
			shortValues,
			uintValues,
			intValues,
			ulongValues,
			longValues,
			floatValues,
			doubleValues);
	}

	static void AssertReadStreamFixedArrayRangeValues(EndianStream stream)
	{
		var boolValues = new[] { true, true, true, false };
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, 0, -2 };

		Assert.AreSame(stream, stream.StreamFixedArray(boolValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(byteValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(sbyteValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(ushortValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(shortValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(uintValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(intValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(ulongValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(longValues.AsSpan(1, 2)));
		Assert.AreSame(stream, stream.StreamFixedArray(floatValues.AsSpan(1, 3)));
		Assert.AreSame(stream, stream.StreamFixedArray(doubleValues.AsSpan(1, 3)));

		AssertFixedArrayRangeValues(
			boolValues,
			byteValues,
			sbyteValues,
			ushortValues,
			shortValues,
			uintValues,
			intValues,
			ulongValues,
			longValues,
			floatValues,
			doubleValues);
	}

	static void AssertFixedArrayRangeValues(
		bool[] boolValues,
		byte[] byteValues,
		sbyte[] sbyteValues,
		ushort[] ushortValues,
		short[] shortValues,
		uint[] uintValues,
		int[] intValues,
		ulong[] ulongValues,
		long[] longValues,
		float[] floatValues,
		double[] doubleValues)
	{
		CollectionAssert.AreEqual(new[] { true, false, true, false }, boolValues);
		CollectionAssert.AreEqual(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, byteValues);
		CollectionAssert.AreEqual(new sbyte[] { -1, 0x11, 0x22, -4 }, sbyteValues);
		CollectionAssert.AreEqual(new ushort[] { 0xAAAA, 0x1234, 0x5678, 0xBBBB }, ushortValues);
		CollectionAssert.AreEqual(new short[] { -1, unchecked((short)0x89AB), 0x1234, -2 }, shortValues);
		CollectionAssert.AreEqual(new uint[] { uint.MaxValue, 0x89ABCDEFU, 0x01234567U, uint.MaxValue }, uintValues);
		CollectionAssert.AreEqual(new int[] { -1, unchecked((int)0x89ABCDEF), 0x01234567, -2 }, intValues);
		CollectionAssert.AreEqual(new ulong[] {
			ulong.MaxValue,
			0x0123456789ABCDEFUL,
			0xFEDCBA9876543210UL,
			ulong.MaxValue,
		}, ulongValues);
		CollectionAssert.AreEqual(new long[] {
			-1,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			-2,
		}, longValues);
		Assert.AreEqual(0x3F800000, BitConverter.SingleToInt32Bits(floatValues[1]));
		Assert.AreEqual(unchecked((int)0xFFC00001), BitConverter.SingleToInt32Bits(floatValues[2]));
		Assert.AreEqual(unchecked((int)0x80000000), BitConverter.SingleToInt32Bits(floatValues[3]));
		Assert.AreEqual(0x3FF0000000000000L, BitConverter.DoubleToInt64Bits(doubleValues[1]));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValues[2]));
		Assert.AreEqual(
			unchecked((long)0x8000000000000000UL),
			BitConverter.DoubleToInt64Bits(doubleValues[3]));
	}

	static byte[] CreateBigEndianPrimitiveBytes() => new byte[] {
		0x12, 0x34,
		0x89, 0xAB,
		0x89, 0xAB, 0xCD, 0xEF,
		0x89, 0xAB, 0xCD, 0xEF,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0xFF, 0xC0, 0x00, 0x01,
		0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
	};

	static byte[] CreateLittleEndianPrimitiveBytes() => new byte[] {
		0x34, 0x12,
		0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0x01, 0x00, 0xC0, 0xFF,
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF,
	};

	static byte[] CreateBigEndianEndianStreamScalarBytes() => PrependByteScalars(CreateBigEndianPrimitiveBytes());

	static byte[] CreateLittleEndianEndianStreamScalarBytes() => PrependByteScalars(CreateLittleEndianPrimitiveBytes());

	static byte[] PrependByteScalars(byte[] primitiveBytes)
	{
		var bytes = new List<byte> { 0x7A, 0x85 };

		bytes.AddRange(primitiveBytes);
		return bytes.ToArray();
	}

	static byte[] CreateBigEndianFixedArrayRangeBytes() => new byte[] {
		0x00, 0x01,
		0x11, 0x22,
		0x11, 0x22,
		0x12, 0x34, 0x56, 0x78,
		0x89, 0xAB, 0x12, 0x34,
		0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67,
		0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0x3F, 0x80, 0x00, 0x00,
		0xFF, 0xC0, 0x00, 0x01,
		0x80, 0x00, 0x00, 0x00,
		0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
		0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	};

	static byte[] CreateLittleEndianFixedArrayRangeBytes() => new byte[] {
		0x00, 0x01,
		0x11, 0x22,
		0x11, 0x22,
		0x34, 0x12, 0x78, 0x56,
		0xAB, 0x89, 0x34, 0x12,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01,
		0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
		0x00, 0x00, 0x80, 0x3F,
		0x01, 0x00, 0xC0, 0xFF,
		0x00, 0x00, 0x00, 0x80,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F,
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
	};
}
