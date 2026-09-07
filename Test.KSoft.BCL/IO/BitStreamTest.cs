using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public class BitStreamTest : BaseTestClass
	{
		enum TestEnum : byte
		{
			None,
		}

		sealed class NonSeekableMemoryStream : MemoryStream
		{
			public override bool CanSeek => false;
		}

		struct TestStructSerializable : IBitStreamSerializable
		{
			public void Serialize(IO.BitStream s)
			{
			}
		}

		sealed class TestClassSerializable : IBitStreamSerializable
		{
			public TestClassSerializable()
			{
			}

			public void Serialize(IO.BitStream s)
			{
			}
		}

		[TestMethod]
		public void WriteWord_MixedWidths_IsReadableByLegacyBitStream()
		{
			var values = new KeyValuePair<uint, int>[] {
				new(10, 7),
				new(0xBEEFBEEF, 32),
				new(12, 7),
				new(0x13371337, 32),
				new(123, 7),
				new(0xDEADC0DE, 32),
				new(0, 7),
				new(111, 7),

				new(1, 1),
				new(2, 2),
				new(7, 3),
				new(14, 4),
				new(21, 5),
				new(42, 6),
				new(14406, 15),
			};

			using (var ms = new MemoryStream())
			{
				using (var bs = new IO.BitStream(ms, FileAccess.Write))
				{
					int expected_bit_position = 0;

					bs.StreamMode = FileAccess.Write;
					foreach (var kv in values)
					{
						bs.WriteWord(kv.Key, kv.Value);
						expected_bit_position += kv.Value;
						Assert.AreEqual(expected_bit_position, bs.BitPosition, "Value=" + kv.Key);
					}
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
				using (var bs_old = new BKSystem.IO.BitStream(ms))
				{
					foreach (var kv in values)
					{
						bs_old.Read(out uint word, 0, kv.Value);
						Assert.AreEqual(kv.Key, word);
					}
				}
			}
		}

		[TestMethod]
		public void ReadWord_LegacyBitStreamOutput_ReturnsOriginalValues()
		{
			var values = new KeyValuePair<uint, int>[] {
				new(10, 7),
				new(0xBEEFBEEF, 32),
				new(12, 7),
				new(0x13371337, 32),
				new(123, 7),
				new(0xDEADC0DE, 32),
				new(0, 7),
				new(111, 7),

				new(1, 1),
				new(2, 2),
				new(7, 3),
				new(14, 4),
				new(21, 5),
				new(42, 6),
				new(14406, 15),
			};

			using (var ms = new MemoryStream())
			{
				using (var bs_old = new BKSystem.IO.BitStream())
				{
					foreach (var kv in values)
					{
						bs_old.Write(kv.Key, 0, kv.Value);
					}

					bs_old.WriteTo(ms);
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
				using (var bs = new IO.BitStream(ms, FileAccess.Read))
				{
					bs.StreamMode = FileAccess.Read;
					foreach (var kv in values)
					{
						bs.ReadWord(out uint word, kv.Value);
						Assert.AreEqual(kv.Key, word);
					}
				}
			}
		}

		[TestMethod]
		public void Read_MixedSignedAndBooleanValues_RoundTrips()
		{
			using (var ms = new MemoryStream())
			{
				using (var bs = new IO.BitStream(ms, FileAccess.Write))
				{
					bs.StreamMode = FileAccess.Write;
					bs.Write((int)1337, 15);
					bs.Write(-21474836480L, 60);
					bs.Write(-1, 30);
					bs.Write(false);
					bs.Write((int)0xDEDEAD, 27);
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
				using (var bs_old = new IO.BitStream(ms, FileAccess.Read))
				{
					bs_old.StreamMode = FileAccess.Read;

					bs_old.Read(out int _int, 15);
					Assert.AreEqual(1337, _int);

					bs_old.Read(out long _long, 60, signExtend: true);
					Assert.AreEqual(-21474836480L, _long);
					bs_old.Read(out _int, 30, signExtend: true);
					Assert.AreEqual(-1, _int);

					bs_old.Read(out bool _bool);
					Assert.AreEqual(false, _bool);

					bs_old.Read(out _int, 27);
					Assert.AreEqual((int)0xDEDEAD, _int);
				}
			}
		}

		[TestMethod]
		public void Constructor_InvalidArguments_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("baseStream", () => _ = new IO.BitStream(null!));
			AssertThrowsArgumentOutOfRange("endPos", () => _ = new IO.BitStream(new MemoryStream(new byte[1]), endPos: 2));
			AssertThrowsArgumentOutOfRange(
				"permissions",
				() => _ = new IO.BitStream(new MemoryStream(), permissions: 0));
			AssertThrowsArgumentNull("streamName", () => _ = new IO.BitStream(new MemoryStream(), streamName: null!));
		}

		[TestMethod]
		public void SeekToStart_NonSeekableStream_ThrowsInvalidOperationException()
		{
			using var stream = new NonSeekableMemoryStream();
			using var bitStream = new IO.BitStream(stream);

			Assert.ThrowsExactly<InvalidOperationException>(() => bitStream.SeekToStart());
		}

		[TestMethod]
		public void ByteBufferMethods_InvalidArguments_ThrowExpectedExceptions()
		{
			using var stream = new MemoryStream(new byte[] { 0xFF });
			using var bitStream = new IO.BitStream(stream, FileAccess.ReadWrite);

			AssertThrowsArgumentOutOfRange("bitCount",
				() => bitStream.Read(new byte[1].AsSpan(), Bits.kByteBitCount + 1));
			AssertThrowsArgumentOutOfRange("bitCount",
				() => bitStream.Write(new byte[1].AsSpan(), Bits.kByteBitCount + 1));
			AssertThrowsArgumentOutOfRange("bitCount",
				() => bitStream.Stream(new byte[1].AsSpan(), Bits.kByteBitCount + 1));
			AssertThrowsArgumentOutOfRange("byteCount", () => _ = bitStream.ReadBytes(-1));
		}

		[TestMethod]
		public void ByteBufferMethods_SlicesAndEmptySpans_RoundTrip()
		{
			byte[] source = [0xAA, 0x12, 0x34, 0xBB];
			byte[] writeBytes = WriteWithBitStream(bs =>
			{
				bs.Write(ReadOnlySpan<byte>.Empty);
				bs.Write(source.AsSpan(1, 2));
			});
			byte[] streamBytes = WriteWithBitStream(bs =>
			{
				Assert.AreSame(bs, bs.Stream(Span<byte>.Empty));
				Assert.AreSame(bs, bs.Stream(source.AsSpan(1, 2)));
			});

			CollectionAssert.AreEqual(new byte[] { 0x12, 0x34 }, writeBytes);
			CollectionAssert.AreEqual(writeBytes, streamBytes);

			byte[] readValues = [0xAA, 0, 0, 0xBB];
			ReadWithBitStream(writeBytes, bs =>
			{
				bs.Read(Span<byte>.Empty);
				bs.Read(readValues.AsSpan(1, 2));
			});
			CollectionAssert.AreEqual(source, readValues);

			byte[] streamValues = [0xAA, 0, 0, 0xBB];
			ReadWithBitStream(streamBytes, bs =>
			{
				Assert.AreSame(bs, bs.Stream(Span<byte>.Empty));
				Assert.AreSame(bs, bs.Stream(streamValues.AsSpan(1, 2)));
			});
			CollectionAssert.AreEqual(source, streamValues);
		}

		[TestMethod]
		public void DateTimeAndStringMethods_InvalidArguments_ThrowExpectedExceptions()
		{
			using var stream = new MemoryStream(new byte[sizeof(long)]);
			using var bitStream = new IO.BitStream(stream, FileAccess.ReadWrite);
			var dateTime = DateTime.UnixEpoch;
			string text = string.Empty;

			AssertThrowsArgumentOutOfRange(
				"bitCount",
				() => _ = bitStream.ReadDateTime(Bits.kInt64BitCount + 1));
			AssertThrowsArgumentOutOfRange(
				"bitCount",
				() => bitStream.Read(out dateTime, Bits.kInt64BitCount + 1));
			AssertThrowsArgumentOutOfRange(
				"bitCount",
				() => bitStream.Write(dateTime, Bits.kInt64BitCount + 1));
			AssertThrowsArgumentOutOfRange(
				"bitCount",
				() => bitStream.Stream(ref dateTime, Bits.kInt64BitCount + 1));

			AssertThrowsArgumentNull(
				"encoding",
				() => _ = bitStream.ReadString((Text.StringStorageEncoding)null!));
			AssertThrowsArgumentNull(
				"encoding",
				() => bitStream.Write(text, (Text.StringStorageEncoding)null!));
			AssertThrowsArgumentNull(
				"encoding",
				() => bitStream.Stream(ref text, (Text.StringStorageEncoding)null!));
			Assert.ThrowsExactly<InvalidDataException>(() =>
				bitStream.ReadString(Memory.Strings.StringStorage.AsciiString, TypeExtensions.kNone));
		}

		[TestMethod]
		public void SerializationHelpers_InvalidArguments_ThrowExpectedExceptions()
		{
			using var stream = new MemoryStream();
			using var bitStream = new IO.BitStream(stream, FileAccess.Write);
			bitStream.StreamMode = FileAccess.Write;
			TestEnum enumValue = TestEnum.None;
			var structValue = new TestStructSerializable();
			var classValue = new TestClassSerializable();
			TestClassSerializable nullClassValue = null!;
			var structValues = new TestStructSerializable[1];
			var classValues = new TestClassSerializable[] { new() };
			var listValues = new List<TestClassSerializable>();
			var context = new object();

			AssertThrowsArgumentNull(
				"implementation",
				() => bitStream.Stream(ref enumValue, 1, null!));
			AssertThrowsArgumentNull(
				"initializer",
				() => bitStream.StreamValue(ref structValue, (Func<TestStructSerializable>)null!));
			AssertThrowsArgumentNull(
				"value",
				() => bitStream.StreamObject((TestClassSerializable)null!));
			AssertThrowsArgumentNull(
				"value",
				() => bitStream.StreamObject(ref nullClassValue, () => new TestClassSerializable()));
			AssertThrowsArgumentNull(
				"initializer",
				() => bitStream.StreamObject(ref classValue, (Func<TestClassSerializable>)null!));

			AssertThrowsArgumentNull("read", () => bitStream.StreamMethods(null!, bs => { }));
			AssertThrowsArgumentNull("write", () => bitStream.StreamMethods(bs => { }, null!));
			AssertThrowsArgumentNull(
				"context",
				() => bitStream.StreamMethods<object>(null!, (value, bs) => { }, (value, bs) => { }));
			AssertThrowsArgumentNull(
				"read",
				() => bitStream.StreamMethods(context, null!, (value, bs) => { }));
			AssertThrowsArgumentNull(
				"write",
				() => bitStream.StreamMethods(context, (value, bs) => { }, null!));

			AssertThrowsArgumentNull(
				"values",
				() => bitStream.StreamValueArray((TestStructSerializable[])null!));
			AssertThrowsArgumentNull(
				"values",
				() => bitStream.StreamObjectArray((TestClassSerializable[])null!, () => new TestClassSerializable()));
			AssertThrowsArgumentNull(
				"initializer",
				() => bitStream.StreamObjectArray(classValues, (Func<TestClassSerializable>)null!));

			AssertThrowsArgumentNull(
				"list",
				() => bitStream.StreamElements<TestClassSerializable, object>(
					null!,
					1,
					context,
					_ => new TestClassSerializable()));
			AssertThrowsArgumentOutOfRange(
				"countBitSize",
				() => bitStream.StreamElements(listValues, Bits.kInt32BitCount + 1, context,
					_ => new TestClassSerializable()));
			AssertThrowsArgumentNull(
				"ctor",
				() => bitStream.StreamElements<TestClassSerializable, object>(
					listValues,
					1,
					context,
					null!));
			AssertThrowsArgumentNull(
				"list",
				() => bitStream.StreamElements<TestClassSerializable>(null!, 1));
			AssertThrowsArgumentOutOfRange(
				"countBitSize",
				() => bitStream.StreamElements<TestClassSerializable>(listValues, Bits.kInt32BitCount + 1));

			Assert.AreEqual(0L, stream.Length);
		}

		[TestMethod]
		public void StreamObjectRead_NullReferenceWithInitializer_AllowsNull()
		{
			using var stream = new MemoryStream(new byte[] { 0 });
			using var bitStream = new IO.BitStream(stream, FileAccess.Read);
			bitStream.StreamMode = FileAccess.Read;
			TestClassSerializable value = null!;

			bitStream.StreamObject(ref value, () => new TestClassSerializable());

			Assert.IsNotNull(value);
		}

		[TestMethod]
		public void WriteWord_CrossesCacheBoundary_WritesExpectedBytes()
		{
			byte[] bytes = WriteWithBitStream(bs =>
			{
				bs.WriteWord(0b10101U, 5);
				Assert.AreEqual(5, bs.BitPosition);

				bs.WriteWord(0xABCDEU, 20);
				Assert.AreEqual(25, bs.BitPosition);

				bs.WriteWord(0b111U, 3);
				Assert.AreEqual(28, bs.BitPosition);
			});

			CollectionAssert.AreEqual(new byte[] { 0xAD, 0x5E, 0x6F, 0x70 }, bytes);
		}

		[TestMethod]
		public void ReadWord_CrossesCacheBoundary_ReadsExpectedValues()
		{
			ReadWithBitStream(new byte[] { 0xAD, 0x5E, 0x6F, 0x70 }, bs =>
			{
				bs.ReadWord(out uint prefix, 5);
				Assert.AreEqual(0b10101U, prefix);
				Assert.AreEqual(5, bs.BitPosition);

				bs.ReadWord(out uint middle, 20);
				Assert.AreEqual(0xABCDEU, middle);
				Assert.AreEqual(25, bs.BitPosition);

				bs.ReadWord(out uint suffix, 3);
				Assert.AreEqual(0b111U, suffix);
				Assert.AreEqual(28, bs.BitPosition);
			});
		}

		[TestMethod]
		public void ReadWriteUInt64_SplitWordCounts_RoundTrips()
		{
			byte[] bytes = WriteWithBitStream(bs =>
			{
				bs.Write(0x89ABCDEFUL, 32);
				bs.Write(0x1FEDCBA98UL, 33);
				bs.Write(0x0123456789ABCDEFUL, Bits.kUInt64BitCount);
			});

			ReadWithBitStream(bytes, bs =>
			{
				bs.Read(out ulong value32, 32);
				Assert.AreEqual(0x89ABCDEFUL, value32);

				bs.Read(out ulong value33, 33);
				Assert.AreEqual(0x1FEDCBA98UL, value33);

				bs.Read(out ulong value64);
				Assert.AreEqual(0x0123456789ABCDEFUL, value64);
			});
		}

		[TestMethod]
		public void ReadSigned_TruncatedValues_ControlsSignExtension()
		{
			byte[] sbyteBytes = WriteWithBitStream(bs => bs.Write((sbyte)-3, 3));
			ReadWithBitStream(sbyteBytes, bs => Assert.AreEqual(5, bs.ReadSByte(3)));
			ReadWithBitStream(sbyteBytes, bs => Assert.AreEqual(-3, bs.ReadSByte(3, signExtend: true)));

			byte[] shortBytes = WriteWithBitStream(bs => bs.Write((short)-321, 10));
			ReadWithBitStream(shortBytes, bs => Assert.AreEqual(703, bs.ReadInt16(10)));
			ReadWithBitStream(shortBytes, bs => Assert.AreEqual(-321, bs.ReadInt16(10, signExtend: true)));

			byte[] intBytes = WriteWithBitStream(bs => bs.Write(-123456, 20));
			ReadWithBitStream(intBytes, bs => Assert.AreEqual(925120, bs.ReadInt32(20)));
			ReadWithBitStream(intBytes, bs => Assert.AreEqual(-123456, bs.ReadInt32(20, signExtend: true)));

			byte[] longBytes = WriteWithBitStream(bs => bs.Write(-5L, 4));
			ReadWithBitStream(longBytes, bs => Assert.AreEqual(11L, bs.ReadInt64(4)));
			ReadWithBitStream(longBytes, bs => Assert.AreEqual(-5L, bs.ReadInt64(4, signExtend: true)));
		}

		[TestMethod]
		public void Stream_ScalarDelegates_RoundTripsRepresentativeValues()
		{
			char writeChar = 'Z';
			short writeShort = -17;
			bool writeBool = true;
			float writeFloat = Bitwise.ByteSwap.SingleFromUInt32(0xC0A00000U);

			byte[] bytes = WriteWithBitStream(bs =>
			{
				bs.Stream(ref writeChar, 7);
				bs.Stream(ref writeShort, 6, signExtend: true);
				bs.Stream(ref writeBool);
				bs.Stream(ref writeFloat);
			});

			char readChar = default;
			short readShort = default;
			bool readBool = default;
			float readFloat = default;
			ReadWithBitStream(bytes, bs =>
			{
				bs.Stream(ref readChar, 7);
				bs.Stream(ref readShort, 6, signExtend: true);
				bs.Stream(ref readBool);
				bs.Stream(ref readFloat);
			});

			Assert.AreEqual(writeChar, readChar);
			Assert.AreEqual(writeShort, readShort);
			Assert.AreEqual(writeBool, readBool);
			Assert.AreEqual(BitConverter.SingleToInt32Bits(writeFloat), BitConverter.SingleToInt32Bits(readFloat));
		}

		[TestMethod]
		public void StreamFixedArray_SignedValues_RoundTripsWithSignExtension()
		{
			var writeValues = new short[] { 10, -3, 2, -1, 20 };
			byte[] bytes = WriteWithBitStream(bs =>
				bs.StreamFixedArray(writeValues.AsSpan(1, 3), 3, signExtend: true));

			var readValues = new short[] { 10, 0, 0, 0, 20 };
			ReadWithBitStream(bytes, bs =>
				bs.StreamFixedArray(readValues.AsSpan(1, 3), 3, signExtend: true));

			CollectionAssert.AreEqual(writeValues, readValues);
		}

		[TestMethod]
		public void StreamArray_ByteValues_RoundTripsLengthAndElements()
		{
			var writeValues = new byte[] { 1, 2, 3 };
			byte[] bytes = WriteWithBitStream(bs => bs.StreamArray(ref writeValues, 3, 2));

			byte[]? readValues = null;
			ReadWithBitStream(bytes, bs => bs.StreamArray(ref readValues!, 3, 2));

			CollectionAssert.AreEqual(writeValues, readValues);
		}

		[TestMethod]
		public void StreamElements_IntValues_RoundTripsCountAndElements()
		{
			var writeValues = new List<int> { -1, 3, -2 };
			byte[] bytes = WriteWithBitStream(bs => bs.StreamElements(writeValues, 3, 3, signExtend: true));

			var readValues = new List<int>();
			ReadWithBitStream(bytes, bs => bs.StreamElements(readValues, 3, 3, signExtend: true));

			CollectionAssert.AreEqual(writeValues, readValues);
		}

		[TestMethod]
		public void StreamElements_SingleValues_RoundTripsBitPatterns()
		{
			var writeValues = new List<float> {
				Bitwise.ByteSwap.SingleFromUInt32(0x3F800000U),
				Bitwise.ByteSwap.SingleFromUInt32(0xFFC00001U),
			};
			byte[] bytes = WriteWithBitStream(bs => bs.StreamElements(writeValues, 3));

			var readValues = new List<float>();
			ReadWithBitStream(bytes, bs => bs.StreamElements(readValues, 3));

			Assert.AreEqual(writeValues.Count, readValues.Count);
			Assert.AreEqual(BitConverter.SingleToInt32Bits(writeValues[0]), BitConverter.SingleToInt32Bits(readValues[0]));
			Assert.AreEqual(BitConverter.SingleToInt32Bits(writeValues[1]), BitConverter.SingleToInt32Bits(readValues[1]));
		}

		[TestMethod]
		public void StreamDouble_CurrentBehavior_WritesLowerThirtyTwoBitsOnly()
		{
			const ulong kWriteBits = 0x3FF3C083126E978DUL;
			double writeValue = BitConverter.Int64BitsToDouble(unchecked((long)kWriteBits));

			byte[] bytes = WriteWithBitStream(bs => bs.Stream(ref writeValue));

			// Characterizes the retained handwritten double writer; this generator packet must not silently fix it.
			CollectionAssert.AreEqual(new byte[] { 0x12, 0x6E, 0x97, 0x8D }, bytes);

			double readValue = default;
			ReadWithBitStream(bytes, bs => bs.Stream(ref readValue));

			Assert.AreEqual(unchecked((long)0x126E978D00000000UL), BitConverter.DoubleToInt64Bits(readValue));
		}

		[TestMethod]
		public void ReadWord_AfterShortInitialFill_ReadsZeroPaddedCacheBits()
		{
			using (var ms = new MemoryStream(new byte[] { 0xFF }))
			using (var bs = new IO.BitStream(ms, FileAccess.Read))
			{
				bs.StreamMode = FileAccess.Read;
				bs.ThrowOnOverflow = FileAccess.Read;

				bs.ReadWord(out uint prefix, 7);
				Assert.AreEqual(0x7FU, prefix);

				bs.ReadWord(out uint suffix, 2);
				Assert.AreEqual(0b10U, suffix);
				Assert.AreEqual(9, bs.BitPosition);
			}
		}

		static byte[] WriteWithBitStream(Action<IO.BitStream> write)
		{
			using (var ms = new MemoryStream())
			{
				using (var bs = new IO.BitStream(ms, FileAccess.Write))
				{
					bs.StreamMode = FileAccess.Write;
					write(bs);
				}

				return ms.ToArray();
			}
		}

		static void ReadWithBitStream(byte[] bytes, Action<IO.BitStream> read)
		{
			using (var ms = new MemoryStream(bytes))
			using (var bs = new IO.BitStream(ms, FileAccess.Read))
			{
				bs.StreamMode = FileAccess.Read;
				read(bs);
			}
		}
	};
}
