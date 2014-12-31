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

	};
}