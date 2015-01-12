using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitArray = System.Collections.BitArray;

namespace KSoft.Collections.Test
{
	// may be a good ref: http://code.google.com/p/google-web-toolkit/issues/detail?id=3279#c19

	[TestClass]
	public class BitSetTest : BaseTestClass
	{
		[TestMethod]
		public void Collections_BitSetBitIndicesEnumeratorTest()
		{
			var bits = new bool[] { false, true, false, true, false, true, };
			var bs = new BitSet(bits);
			Assert.AreEqual(bits.Length, bs.Length);
			Assert.AreEqual(bs.Length / 2, bs.Cardinality);
			int idx_count = 0;

			foreach (int idx in bs.ClearBitIndices)
			{
				Assert.IsTrue(idx % 2 == 0);
				idx_count++;
			}
			Assert.AreEqual(bs.Length / 2, idx_count);

			foreach (int idx in bs.SetBitIndices)
			{
				Assert.IsTrue(idx % 2 == 1);
				idx_count++;
			}
			Assert.AreEqual(bs.Length, idx_count);
		}

		[TestMethod]
		public void Collections_BitSetLengthPropertiesTest()
		{
			const int k_initial_length = 96;
			const int k_smaller_length = 80;
			const int k_larger_length = 120;
			const int k_aligned_word_length = 64;

			var bs = new BitSet(k_initial_length, defaultValue: true, fixedLength: false);
			Assert.AreEqual(k_initial_length, bs.Cardinality);
			Assert.AreEqual(0, bs.CardinalityZeros);

			bs.Length = k_smaller_length;
			Assert.AreEqual(k_smaller_length, bs.Cardinality);
			Assert.AreEqual(0, bs.CardinalityZeros);

			bs.Length = k_larger_length;
			Assert.AreEqual(k_smaller_length, bs.Cardinality);
			Assert.AreEqual(k_larger_length-k_smaller_length, bs.CardinalityZeros);

			bs[k_larger_length - 1] = true;
			Assert.AreEqual(k_smaller_length+1, bs.Cardinality);

			bs.Length = k_aligned_word_length;
			Assert.AreEqual(k_aligned_word_length, bs.Cardinality);
			Assert.AreEqual(0, bs.CardinalityZeros);
		}

		[TestMethod]
		public void Collections_BitSetOperationsTest()
		{
			var lhs_bits = new bool[] { false, false, true, };
			var lhs_bs = new BitSet(lhs_bits);
			// rhs is our operation input, and it needs to be longer than lhs to also validate 
			// ClearAlignmentOnlyBitsForBitOperation's logic is working as intended
			var rhs_bits = new bool[] { false, true, false, true, };
			var rhs_bs = new BitSet(rhs_bits);

			lhs_bs.Or(rhs_bs);
			Assert.AreEqual(2, lhs_bs.Cardinality);
			Assert.AreEqual(1, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], true);

			// also undoes the OR operation
			lhs_bs.AndNot(rhs_bs);
			Assert.AreEqual(1, lhs_bs.Cardinality);
			Assert.AreEqual(2, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);

			lhs_bs.And(rhs_bs);
			Assert.AreEqual(0, lhs_bs.Cardinality);
			Assert.AreEqual(3, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], false);

