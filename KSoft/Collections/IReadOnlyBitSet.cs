using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	[Contracts.ContractClass(typeof(IReadOnlyBitSetContract))]
	public interface IReadOnlyBitSet : ICloneable,
		IReadOnlyCollection<bool>,
		IComparable<IReadOnlyBitSet>, IEquatable<IReadOnlyBitSet>
	{
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>IE, the index of the highest addressable bit plus one</remarks>
		int Length { get; }

		/// <summary>Number of bits set to true</summary>
		int Cardinality { get; }
		/// <summary>Number of bits set to false</summary>
		int CardinalityZeros { get; }

		#region Access
		bool this[int bitIndex] { get; }

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		bool Get(int bitIndex);

		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		int NextClearBitIndex(int startBitIndex = 0);
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		int NextSetBitIndex(int startBitIndex = 0);

		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		EnumeratorWrapper<int> ClearBitIndices { get; }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		EnumeratorWrapper<int> SetBitIndices { get; }
		#endregion
	};
	[Contracts.ContractClassFor(typeof(IReadOnlyBitSet))]
	abstract class IReadOnlyBitSetContract : IReadOnlyBitSet
	{
		public int Length { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }

		public int Cardinality { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
		public int CardinalityZeros { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }

		#region Access
		public bool this[int bitIndex] { get {
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			throw new NotImplementedException();
		} }

		public bool Get(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			throw new NotImplementedException();
		}

		public int NextClearBitIndex(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < Length);

			throw new NotImplementedException();
		}
		public int NextSetBitIndex(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < Length);

			throw new NotImplementedException();
		}

		public EnumeratorWrapper<int> ClearBitIndices { get { throw new NotImplementedException(); } }
		public EnumeratorWrapper<int> SetBitIndices { get { throw new NotImplementedException(); } }
		#endregion

		#region IReadOnlyCollection<bool> Members
		int IReadOnlyCollection<bool>.Count
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region ICloneable Members
		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IComparable<IReadOnlyBitSet> Members
		int IComparable<IReadOnlyBitSet>.CompareTo(IReadOnlyBitSet other)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IEquatable<IReadOnlyBitSet> Members
		bool IEquatable<IReadOnlyBitSet>.Equals(IReadOnlyBitSet other)
		{
			throw new NotImplementedException();
		}
		#endregion
	};
}