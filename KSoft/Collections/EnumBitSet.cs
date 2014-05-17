using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public class EnumBitSet<TEnum> : //ICloneable,
		ICollection<TEnum>, System.Collections.ICollection,
		IComparable<EnumBitSet<TEnum>>, IEquatable<EnumBitSet<TEnum>>,
		IO.IEndianStreamSerializable
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		static readonly Func<int, TEnum> FromInt32 = Reflection.EnumValue<TEnum>.FromInt32;
		static readonly Func<TEnum, int> ToInt32 = Reflection.EnumValue<TEnum>.ToInt32;

		static readonly int kBitSetLength = EnumBitEncoder32<TEnum>.kBitCount;

		BitSet mBits;

		/// <summary>Returns the "logical size" of the BitSet</summary>
		public int Length			{ get { return kBitSetLength; } }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality		{ get { return mBits.Cardinality; } }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros	{ get { return mBits.CardinalityZeros; } }

		#region Ctor
		static string CtorExceptionMsgTEnumIsFlags { get {
			return string.Format("Tried to use a Flags enum in an EnumBitSet - {0}",
				Reflection.EnumUtil<TEnum>.EnumType.Name);
		} }
		static string CtorExceptionMsgTEnumHasNone { get {
			return string.Format("Tried to use a Enum with a NONE member in an EnumBitSet - {0}",
				Reflection.EnumUtil<TEnum>.EnumType.Name);
		} }
		public EnumBitSet()
		{
			Contract.Requires<ArgumentException>(!Reflection.EnumUtil<TEnum>.IsFlags, CtorExceptionMsgTEnumIsFlags);
			Contract.Requires<ArgumentException>(!EnumBitEncoder32<TEnum>.kHasNone, CtorExceptionMsgTEnumHasNone);

			mBits = new BitSet(kBitSetLength);
		}
		#endregion

		#region Access
		public bool this[TEnum bitIndex] {
			get {
				int actual_index = ToInt32(bitIndex);
				return mBits[actual_index];
			}
			set {
				int actual_index = ToInt32(bitIndex);
				mBits[actual_index] = value;
			}
		}

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		public bool Get(TEnum bitIndex)
		{
			int actual_index = ToInt32(bitIndex);
			return mBits[actual_index];
		}
		/// <summary>Set the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		public void Set(TEnum bitIndex, bool value)
		{
			int actual_index = ToInt32(bitIndex);
			mBits[actual_index] = value;
		}

		/// <summary>Flip the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		public void Toggle(TEnum bitIndex)
		{
			int actual_index = ToInt32(bitIndex);
			mBits.Toggle(actual_index);
		}

		public void SetAll(bool value)
		{
			mBits.SetAll(value);
		}

		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(TEnum startBitIndex)
		{
			int actual_index = ToInt32(startBitIndex);
			return mBits.NextClearBitIndex(actual_index);
		}
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(TEnum startBitIndex)
		{
			int actual_index = ToInt32(startBitIndex);
			return mBits.NextSetBitIndex(actual_index);
		}

		/// <summary>Get the enum member of the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <param name="noneSentienlValue">Member to use when there isn't another clear bit after <see cref="startBitIndex"/></param>
		/// <returns>The enum member whose bit is 0, or <see cref="noneSentienlValue"/> there isn't another one</returns>
		public TEnum NextClearBit(TEnum startBitIndex, TEnum noneSentienlValue)
		{
			int bit_index = NextClearBitIndex(startBitIndex);
			return bit_index.IsNotNone()
				? FromInt32(bit_index)
				: noneSentienlValue;
		}
		/// <summary>Get the enum member of the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <param name="noneSentienlValue">Member to use when there isn't another set bit after <see cref="startBitIndex"/></param>
		/// <returns>The enum member whose bit is 0, or <see cref="noneSentienlValue"/> there isn't another one</returns>
		public TEnum NextSetBit(TEnum startBitIndex, TEnum noneSentienlValue)
		{
			int bit_index = NextSetBitIndex(startBitIndex);
			return bit_index.IsNotNone()
				? FromInt32(bit_index)
				: noneSentienlValue;
		}

		/// <summary>Enumeration of enum members whose bits are 0 (clear)</summary>
		public EnumeratorWrapper<TEnum> ClearBitIndices { get {
			return new EnumeratorWrapper<TEnum>(new EnumeratorBitState(mBits.ClearBitIndices.GetEnumerator()));
		} }
		/// <summary>Enumeration of enum members whose bits are 1 (set)</summary>
		public EnumeratorWrapper<TEnum> SetBitIndices { get {
			return new EnumeratorWrapper<TEnum>(new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator()));
		} }

		public IEnumerator<TEnum> GetEnumerator()
		{ return new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator()); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{ return new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator()); }
		#endregion

		#region Bit Operations
		/// <summary>Bit AND this set with another</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> And(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.And(value.mBits);
			return this;
		}
		/// <summary>Clears all of the bits in this set whose corresponding bit is set in the specified BitSet</summary>
		/// <param name="value">set the BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> AndNot(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.AndNot(value.mBits);
			return this;
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Or(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.Or(value.mBits);
			return this;
		}
		/// <summary>Bit XOR this set with another</summary>
		/// <param name="value">Set with the bits to XOR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Xor(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.Xor(value.mBits);
			return this;
		}

		/// <summary>Inverts all bits in this set</summary>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Not()
		{
			mBits.Not();
			return this;
		}
		#endregion

		#region ICollection<TEnum> Members
		// We return mBits' since we're the only ones who can ever touch it
		public object SyncRoot { get { return mBits.SyncRoot; } }
		bool System.Collections.ICollection.IsSynchronized { get { return false; } }

		public void Add(TEnum item)					{ Set(item, true); }
		public void Clear()							{ mBits.Clear(); }
		public bool Contains(TEnum item)			{ return Get(item); }

		void ICollection<TEnum>.CopyTo(TEnum[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool Remove(TEnum item)
		{
			bool existed = Get(item);
			Set(item, false);

			return existed;
		}

		/// <summary>returns <see cref="Cardinality"/></summary>
		int ICollection<TEnum>.Count				{ get { return Cardinality; } }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int System.Collections.ICollection.Count	{ get { return Cardinality; } }
		bool ICollection<TEnum>.IsReadOnly			{ get { return true; } }

		public void CopyTo(Array array, int arrayIndex)
		{
			(mBits as System.Collections.ICollection).CopyTo(array, arrayIndex);
		}
		#endregion

		public void CopyTo(bool[] array, int arrayIndex)
		{
			mBits.CopyTo(array, arrayIndex);
		}

		public override int GetHashCode()				{ return mBits.GetHashCode(); }
		#region IComparable<EnumBitSet<TEnum>> Members
		public int CompareTo(EnumBitSet<TEnum> other)	{ return mBits.CompareTo(other.mBits); }
		#endregion
		#region IEquatable<EnumBitSet<TEnum>> Members
		public bool Equals(EnumBitSet<TEnum> other)		{ return mBits.Equals(other.mBits); }
		#endregion

		sealed class EnumeratorBitState : IEnumerator<TEnum>, ICloneable
		{
			readonly IEnumerator<int> mEnumerator;

			public EnumeratorBitState(IEnumerator<int> bitStateEnumerator)
			{
				mEnumerator = bitStateEnumerator;
			}
			public object Clone()		{ return MemberwiseClone(); }

			public TEnum Current		{ get { return FromInt32(mEnumerator.Current); } }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public bool MoveNext()		{ return mEnumerator.MoveNext(); }
			public void Reset()			{ mEnumerator.Reset(); }

			void IDisposable.Dispose()	{ mEnumerator.Dispose(); }
		};

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			mBits.Serialize(s);
		}

		public void SerializeWords(IO.EndianStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			mBits.SerializeWords(s, streamedFormat);
		}
		#endregion

		public void SerializeWords(IO.BitStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			mBits.SerializeWords(s, streamedFormat);
		}
	};
}