﻿using System;
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
		const ulong kMiddleNybbles = 0x6666666666666666UL; // Bit pattern middle bits are set in a nybble

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

				expected_bit_count = Bits.kInt16BitCount / 2;	i32 = Bits.BitCount(unchecked((ushort)kBitCountValue));
				Assert.AreEqual(expected_bit_count, i32);

				expected_bit_count = Bits.kInt32BitCount / 2;	i32 = Bits.BitCount(unchecked((uint)kBitCountValue));
				Assert.AreEqual(expected_bit_count, i32);

				expected_bit_count = Bits.kInt64BitCount / 2;	i32 = Bits.BitCount(kBitCountValue);
				Assert.AreEqual(expected_bit_count, i32);
			}

			{
				uint u32;
				ulong u64;

				u32 = Bits.BitCountToMask32(Bits.kInt32BitCount);
				Assert.AreEqual(u32, uint.MaxValue);

				u32 = Bits.BitCountToMask32(Bits.kInt32BitCount-1);
				Assert.AreEqual(u32, uint.MaxValue>>1);

				u64 = Bits.BitCountToMask64(Bits.kInt64BitCount);
				Assert.AreEqual(u64, ulong.MaxValue);

				u64 = Bits.BitCountToMask64(Bits.kInt64BitCount-1);
				Assert.AreEqual(u64, ulong.MaxValue>>1);
			}
		}

//		[TestMethod]
		public void Bits_BitCountToMaskTest()
		{
			// TODO
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
		public void Bits_BitSwapTest()
		{
			// TODO
		}
		#endregion

		#region Leading/Trailing ZerosCount
		[TestMethod]
		public void Bits_LeadingZerosCountTest()
		{
			Assert.AreEqual(Bits.kInt32BitCount, Bits.LeadingZerosCount(uint.MinValue));
			for (uint x = 0, bits = uint.MaxValue; x < Bits.kInt32BitCount; x++, bits >>= 1)
				Assert.AreEqual(x, (uint)Bits.LeadingZerosCount(bits));

			Assert.AreEqual(Bits.kInt64BitCount, Bits.LeadingZerosCount(ulong.MinValue));
			for (ulong x = 0, bits = ulong.MaxValue; x < Bits.kInt64BitCount; x++, bits >>= 1)
				Assert.AreEqual(x, (ulong)Bits.LeadingZerosCount(bits));
		}

		[TestMethod]
		public void Bits_TrailingZerosCountTest()
		{
			Assert.AreEqual(Bits.kInt32BitCount, Bits.TrailingZerosCount(uint.MinValue));
			for (uint x = 0, bits = 1; x < Bits.kInt32BitCount; x++, bits <<= 1)
				Assert.AreEqual(x, (uint)Bits.TrailingZerosCount(bits));

			Assert.AreEqual(Bits.kInt64BitCount, Bits.TrailingZerosCount(ulong.MinValue));
			for (ulong x = 0, bits = 1; x < Bits.kInt64BitCount; x++, bits <<= 1)
				Assert.AreEqual(x, (ulong)Bits.TrailingZerosCount(bits));
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

			// TODO: GetBitmaskFlag
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
		static readonly byte[] kBitmaskLookup8 = new byte[] {
			0x00, 
			0x01, 0x03, 0x07, 0x0F, 
			0x1F, 0x3F, 0x7F, 0xFF, // 8-bit
		};
		static readonly ushort[] kBitmaskLookup16 = new ushort[] {
			0x0000, 
			0x0001, 0x0003, 0x0007, 0x000F, 
			0x001F, 0x003F, 0x007F, 0x00FF, // 8-bit

			0x01FF,	0x03FF, 0x07FF, 0x0FFF, 
			0x1FFF, 0x3FFF,	0x7FFF, 0xFFFF, // 16-bit
		};
		static readonly uint[] kBitmaskLookup32 = new uint[] {
			0x00000000, 
			0x00000001, 0x00000003, 0x00000007, 0x0000000F, 
			0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF, // 8-bit

			0x000001FF,	0x000003FF, 0x000007FF, 0x00000FFF, 
			0x00001FFF, 0x00003FFF,	0x00007FFF, 0x0000FFFF, // 16-bit

			0x0001FFFF, 0x0003FFFF, 0x0007FFFF,	0x000FFFFF, 
			0x001FFFFF, 0x003FFFFF, 0x007FFFFF, 0x00FFFFFF,	// 24-bit

			0x01FFFFFF, 0x03FFFFFF, 0x07FFFFFF, 0x0FFFFFFF, 
			0x1FFFFFFF,	0x3FFFFFFF, 0x7FFFFFFF, 0xFFFFFFFF, // 32-bit
		};
		static readonly ulong[] kBitmaskLookup64 = new ulong[] {
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
		};

		[TestMethod]
		public void Bits_TestBitmaskLookUpTableGenerators()
		{
			byte[] generated8;
			Bits.BitmaskLookUpTableGenerate(Bits.kByteBitCount, out generated8);
			Assert.IsTrue(generated8.EqualsArray(kBitmaskLookup8));

			ushort[] generated16;
			Bits.BitmaskLookUpTableGenerate(Bits.kInt16BitCount, out generated16);
			Assert.IsTrue(generated16.EqualsArray(kBitmaskLookup16));

			uint[] generated32;
			Bits.BitmaskLookUpTableGenerate(Bits.kInt32BitCount, out generated32);
			Assert.IsTrue(generated32.EqualsArray(kBitmaskLookup32));

			ulong[] generated64;
			Bits.BitmaskLookUpTableGenerate(Bits.kInt64BitCount, out generated64);
			Assert.IsTrue(generated64.EqualsArray(kBitmaskLookup64));

		}
		#endregion
	};
}