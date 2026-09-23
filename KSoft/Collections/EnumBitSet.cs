using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	using StateFilterEnumerator = IReadOnlyBitSetEnumerators.StateFilterEnumerator;

	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	/// <summary>A mutable reference-backed bit set addressed through a declared enum-index domain.</summary>
	/// <typeparam name="TEnum">The bit-index enum governed by <see cref="EnumBitTraits{TBits}"/>.</typeparam>
	/// <remarks>Copying a reference shares mutations. Member validity, negative sentinels, and independence from enum-value encoding follow <see cref="EnumBitTraits{TBits}"/>. Storage operations include nonmember positions, while named enumeration and collection count exclude them. The legacy <see cref="ICollection{T}.IsReadOnly"/> implementation reports true despite supporting mutation, and its typed <c>CopyTo</c> throws <see cref="NotSupportedException"/>.</remarks>
	public sealed class EnumBitSet<TEnum>
		: ICollection<TEnum>, System.Collections.ICollection
		, IComparable<EnumBitSet<TEnum>>, IEquatable<EnumBitSet<TEnum>>
		, IO.IEndianStreamSerializable
		where TEnum : struct, Enum
	{
		readonly BitSet mBits;
		readonly TEnum mInvalidSentinelValue;

		/// <summary>Gets the logical bit-position extent, not the number of names or enum-value encoding width.</summary>
		public int Length			{ get => mBits.Length; }
		/// <summary>Gets the number of stored set bits, including positions without declared members.</summary>
		public int Cardinality		{ get => mBits.Cardinality; }
		/// <summary>Gets the number of stored clear bits within <see cref="Length"/>, including nonmember positions.</summary>
		public int CardinalityZeros	{ get => mBits.CardinalityZeros; }

		/// <summary>Gets the caller-supplied value returned unchanged when an enum-returning search finds no member.</summary>
		/// <remarks>This value is not validated as a usable bit. The default zero is ambiguous if zero names a real member.</remarks>
		public TEnum InvalidSentinelValue { get => mInvalidSentinelValue; }

		#region Ctor
		/// <summary>Creates an all-clear set for the enum's logical extent.</summary>
		/// <param name="invalidSentinelValue">Unvalidated result for an exhausted enum-returning search; use a distinct sentinel when zero is a real member.</param>
		/// <exception cref="ArgumentException">The enum-index domain is invalid.</exception>
		public EnumBitSet(TEnum invalidSentinelValue = default)
		{
			mBits = new BitSet(EnumBitTraits<TEnum>.Length);
			mInvalidSentinelValue = invalidSentinelValue;
		}
		#endregion

		#region Access
		/// <summary>Gets or changes a usable declared bit in this set.</summary>
		/// <param name="bitIndex">The declared bit to address.</param>
		/// <returns>The stored bit state.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared member.</exception>
		public bool this[TEnum bitIndex]
		{
			get {
				int actual_index = EnumBitTraits<TEnum>.ToIndex(bitIndex, nameof(bitIndex));
				return mBits[actual_index];
			}
			set {
				int actual_index = EnumBitTraits<TEnum>.ToIndex(bitIndex, nameof(bitIndex));
				mBits[actual_index] = value;
			}
		}

		/// <summary>Gets the state of a usable declared bit.</summary>
		/// <param name="bitIndex">The declared bit to read.</param>
		/// <returns>The stored bit state.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is negative, undefined, or an exclusive-bound member.</exception>
		public bool Get(TEnum bitIndex)
		{
			int actual_index = EnumBitTraits<TEnum>.ToIndex(bitIndex, nameof(bitIndex));
			return mBits[actual_index];
		}
		/// <summary>Tests a usable declared bit, with the same behavior as <see cref="Get"/>.</summary>
		/// <param name="bitIndex">The declared bit to test.</param>
		/// <returns>The stored bit state.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared member.</exception>
		public bool Test(TEnum bitIndex) => Get(bitIndex);
		/// <summary>Changes a usable declared bit in place.</summary>
		/// <param name="bitIndex">The declared bit to change.</param>
		/// <param name="value">The new bit state.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared member.</exception>
		public void Set(TEnum bitIndex, bool value)
		{
			int actual_index = EnumBitTraits<TEnum>.ToIndex(bitIndex, nameof(bitIndex));
			mBits[actual_index] = value;
		}

		/// <summary>Flips a usable declared bit in place.</summary>
		/// <param name="bitIndex">The declared bit to change.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared member.</exception>
		public void Toggle(TEnum bitIndex)
		{
			int actual_index = EnumBitTraits<TEnum>.ToIndex(bitIndex, nameof(bitIndex));
			mBits.Toggle(actual_index);
		}

		/// <summary>Changes every stored position within the logical extent, including nonmember positions, in place.</summary>
		/// <param name="value">The state to assign to every position.</param>
		public void SetAll(bool value) => mBits.SetAll(value);

		/// <summary>Finds a physical clear position at or after a declared starting member.</summary>
		/// <param name="startBitIndex">The usable declared starting member, included in the search.</param>
		/// <returns>A physical clear index, which need not have a declaration, or -1 on exhaustion.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startBitIndex"/> is not a usable declared member.</exception>
		public int NextClearBitIndex(TEnum startBitIndex)
		{
			int actual_index = EnumBitTraits<TEnum>.ToIndex(startBitIndex, nameof(startBitIndex));
			return mBits.NextClearBitIndex(actual_index);
		}
		/// <summary>Finds a physical set position at or after a declared starting member.</summary>
		/// <param name="startBitIndex">The usable declared starting member, included in the search.</param>
		/// <returns>A physical set index, which need not have a declaration, or -1 on exhaustion.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startBitIndex"/> is not a usable declared member.</exception>
		public int NextSetBitIndex(TEnum startBitIndex)
		{
			int actual_index = EnumBitTraits<TEnum>.ToIndex(startBitIndex, nameof(startBitIndex));
			return mBits.NextSetBitIndex(actual_index);
		}

		/// <summary>Finds a declared clear bit at or after the supplied member, skipping gaps.</summary>
		/// <param name="startBitIndex">The usable declared starting member, included in the search.</param>
		/// <returns>The next member whose bit is 0, or <see cref="InvalidSentinelValue"/> on exhaustion.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startBitIndex"/> is not a usable declared member.</exception>
		public TEnum NextClearBit(TEnum startBitIndex)
		{
			return NextDeclaredBit(startBitIndex, false);
		}
		/// <summary>Finds a declared set bit at or after the supplied member, skipping gaps.</summary>
		/// <param name="startBitIndex">The usable declared starting member, included in the search.</param>
		/// <returns>The next member whose bit is 1, or <see cref="InvalidSentinelValue"/> on exhaustion.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startBitIndex"/> is not a usable declared member.</exception>
		public TEnum NextSetBit(TEnum startBitIndex)
		{
			return NextDeclaredBit(startBitIndex, true);
		}

		TEnum NextDeclaredBit(TEnum startBitIndex, bool state)
		{
			int start = EnumBitTraits<TEnum>.ToIndex(startBitIndex, nameof(startBitIndex));
			var indices = EnumBitTraits<TEnum>.Indices;
			if (indices.Length == Length)
			{
				int index = mBits.NextBitIndex(start, state);
				return index >= 0 ? EnumBitTraits<TEnum>.FromIndex(index) : mInvalidSentinelValue;
			}

			// Sparse domains must skip gaps without scanning their physical storage or revisiting earlier members.
			for (int offset = indices.BinarySearch(start); offset < indices.Length; offset++)
			{
				int index = indices[offset];
				if (mBits[index] == state)
				{
					return EnumBitTraits<TEnum>.FromIndex(index);
				}
			}
			return mInvalidSentinelValue;
		}

		/// <summary>Enumerates declared clear members once per numeric index in ascending order, excluding gaps.</summary>
		public EnumeratorWrapper<TEnum, EnumeratorBitState> ClearBitIndices =>
			new(new EnumeratorBitState(mBits.ClearBitIndices.GetEnumerator()));
		/// <summary>Enumerates declared set members once per numeric index in ascending order, excluding gaps.</summary>
		public EnumeratorWrapper<TEnum, EnumeratorBitState> SetBitIndices =>
			new(new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator()));

		/// <summary>Gets an enumerator over declared set members in ascending index order.</summary>
		/// <returns>An enumerator that skips nonmember positions.</returns>
		public EnumeratorBitState GetEnumerator() => new(mBits.SetBitIndices.GetEnumerator());
		IEnumerator<TEnum> IEnumerable<TEnum>.GetEnumerator() => new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator());
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator());
		#endregion

		#region Bit Operations
		/// <summary>ANDs the stored bits with another set in place, including nonmember positions.</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> And(EnumBitSet<TEnum> value)
		{
			ArgumentNullException.ThrowIfNull(value);

			mBits.And(value.mBits);
			return this;
		}
		/// <summary>Clears stored bits whose corresponding bits are set in the other set, in place, including nonmember positions.</summary>
		/// <param name="value">set the BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> AndNot(EnumBitSet<TEnum> value)
		{
			ArgumentNullException.ThrowIfNull(value);

			mBits.AndNot(value.mBits);
			return this;
		}
		/// <summary>ORs the stored bits with another set in place, including nonmember positions.</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Or(EnumBitSet<TEnum> value)
		{
			ArgumentNullException.ThrowIfNull(value);

			mBits.Or(value.mBits);
			return this;
		}
		/// <summary>XORs the stored bits with another set in place, including nonmember positions.</summary>
		/// <param name="value">Set with the bits to XOR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Xor(EnumBitSet<TEnum> value)
		{
			ArgumentNullException.ThrowIfNull(value);

			mBits.Xor(value.mBits);
			return this;
		}

		/// <summary>Inverts all stored bits within the logical extent in place, including nonmember positions.</summary>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Not()
		{
			mBits.Not();
			return this;
		}
		#endregion

		#region ICollection<TEnum> Members
		// We return mBits' since we're the only ones who can ever touch it
		public object SyncRoot { get => mBits.SyncRoot; }
		bool System.Collections.ICollection.IsSynchronized { get => false; }

		public void Add(TEnum item)					=> Set(item, true);
		/// <summary>Clears every stored position in place, including nonmember positions.</summary>
		public void Clear()							=> mBits.Clear();
		public bool Contains(TEnum item)			=> Get(item);

		/// <summary>The legacy typed-array copy operation is not implemented.</summary>
		/// <param name="array">The requested destination.</param>
		/// <param name="arrayIndex">The requested destination offset.</param>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		void ICollection<TEnum>.CopyTo(TEnum[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(TEnum item)
		{
			bool existed = Get(item);
			Set(item, false);

			return existed;
		}

		int NamedCardinality
		{
			get
			{
				if (EnumBitTraits<TEnum>.DeclaredCount == Length)
				{
					return Cardinality;
				}
				int count = 0;
				foreach (int index in EnumBitTraits<TEnum>.Indices)
				{
					if (mBits[index]) count++;
				}
				return count;
			}
		}
		/// <summary>Number of declared members whose bits are set.</summary>
		int ICollection<TEnum>.Count				{ get => NamedCardinality; }
		/// <summary>Number of declared members whose bits are set, matching named enumeration rather than physical cardinality.</summary>
		int System.Collections.ICollection.Count	{ get => NamedCardinality; }
		bool ICollection<TEnum>.IsReadOnly			{ get => true; }

		public void CopyTo(Array array, int arrayIndex) => (mBits as System.Collections.ICollection).CopyTo(array, arrayIndex);
		#endregion

		public void CopyTo(bool[] array, int arrayIndex) => mBits.CopyTo(array, arrayIndex);

		public override bool Equals(object? obj)
		{
			if (obj is EnumBitSet<TEnum> o)
			{
				return this.Equals(o);
			}

			return false;
		}

		public override int GetHashCode()				=> mBits.GetHashCode();
		#region IComparable<EnumBitSet<TEnum>> Members
		public int CompareTo(EnumBitSet<TEnum>? other)	=> mBits.CompareTo(other!.mBits);
		#endregion
		#region IEquatable<EnumBitSet<TEnum>> Members
		public bool Equals(EnumBitSet<TEnum>? other)		=> mBits.Equals(other!.mBits);
		#endregion

		public struct EnumeratorBitState
			: IEnumerator<TEnum>
		{
			StateFilterEnumerator mEnumerator;

			public EnumeratorBitState(StateFilterEnumerator bitStateEnumerator)
			{
				mEnumerator = bitStateEnumerator;
			}

			public readonly TEnum Current		=> EnumBitTraits<TEnum>.FromIndex(mEnumerator.Current);
			readonly object System.Collections.IEnumerator.Current => this.Current;

			public bool MoveNext()
			{
				while (mEnumerator.MoveNext())
				{
					if (EnumBitTraits<TEnum>.IsDefinedIndex(mEnumerator.Current))
					{
						return true;
					}
				}
				return false;
			}
			public void Reset()			=> mEnumerator.Reset();

			public readonly void Dispose()		=> mEnumerator.Dispose();
		};

		#region IEndianStreamSerializable Members
		/// <summary>The delegated general BitSet serialization format is not implemented.</summary>
		/// <param name="s">The requested serialization stream.</param>
		/// <exception cref="NotImplementedException">Always thrown by the underlying BitSet.</exception>
		public void Serialize(IO.EndianStream s) => mBits.Serialize(s);

		/// <summary>Streams the underlying implementation words without a logical-length or enum-definition header.</summary>
		/// <param name="s">The stream controlling byte order and read/write mode.</param>
		/// <param name="streamedFormat">Bit order within each streamed word, independent of the stream's byte order.</param>
		/// <remarks>Both endpoints must agree on extent and layout. Corrected enum extents can change the word count; historical undersized word-only streams are not detected or migrated automatically.</remarks>
		public void SerializeWords(IO.EndianStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat) => mBits.SerializeWords(s, streamedFormat);
		#endregion

		/// <summary>Streams the underlying implementation words through a bit stream without a length or enum-definition header.</summary>
		/// <param name="s">The stream controlling read/write mode.</param>
		/// <param name="streamedFormat">Bit order within each streamed word.</param>
		/// <remarks>Both endpoints must agree on extent and layout; this uses the same word-count contract as <see cref="SerializeWords(IO.EndianStream, Shell.EndianFormat)"/>.</remarks>
		public void SerializeWords(IO.BitStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
			=> mBits.SerializeWords(s, streamedFormat);
	};
}