			// at this point lhs is zero, and the only bit in rhs which is on (relative to lhs's bit-space) is the 2nd
			lhs_bs.Xor(rhs_bs);
			Assert.AreEqual(1, lhs_bs.Cardinality);
			Assert.AreEqual(2, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], false);
			Assert.AreEqual(lhs_bs[1], true);
			Assert.AreEqual(lhs_bs[2], false);

			lhs_bs.Not();
			Assert.AreEqual(2, lhs_bs.Cardinality);
			Assert.AreEqual(1, lhs_bs.CardinalityZeros);
			Assert.AreEqual(lhs_bs[0], true);
			Assert.AreEqual(lhs_bs[1], false);
			Assert.AreEqual(lhs_bs[2], true);
		}

		[TestMethod]
		public void Collections_BitSetOperationsWithEmptyInputTest()
		{
			var bs_empty = new BitSet();
			var bits = new bool[] { false, false, true, };
			var bs = new BitSet(bits);
			Action bs_verify_unchanged_func = () =>
			{
				Assert.AreEqual(1, bs.Cardinality);
				Assert.AreEqual(2, bs.CardinalityZeros);
				Assert.AreEqual(bs[0], false);
				Assert.AreEqual(bs[1], false);
				Assert.AreEqual(bs[2], true);
			};

			bs.Or(bs_empty);
			bs_verify_unchanged_func();

			bs.AndNot(bs_empty);
			bs_verify_unchanged_func();

			bs.And(bs_empty);
			bs_verify_unchanged_func();

			bs.Xor(bs_empty);
			bs_verify_unchanged_func();
		}

		[TestMethod]
		public void Collections_BitSetOverlapsTest()
		{
			var empty_bs = new BitSet();
			var lhs_bits = new bool[] { false, false, };
			var lhs_bs = new BitSet(lhs_bits);
			var rhs_bits = new bool[] { false, true, };
			var rhs_bs = new BitSet(rhs_bits);

			Assert.IsTrue (lhs_bs.Overlaps(rhs_bs));
			Assert.IsFalse(lhs_bs.OverlapsSansZeros(rhs_bs));
			Assert.IsFalse(lhs_bs.Overlaps(empty_bs));
			Assert.IsFalse(lhs_bs.OverlapsSansZeros(empty_bs));
		}

		[TestMethod]
		public void Collections_BitSetSuperAndSubsetTest()
		{
			// the initial superset
			var lhs_bits = new bool[] { false, true, false, true, false, true, true };
			var lhs_bs = new BitSet(lhs_bits);
			// the initial subset
			var rhs_bits = new bool[] { false, true, false, true, false, true, };
			var rhs_bs = new BitSet(rhs_bits);

			lhs_bs.FixedLength = false;

			Assert.IsTrue(lhs_bs.IsSupersetOf(rhs_bs));
			Assert.IsTrue(rhs_bs.IsSubsetOf(lhs_bs));
			// now test the inverse of the above operations. should have the inverse results
			Assert.IsFalse(lhs_bs.IsSubsetOf(rhs_bs));
			Assert.IsFalse(rhs_bs.IsSupersetOf(lhs_bs));

			// now set LHS to be smaller than RHS.
			lhs_bs.Length -= 2;
			// what was once true, should now be false
			Assert.IsTrue(lhs_bs.IsSubsetOf(rhs_bs));
			Assert.IsTrue(rhs_bs.IsSupersetOf(lhs_bs));
			// now test the inverse of the above operations. should have the inverse results
			Assert.IsFalse(lhs_bs.IsSupersetOf(rhs_bs));
			Assert.IsFalse(rhs_bs.IsSubsetOf(lhs_bs));
		}

		[TestMethod]
		public void Collections_BitSetRangeOperationsTest()
		{
			const int k_initial_size = 33;
			var bs = new BitSet(k_initial_size, true);

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

			bs.SetBits(31, 2);
			Assert.IsTrue(bs.TestBits(0, bs.Length));
		}

		static KeyValuePair<BitArray, BitSet> NewBitSetAndArray(Random rand, int length)
		{
			var bitarray = new BitArray(length);
			var bitset = new BitSet(length);
			for (int x = 0; x < length; x++)
			{
				bool bit = rand.NextBoolean();
				bitarray[x] = bit;
				bitset[x] = bit;
			}

			return new KeyValuePair<BitArray, BitSet>(bitarray, bitset);
		}
		static void CompareBitArrayAndSet(BitArray bitarray, BitSet bitset)
		{
			Assert.AreEqual(bitarray.Length, bitset.Length);

			#region Compare via indexing
			for (int x = 0; x < bitarray.Length; x++)
			{
				Assert.AreEqual(bitarray[x], bitset[x]);
			}
			#endregion

			#region Compare via BitSet's indices enumerators
			int cardinality_zeros = 0;
			foreach (int idx in bitset.ClearBitIndices)
			{
				Assert.IsFalse(bitarray[idx]);
				cardinality_zeros++;
			}

			int cardinality = 0;
			foreach (int idx in bitset.SetBitIndices)
			{
				Assert.IsTrue(bitarray[idx]);
				cardinality++;
			}

			Assert.AreEqual(cardinality, bitset.Cardinality);
			Assert.AreEqual(cardinality_zeros, bitset.CardinalityZeros);
			#endregion

			#region Compare via GetEnumerator
			var bitarray_iter = bitarray.GetEnumerator();
			var bitset_iter = bitset.GetEnumerator();
			while(bitarray_iter.MoveNext() && bitset_iter.MoveNext())
			{
				Assert.AreEqual((bool)bitarray_iter.Current, bitset_iter.Current);
			}
			#endregion
		}
		[TestMethod]
		public void Collections_BitSetValidateWithBitArrayTest()
		{
			const int k_max_random_length = 10000;

			var rand = new Random();
			int length = rand.Next(k_max_random_length);
			Console.WriteLine("Bit vector length: {0}", length);

			var pair = NewBitSetAndArray(rand, length);
			var bitarray = pair.Key;
			var bitset = pair.Value;
			for (int x = 0; x < length; x++)
			{
				bool bit = rand.NextBoolean();
				bitarray[x] = bit;
				bitset[x] = bit;
			}

			CompareBitArrayAndSet(bitarray, bitset);

			#region And
			pair = NewBitSetAndArray(rand, length);
			var bitarray_mod = pair.Key;
			var bitset_mod = pair.Value;

			bitarray.And(bitarray_mod);
			bitset.And(bitset_mod);
			CompareBitArrayAndSet(bitarray, bitset);
			#endregion

			// BitArray has no AndNot operation

			#region Or
			pair = NewBitSetAndArray(rand, length);
			bitarray_mod = pair.Key;
			bitset_mod = pair.Value;

			bitarray.Or(bitarray_mod);
			bitset.Or(bitset_mod);
			CompareBitArrayAndSet(bitarray, bitset);
			#endregion

			#region Xor
			pair = NewBitSetAndArray(rand, length);
			bitarray_mod = pair.Key;
			bitset_mod = pair.Value;

			bitarray.Xor(bitarray_mod);
			bitset.Xor(bitset_mod);
			CompareBitArrayAndSet(bitarray, bitset);
			#endregion

			#region Not
			CompareBitArrayAndSet(bitarray.Not(), bitset.Not());
			#endregion

			#region SetAll(true)
			bitarray.SetAll(true);
			bitset.SetAll(true);
			CompareBitArrayAndSet(bitarray, bitset);
			#endregion
			#region SetAll(false)
			bitarray.SetAll(false);
			bitset.SetAll(false);
			CompareBitArrayAndSet(bitarray, bitset);
			#endregion
		}
	};
}