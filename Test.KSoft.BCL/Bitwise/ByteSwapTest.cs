using System;
using System.Buffers.Binary;
using System.Diagnostics;
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
		const int kBenchmarkWarmupIterations = 20_000;
		const int kBenchmarkIterations = 2_000_000;

		static ushort gBenchmarkUInt16;
		static uint gBenchmarkUInt32;
		static ulong gBenchmarkUInt64;

		static short[] CreateInt32SwapCodes() =>
		[
			(short)BsCode.ArrayStart, 1,
			(short)BsCode.Int32,
			(short)BsCode.ArrayEnd,
		];

		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void ByteSwap_SwapIntegersTest()
		{
			ulong value_before = kBeforeValue;
			ulong value_after = kAfterValue;

			// UInt64
			Assert.AreEqual(value_after, ByteSwap.SwapUInt64(value_before));
			Assert.AreEqual((long)value_after, ByteSwap.SwapInt64((long)value_before));

			// UInt32
			value_before >>= 32;
			value_after >>= 32;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt32((uint)value_before));
			Assert.AreEqual((int)value_after,  ByteSwap.SwapInt32 ((int) value_before));

			// UInt16
			value_before >>= 16;
			value_after >>= 16;
			Assert.AreEqual((ushort)value_after, ByteSwap.SwapUInt16((ushort)value_before));
			Assert.AreEqual((short)value_after,  ByteSwap.SwapInt16 ((short) value_before));

			// UInt40
			value_before = kBeforeValueUInt40;
			value_after = kAfterValueUInt40;
			Assert.AreEqual(value_after, ByteSwap.SwapUInt40(value_before));
			Assert.AreEqual((long)value_after, ByteSwap.SwapInt40((long)value_before));

			// UInt24
			value_before = kBeforeValueUInt24;
			value_after = kAfterValueUInt24;
			Assert.AreEqual((uint)value_after, ByteSwap.SwapUInt24((uint)value_before));
			Assert.AreEqual((int)value_after,  ByteSwap.SwapInt24 ((int) value_before));
		}

		[TestMethod]
		public void SwapNaturalWidthsMatchesBinaryPrimitivesTest()
		{
			ushort[] values16 =
			[
				0x0000,
				0x0001,
				0x00FF,
				0x8000,
				0xFFFF,
			];
			foreach (ushort value in values16)
			{
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(value), ByteSwap.SwapUInt16(value));
				Assert.AreEqual(
					BinaryPrimitives.ReverseEndianness(unchecked((short)value)),
					ByteSwap.SwapInt16(unchecked((short)value)));
			}

			uint[] values32 =
			[
				0x00000000,
				0x00000001,
				0x00FF00FF,
				0x80000000,
				0xFFFFFFFF,
			];
			foreach (uint value in values32)
			{
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(value), ByteSwap.SwapUInt32(value));
				Assert.AreEqual(
					BinaryPrimitives.ReverseEndianness(unchecked((int)value)),
					ByteSwap.SwapInt32(unchecked((int)value)));
			}

			ulong[] values64 =
			[
				0x0000000000000000,
				0x0000000000000001,
				0x00FF00FF00FF00FF,
				0x8000000000000000,
				0xFFFFFFFFFFFFFFFF,
			];
			foreach (ulong value in values64)
			{
				Assert.AreEqual(BinaryPrimitives.ReverseEndianness(value), ByteSwap.SwapUInt64(value));
				Assert.AreEqual(
					BinaryPrimitives.ReverseEndianness(unchecked((long)value)),
					ByteSwap.SwapInt64(unchecked((long)value)));
			}
		}

		[TestMethod]
		public void SwapNaturalWidthByRefMatchesReturnValueTest()
		{
			// The return-value and by-ref overloads are generated as separate bodies. Keep both covered so the
			// natural-width BCL route cannot accidentally modernize only one overload family.
			ushort u16 = 0x1234;
			ByteSwap.Swap(ref u16);
			Assert.AreEqual(ByteSwap.SwapUInt16(0x1234), u16);

			short i16_original = unchecked((short)0x9234);
			short i16 = i16_original;
			ByteSwap.Swap(ref i16);
			Assert.AreEqual(ByteSwap.SwapInt16(i16_original), i16);

			uint u32 = 0x12345678U;
			ByteSwap.Swap(ref u32);
			Assert.AreEqual(ByteSwap.SwapUInt32(0x12345678U), u32);

			int i32_original = unchecked((int)0x92345678U);
			int i32 = i32_original;
			ByteSwap.Swap(ref i32);
			Assert.AreEqual(ByteSwap.SwapInt32(i32_original), i32);

			ulong u64 = 0x123456789ABCDEF0UL;
			ByteSwap.Swap(ref u64);
			Assert.AreEqual(ByteSwap.SwapUInt64(0x123456789ABCDEF0UL), u64);

			long i64_original = unchecked((long)0x923456789ABCDEF0UL);
			long i64 = i64_original;
			ByteSwap.Swap(ref i64);
			Assert.AreEqual(ByteSwap.SwapInt64(i64_original), i64);
		}

		[TestMethod]
		public void SwapSignedPartialWidthEdgeCasesTest()
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
		public void SwapFloatingPointPreservesBitPayloadsTest()
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
				float value = ByteSwap.SingleFromUInt32(bits);
				float swapped = ByteSwap.SwapSingle(value);
				Assert.AreEqual(ByteSwap.SwapUInt32(bits), ByteSwap.SingleToUInt32(swapped));

				ByteSwap.SwapSingle(ref value);
				Assert.AreEqual(ByteSwap.SwapUInt32(bits), ByteSwap.SingleToUInt32(value));
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
				double value = ByteSwap.DoubleFromUInt64(bits);
				double swapped = ByteSwap.SwapDouble(value);
				Assert.AreEqual(ByteSwap.SwapUInt64(bits), ByteSwap.DoubleToUInt64(swapped));

				ByteSwap.SwapDouble(ref value);
				Assert.AreEqual(ByteSwap.SwapUInt64(bits), ByteSwap.DoubleToUInt64(value));
			}
		}

		[TestMethod]
		public void ByteSwap_ReplaceBytesTest()
		{
			byte[] buffer = new byte[sizeof(ulong)];
			byte[] buffer_bc;
			ulong value = kBeforeValue;

			// UInt64
			ByteSwap.ReplaceBytes(buffer, 0, value);
			buffer_bc = BitConverter.GetBytes(value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt40
			value = kBeforeValueUInt40;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt40(buffer, 0, value);
			buffer_bc = BitConverter.GetBytes(value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt32
			value >>= 32;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (uint)value);
			buffer_bc = BitConverter.GetBytes((uint)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt24
			value = kBeforeValueUInt24;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt24(buffer, 0, (uint)value);
			buffer_bc = BitConverter.GetBytes((uint)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt16
			value >>= 16;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (ushort)value);
			buffer_bc = BitConverter.GetBytes((ushort)value);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));
		}

		[TestMethod]
		public void ReplaceBytesWritesHostEndianAtOffsetsTest()
		{
			byte[] buffer = CreateSentinelBuffer(24);

			int next_offset = ByteSwap.ReplaceBytes(buffer, 1, (ushort)0x1234);
			Assert.AreEqual(1 + sizeof(ushort), next_offset);
			AssertHostEndianBytes(buffer, 1, 0x1234, sizeof(ushort));

			next_offset = ByteSwap.ReplaceBytesUInt24(buffer, 5, 0x123456);
			Assert.AreEqual(5 + ByteSwap.kSizeOfUInt24, next_offset);
			AssertHostEndianBytes(buffer, 5, 0x123456, ByteSwap.kSizeOfUInt24);

			next_offset = ByteSwap.ReplaceBytes(buffer, 9, 0x12345678U);
			Assert.AreEqual(9 + sizeof(uint), next_offset);
			AssertHostEndianBytes(buffer, 9, 0x12345678, sizeof(uint));

			next_offset = ByteSwap.ReplaceBytesUInt40(buffer, 14, 0x123456789AUL);
			Assert.AreEqual(14 + ByteSwap.kSizeOfUInt40, next_offset);
			AssertHostEndianBytes(buffer, 14, 0x123456789AUL, ByteSwap.kSizeOfUInt40);

			Assert.AreEqual(0xCC, buffer[0]);
			Assert.AreEqual(0xCC, buffer[4]);
			Assert.AreEqual(0xCC, buffer[8]);
			Assert.AreEqual(0xCC, buffer[13]);
			Assert.AreEqual(0xCC, buffer[19]);
		}

		// NOTE: ByteSwap_ReplaceBytesTest should be tested before SwapBufferTest (see OrderedTests_ByteSwap)
		[TestMethod]
		public void ByteSwap_SwapBufferTest()
		{
			byte[] buffer = new byte[sizeof(ulong)];
			byte[] buffer_bc;
			ulong value_before = kBeforeValue;
			ulong value_after = kAfterValue;

			// UInt64
			ByteSwap.ReplaceBytes(buffer, 0, value_before);
			ByteSwap.SwapInt64(buffer, 0);
			buffer_bc = BitConverter.GetBytes(value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt32
			value_before >>= 32;
			value_after >>= 32;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (uint)value_before);
			ByteSwap.SwapInt32(buffer, 0);
			buffer_bc = BitConverter.GetBytes((uint)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt16
			value_before >>= 16;
			value_after >>= 16;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytes(buffer, 0, (ushort)value_before);
			ByteSwap.SwapInt16(buffer, 0);
			buffer_bc = BitConverter.GetBytes((ushort)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt40
			value_before = kBeforeValueUInt40;
			value_after = kAfterValueUInt40;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt40(buffer, 0, value_before);
			ByteSwap.SwapInt40(buffer, 0);
			buffer_bc = BitConverter.GetBytes(value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));

			// UInt24
			value_before = kBeforeValueUInt24;
			value_after = kAfterValueUInt24;
			Array.Clear(buffer, 0, buffer.Length);
			ByteSwap.ReplaceBytesUInt24(buffer, 0, (uint)value_before);
			ByteSwap.SwapInt24(buffer, 0);
			buffer_bc = BitConverter.GetBytes((uint)value_after);
			Assert.IsTrue(buffer_bc.EqualsArray(buffer));
		}

		[TestMethod]
		public void SwapBufferOverlappingWindowsTest()
		{
			byte[] buffer =
			[
				0x01,
				0x02,
				0x03,
				0x04,
				0x05,
				0x06,
			];

			int next_offset = ByteSwap.SwapInt32(buffer, 0);
			Assert.AreEqual(sizeof(int), next_offset);
			CollectionAssert.AreEqual(new byte[] { 0x04, 0x03, 0x02, 0x01, 0x05, 0x06 }, buffer);

			next_offset = ByteSwap.SwapInt32(buffer, 2);
			Assert.AreEqual(2 + sizeof(int), next_offset);
			CollectionAssert.AreEqual(new byte[] { 0x04, 0x03, 0x06, 0x05, 0x01, 0x02 }, buffer);
		}

		[TestMethod]
		public void BufferOffsetValidationTest()
		{
			AssertThrows<ArgumentNullException>(() => ByteSwap.SwapUInt16(null, 0));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt16(new byte[2], -1));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt16(new byte[2], 1));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt16(new byte[2], 2));

			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt24(new byte[3], 1));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt32(new byte[4], 1));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt40(new byte[5], 1));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.SwapUInt64(new byte[8], 1));

			AssertThrows<ArgumentNullException>(() => ByteSwap.ReplaceBytes(null, 0, 0x1234U));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.ReplaceBytes(new byte[4], -1, 0x1234U));
			AssertThrows<ArgumentOutOfRangeException>(() => ByteSwap.ReplaceBytes(new byte[4], 1, 0x12345678U));
			AssertThrows<ArgumentOutOfRangeException>(
				() => ByteSwap.ReplaceBytesUInt24(new byte[3], 1, 0x123456));
			AssertThrows<ArgumentOutOfRangeException>(
				() => ByteSwap.ReplaceBytesUInt40(new byte[5], 1, 0x123456789AUL));
			AssertThrows<ArgumentOutOfRangeException>(
				() => ByteSwap.SwapData(ByteSwap.kInt32Definition, new byte[sizeof(uint) - 1]));
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

		// #NOTE Assumes ByteSwap.ReplaceBytes isn't broken
		[TestMethod]
		public void ByteSwap_SwapDataIntegersTest()
		{
			var buffer = new byte[sizeof(ulong) + sizeof(uint) + sizeof(ushort)];
			int buffer_index;

			buffer_index = 0;
			{
				ReplaceBytes(Bits.kInt64BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
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
		public void ByteSwap_SwapDataNestedArraysTest()
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
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);
				ReplaceBytes(Bits.kInt16BitCount, kBeforeValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt64BitCount, kSkipValue, buffer, ref buffer_index);

				ReplaceBytes(Bits.kInt32BitCount, kBeforeValue, buffer, ref buffer_index);
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

		private static void ReplaceBytes(int bitCount, ulong value, byte[] buffer, ref int bufferIndex)
		{
			switch (bitCount)
			{
				case Bits.kInt16BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((ushort)value));
					break;

				case Bits.kInt32BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((uint)value));
					break;

				case Bits.kInt64BitCount:
					bufferIndex = ByteSwap.ReplaceBytes(buffer, bufferIndex, unchecked((ulong)value));
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

		private static void AssertThrows<TException>(Action action)
			where TException : Exception
		{
			try
			{
				action();
			}
			catch (TException)
			{
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(string.Format("Expected {0}, got {1}: {2}",
					typeof(TException).Name, ex.GetType().Name, ex.Message));
			}

			Assert.Fail(string.Format("Expected {0}, but no exception was thrown.", typeof(TException).Name));
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

		#region Benchmark
		[TestMethod]
		[TestCategory("Benchmark")]
		public void BenchmarkCurrentVsBinaryPrimitivesPrototype()
		{
			WarmUpBenchmarkPrototype();

			TimeSpan current16 = Measure(() => gBenchmarkUInt16 = BenchmarkCurrentUInt16(kBenchmarkIterations));
			ushort current16_result = gBenchmarkUInt16;
			TimeSpan bcl16 = Measure(() => gBenchmarkUInt16 = BenchmarkBclUInt16(kBenchmarkIterations));
			Assert.AreEqual(current16_result, gBenchmarkUInt16);

			TimeSpan current32 = Measure(() => gBenchmarkUInt32 = BenchmarkCurrentUInt32(kBenchmarkIterations));
			uint current32_result = gBenchmarkUInt32;
			TimeSpan bcl32 = Measure(() => gBenchmarkUInt32 = BenchmarkBclUInt32(kBenchmarkIterations));
			Assert.AreEqual(current32_result, gBenchmarkUInt32);

			TimeSpan current64 = Measure(() => gBenchmarkUInt64 = BenchmarkCurrentUInt64(kBenchmarkIterations));
			ulong current64_result = gBenchmarkUInt64;
			TimeSpan bcl64 = Measure(() => gBenchmarkUInt64 = BenchmarkBclUInt64(kBenchmarkIterations));
			Assert.AreEqual(current64_result, gBenchmarkUInt64);

			WriteBenchmarkResult("UInt16", current16, bcl16);
			WriteBenchmarkResult("UInt32", current32, bcl32);
			WriteBenchmarkResult("UInt64", current64, bcl64);
		}

		private static TimeSpan Measure(Action action)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var stopwatch = Stopwatch.StartNew();
			action();
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}

		private static void WarmUpBenchmarkPrototype()
		{
			gBenchmarkUInt16 = BenchmarkCurrentUInt16(kBenchmarkWarmupIterations);
			gBenchmarkUInt16 = BenchmarkBclUInt16(kBenchmarkWarmupIterations);
			gBenchmarkUInt32 = BenchmarkCurrentUInt32(kBenchmarkWarmupIterations);
			gBenchmarkUInt32 = BenchmarkBclUInt32(kBenchmarkWarmupIterations);
			gBenchmarkUInt64 = BenchmarkCurrentUInt64(kBenchmarkWarmupIterations);
			gBenchmarkUInt64 = BenchmarkBclUInt64(kBenchmarkWarmupIterations);
		}

		private static ushort BenchmarkCurrentUInt16(int iterations)
		{
			ushort result = 0;
			for (int x = 0; x < iterations; x++)
			{
				result ^= ByteSwap.SwapUInt16(unchecked((ushort)x));
			}

			return result;
		}

		private static ushort BenchmarkBclUInt16(int iterations)
		{
			ushort result = 0;
			for (int x = 0; x < iterations; x++)
			{
				result ^= BinaryPrimitives.ReverseEndianness(unchecked((ushort)x));
			}

			return result;
		}

		private static uint BenchmarkCurrentUInt32(int iterations)
		{
			uint result = 0;
			uint value = 0x12345678;
			for (int x = 0; x < iterations; x++)
			{
				result ^= ByteSwap.SwapUInt32(value + unchecked((uint)x));
			}

			return result;
		}

		private static uint BenchmarkBclUInt32(int iterations)
		{
			uint result = 0;
			uint value = 0x12345678;
			for (int x = 0; x < iterations; x++)
			{
				result ^= BinaryPrimitives.ReverseEndianness(value + unchecked((uint)x));
			}

			return result;
		}

		private static ulong BenchmarkCurrentUInt64(int iterations)
		{
			ulong result = 0;
			ulong value = 0x123456789ABCDEF0;
			for (int x = 0; x < iterations; x++)
			{
				result ^= ByteSwap.SwapUInt64(value + unchecked((uint)x));
			}

			return result;
		}

		private static ulong BenchmarkBclUInt64(int iterations)
		{
			ulong result = 0;
			ulong value = 0x123456789ABCDEF0;
			for (int x = 0; x < iterations; x++)
			{
				result ^= BinaryPrimitives.ReverseEndianness(value + unchecked((uint)x));
			}

			return result;
		}

		private void WriteBenchmarkResult(string name, TimeSpan current, TimeSpan bcl)
		{
			double ratio = bcl.TotalMilliseconds / current.TotalMilliseconds;
			TestContext.WriteLine("{0}: current={1:F3} ms, BinaryPrimitives={2:F3} ms, bcl/current={3:F3}",
				name, current.TotalMilliseconds, bcl.TotalMilliseconds, ratio);
		}
		#endregion
	};
}
