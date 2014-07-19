using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			// TODO: need to validate our ZeroAlignmentOnlyBitsForBitOperation logic is working as intended
		}

		[TestMethod]
		public void Collections_BitSetOverlapsTest()
		{
			var lhs_bits = new bool[] { false, false, };
			var lhs_bs = new BitSet(lhs_bits);
			var rhs_bits = new bool[] { false, true, };
			var rhs_bs = new BitSet(rhs_bits);

			Assert.IsTrue (lhs_bs.Overlaps(rhs_bs));
			Assert.IsFalse(lhs_bs.OverlapsSansZeros(rhs_bs));
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
	};
}