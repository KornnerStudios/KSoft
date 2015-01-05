using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using StateFilterEnumerator = IReadOnlyBitSetEnumerators.StateFilterEnumerator;

	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	partial class BitSet
	{
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(int startBitIndex = 0)
		{
			return NextBitIndex(startBitIndex, false);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		public StateFilterEnumeratorWrapper ClearBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, false));
		} }

		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(int startBitIndex = 0)
		{
			return NextBitIndex(startBitIndex, true);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		public StateFilterEnumeratorWrapper SetBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, true));
		} }


		public void ClearBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;
			
			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			var from_word_mask = kVectorElementSectionBitMask(from_word_index);
			var last_word_mask = GetCabooseRetainedBitsMask(last_word_index);

			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask & last_word_mask;
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Remove(ref mArray[from_word_index], mask);
			}
			else
			{
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Remove(ref mArray[from_word_index], from_word_mask);

				for (int x = from_word_index+1; x < last_word_index; x++)
				{
					RecalculateCardinalityUndoRound(x);
					mArray[x] = kWordAllBitsClear;
				}

				RecalculateCardinalityUndoRound(last_word_index);
				Bitwise.Flags.Remove(ref mArray[last_word_index], last_word_mask);
			}
		}

		public void SetBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;
			
			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			var from_word_mask = kVectorElementSectionBitMask(from_word_index);
			var last_word_mask = GetCabooseRetainedBitsMask(last_word_index);

			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask & last_word_mask;
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Add(ref mArray[from_word_index], mask);
				RecalculateCardinalityRound(from_word_index);
			}
			else
			{
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Add(ref mArray[from_word_index], from_word_mask);
				RecalculateCardinalityRound(from_word_index);

				for (int x = from_word_index+1; x < last_word_index; x++)
				{
					RecalculateCardinalityUndoRound(x);
					mArray[x] = kWordAllBitsSet;
					Cardinality += kWordBitCount;
				}

				RecalculateCardinalityUndoRound(last_word_index);
				Bitwise.Flags.Add(ref mArray[last_word_index], last_word_mask);
				RecalculateCardinalityRound(last_word_index);
			}
		}

		public void ToggleBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;
			
			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			var from_word_mask = kVectorElementSectionBitMask(from_word_index);
			var last_word_mask = GetCabooseRetainedBitsMask(last_word_index);

			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask & last_word_mask;
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Toggle(ref mArray[from_word_index], mask);
				RecalculateCardinalityRound(from_word_index);
			}
			else
			{
				RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Toggle(ref mArray[from_word_index], from_word_mask);
				RecalculateCardinalityRound(from_word_index);

				for (int x = from_word_index+1; x < last_word_index; x++)
				{
					RecalculateCardinalityUndoRound(x);
					Bitwise.Flags.Toggle(ref mArray[x], mArray[x]);
					RecalculateCardinalityRound(x);
				}

				RecalculateCardinalityUndoRound(last_word_index);
				Bitwise.Flags.Toggle(ref mArray[last_word_index], last_word_mask);
				RecalculateCardinalityRound(last_word_index);
			}
		}

		public bool TestBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return false;
			
			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			var from_word_mask = kVectorElementSectionBitMask(from_word_index);
			var last_word_mask = GetCabooseRetainedBitsMask(last_word_index);

			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask & last_word_mask;
				return Bitwise.Flags.Test(mArray[from_word_index], mask);
			}
			else
			{
				if (Bitwise.Flags.Test(mArray[from_word_index], from_word_mask))
					return true;

				for (int x = from_word_index+1; x < last_word_index; x++)
				{
					if (mArray[x] > kWordAllBitsClear)
						return true;
				}

				return Bitwise.Flags.Test(mArray[last_word_index], last_word_mask);
			}
		}

	};
}