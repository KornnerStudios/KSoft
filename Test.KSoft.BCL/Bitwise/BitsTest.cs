using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class BitsTest : BaseTestClass
	{
		const ulong kEvenBits = 0xAAAAAAAAAAAAAAAAUL;
		const ulong kOddBits = 0x5555555555555555UL;
		const ulong kEvenNybbles = 0x3333333333333333UL;
		const ulong kOddNybbles = 0xCCCCCCCCCCCCCCCCUL;
		[SuppressMessage("Microsoft.Design", "CA1823:AvoidUnusedPrivateFields")]
		const ulong kMiddleNybbles = 0x6666666666666666UL; // Bit pattern middle bits are set in a nybble

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

		#region MemoryCopier
		[TestMethod]
		public void MemoryCopier_CopyWithOffsets_CopiesRequestedBytes()
		{
			var copier = new Bits.MemoryCopier<byte, byte>(false);
			byte[] source = [0x10, 0x20, 0x30, 0x40];
			byte[] destination = [0xAA, 0xBB, 0xCC, 0xDD];

			copier.Copy(destination, 1, source, 1, 2);

			CollectionAssert.AreEqual(new byte[] { 0xAA, 0x20, 0x30, 0xDD }, destination);
		}

		[TestMethod]
		public void MemoryCopier_CopyWithoutOffsets_CopiesRequestedBytes()
		{
			var copier = new Bits.MemoryCopier<byte, byte>(false);
			byte[] source = [0x10, 0x20, 0x30, 0x40];
			byte[] destination = [0xAA, 0xBB, 0xCC, 0xDD];

			copier.Copy(destination, source, 3);

			CollectionAssert.AreEqual(new byte[] { 0x10, 0x20, 0x30, 0xDD }, destination);
		}

		[TestMethod]
		public void MemoryCopier_CopyWithOffsets_InvalidArgumentsThrowExpectedExceptions()
		{
			var copier = new Bits.MemoryCopier<byte, byte>(false);

			AssertThrowsArgumentNull("dst", () => copier.Copy(null!, 0, new byte[1], 0, 1));
			AssertThrowsArgumentNull("src", () => copier.Copy(new byte[1], 0, null!, 0, 1));
			AssertThrowsArgumentOutOfRange("srcCopyCount", () => copier.Copy(new byte[1], 0, new byte[1], 0, -1));
			AssertThrowsArgumentOutOfRange("dstOffset", () => copier.Copy(new byte[1], -1, new byte[1], 0, 1));
			AssertThrowsArgumentOutOfRange("dstOffset", () => copier.Copy(new byte[1], 1, new byte[1], 0, 0));
			AssertThrowsArgumentOutOfRange("srcOffset", () => copier.Copy(new byte[1], 0, new byte[1], -1, 1));
			AssertThrowsArgumentOutOfRange("srcOffset", () => copier.Copy(new byte[1], 0, new byte[1], 1, 0));
			AssertThrowsArgumentOutOfRange("srcCopyCount", () => copier.Copy(new byte[2], 0, new byte[2], 1, 2));
		}

		[TestMethod]
		public void MemoryCopier_CopyWithoutOffsets_InvalidArgumentsThrowExpectedExceptions()
		{
			var copier = new Bits.MemoryCopier<byte, byte>(false);

			AssertThrowsArgumentNull("dst", () => copier.Copy(null!, new byte[1], 1));
			AssertThrowsArgumentNull("src", () => copier.Copy(new byte[1], null!, 1));
			AssertThrowsArgumentOutOfRange("srcCopyCount", () => copier.Copy(new byte[1], new byte[1], -1));
			AssertThrowsArgumentOutOfRange("srcCopyCount", () => copier.Copy(new byte[1], new byte[2], 2));
		}
		#endregion

		#region BitCount
		[TestMethod]
		public void Bits_BitCountTest()
		{
			Assert.AreEqual(0, Bits.BitCount(byte.MinValue));
			Assert.AreEqual(Bits.kByteBitCount / 2, Bits.BitCount( unchecked((byte)kEvenBits) ));
			Assert.AreEqual(Bits.kByteBitCount / 2, Bits.BitCount( unchecked((byte)kOddBits) ));
			Assert.AreEqual(Bits.kByteBitCount / 2, Bits.BitCount( unchecked((byte)kEvenNybbles) ));
			Assert.AreEqual(Bits.kByteBitCount / 2, Bits.BitCount( unchecked((byte)kOddNybbles) ));
			Assert.AreEqual(Bits.kByteBitCount, Bits.BitCount(byte.MaxValue));

			Assert.AreEqual(0, Bits.BitCount(ushort.MinValue));
			Assert.AreEqual(Bits.kInt16BitCount / 2, Bits.BitCount( unchecked((ushort)kEvenBits) ));
			Assert.AreEqual(Bits.kInt16BitCount / 2, Bits.BitCount( unchecked((ushort)kOddBits) ));
			Assert.AreEqual(Bits.kInt16BitCount / 2, Bits.BitCount( unchecked((ushort)kEvenNybbles) ));
			Assert.AreEqual(Bits.kInt16BitCount / 2, Bits.BitCount( unchecked((ushort)kOddNybbles) ));
			Assert.AreEqual(Bits.kInt16BitCount, Bits.BitCount(ushort.MaxValue));

			Assert.AreEqual(0, Bits.BitCount(uint.MinValue));
			Assert.AreEqual(Bits.kInt32BitCount / 2, Bits.BitCount( unchecked((uint)kEvenBits) ));
			Assert.AreEqual(Bits.kInt32BitCount / 2, Bits.BitCount( unchecked((uint)kOddBits) ));
			Assert.AreEqual(Bits.kInt32BitCount / 2, Bits.BitCount( unchecked((uint)kEvenNybbles) ));
			Assert.AreEqual(Bits.kInt32BitCount / 2, Bits.BitCount( unchecked((uint)kOddNybbles) ));
			Assert.AreEqual(Bits.kInt32BitCount, Bits.BitCount(uint.MaxValue));

			Assert.AreEqual(0, Bits.BitCount(ulong.MinValue));
			Assert.AreEqual(Bits.kInt64BitCount / 2, Bits.BitCount( unchecked((ulong)kEvenBits) ));
			Assert.AreEqual(Bits.kInt64BitCount / 2, Bits.BitCount( unchecked((ulong)kOddBits) ));
			Assert.AreEqual(Bits.kInt64BitCount / 2, Bits.BitCount( unchecked((ulong)kEvenNybbles) ));
			Assert.AreEqual(Bits.kInt64BitCount / 2, Bits.BitCount( unchecked((ulong)kOddNybbles) ));
			Assert.AreEqual(Bits.kInt64BitCount, Bits.BitCount(ulong.MaxValue));
		}
		[TestMethod]
		public void Bits_BitCountTest2()
		{
			{
				const ulong kBitCountValue = 0xAAAAAAAAAAAAAAAA;
				int i32;
				int expected_bit_count;

				expected_bit_count = Bits.kByteBitCount / 2;	i32 = Bits.BitCount(unchecked((byte)kBitCountValue));
				Assert.AreEqual(expected_bit_count, i32);
				Assert.AreEqual(System.Numerics.BitOperations.PopCount(unchecked((byte)kBitCountValue)), i32);

				expected_bit_count = Bits.kInt16BitCount / 2;	i32 = Bits.BitCount(unchecked((ushort)kBitCountValue));
				Assert.AreEqual(expected_bit_count, i32);
				Assert.AreEqual(System.Numerics.BitOperations.PopCount(unchecked((ushort)kBitCountValue)), i32);

				expected_bit_count = Bits.kInt32BitCount / 2;	i32 = Bits.BitCount(unchecked((uint)kBitCountValue));
				Assert.AreEqual(expected_bit_count, i32);
				Assert.AreEqual(System.Numerics.BitOperations.PopCount(unchecked((uint)kBitCountValue)), i32);

				expected_bit_count = Bits.kInt64BitCount / 2;	i32 = Bits.BitCount(kBitCountValue);
				Assert.AreEqual(expected_bit_count, i32);
				Assert.AreEqual(System.Numerics.BitOperations.PopCount(kBitCountValue), i32);
			}
		}

		[TestMethod]
		public void BitCountToMask_Test()
		{
			{
				uint u32;
				ulong u64;

				u32 = Bits.BitCountToMask32(0);
				Assert.AreEqual(0U,					u32);

				u32 = Bits.BitCountToMask32(Bits.kInt32BitCount);
				Assert.AreEqual(uint.MaxValue,		u32);

				u32 = Bits.BitCountToMask32(Bits.kInt32BitCount-1);
				Assert.AreEqual(uint.MaxValue>>1,	u32);

				u64 = Bits.BitCountToMask64(0);
				Assert.AreEqual(0UL,				u64);

				u64 = Bits.BitCountToMask64(Bits.kInt64BitCount);
				Assert.AreEqual(ulong.MaxValue,		u64);

				u64 = Bits.BitCountToMask64(Bits.kInt64BitCount-1);
				Assert.AreEqual(ulong.MaxValue>>1,	u64);
			}
		}

		[TestMethod]
		public void BitCountToMask_TestInRangeValues()
		{
			for (int bitCount = Bits.kInt32BitCount; bitCount > 0; bitCount--)
			{
				uint u32 = Bits.BitCountToMask32(bitCount);

				int expectedShift = Bits.kInt32BitCount - bitCount;
				var expectedValue = uint.MaxValue>>expectedShift;
				Assert.AreEqual(expectedValue, u32,
					$"bitCount={bitCount}");
			}

			for (int bitCount = Bits.kInt64BitCount; bitCount > 0; bitCount--)
			{
				ulong u64 = Bits.BitCountToMask64(bitCount);

				int expectedShift = Bits.kInt64BitCount - bitCount;
				var expectedValue = ulong.MaxValue>>expectedShift;
				Assert.AreEqual(expectedValue, u64,
					$"bitCount={bitCount}");
			}
		}

		[TestMethod]
		public void BitCountToMask_TestThrowsOnOutOfRange()
		{
			Assert.Throws<ArgumentOutOfRangeException>(()
				=> Bits.BitCountToMask32(-1));
			Assert.Throws<ArgumentOutOfRangeException>(()
				=> Bits.BitCountToMask32(Bits.kInt32BitCount+1));

			Assert.Throws<ArgumentOutOfRangeException>(()
				=> Bits.BitCountToMask64(-1));
			Assert.Throws<ArgumentOutOfRangeException>(()
				=> Bits.BitCountToMask64(Bits.kInt64BitCount+1));
		}
		#endregion

		#region BitReverse
		[TestMethod]
		public void Bits_BitReverseTest()
		{
			//////////////////////////////////////////////////////////////////////////
			{// Int8
				var even_bits = unchecked((byte)kEvenBits);
				var odd_bits = unchecked((byte)kOddBits);
				Assert.AreEqual(odd_bits, Bits.BitReverse(even_bits));
			}
			//////////////////////////////////////////////////////////////////////////
			{// Int16
				var even_bits = unchecked((ushort)kEvenBits);
				var odd_bits = unchecked((ushort)kOddBits);
				Assert.AreEqual(odd_bits, Bits.BitReverse(even_bits));
			}
			//////////////////////////////////////////////////////////////////////////
			{// Int32
				var even_bits = unchecked((uint)kEvenBits);
				var odd_bits = unchecked((uint)kOddBits);
				Assert.AreEqual(odd_bits, Bits.BitReverse(even_bits));
			}
			//////////////////////////////////////////////////////////////////////////
			{// Int64
				var even_bits = unchecked((ulong)kEvenBits);
				var odd_bits = unchecked((ulong)kOddBits);
				Assert.AreEqual(odd_bits, Bits.BitReverse(even_bits));
			}
		}
		[TestMethod]
		public void Bits_BitReverseTest2()
		{
			//////////////////////////////////////////////////////////////////////////
			// Byte
			{
				const byte kInput = 0xDE,					kOutput = 0x7B;

				var result = Bits.BitReverse(kInput);
				Assert.AreEqual(kOutput, result);
				result = Bits.BitReverse(result);
				Assert.AreEqual(kInput, result);
			}

			//////////////////////////////////////////////////////////////////////////
			// Int16
			{
				const ushort kInput = 0xDEAD,				kOutput = 0xB57B;

				var result = Bits.BitReverse(kInput);
				Assert.AreEqual(kOutput, result);
				result = Bits.BitReverse(result);
				Assert.AreEqual(kInput, result);
			}

			//////////////////////////////////////////////////////////////////////////
			// Int32
			{
				const uint kInput = 0xDEADBEEF,				kOutput = 0xF77DB57B;

				var result = Bits.BitReverse(kInput);
				Assert.AreEqual(kOutput, result);
				result = Bits.BitReverse(result);
				Assert.AreEqual(kInput, result);
			}

			//////////////////////////////////////////////////////////////////////////
			// Int64
			{
				const ulong kInput = 0xDEADBEEFDEADBEEF,	kOutput = 0xF77DB57BF77DB57B;

				var result = Bits.BitReverse(kInput);
				Assert.AreEqual(kOutput, result);
				result = Bits.BitReverse(result);
				Assert.AreEqual(kInput, result);
			}
		}
		#endregion

		#region BitSwap
		[SuppressMessage("Microsoft.Design", "CA1822:MarkMembersAsStatic", Justification="#TODO_UNITTEST")]
		public void Bits_BitSwapTest()
		{
			// #TODO_UNITTEST
		}
		#endregion

		#region Rotate
		[TestMethod]
		public void RotateMajorWidthsMatchBitOperationsTest()
		{
			const uint kUInt32Value = 0x81234567U;
			foreach (int shift in new[] { 0, 1, 7, 16, Bits.kInt32BitCount-1 })
			{
				Assert.AreEqual(BitOperations.RotateLeft(kUInt32Value, shift), Bits.RotateLeft(kUInt32Value, shift),
					$"uint left shift={shift}");
				Assert.AreEqual(BitOperations.RotateRight(kUInt32Value, shift), Bits.RotateRight(kUInt32Value, shift),
					$"uint right shift={shift}");
			}

			const ulong kUInt64Value = 0x8123456789ABCDEFUL;
			foreach (int shift in new[] { 0, 1, 7, 32, Bits.kInt64BitCount-1 })
			{
				Assert.AreEqual(BitOperations.RotateLeft(kUInt64Value, shift), Bits.RotateLeft(kUInt64Value, shift),
					$"ulong left shift={shift}");
				Assert.AreEqual(BitOperations.RotateRight(kUInt64Value, shift), Bits.RotateRight(kUInt64Value, shift),
					$"ulong right shift={shift}");
			}
		}

		[TestMethod]
		public void RotateNarrowWidthsStayWidthLimitedTest()
		{
			// BitOperations only exposes 32/64-bit rotates. These assertions catch accidental use for byte/ushort,
			// where the high bit must wrap back into the narrow value rather than disappear after a cast.
			Assert.AreEqual((byte)0x03, Bits.RotateLeft((byte)0x81, 1));
			Assert.AreEqual((byte)0xC0, Bits.RotateRight((byte)0x81, 1));
			Assert.AreEqual((ushort)0x0003, Bits.RotateLeft((ushort)0x8001, 1));
			Assert.AreEqual((ushort)0xC000, Bits.RotateRight((ushort)0x8001, 1));
		}
		#endregion

		#region Leading/Trailing ZerosCount
		[TestMethod]
		public void LeadingZerosCountNarrowWidthsStayWidthLimitedTest()
		{
			Assert.AreEqual(Bits.kByteBitCount, (int)Bits.LeadingZerosCount(byte.MinValue));
			Assert.AreEqual(0, (int)Bits.LeadingZerosCount(byte.MaxValue));
			Assert.AreEqual(7, (int)Bits.LeadingZerosCount((byte)0x01));
			Assert.AreEqual(1, (int)Bits.LeadingZerosCount((byte)0x40));

			Assert.AreEqual(Bits.kInt16BitCount, (int)Bits.LeadingZerosCount(ushort.MinValue));
			Assert.AreEqual(0, (int)Bits.LeadingZerosCount(ushort.MaxValue));
			Assert.AreEqual(15, (int)Bits.LeadingZerosCount((ushort)0x0001));
			Assert.AreEqual(1, (int)Bits.LeadingZerosCount((ushort)0x4000));
		}

		[TestMethod]
		public void TrailingZerosCountMatchesBitOperationsTest()
		{
			foreach (uint value in new[] { 0U, 1U, 0x10U, 0x80000000U, 0xF0001000U, uint.MaxValue })
			{
				Assert.AreEqual(BitOperations.TrailingZeroCount(value), (int)Bits.TrailingZerosCount(value),
					$"uint value=0x{value:X8}");
			}

			foreach (ulong value in new[] { 0UL, 1UL, 0x10UL, 0x8000000000000000UL, 0xF000100000000000UL,
				ulong.MaxValue })
			{
				Assert.AreEqual(BitOperations.TrailingZeroCount(value), (int)Bits.TrailingZerosCount(value),
					$"ulong value=0x{value:X16}");
			}
		}

		[TestMethod]
		public void Bits_LeadingZerosCountTest()
		{
			Assert.AreEqual(Bits.kInt32BitCount, Bits.LeadingZerosCount(uint.MinValue));
			for (uint x = 0, bits = uint.MaxValue; x < Bits.kInt32BitCount; x++, bits >>= 1)
			{
				Assert.AreEqual(x, (uint)Bits.LeadingZerosCount(bits));
			}

			Assert.AreEqual(Bits.kInt64BitCount, Bits.LeadingZerosCount(ulong.MinValue));
			for (ulong x = 0, bits = ulong.MaxValue; x < Bits.kInt64BitCount; x++, bits >>= 1)
			{
				Assert.AreEqual(x, (ulong)Bits.LeadingZerosCount(bits));
			}
		}

		[TestMethod]
		public void Bits_TrailingZerosCountTest()
		{
			Assert.AreEqual(Bits.kInt32BitCount, Bits.TrailingZerosCount(uint.MinValue));
			for (uint x = 0, bits = 1; x < Bits.kInt32BitCount; x++, bits <<= 1)
			{
				Assert.AreEqual(x, (uint)Bits.TrailingZerosCount(bits));
			}

			Assert.AreEqual(Bits.kInt64BitCount, Bits.TrailingZerosCount(ulong.MinValue));
			for (ulong x = 0, bits = 1; x < Bits.kInt64BitCount; x++, bits <<= 1)
			{
				Assert.AreEqual(x, (ulong)Bits.TrailingZerosCount(bits));
			}
		}
		#endregion

		#region GetBitmask
		[TestMethod]
		public void Bits_GetBitmaskTest()
		{
			int i32;

			i32 = Bits.GetMaxEnumBits(2);
			Assert.AreEqual(1, i32);
			i32 = Bits.GetMaxEnumBits(3);
			Assert.AreEqual(2, i32);
			i32 = Bits.GetMaxEnumBits(7);
			Assert.AreEqual(3, i32);
			i32 = Bits.GetMaxEnumBits(0xCFFF);
			Assert.AreEqual(16, i32);

			// #TODO_UNITTEST: GetBitmaskFlag
		}
		#endregion

		[TestMethod]
		public void Bits_SignExtendTest()
		{
			int i32;

			i32 = Bits.SignExtend((int)0x3FF, 10);
			Assert.AreEqual(-1, i32);

			i32 = Bits.SignExtendWithoutClear((int)0x3FF, 10);
			Assert.AreEqual(-1, i32);

			long i64;

			i64 = Bits.SignExtend((long)0x3FF, 10);
			Assert.AreEqual(-1, i64);

			i64 = Bits.SignExtendWithoutClear((long)0x3FF, 10);
			Assert.AreEqual(-1, i64);
		}

		#region BitmaskLookUpTable
		static readonly byte[] kBitmaskLookup8 = [
			0x00,
			0x01, 0x03, 0x07, 0x0F,
			0x1F, 0x3F, 0x7F, 0xFF, // 8-bit
		];
		static readonly ushort[] kBitmaskLookup16 = [
			0x0000,
			0x0001, 0x0003, 0x0007, 0x000F,
			0x001F, 0x003F, 0x007F, 0x00FF, // 8-bit

			0x01FF,	0x03FF, 0x07FF, 0x0FFF,
			0x1FFF, 0x3FFF,	0x7FFF, 0xFFFF, // 16-bit
		];
		static readonly uint[] kBitmaskLookup32 = [
			0x00000000,
			0x00000001, 0x00000003, 0x00000007, 0x0000000F,
			0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF, // 8-bit

			0x000001FF,	0x000003FF, 0x000007FF, 0x00000FFF,
			0x00001FFF, 0x00003FFF,	0x00007FFF, 0x0000FFFF, // 16-bit

			0x0001FFFF, 0x0003FFFF, 0x0007FFFF,	0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,	// 24-bit

			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF,
			0x1FFFFFFF,	0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF, // 32-bit
		];
		static readonly ulong[] kBitmaskLookup64 = [
			0x00000000,
			0x00000001, 0x00000003, 0x00000007, 0x0000000F,
			0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF, // 8-bit

			0x000001FF,	0x000003FF, 0x000007FF, 0x00000FFF,
			0x00001FFF, 0x00003FFF,	0x00007FFF, 0x0000FFFF, // 16-bit

			0x0001FFFF, 0x0003FFFF, 0x0007FFFF,	0x000FFFFF,
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,	// 24-bit

			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF,
			0x1FFFFFFF,	0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF, // 32-bit


			0x00000001FFFFFFFF, 0x00000003FFFFFFFF, 0x00000007FFFFFFFF, 0x0000000FFFFFFFFF,
			0x0000001FFFFFFFFF, 0x0000003FFFFFFFFF, 0x0000007FFFFFFFFF, 0x000000FFFFFFFFFF, // 40-bit

			0x000001FFFFFFFFFF,	0x000003FFFFFFFFFF, 0x000007FFFFFFFFFF, 0x00000FFFFFFFFFFF,
			0x00001FFFFFFFFFFF, 0x00003FFFFFFFFFFF,	0x00007FFFFFFFFFFF, 0x0000FFFFFFFFFFFF, // 48-bit

			0x0001FFFFFFFFFFFF, 0x0003FFFFFFFFFFFF, 0x0007FFFFFFFFFFFF,	0x000FFFFFFFFFFFFF,
			0x001FFFFFFFFFFFFF, 0x003FFFFFFFFFFFFF, 0x007FFFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF,	// 56-bit

			0x01FFFFFFFFFFFFFF, 0x03FFFFFFFFFFFFFF, 0x07FFFFFFFFFFFFFF, 0x0FFFFFFFFFFFFFFF,
			0x1FFFFFFFFFFFFFFF,	0x3FFFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, // 64-bit
		];

		[TestMethod]
		public void Bits_TestBitmaskLookUpTableGenerators()
		{
			Bits.BitmaskLookUpTableGenerate(Bits.kByteBitCount, out byte[] generated8);
			CollectionAssert.AreEqual(kBitmaskLookup8, generated8);

			Bits.BitmaskLookUpTableGenerate(Bits.kInt16BitCount, out ushort[] generated16);
			CollectionAssert.AreEqual(kBitmaskLookup16, generated16);

			Bits.BitmaskLookUpTableGenerate(Bits.kInt32BitCount, out uint[] generated32);
			CollectionAssert.AreEqual(kBitmaskLookup32, generated32);

			Bits.BitmaskLookUpTableGenerate(Bits.kInt64BitCount, out ulong[] generated64);
			CollectionAssert.AreEqual(kBitmaskLookup64, generated64);
		}
		#endregion

		#region NoneableEncodingTraits
		[TestMethod]
		public void Bits_NoneableEncodingTraitsTest()
		{
			#region 32-bit
			int i32_max_value;
			uint i32_bit_mask;

			// smallest value case
			i32_max_value = 1;
			i32_bit_mask = Bits.GetNoneableEncodingTraits(i32_max_value, out int i32_bit_count);
			Assert.AreEqual(1, i32_bit_count);
			Assert.AreEqual(0x1U, i32_bit_mask);

			i32_max_value = 7;
			i32_bit_mask = Bits.GetNoneableEncodingTraits(i32_max_value, out i32_bit_count);
			Assert.AreEqual(3, i32_bit_count);
			Assert.AreEqual(0x7U, i32_bit_mask);

			// this should output a 'inefficient' warning
			i32_max_value = 8;
			i32_bit_mask = Bits.GetNoneableEncodingTraits(i32_max_value, out i32_bit_count);
			Assert.AreEqual(4, i32_bit_count);
			Assert.AreEqual(0xFU, i32_bit_mask);
			#endregion

			#region 64-bit
			long i64_max_value;
			ulong i64_bit_mask;

			i64_max_value = long.MaxValue >> 1;
			i64_bit_mask = Bits.GetNoneableEncodingTraits(i64_max_value, out int i64_bit_count);
			Assert.AreEqual(62, i64_bit_count);
			Assert.AreEqual(0x3FFFFFFFFFFFFFFFUL, i64_bit_mask);

			// this should output a 'inefficient' warning
			i64_max_value++;
			i64_bit_mask = Bits.GetNoneableEncodingTraits(i64_max_value, out i64_bit_count);
			Assert.AreEqual(63, i64_bit_count);
			Assert.AreEqual(0x7FFFFFFFFFFFFFFFUL, i64_bit_mask);

			// largest value case
			i64_max_value = long.MaxValue - 1;
			i64_bit_mask = Bits.GetNoneableEncodingTraits(i64_max_value, out i64_bit_count);
			Assert.AreEqual(63, i64_bit_count);
			Assert.AreEqual(0x7FFFFFFFFFFFFFFFUL, i64_bit_mask);
			#endregion
		}

		[TestMethod]
		// we expect an (internal) System.Diagnostics.Contracts.__ContractsRuntime+ContractException
		public void Bits_NoneableEncodingTraitsInputTooSmallTest()
		{
			Assert.Throws<Exception>(() =>
				Bits.GetNoneableEncodingTraits(0, out int _)
			);
		}

		[TestMethod]
		// we expect an (internal) System.Diagnostics.Contracts.__ContractsRuntime+ContractException
		public void Bits_NoneableEncodingTraitsInputTooLargeTest()
		{
			Assert.Throws<Exception>(() =>
				Bits.GetNoneableEncodingTraits(int.MaxValue, out int _)
			);
		}
		#endregion

		class TestUnionData1
		{
			public string Str;
			public bool Bool = false;
		};
		class TestUnionData2
		{
			public int Index;
		};
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
		struct TestUnion
		{
			[System.Runtime.InteropServices.FieldOffset(0)]
			public TestUnionData1 Data1;
			[System.Runtime.InteropServices.FieldOffset(0)]
			public TestUnionData2 Data2;
		};
		[TestMethod]
		public void Union_Test()
		{
			var t = new TestUnion
			{
				Data1 = new TestUnionData1()
				{
					Str = ""
				}
			};
			Console.WriteLine(t.Data2.Index);

			var t2 = new TestUnion
			{
				Data2 = new TestUnionData2
				{
					Index = -1
				}
			};
			Console.WriteLine(t2.Data1);
		}
	};
}
