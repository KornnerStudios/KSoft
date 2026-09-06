using System;
using System.Collections.Generic;
using System.IO;
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

		AssertThrowsArgumentNull("buffer", () => _ = reader.Read((byte[])null!, 0));
		AssertThrowsArgumentNull("buffer", () => _ = reader.Read((byte[])null!));
		AssertThrowsArgumentOutOfRange("count", () => _ = reader.Read(new byte[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => _ = reader.Read(new byte[1], 2));
		AssertThrowsArgumentNull("buffer", () => _ = reader.Read((char[])null!, 0));
		AssertThrowsArgumentNull("buffer", () => _ = reader.Read((char[])null!));
		AssertThrowsArgumentOutOfRange("count", () => _ = reader.Read(new char[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => _ = reader.Read(new char[1], 2));

		AssertThrowsArgumentNull("value", () => writer.Write((byte[])null!, 0));
		AssertThrowsArgumentOutOfRange("count", () => writer.Write(new byte[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => writer.Write(new byte[1], 2));
		AssertThrowsArgumentNull("value", () => writer.Write((char[])null!, 0));
		AssertThrowsArgumentOutOfRange("count", () => writer.Write(new char[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => writer.Write(new char[1], 2));

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
			Assert.AreSame(byteBuffer, reader.Read(byteBuffer, 2));
			CollectionAssert.AreEqual(new byte[] { 1, 2, 0 }, byteBuffer);
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.UTF8.GetBytes("abcd")), Shell.EndianFormat.Big))
		{
			var charBuffer = new char[3];
			Assert.AreSame(charBuffer, reader.Read(charBuffer, 2));
			CollectionAssert.AreEqual(new[] { 'a', 'b', '\0' }, charBuffer);
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
			Assert.AreSame(values, reader.ReadFixedArray(values));
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
	public void Stream_SpanByteShortStream_PartialReadMatchesLegacyArrayBehaviorAndActualCounts()
	{
		byte[] available = { 0xAA, 0xBB };
		const int requestedCount = 4;

		// Legacy KSoft-declared array overload: array-return identity preserved, actual transferred
		// count is discarded internally but still observable via stream position.
		using (var legacyReader = new EndianReader(new MemoryStream(available)))
		{
			var legacyBuffer = new byte[requestedCount];
			Assert.AreSame(legacyBuffer, legacyReader.Read(legacyBuffer, requestedCount));
			CollectionAssert.AreEqual(new byte[] { 0xAA, 0xBB, 0, 0 }, legacyBuffer);
			Assert.AreEqual(available.Length, legacyReader.BaseStream.Position);
		}

		// Inherited three-argument BinaryReader.Read is not an EndianReader-declared overload (no
		// KSOFTSPAN002 attribute applies) and exposes the actual transferred count directly.
		int actualInherited;
		using (var inheritedReader = new EndianReader(new MemoryStream(available)))
		{
			var inheritedBuffer = new byte[requestedCount];
			actualInherited = inheritedReader.Read(inheritedBuffer, 0, requestedCount);
			CollectionAssert.AreEqual(new byte[] { 0xAA, 0xBB, 0, 0 }, inheritedBuffer);
		}
		Assert.AreEqual(available.Length, actualInherited);

		// Canonical Span<byte> path: same partial-read contract; the actual count is the direct return value.
		using (var spanReader = new EndianReader(new MemoryStream(available)))
		{
			var spanBuffer = new byte[requestedCount];
			int actualSpan = spanReader.Read(spanBuffer.AsSpan());

			Assert.AreEqual(actualInherited, actualSpan);
			CollectionAssert.AreEqual(new byte[] { 0xAA, 0xBB, 0, 0 }, spanBuffer);
			Assert.AreEqual(available.Length, spanReader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Stream_SpanCharShortStream_PartialReadMatchesLegacyArrayBehaviorAndActualCounts()
	{
		byte[] availableBytes = Encoding.UTF8.GetBytes("AB");
		const int requestedCount = 4;

		using (var legacyReader = new EndianReader(new MemoryStream(availableBytes)))
		{
			var legacyBuffer = new char[requestedCount];
			Assert.AreSame(legacyBuffer, legacyReader.Read(legacyBuffer, requestedCount));
			CollectionAssert.AreEqual(new[] { 'A', 'B', '\0', '\0' }, legacyBuffer);
			Assert.AreEqual(availableBytes.Length, legacyReader.BaseStream.Position);
		}

		int actualInherited;
		using (var inheritedReader = new EndianReader(new MemoryStream(availableBytes)))
		{
			var inheritedBuffer = new char[requestedCount];
			actualInherited = inheritedReader.Read(inheritedBuffer, 0, requestedCount);
			CollectionAssert.AreEqual(new[] { 'A', 'B', '\0', '\0' }, inheritedBuffer);
		}
		Assert.AreEqual(2, actualInherited);

		using (var spanReader = new EndianReader(new MemoryStream(availableBytes)))
		{
			var spanBuffer = new char[requestedCount];
			int actualSpan = spanReader.Read(spanBuffer.AsSpan());

			Assert.AreEqual(actualInherited, actualSpan);
			CollectionAssert.AreEqual(new[] { 'A', 'B', '\0', '\0' }, spanBuffer);
			Assert.AreEqual(availableBytes.Length, spanReader.BaseStream.Position);
		}
	}

	[TestMethod]
	public void Stream_EmptyByteBuffer_LegacyWholeArraySucceedsAndSpanEndSliceSucceeds()
	{
		using var readStream = new MemoryStream(new byte[] { 1, 2, 3 });
		using var reader = new EndianReader(readStream);
		using var readEndianStream = EndianStream.UsingReader(reader);

		// Legacy whole-array overload: count==0/length==0 already succeeds with no I/O (array-return
		// identity preserved).
		Assert.AreSame(Array.Empty<byte>(), reader.Read(Array.Empty<byte>()));
		Assert.AreEqual(0L, readStream.Position);

		// Facade over the same legacy whole-array overload: also succeeds with no I/O.
		Assert.AreSame(readEndianStream, readEndianStream.Stream(Array.Empty<byte>()));
		Assert.AreEqual(0L, readStream.Position);

		// Canonical empty end-of-buffer Span slice: legal, succeeds, no I/O.
		byte[] buffer = new byte[3];
		Assert.AreSame(readEndianStream, readEndianStream.Stream(buffer.AsSpan(buffer.Length, 0)));
		Assert.AreEqual(0L, readStream.Position);

		// The legacy (index, count) overload's stricter validation is unchanged: index == Length is
		// still rejected even when count == 0 (also asserted in
		// EndianStreamFacadeDirectGuards_ThrowExpectedExceptions).
		AssertThrowsArgumentOutOfRange("index", () => readEndianStream.Stream(buffer, buffer.Length, 0));
	}

	[TestMethod]
	public void Stream_EmptyCharBuffer_LegacyWholeArraySucceedsAndSpanEndSliceSucceeds()
	{
		using var readStream = new MemoryStream(Encoding.UTF8.GetBytes("abc"));
		using var reader = new EndianReader(readStream);
		using var readEndianStream = EndianStream.UsingReader(reader);

		Assert.AreSame(Array.Empty<char>(), reader.Read(Array.Empty<char>()));
		Assert.AreEqual(0L, readStream.Position);

		Assert.AreSame(readEndianStream, readEndianStream.Stream(Array.Empty<char>()));
		Assert.AreEqual(0L, readStream.Position);

		char[] buffer = new char[3];
		Assert.AreSame(readEndianStream, readEndianStream.Stream(buffer.AsSpan(buffer.Length, 0)));
		Assert.AreEqual(0L, readStream.Position);

		AssertThrowsArgumentOutOfRange("index", () => readEndianStream.Stream(buffer, buffer.Length, 0));
	}

	[TestMethod]
	public void Stream_ReadWritePositions_AdvanceByActualTransferredBytesForLegacyAndSpanPaths()
	{
		using (var writeStream = new MemoryStream())
		{
			using var writer = new EndianWriter(writeStream) { BaseStreamOwner = false };
			using var writeEndianStream = EndianStream.UsingWriter(writer);

			writeEndianStream.Stream(new byte[] { 1, 2, 3 }); // legacy whole-array facade overload
			Assert.AreEqual(3L, writeStream.Position);

			writeEndianStream.Stream(new Span<byte>(new byte[] { 4, 5 })); // canonical Span facade overload
			Assert.AreEqual(5L, writeStream.Position);
		}

		using (var readStream = new MemoryStream(new byte[] { 9, 8, 7, 6, 5 }))
		using (var reader = new EndianReader(readStream))
		using (var readEndianStream = EndianStream.UsingReader(reader))
		{
			var legacyDestination = new byte[3];
			readEndianStream.Stream(legacyDestination); // legacy whole-array facade overload
			Assert.AreEqual(3L, readStream.Position);

			var spanDestination = new byte[2];
			readEndianStream.Stream(spanDestination.AsSpan()); // canonical Span facade overload
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

		AssertThrowsArgumentNull("value", () => endianStream.Stream((byte[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("index", () => endianStream.Stream(new byte[1], -1, 0));
		AssertThrowsArgumentOutOfRange("index", () => endianStream.Stream(new byte[1], 1, 0));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new byte[1], 0, -1));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new byte[2], 1, 2));
		AssertThrowsArgumentNull("value", () => endianStream.Stream((byte[])null!, 0));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new byte[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new byte[1], 2));
		AssertThrowsArgumentNull("value", () => endianStream.Stream((byte[])null!));

		AssertThrowsArgumentNull("value", () => endianStream.Stream((char[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("index", () => endianStream.Stream(new char[1], -1, 0));
		AssertThrowsArgumentOutOfRange("index", () => endianStream.Stream(new char[1], 1, 0));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new char[1], 0, -1));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new char[2], 1, 2));
		AssertThrowsArgumentNull("value", () => endianStream.Stream((char[])null!, 0));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new char[1], -1));
		AssertThrowsArgumentOutOfRange("count", () => endianStream.Stream(new char[1], 2));
		AssertThrowsArgumentNull("value", () => endianStream.Stream((char[])null!));

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
	public void FixedArray_InvalidArguments_ThrowsExplicitExceptions()
	{
		using var writerStream = new MemoryStream();
		using var writer = new EndianWriter(writerStream, Shell.EndianFormat.Big) { BaseStreamOwner = false };

		Assert.Throws<ArgumentNullException>(() => writer.WriteFixedArray((ushort[])null!, 0, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteFixedArray(new ushort[1], -1, 1));
		Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteFixedArray(new ushort[1], 0, -1));

		using var reader = new EndianReader(
			new MemoryStream(new byte[] { 0x12, 0x34 }),
			Shell.EndianFormat.Big);

		Assert.Throws<ArgumentNullException>(() => reader.ReadFixedArray((ushort[])null!, 0, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadFixedArray(new ushort[1], -1, 1));
		Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadFixedArray(new ushort[1], 0, -1));
	}

	[TestMethod]
	public void FixedArray_OutOfRangeWrite_PreservesPartialSideEffects()
	{
		using var writerStream = new MemoryStream();
		using (var writer = new EndianWriter(writerStream, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			Assert.Throws<IndexOutOfRangeException>(()
				=> writer.WriteFixedArray(new ushort[] { 0x1111, 0x2233 }, 1, 2));
		}

		CollectionAssert.AreEqual(new byte[] { 0x22, 0x33 }, writerStream.ToArray());

		using var readStream = new MemoryStream(new byte[] { 0x11, 0x22, 0x33, 0x44 });
		using var reader = new EndianReader(readStream, Shell.EndianFormat.Big);
		var values = new ushort[2];

		Assert.Throws<IndexOutOfRangeException>(() => reader.ReadFixedArray(values, 1, 2));
		Assert.AreEqual(4L, readStream.Position);
		Assert.AreEqual((ushort)0x1122, values[1]);
	}

	[TestMethod]
	public void BoolFixedArrayGuards_ThrowExpectedExceptions()
	{
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream, Shell.EndianFormat.Big) { BaseStreamOwner = false };
		using var writerEndianStream = EndianStream.UsingWriter(writer);
		using var reader = new EndianReader(new MemoryStream(new byte[2]), Shell.EndianFormat.Big);
		using var readerEndianStream = EndianStream.UsingReader(reader);

		AssertThrowsArgumentNull("array", () => writer.WriteFixedArray((bool[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => writer.WriteFixedArray(new bool[1], -1, 0));
		AssertThrowsArgumentOutOfRange("length", () => writer.WriteFixedArray(new bool[1], 0, -1));
		AssertThrowsArgumentNull("array", () => writer.WriteFixedArray((bool[])null!));

		AssertThrowsArgumentNull("array", () => reader.ReadFixedArray((bool[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => reader.ReadFixedArray(new bool[1], -1, 0));
		AssertThrowsArgumentOutOfRange("length", () => reader.ReadFixedArray(new bool[1], 0, -1));
		AssertThrowsArgumentNull("array", () => reader.ReadFixedArray((bool[])null!));

		AssertThrowsArgumentNull("array", () => writerEndianStream.StreamFixedArray((bool[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => writerEndianStream.StreamFixedArray(new bool[1], -1, 0));
		AssertThrowsArgumentOutOfRange("length", () => writerEndianStream.StreamFixedArray(new bool[1], 0, -1));
		AssertThrowsArgumentNull("array", () => writerEndianStream.StreamFixedArray((bool[])null!));

		AssertThrowsArgumentNull("array", () => readerEndianStream.StreamFixedArray((bool[])null!, 0, 0));
		AssertThrowsArgumentOutOfRange("startIndex", () => readerEndianStream.StreamFixedArray(new bool[1], -1, 0));
		AssertThrowsArgumentOutOfRange("length", () => readerEndianStream.StreamFixedArray(new bool[1], 0, -1));
		AssertThrowsArgumentNull("array", () => readerEndianStream.StreamFixedArray((bool[])null!));
	}

	[TestMethod]
	public void BoolFixedArray_OutOfRangeWrite_PreservesPartialSideEffects()
	{
		using var writerStream = new MemoryStream();
		using (var writer = new EndianWriter(writerStream, Shell.EndianFormat.Big) { BaseStreamOwner = false })
		{
			Assert.Throws<IndexOutOfRangeException>(()
				=> writer.WriteFixedArray(new bool[] { true, false }, 1, 2));
		}

		CollectionAssert.AreEqual(new byte[] { 0 }, writerStream.ToArray());

		using var readStream = new MemoryStream(new byte[] { 1, 0 });
		using var reader = new EndianReader(readStream, Shell.EndianFormat.Big);
		var values = new bool[2];

		Assert.Throws<IndexOutOfRangeException>(() => reader.ReadFixedArray(values, 1, 2));
		Assert.AreEqual(2L, readStream.Position);
		Assert.IsTrue(values[1]);
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
		writer.WriteFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, 1, 2);
		writer.WriteFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }, 1, 2);
		writer.WriteFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }, 1, 2);
		writer.WriteFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }, 1, 2);
		writer.WriteFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }, 1, 2);
		writer.WriteFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }, 1, 2);
		writer.WriteFixedArray(new ulong[] { 0, 0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, 0 }, 1, 2);
		writer.WriteFixedArray(new long[] {
			0,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			0,
		}, 1, 2);
		writer.WriteFixedArray(new float[] {
			0,
			BitConverter.Int32BitsToSingle(0x3F800000),
			BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
			0,
		}, 1, 2);
		writer.WriteFixedArray(new double[] {
			0,
			BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
			BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
			0,
		}, 1, 2);
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
		stream.StreamFixedArray(new byte[] { 0xA0, 0x11, 0x22, 0xA3 }, 1, 2);
		stream.StreamFixedArray(new sbyte[] { -1, 0x11, 0x22, -4 }, 1, 2);
		stream.StreamFixedArray(new ushort[] { 0, 0x1234, 0x5678, 0 }, 1, 2);
		stream.StreamFixedArray(new short[] { 0, unchecked((short)0x89AB), 0x1234, 0 }, 1, 2);
		stream.StreamFixedArray(new uint[] { 0, 0x89ABCDEFU, 0x01234567U, 0 }, 1, 2);
		stream.StreamFixedArray(new int[] { 0, unchecked((int)0x89ABCDEF), 0x01234567, 0 }, 1, 2);
		stream.StreamFixedArray(new ulong[] { 0, 0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL, 0 }, 1, 2);
		stream.StreamFixedArray(new long[] {
			0,
			0x0123456789ABCDEFL,
			unchecked((long)0xFEDCBA9876543210UL),
			0,
		}, 1, 2);
		stream.StreamFixedArray(new float[] {
			0,
			BitConverter.Int32BitsToSingle(0x3F800000),
			BitConverter.Int32BitsToSingle(unchecked((int)0xFFC00001)),
			0,
		}, 1, 2);
		stream.StreamFixedArray(new double[] {
			0,
			BitConverter.Int64BitsToDouble(0x3FF0000000000000L),
			BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8000000000001UL)),
			0,
		}, 1, 2);
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
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, -2 };

		reader.ReadFixedArray(byteValues, 1, 2);
		reader.ReadFixedArray(sbyteValues, 1, 2);
		reader.ReadFixedArray(ushortValues, 1, 2);
		reader.ReadFixedArray(shortValues, 1, 2);
		reader.ReadFixedArray(uintValues, 1, 2);
		reader.ReadFixedArray(intValues, 1, 2);
		reader.ReadFixedArray(ulongValues, 1, 2);
		reader.ReadFixedArray(longValues, 1, 2);
		reader.ReadFixedArray(floatValues, 1, 2);
		reader.ReadFixedArray(doubleValues, 1, 2);

		AssertFixedArrayRangeValues(
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
		var byteValues = new byte[] { 0xA0, 0, 0, 0xA3 };
		var sbyteValues = new sbyte[] { -1, 0, 0, -4 };
		var ushortValues = new ushort[] { 0xAAAA, 0, 0, 0xBBBB };
		var shortValues = new short[] { -1, 0, 0, -2 };
		var uintValues = new uint[] { uint.MaxValue, 0, 0, uint.MaxValue };
		var intValues = new int[] { -1, 0, 0, -2 };
		var ulongValues = new ulong[] { ulong.MaxValue, 0, 0, ulong.MaxValue };
		var longValues = new long[] { -1, 0, 0, -2 };
		var floatValues = new float[] { -1, 0, 0, -2 };
		var doubleValues = new double[] { -1, 0, 0, -2 };

		stream.StreamFixedArray(byteValues, 1, 2);
		stream.StreamFixedArray(sbyteValues, 1, 2);
		stream.StreamFixedArray(ushortValues, 1, 2);
		stream.StreamFixedArray(shortValues, 1, 2);
		stream.StreamFixedArray(uintValues, 1, 2);
		stream.StreamFixedArray(intValues, 1, 2);
		stream.StreamFixedArray(ulongValues, 1, 2);
		stream.StreamFixedArray(longValues, 1, 2);
		stream.StreamFixedArray(floatValues, 1, 2);
		stream.StreamFixedArray(doubleValues, 1, 2);

		AssertFixedArrayRangeValues(
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
		Assert.AreEqual(0x3FF0000000000000L, BitConverter.DoubleToInt64Bits(doubleValues[1]));
		Assert.AreEqual(unchecked((long)0xFFF8000000000001UL), BitConverter.DoubleToInt64Bits(doubleValues[2]));
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
		0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0xFF, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
	};

	static byte[] CreateLittleEndianFixedArrayRangeBytes() => new byte[] {
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
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F,
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0xFF,
	};
}
