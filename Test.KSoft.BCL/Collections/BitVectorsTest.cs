using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitArray = System.Collections.BitArray;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class BitVectorsTest : BaseTestClass
	{
		/// <summary>0101 0101</summary>
		const byte kMaskEvenBits = 0x55;
		/// <summary>1010 1010</summary>
		const byte kMaskOddBits = unchecked((byte)~kMaskEvenBits);

		private static ulong MakeBitPatern(byte pattern, int bitLength)
		{
			Assert.AreEqual(0, bitLength % Bits.kByteBitCount);

			ulong word = 0;
			for(int x = 0; x < bitLength; x += Bits.kByteBitCount)
			{
				word |= pattern;
				word <<= Bits.kByteBitCount;
			}

			return word;
		}

		[TestMethod]
		public void Collections_BitVectorsBitIndicesEnumeratorTest()
		{
			var oddBits32 = (uint)MakeBitPatern(kMaskOddBits, Bits.kInt32BitCount);
			var oddBits64 =       MakeBitPatern(kMaskOddBits, Bits.kInt64BitCount);

			#region 32-bit
			{
				var bv32 = new BitVector32(oddBits32);

				Assert.AreEqual(bv32.Length / 2, bv32.Cardinality);
				int idx_count = 0;

				foreach (int idx in bv32.ClearBitIndices)
				{
					Assert.IsTrue(idx % 2 == 0);
					idx_count++;
				}
				Assert.AreEqual(bv32.Length / 2, idx_count);

				foreach (int idx in bv32.SetBitIndices)
				{
					Assert.IsTrue(idx % 2 == 1);
					idx_count++;
				}
				Assert.AreEqual(bv32.Length, idx_count);
			}
			#endregion

			#region 64-bit
			{
				var bv64 = new BitVector64(oddBits64);

				Assert.AreEqual(bv64.Length / 2, bv64.Cardinality);
				int idx_count = 0;

				foreach (int idx in bv64.ClearBitIndices)
				{
					Assert.IsTrue(idx % 2 == 0);
					idx_count++;
				}
				Assert.AreEqual(bv64.Length / 2, idx_count);

				foreach (int idx in bv64.SetBitIndices)
				{
					Assert.IsTrue(idx % 2 == 1);
					idx_count++;
				}
				Assert.AreEqual(bv64.Length, idx_count);
			}
			#endregion
		}

		#region BitVectorsOperationsTest
		// Based on Collections_BitSetOperationsTest
		[TestMethod]
		public void Collections_BitVectorsOperationsTest()
		{
			BitVectorsOperationsTest32();
			BitVectorsOperationsTest64();
		}
		private void BitVectorsOperationsTest32()
		{
			const int k_bit_count = Bits.kInt32BitCount;

			var lhs_bits = 0x4; // 0100b
			var lhs_bs = new BitVector32(lhs_bits);
			var rhs_bits = 0xA; // 1010b
			var rhs_bs = new BitVector32(rhs_bits);

			lhs_bs = lhs_bs.Or(rhs_bs);
			Assert.AreEqual(3, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], true);

			// also undoes the OR operation
			lhs_bs = lhs_bs.AndNot(rhs_bs);
			Assert.AreEqual(1, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], false);

			lhs_bs = lhs_bs.And(rhs_bs);
			Assert.AreEqual(0, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], false);
			Assert.AreEqual(lhs_bs[3], false);

			// at this point lhs is zero, and the only bit in rhs which is on (relative to lhs's bit-space) is the 2nd
			lhs_bs = lhs_bs.Xor(rhs_bs);
			Assert.AreEqual(2, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], false);
			Assert.AreEqual(lhs_bs[3], true);

			lhs_bs = lhs_bs.Not();
			Assert.AreEqual(k_bit_count - 2, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], true);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], false);
		}
		private void BitVectorsOperationsTest64()
		{
			const int k_bit_count = Bits.kInt64BitCount;

			var lhs_bits = 0x4; // 0100b
			var lhs_bs = new BitVector64(lhs_bits);
			var rhs_bits = 0xA; // 1010b
			var rhs_bs = new BitVector64(rhs_bits);

			lhs_bs = lhs_bs.Or(rhs_bs);
			Assert.AreEqual(3, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], true);

			// also undoes the OR operation
			lhs_bs = lhs_bs.AndNot(rhs_bs);
			Assert.AreEqual(1, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], false);

			lhs_bs = lhs_bs.And(rhs_bs);
			Assert.AreEqual(0, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], false);
			Assert.AreEqual(lhs_bs[3], false);

			// at this point lhs is zero, and the only bit in rhs which is on (relative to lhs's bit-space) is the 2nd
			lhs_bs = lhs_bs.Xor(rhs_bs);
			Assert.AreEqual(2, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], false);
			Assert.AreEqual(lhs_bs[3], true);

			lhs_bs = lhs_bs.Not();
			Assert.AreEqual(k_bit_count - 2, lhs_bs.Cardinality);
			Assert.AreEqual(k_bit_count - lhs_bs.Cardinality, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], true);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);
			Assert.AreEqual(lhs_bs[3], false);
		}
		#endregion

		[TestMethod]
		public void Collections_BitVectorsRangeOperationsTest()
		{
			const int k_initial_size = Bits.kInt32BitCount;
			uint bits = (uint)((1UL << k_initial_size) - 1);
			var bs = new BitVector32(bits);

			Assert.AreEqual(k_initial_size, bs.Cardinality);
			Assert.AreEqual(0, bs.CardinalityZeros);

			// clear all but the first bit
			bs.ClearBits(1, k_initial_size-1);
			Assert.AreEqual(1, bs.Cardinality);
			Assert.AreEqual(k_initial_size - 1, bs.CardinalityZeros);

			// set all the modified bits back
			bs.SetBits(1, k_initial_size-1);
			Assert.AreEqual(k_initial_size, bs.Cardinality);
			Assert.AreEqual(0, bs.CardinalityZeros);

			// will invert all the bits (to false)
			bs.ToggleBits(0, bs.Length);
			Assert.AreEqual(0, bs.Cardinality);
			Assert.AreEqual(k_initial_size, bs.CardinalityZeros);

			// this should do nothing
			bs.SetBits(1, 0);
			Assert.AreEqual(0, bs.Cardinality);
			Assert.AreEqual(k_initial_size, bs.CardinalityZeros);

			Assert.IsFalse(bs.TestBits(0, bs.Length));

			bs.SetBits(k_initial_size-2, 2);
			Assert.IsTrue(bs.TestBits(0, bs.Length));
		}
	};
}