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

	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	static void AssertThrowsArgument(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
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

	static EndianStream UnusedStreamArrayValue(ref TestStructSerializable value)
	{
		throw new InvalidOperationException("The zero-count test path should not invoke the stream delegate.");
	}

	[TestMethod]
	public void Constructors_NullEndianReaderInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new EndianReader(null!), "input");
		AssertThrowsArgumentNull(
			() => _ = new EndianReader(null!, Encoding.UTF8, Shell.EndianFormat.Big),
			"input");
		AssertThrowsArgumentNull(
			() => _ = new EndianReader(new MemoryStream(), null!, Shell.EndianFormat.Big),
			"encoding");
	}

	[TestMethod]
	public void Constructors_NullEndianWriterInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new EndianWriter(null!), "output");
		AssertThrowsArgumentNull(
			() => _ = new EndianWriter(null!, Encoding.UTF8, Shell.EndianFormat.Big),
			"output");
		AssertThrowsArgumentNull(
			() => _ = new EndianWriter(new MemoryStream(), null!, Shell.EndianFormat.Big),
			"encoding");
	}

	[TestMethod]
	public void Constructors_NullEndianStreamInputs_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new EndianStream(null!), "baseStream");
		AssertThrowsArgumentNull(
			() => _ = new EndianStream(null!, Encoding.UTF8, Shell.EndianFormat.Big),
			"baseStream");
		AssertThrowsArgumentNull(
			() => _ = new EndianStream(new MemoryStream(), null!, Shell.EndianFormat.Big),
			"encoding");
		AssertThrowsArgumentNull(() => _ = EndianStream.UsingReader(null!), "reader");
		AssertThrowsArgumentNull(() => _ = EndianStream.UsingWriter(null!), "writer");
	}

	[TestMethod]
	public void ReaderWriterDirectGuards_ThrowExpectedExceptions()
	{
		using var readStream = new MemoryStream(new byte[16]);
		using var reader = new EndianReader(readStream);
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream);

		AssertThrowsArgumentOutOfRange(() => reader.Pad(0), "byteCount");
		AssertThrowsArgumentOutOfRange(() => reader.Pad(-1), "byteCount");
		AssertThrowsArgumentOutOfRange(() => writer.Pad(0), "byteCount");
		AssertThrowsArgumentOutOfRange(() => writer.Pad(-1), "byteCount");

		AssertThrowsArgumentNull(() => _ = reader.Read((byte[])null!, 0), "buffer");
		AssertThrowsArgumentNull(() => _ = reader.Read((byte[])null!), "buffer");
		AssertThrowsArgumentOutOfRange(() => _ = reader.Read(new byte[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => _ = reader.Read(new byte[1], 2), "count");
		AssertThrowsArgumentNull(() => _ = reader.Read((char[])null!, 0), "buffer");
		AssertThrowsArgumentNull(() => _ = reader.Read((char[])null!), "buffer");
		AssertThrowsArgumentOutOfRange(() => _ = reader.Read(new char[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => _ = reader.Read(new char[1], 2), "count");

		AssertThrowsArgumentNull(() => writer.Write((byte[])null!, 0), "value");
		AssertThrowsArgumentOutOfRange(() => writer.Write(new byte[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => writer.Write(new byte[1], 2), "count");
		AssertThrowsArgumentNull(() => writer.Write((char[])null!, 0), "value");
		AssertThrowsArgumentOutOfRange(() => writer.Write(new char[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => writer.Write(new char[1], 2), "count");

		AssertThrowsArgumentNull(() => _ = reader.ReadTag32(null!), "tag");
		AssertThrowsArgumentOutOfRange(() => _ = reader.ReadTag32(new char[3]), "tag");
		AssertThrowsArgumentNull(() => _ = reader.ReadTag64(null!), "tag");
		AssertThrowsArgumentOutOfRange(() => _ = reader.ReadTag64(new char[7]), "tag");
		AssertThrowsArgumentNull(() => writer.WriteTag32(null!), "tag");
		AssertThrowsArgumentOutOfRange(() => writer.WriteTag32(new char[3]), "tag");
		AssertThrowsArgumentOutOfRange(() => writer.WriteTag32(new char[5]), "tag");

		AssertThrowsArgumentNull(() => _ = reader.ReadString((Text.StringStorageEncoding)null!, 0), "encoding");
		AssertThrowsArgumentNull(() => _ = reader.ReadString((Text.StringStorageEncoding)null!), "encoding");
		AssertThrowsArgumentNull(() => writer.Write("test", (Text.StringStorageEncoding)null!), "encoding");
		AssertThrowsArgumentNull(() => writer.Write("test".AsSpan(), (Text.StringStorageEncoding)null!), "encoding");
		Assert.ThrowsExactly<InvalidDataException>(() =>
			reader.ReadString(Memory.Strings.StringStorage.AsciiString, TypeExtensions.kNone));
		AssertThrowsArgumentNull(() => _ = reader.Read<TestEnum>(null!), "implementation");
		AssertThrowsArgumentNull(() => writer.Write(TestEnum.None, null!), "implementation");
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
	public void ReaderDirectArrayAndTagHelpers_ReturnExpectedBuffers()
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
			Assert.AreSame(tag32, reader.ReadTag32(tag32));
			CollectionAssert.AreEqual(new[] { 'A', 'B', 'C', 'D', '\0' }, tag32);

			Assert.AreEqual("1234", new string(reader.ReadTag32()));
		}

		using (var reader = new EndianReader(new MemoryStream(Encoding.ASCII.GetBytes("ABCDEFGH")), Shell.EndianFormat.Big))
		{
			var tag64 = reader.ReadTag64();
			Assert.AreEqual(8, tag64.Length);
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
			reader.ReadTag64(tag64);

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

			CollectionAssert.AreEqual(expectedBytes, stream.ToArray());
		}
	}

	[TestMethod]
	public void TagSpanOverloads_RejectUndersizedBuffers()
	{
		using var readStream = new MemoryStream(new byte[16]);
		using var reader = new EndianReader(readStream);
		using var writeStream = new MemoryStream();
		using var writer = new EndianWriter(writeStream);

		AssertThrowsArgumentOutOfRange(() => ReadTag32WithUndersizedSpan(reader), "tag");
		AssertThrowsArgumentOutOfRange(() => ReadTag64WithUndersizedSpan(reader), "tag");
		AssertThrowsArgumentOutOfRange(() => WriteTag32WithUndersizedSpan(writer), "tag");
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

		AssertThrowsArgumentOutOfRange(() => endianStream.Pad(0), "byteCount");
		AssertThrowsArgumentOutOfRange(() => endianStream.Pad(-1), "byteCount");

		AssertThrowsArgumentNull(() => endianStream.Stream((byte[])null!, 0, 0), "value");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[1], -1, 0), "index");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[1], 1, 0), "index");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[1], 0, -1), "count");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[2], 1, 2), "count");
		AssertThrowsArgumentNull(() => endianStream.Stream((byte[])null!, 0), "value");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new byte[1], 2), "count");
		AssertThrowsArgumentNull(() => endianStream.Stream((byte[])null!), "value");

		AssertThrowsArgumentNull(() => endianStream.Stream((char[])null!, 0, 0), "value");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[1], -1, 0), "index");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[1], 1, 0), "index");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[1], 0, -1), "count");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[2], 1, 2), "count");
		AssertThrowsArgumentNull(() => endianStream.Stream((char[])null!, 0), "value");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[1], -1), "count");
		AssertThrowsArgumentOutOfRange(() => endianStream.Stream(new char[1], 2), "count");
		AssertThrowsArgumentNull(() => endianStream.Stream((char[])null!), "value");

		AssertThrowsArgumentNull(() => endianStream.Stream(ref text, (Text.StringStorageEncoding)null!), "encoding");
		AssertThrowsArgumentNull(() => endianStream.Stream(ref text, (Text.StringStorageEncoding)null!, 0), "encoding");
		AssertThrowsArgumentNull(() => endianStream.Stream(ref enumValue, (IEnumEndianStreamer<TestEnum>)null!),
			"implementation");

		AssertThrowsArgumentNull(
			() => endianStream.StreamSignature(null!, Memory.Strings.StringStorage.CStringAscii),
			"signature");
		AssertThrowsArgument(
			() => endianStream.StreamSignature(string.Empty, Memory.Strings.StringStorage.CStringAscii),
			"signature");
		AssertThrowsArgumentNull(
			() => endianStream.StreamSignature(null!, (Text.StringStorageEncoding)null!),
			"signature");
		AssertThrowsArgumentNull(
			() => endianStream.StreamSignature("test", (Text.StringStorageEncoding)null!),
			"encoding");
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

		AssertThrowsArgumentNull(
			() => endianStream.StreamValue(ref structValue, (Func<TestStructStreamable>)null!),
			"initializer");
		AssertThrowsArgumentNull(() => endianStream.StreamObject((TestClassStreamable)null!), "value");
		AssertThrowsArgumentNull(
			() => endianStream.StreamObject(ref nullClassValue, () => new TestClassStreamable()),
			"value");
		AssertThrowsArgumentNull(
			() => endianStream.StreamObject(ref classValue, (Func<TestClassStreamable>)null!),
			"initializer");
		AssertThrowsArgumentNull(() => endianStream.Stream((TestClassSerializable)null!), "value");
		AssertThrowsArgumentNull(
			() => endianStream.Stream(ref nullSerializableValue, () => new TestClassSerializable()),
			"value");
		AssertThrowsArgumentNull(
			() => endianStream.Stream(ref serializableValue, (Func<TestClassSerializable>)null!),
			"initializer");

		AssertThrowsArgumentNull(
			() => endianStream.StreamValueMethods(ref intValue, null!, (w, value) => { }),
			"read");
		AssertThrowsArgumentNull(
			() => endianStream.StreamValueMethods(ref intValue, (EndianReader r, out int value) => value = 0, null!),
			"write");
		AssertThrowsArgumentNull(
			() => endianStream.StreamObjectMethods<object>(null!, (r, value) => { }, (w, value) => { }),
			"theObj");
		AssertThrowsArgumentNull(
			() => endianStream.StreamObjectMethods(context, null!, (w, value) => { }),
			"read");
		AssertThrowsArgumentNull(
			() => endianStream.StreamObjectMethods(context, (r, value) => { }, null!),
			"write");
		AssertThrowsArgumentNull(() => endianStream.StreamMethods(null!, w => { }), "read");
		AssertThrowsArgumentNull(() => endianStream.StreamMethods(r => { }, null!), "write");
		AssertThrowsArgumentNull(
			() => endianStream.StreamMethods<object>(null!, (value, r) => { }, (value, w) => { }),
			"context");
		AssertThrowsArgumentNull(
			() => endianStream.StreamMethods(context, null!, (value, w) => { }),
			"read");
		AssertThrowsArgumentNull(
			() => endianStream.StreamMethods(context, (value, r) => { }, null!),
			"write");

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

		AssertThrowsArgumentNull(() => endianStream.StreamArray((TestStructSerializable[])null!), "values");
		AssertThrowsArgumentNull(() => endianStream.StreamArrayInt32(ref nullStructValues), "values");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayInt32(ref nullStructValues, UnusedStreamArrayValue),
			"values");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayInt32(ref structValues, nullStreamFunc),
			"streamFunc");
		Assert.AreEqual(0L, writeStream.Length);

		AssertThrowsArgumentNull(
			() => endianStream.StreamArray((TestClassSerializable[])null!, () => new TestClassSerializable()),
			"values");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArray(classValues, (Func<TestClassSerializable>)null!),
			"initializer");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayInt32(ref nullClassValues, () => new TestClassSerializable()),
			"values");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayInt32(ref classValues, (Func<TestClassSerializable>)null!),
			"initializer");
		Assert.AreEqual(0L, writeStream.Length);

		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayMethods(
				ref nullStructValues,
				readArray,
				writeArray),
			"array");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayMethods(
				ref structValues,
				(EndianStream.ReadArrayDelegate<TestStructSerializable>)null!,
				writeArray),
			"read");
		AssertThrowsArgumentNull(
			() => endianStream.StreamArrayMethods(
				ref structValues,
				readArray,
				(EndianStream.WriteArrayDelegate<TestStructSerializable>)null!),
			"write");

		AssertThrowsArgumentNull(
			() => endianStream.StreamListElementsWithClear<TestClassSerializable>(
				null!,
				0,
				() => new TestClassSerializable()),
			"values");
		AssertThrowsArgumentNull(
			() => endianStream.StreamListElementsWithClear(
				listValues,
				0,
				(Func<TestClassSerializable>)null!),
			"initializer");
	}

	[TestMethod]
	public void EndianStreamArrayInt32ReadAllowsNullArraysWithValidInitializersTest()
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
	public void PrimitiveWritesUseDeclaredEndianByteOrderTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianPrimitiveBytes(), WritePrimitives(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void PrimitiveReadsUseDeclaredEndianByteOrderTest()
	{
		AssertReadPrimitives(CreateBigEndianPrimitiveBytes(), Shell.EndianFormat.Big);
		AssertReadPrimitives(CreateLittleEndianPrimitiveBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void PrimitiveReadsKeepEndOfStreamBehaviorTest()
	{
		using var reader = new EndianReader(new MemoryStream(new byte[] { 0x12, 0x34, 0x56 }), Shell.EndianFormat.Big);

		Assert.Throws<EndOfStreamException>(() => reader.ReadUInt32());
		Assert.AreEqual(3L, reader.BaseStream.Position);
	}

	[TestMethod]
	public void PrimitiveReadsAndWritesHonorEndianSwitchTest()
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
	public void PrimitiveReadsAndWritesTreatUnknownByteOrderAsBigEndianTest()
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
	public void FixedArrayWritesUseDeclaredEndianByteOrderAndRangeTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteFixedArrayRange(Shell.EndianFormat.Little));
	}

	[TestMethod]
	public void FixedArrayReadsUseDeclaredEndianByteOrderAndRangeTest()
	{
		AssertReadFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void EndianStreamStreamsScalarsUseDeclaredEndianByteOrderTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianEndianStreamScalarBytes(),
			WriteEndianStreamScalars(Shell.EndianFormat.Little));

		AssertReadEndianStreamScalars(CreateBigEndianEndianStreamScalarBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamScalars(CreateLittleEndianEndianStreamScalarBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void EndianStreamFixedArrayUsesDeclaredEndianByteOrderAndRangeTest()
	{
		CollectionAssert.AreEqual(CreateBigEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Big));
		CollectionAssert.AreEqual(CreateLittleEndianFixedArrayRangeBytes(),
			WriteEndianStreamFixedArrayRange(Shell.EndianFormat.Little));

		AssertReadEndianStreamFixedArrayRange(CreateBigEndianFixedArrayRangeBytes(), Shell.EndianFormat.Big);
		AssertReadEndianStreamFixedArrayRange(CreateLittleEndianFixedArrayRangeBytes(), Shell.EndianFormat.Little);
	}

	[TestMethod]
	public void FixedArrayInvalidArgumentsThrowExplicitExceptionsTest()
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
	public void FixedArrayOutOfRangePreservesPartialSideEffectsTest()
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

		AssertThrowsArgumentNull(() => writer.WriteFixedArray((bool[])null!, 0, 0), "array");
		AssertThrowsArgumentOutOfRange(() => writer.WriteFixedArray(new bool[1], -1, 0), "startIndex");
		AssertThrowsArgumentOutOfRange(() => writer.WriteFixedArray(new bool[1], 0, -1), "length");
		AssertThrowsArgumentNull(() => writer.WriteFixedArray((bool[])null!), "array");

		AssertThrowsArgumentNull(() => reader.ReadFixedArray((bool[])null!, 0, 0), "array");
		AssertThrowsArgumentOutOfRange(() => reader.ReadFixedArray(new bool[1], -1, 0), "startIndex");
		AssertThrowsArgumentOutOfRange(() => reader.ReadFixedArray(new bool[1], 0, -1), "length");
		AssertThrowsArgumentNull(() => reader.ReadFixedArray((bool[])null!), "array");

		AssertThrowsArgumentNull(() => writerEndianStream.StreamFixedArray((bool[])null!, 0, 0), "array");
		AssertThrowsArgumentOutOfRange(() => writerEndianStream.StreamFixedArray(new bool[1], -1, 0), "startIndex");
		AssertThrowsArgumentOutOfRange(() => writerEndianStream.StreamFixedArray(new bool[1], 0, -1), "length");
		AssertThrowsArgumentNull(() => writerEndianStream.StreamFixedArray((bool[])null!), "array");

		AssertThrowsArgumentNull(() => readerEndianStream.StreamFixedArray((bool[])null!, 0, 0), "array");
		AssertThrowsArgumentOutOfRange(() => readerEndianStream.StreamFixedArray(new bool[1], -1, 0), "startIndex");
		AssertThrowsArgumentOutOfRange(() => readerEndianStream.StreamFixedArray(new bool[1], 0, -1), "length");
		AssertThrowsArgumentNull(() => readerEndianStream.StreamFixedArray((bool[])null!), "array");
	}

	[TestMethod]
	public void BoolFixedArrayOutOfRangePreservesPartialSideEffectsTest()
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
	public void BaseStateAndTypeExtensionsUseEndianStreamBehaviorTest()
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
	public void VirtualAddressTranslationTranslatesRelativePointersTest()
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
