using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	public interface IReadOnlyBitSet
		: ICloneable
		, IReadOnlyCollection<bool>
		, IComparable<IReadOnlyBitSet>
		, IEquatable<IReadOnlyBitSet>
	{
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>IE, the index of the highest addressable bit plus one</remarks>
		int Length { get; }

		/// <summary>Number of bits set to true</summary>
		int Cardinality { get; }
		/// <summary>Number of bits set to false</summary>
		int CardinalityZeros { get; }

		bool IsAllClear { get; }

		/// <summary>Versioning id used to sanity check enumerators</summary>
		int Version { get; }

		#region Access
		bool this[int bitIndex] { get; }
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		bool this[int frombitIndex, int toBitIndex] { get; }

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1716:Identifiers should not conflict with keywords", Justification = "Established public bitset access API retained for source compatibility.")]
		bool Get(int bitIndex);

		int NextBitIndex(int startBitIndex, bool stateFilter);
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		int NextClearBitIndex(int startBitIndex = 0);
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		int NextSetBitIndex(int startBitIndex = 0);

		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		StateFilterEnumeratorWrapper ClearBitIndices { get; }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		StateFilterEnumeratorWrapper SetBitIndices { get; }

		/// <summary>Test to see if any bit is on in a range of bits</summary>
		/// <param name="startBitIndex">Bit index to start testing at</param>
		/// <param name="bitCount">number of bits to test</param>
		/// <returns>True if ANY bit is on (set) in the range, false is they're all off (clear)</returns>
		bool TestBits(int startBitIndex, int bitCount);
		#endregion

		#region ISet-like interfaces
		/// <summary>Determines whether this set is a subset of a specified BitSet (this is included in other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set is a subset of other; otherwise, false</returns>
		/// <remarks>
		/// If <paramref name="other"/> contains the same bits as the current set, the current set is still considered a subset of other.
		///
		/// This method always returns <b>false</b> if the current set has elements that are not in <paramref name="other"/>.
		/// </remarks>
		bool IsSubsetOf(IReadOnlyBitSet other);
		/// <summary>Determines whether this set is a superset of a specified BitSet (this includes all of other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set is a superset of other; otherwise, false</returns>
		/// <remarks>
		/// If <paramref name="other"/> contains the same elements as the current set, the current set is still considered a superset of other.
		///
		/// This method always returns <b>false</b> if the current set has fewer elements than <paramref name="other"/>.
		/// </remarks>
		bool IsSupersetOf(IReadOnlyBitSet other);
		/// <summary>Determines whether this set overlaps with the specified BitSet (this has 1+ bits as other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set and other share at least one common element; otherwise, false</returns>
		/// <remarks>{0, 0}.Overlaps({0, 1}) would thus be true.</remarks>
		bool Overlaps(IReadOnlyBitSet other);
		/// <summary>Determines whether this set's TRUE-bits overlaps with the specified BitSet (this has 1+ bits as other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set and other share at least one common TRUE-bit element; otherwise, false</returns>
		/// <remarks>{0, 0}.Overlaps({0, 1}) would thus be false.</remarks>
		bool OverlapsSansZeros(IReadOnlyBitSet other);
//		/// <summary>Determines whether this set and the specified BitSet contain the same elements</summary>
//		/// <param name="other">The BitSet to compare to the current set</param>
//		/// <returns>true if the current set is equal to other; otherwise, false</returns>
//		bool SetEquals(IReadOnlyBitSet other);
		#endregion
	};
}
