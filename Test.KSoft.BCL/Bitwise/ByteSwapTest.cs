using System;
using System.Buffers.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class ByteSwapTest : BaseTestClass
	{
		const ulong kBeforeValue = 0x1234123412341234;
		const ulong kAfterValue = 0x3412341234123412;
		const ulong kSkipValue = 0x012345678ABCDEF0;

		const ulong kBeforeValueUInt40 = 0x123456789A;
		const ulong kAfterValueUInt40 = 0x9A78563412;

		const ulong kBeforeValueUInt24 = 0x123456;
		const ulong kAfterValueUInt24 = 0x563412;

		static short[] CreateInt32SwapCodes() =>
		[
			(short)BsCode.ArrayStart, 1,
			(short)BsCode.Int32,
			(short)BsCode.ArrayEnd,
		];

		[TestMethod]
		public void Swap_PartialWidthIntegerValues_ReversesByteOrder()
		{
			// UInt40
			ulong value_before = kBeforeValueUInt40;
			ulong value_after = kAfterValueUInt40;
			Assert.AreEqual(value_after, ByteSwap.SwapUInt40(value_before));
			Assert.AreEqual((long)value_after, ByteSwap.SwapInt40((long)value_before));

			// UInt24
			value_before = kBeforeValueUInt24;
			value_after = kAfterValueUInt24;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt24((uint)value_before));
			Assert.AreEqual((int)value_after,  ByteSwap.SwapInt24 ((int) value_before));
		}

		[TestMethod]
		public void Swap_SignedPartialWidths_PreservesExpectedBitPatterns()
		{
			Assert.AreEqual(0x00FFFFFF, ByteSwap.SwapInt24(-1));
			Assert.AreEqual(0x00000080, ByteSwap.SwapInt24(unchecked((int)0xFF800000)));
			Assert.AreEqual(0x00FFFF7F, ByteSwap.SwapInt24(0x007FFFFF));
			Assert.AreEqual(0x00563412U, ByteSwap.SwapUInt24(0xFF123456));

			int int24 = unchecked((int)0xFF800000);
			uint uint24 = 0xFF123456;
			ByteSwap.SwapInt24(ref int24);
			ByteSwap.SwapUInt24(ref uint24);
			Assert.AreEqual(0x00000080, int24);
			Assert.AreEqual(0x00563412U, uint24);

			Assert.AreEqual(0x000000FFFFFFFFFFL, ByteSwap.SwapInt40(-1));
			Assert.AreEqual(0x0000000000000080L, ByteSwap.SwapInt40(unchecked((long)0xFFFFFF8000000000UL)));
			Assert.AreEqual(0x000000FFFFFFFF7FL, ByteSwap.SwapInt40(0x0000007FFFFFFFFF));
			Assert.AreEqual(0x0000009A78563412UL, ByteSwap.SwapUInt40(0x00FFFF123456789AUL));

			long int40 = unchecked((long)0xFFFFFF8000000000UL);
			ulong uint40 = 0x00FFFF123456789AUL;
			ByteSwap.SwapInt40(ref int40);
			ByteSwap.SwapUInt40(ref uint40);
			Assert.AreEqual(0x0000000000000080L, int40);
			Assert.AreEqual(0x0000009A78563412UL, uint40);
		}

		[TestMethod]
		public void Swap_FloatingPointValues_PreservesBitPayloads()
		{
			uint[] single_bits =
			[
				0x00000000,
				0x80000000,
				0x3F800000,
				0x7FC12345,
				0xFFC12345,
			];
			foreach (uint bits in single_bits)
			{
				float value = BitConverter.UInt32BitsToSingle(bits);
				byte[] bytes = new byte[sizeof(float)];
				BitConverter.TryWriteBytes(bytes.AsSpan(), value);
				Assert.AreEqual(bits, BitConverter.ToUInt32(bytes));

				float swapped = ByteSwap.SwapSingle(value);
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(bits), BitConverter.SingleToUInt32Bits(swapped));

				ByteSwap.SwapSingle(ref value);
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(bits), BitConverter.SingleToUInt32Bits(value));
			}

			ulong[] double_bits =
			[
				0x0000000000000000,
				0x8000000000000000,
				0x3FF0000000000000,
				0x7FF8123456789ABC,
				0xFFF8123456789ABC,
			];
			foreach (ulong bits in double_bits)
			{
				double value = BitConverter.UInt64BitsToDouble(bits);
				byte[] bytes = new byte[sizeof(double)];
				BitConverter.TryWriteBytes(bytes.AsSpan(), value);
				Assert.AreEqual(bits, BitConverter.ToUInt64(bytes));

				double swapped = ByteSwap.SwapDouble(value);
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(bits), BitConverter.DoubleToUInt64Bits(swapped));

				ByteSwap.SwapDouble(ref value);
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(bits), BitConverter.DoubleToUInt64Bits(value));
			}
		}

		[TestMethod]
		public void ReplaceBytes_PartialWidthSpans_WriteHostEndianBytes()
		{
			byte[] buffer = CreateSentinelBuffer(21);

			ByteSwap.ReplaceBytesUInt24(buffer.AsSpan(1, ByteSwap.kSizeOfUInt24), 0x123456);
			AssertHostEndianBytes(buffer, 1, 0x123456, ByteSwap.kSizeOfUInt24);

			ByteSwap.ReplaceBytesInt24(
				buffer.AsSpan(5, ByteSwap.kSizeOfInt24),
				unchecked((int)0xFF923456));
			AssertHostEndianBytes(buffer, 5, 0x923456, ByteSwap.kSizeOfInt24);

			ByteSwap.ReplaceBytesUInt40(buffer.AsSpan(9, ByteSwap.kSizeOfUInt40), 0x123456789AUL);
			AssertHostEndianBytes(buffer, 9, 0x123456789AUL, ByteSwap.kSizeOfUInt40);

			ByteSwap.ReplaceBytesInt40(
				buffer.AsSpan(15, ByteSwap.kSizeOfInt40),
				unchecked((long)0xFFFFFF923456789AUL));
			AssertHostEndianBytes(buffer, 15, 0x923456789AUL, ByteSwap.kSizeOfInt40);

			Assert.AreEqual(0xCC, buffer[0]);
			Assert.AreEqual(0xCC, buffer[4]);
			Assert.AreEqual(0xCC, buffer[8]);
			Assert.AreEqual(0xCC, buffer[14]);
			Assert.AreEqual(0xCC, buffer[20]);
		}

		[TestMethod]
		public void SwapBuffer_PartialWidthSpans_SwapInPlaceAndPreserveSentinels()
		{
			byte[] buffer =
			[
				0xCC, 0x12, 0x34, 0x56, 0xCC,
				0x92, 0x34, 0x56, 0xCC,
				0x12, 0x34, 0x56, 0x78, 0x9A, 0xCC,
				0x92, 0x34, 0x56, 0x78, 0x9A, 0xCC,
			];

			ByteSwap.SwapUInt24(buffer.AsSpan(1, ByteSwap.kSizeOfUInt24));
			ByteSwap.SwapInt24(buffer.AsSpan(5, ByteSwap.kSizeOfInt24));
			ByteSwap.SwapUInt40(buffer.AsSpan(9, ByteSwap.kSizeOfUInt40));
			ByteSwap.SwapInt40(buffer.AsSpan(15, ByteSwap.kSizeOfInt40));

			CollectionAssert.AreEqual(
				new byte[]
				{
					0xCC, 0x56, 0x34, 0x12, 0xCC,
					0x56, 0x34, 0x92, 0xCC,
					0x9A, 0x78, 0x56, 0x34, 0x12, 0xCC,
					0x9A, 0x78, 0x56, 0x34, 0x92, 0xCC,
				},
				buffer);

			byte[] overlapping = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06];
			ByteSwap.SwapUInt24(overlapping.AsSpan(0, ByteSwap.kSizeOfUInt24));
			ByteSwap.SwapInt40(overlapping.AsSpan(1, ByteSwap.kSizeOfInt40));
			CollectionAssert.AreEqual(new byte[] { 0x03, 0x06, 0x05, 0x04, 0x01, 0x02 }, overlapping);
		}

		[TestMethod]
		public void PartialWidthSpanOperations_ShortSpansFailBeforeMutation()
		{
			byte[] short24 = [0x12, 0x34];
			byte[] expected24 = (byte[])short24.Clone();
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt24(short24));
			CollectionAssert.AreEqual(expected24, short24);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.SwapInt24(short24));
			CollectionAssert.AreEqual(expected24, short24);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.ReplaceBytesUInt24(short24, 0x123456));
			CollectionAssert.AreEqual(expected24, short24);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.ReplaceBytesInt24(short24, -1));
			CollectionAssert.AreEqual(expected24, short24);

			byte[] short40 = [0x12, 0x34, 0x56, 0x78];
			byte[] expected40 = (byte[])short40.Clone();
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt40(short40));
			CollectionAssert.AreEqual(expected40, short40);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.SwapInt40(short40));
			CollectionAssert.AreEqual(expected40, short40);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(
				() => ByteSwap.ReplaceBytesUInt40(short40, 0x123456789AUL));
			CollectionAssert.AreEqual(expected40, short40);
			Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ByteSwap.ReplaceBytesInt40(short40, -1));
			CollectionAssert.AreEqual(expected40, short40);
		}

		[TestMethod]
		public void BsDefinition_InvalidArguments_ThrowExpectedExceptions()
		{
			var codes = CreateInt32SwapCodes();

			AssertThrowsArgumentNull("name", () => new ByteSwap.BsDefinition(null!, sizeof(int), codes));
			AssertThrowsArgumentNull("name", () => new ByteSwap.BsDefinition("", sizeof(int), codes));
			AssertThrowsArgumentOutOfRange("sizeOf", () => new ByteSwap.BsDefinition("Int32", 0, codes));
			AssertThrowsArgumentNull("bsCodes", () => new ByteSwap.BsDefinition("Int32", sizeof(int), null!));
			AssertThrowsArgument("bsCodes", () => new ByteSwap.BsDefinition("Int32", sizeof(int), (short)BsCode.Int32));
		}

		[TestMethod]
		public void SwapData_InvalidArguments_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("definition", () => ByteSwap.SwapData(null!, new byte[sizeof(int)]));
			AssertThrowsArgumentNull("buffer", () => ByteSwap.SwapData(ByteSwap.kInt32Definition, null!));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => ByteSwap.SwapData(ByteSwap.kInt32Definition, new byte[sizeof(int)], -1));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => ByteSwap.SwapData(ByteSwap.kInt32Definition, new byte[sizeof(int)], sizeof(int) + 1));
			AssertThrowsArgumentOutOfRange("count",
				() => ByteSwap.SwapData(ByteSwap.kInt32Definition, new byte[sizeof(int)], count: 0));
			AssertThrowsArgumentOutOfRange("count",
				() => ByteSwap.SwapData(ByteSwap.kInt32Definition, new byte[sizeof(int)], count: 2));
		}

		[TestMethod]
		public void Swapper_InvalidArguments_ThrowExpectedExceptions()
		{
			var codes = CreateInt32SwapCodes();

			AssertThrowsArgumentOutOfRange("sizeOf", () => new ByteSwap.Swapper(0, codes));
			AssertThrowsArgumentNull("codes", () => new ByteSwap.Swapper(sizeof(int), null!));
			AssertThrowsArgumentNull("definition", () => new ByteSwap.Swapper(null!));

			var swapper = new ByteSwap.Swapper(ByteSwap.kInt32Definition);
			AssertThrowsArgumentNull("buffer", () => swapper.SwapData(null!));
			AssertThrowsArgumentOutOfRange("startIndex", () => swapper.SwapData(new byte[sizeof(int)], -1));
			AssertThrowsArgumentOutOfRange("startIndex", () => swapper.SwapData(new byte[sizeof(int)], sizeof(int) + 1));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => swapper.SwapData(null, -1, out _, out _));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => swapper.SwapData(new byte[sizeof(int)], sizeof(int) + 1, out _, out _));
		}

		[TestMethod]
		public void Swapper_NullBufferSizeQuery_ReturnsDefinitionSize()
		{
			var swapper = new ByteSwap.Swapper(ByteSwap.kInt32Definition);

			var result = swapper.SwapData(null, 0, out int sizeInBytes, out int sizeInCodes);

			Assert.AreEqual(TypeExtensions.kNone, result);
			Assert.AreEqual(sizeof(int), sizeInBytes);
			Assert.AreEqual(ByteSwap.kInt32Definition.ByteSwapCodes.Length, sizeInCodes);
		}

		// #NOTE Assumes BitConverter.TryWriteBytes isn't broken
		[TestMethod]
		public void SwapData_IntegerDefinition_SwapsExpectedBytes()
		{
			var buffer = new byte[sizeof(ulong) + sizeof(uint) + sizeof(ushort)];
			int buffer_index;

			buffer_index = 0;
			{
				WriteBytes(Bits.kInt64BitCount, kBeforeValue, buffer, ref buffer_index);
				WriteBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
				WriteBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt64Definition, buffer, buffer_index);
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt32Definition, buffer, buffer_index);
				buffer_index = ByteSwap.SwapData(ByteSwap.kInt16Definition, buffer, buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				AssertBytesAreEqual(Bits.kInt64BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);
		}

		[TestMethod]
		public void SwapData_NestedArrayDefinition_SwapsExpectedBytes()
		{
			var bs_codes = new short[]
			{
				(int)BsCode.ArrayStart, 1,
					(int)BsCode.ArrayStart, 2,
						(int)BsCode.Int16,
					(int)BsCode.ArrayEnd,
					(int)BsCode.Int32,
					(int)sizeof(ulong),
					(int)BsCode.Int32,
				(int)BsCode.ArrayEnd,
			};
			int structure_count = 2;
			int structure_size =
				(sizeof(ushort) * 2) +
				sizeof(uint) +
				sizeof(ulong) +
				sizeof(uint);
			var bs_definiton = new ByteSwap.BsDefinition("UnitTest", structure_size, bs_codes);

			var buffer = new byte[structure_size * structure_count];
			int buffer_index;

			buffer_index = 0;
			for (int x = 0; x < structure_count; x++)
			{
				WriteBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
				WriteBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);

				WriteBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);

				WriteBytes(Bits.kInt64BitCount, kSkipValue, buffer, ref buffer_index);

				WriteBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			{
				buffer_index += ByteSwap.SwapData(bs_definiton, buffer, buffer_index, structure_count);
			}
			Assert.AreEqual(buffer.Length, buffer_index);

			buffer_index = 0;
			for (int x = 0; x < structure_count; x++)
			{
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);
				AssertBytesAreEqual(Bits.kInt16BitCount, kAfterValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt64BitCount, kSkipValue, buffer, ref buffer_index);

				AssertBytesAreEqual(Bits.kInt32BitCount, kAfterValue, buffer, ref buffer_index);
			}
			Assert.AreEqual(buffer.Length, buffer_index);
		}

		private static void WriteBytes(int bitCount, ulong value, byte[] buffer, ref int bufferIndex)
		{
			switch (bitCount)
			{
				case Bits.kInt16BitCount:
					BitConverter.TryWriteBytes(
						buffer.AsSpan(bufferIndex, sizeof(ushort)),
						unchecked((ushort)value));
					bufferIndex += sizeof(ushort);
					break;

				case Bits.kInt32BitCount:
					BitConverter.TryWriteBytes(
						buffer.AsSpan(bufferIndex, sizeof(uint)),
						unchecked((uint)value));
					bufferIndex += sizeof(uint);
					break;

				case Bits.kInt64BitCount:
					BitConverter.TryWriteBytes(
						buffer.AsSpan(bufferIndex, sizeof(ulong)),
						unchecked((ulong)value));
					bufferIndex += sizeof(ulong);
					break;
			}
		}

		private static byte[] CreateSentinelBuffer(int length)
		{
			var buffer = new byte[length];
			for (int x = 0; x < buffer.Length; x++)
			{
				buffer[x] = 0xCC;
			}

			return buffer;
		}

		private static void AssertHostEndianBytes(byte[] buffer, int offset, ulong value, int byteCount)
		{
			for (int x = 0; x < byteCount; x++)
			{
				int shift = BitConverter.IsLittleEndian
					? x * Bits.kByteBitCount
					: (byteCount - 1 - x) * Bits.kByteBitCount;

				Assert.AreEqual((byte)(value >> shift), buffer[offset + x]);
			}
		}

		private void AssertBytesAreEqual(int bitCount, ulong expectedValue, byte[] buffer, ref int bufferIndex)
		{
			var invariant_culture_info = KSoft.Util.InvariantCultureInfo;

			switch (bitCount)
			{
				case Bits.kInt16BitCount:
					Assert.AreEqual(
						unchecked((ushort)expectedValue).ToString("X4", invariant_culture_info),
						BitConverter.ToUInt16(buffer, bufferIndex).ToString("X4", invariant_culture_info));
					bufferIndex += sizeof(ushort);
					break;

				case Bits.kInt32BitCount:
					Assert.AreEqual(
						unchecked((uint)expectedValue).ToString("X8", invariant_culture_info),
						BitConverter.ToUInt32(buffer, bufferIndex).ToString("X8", invariant_culture_info));
					bufferIndex += sizeof(uint);
					break;

				case Bits.kInt64BitCount:
					Assert.AreEqual(
						unchecked((ulong)expectedValue).ToString("X16", invariant_culture_info),
						BitConverter.ToUInt64(buffer, bufferIndex).ToString("X16", invariant_culture_info));
					bufferIndex += sizeof(ulong);
					break;
			}
		}

	};
}
