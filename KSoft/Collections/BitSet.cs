using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// the 'implementation word'
using TWord = System.UInt32;

namespace KSoft.Collections
{
	// http://docs.oracle.com/javase/1.4.2/docs/api/java/util/BitSet.html
	// http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/6-b14/java/util/BitSet.java

	[System.Diagnostics.DebuggerDisplay("Length = {Length}, Cardinality = {Cardinality}")]
	[Serializable, System.Runtime.InteropServices.ComVisible(true)]
	public partial class BitSet : ICloneable,
		ICollection<bool>, System.Collections.ICollection,
		IComparable<BitSet>, IEquatable<BitSet>,
		IO.IEndianStreamSerializable
	{
		#region Constants
		/// <summary>Number of bytes in the implementation word</summary>
		static readonly int kWordByteCount;
		/// <summary>Number of bits in the implementation word</summary>
		const int kWordBitCount = sizeof(TWord) * Bits.kByteBitCount;

		static readonly Bits.VectorLengthInT kVectorLengthInT;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementBitMask;
		static readonly Bits.VectorIndexInT kVectorIndexInT;
		static readonly Bits.VectorBitCursorInT kVectorBitCursorInT;

		static BitSet()
		{
			int word_bit_count; // we define a const for this, so ignore it
			int word_bit_shift, word_bit_mod; // unused
			bool success = Bits.GetBitConstants(typeof(TWord),
				out kWordByteCount, out word_bit_count, out word_bit_shift, out word_bit_mod);
			Contract.Assert(success, "TWord is an invalid type for BitSet");

			kVectorLengthInT = Bits.GetVectorLengthInT<TWord>();
			Bits.GetVectorElementBitMaskInT(out kVectorElementBitMask);
			kVectorIndexInT = Bits.GetVectorIndexInT<TWord>();
			kVectorBitCursorInT = Bits.GetVectorBitCursorInT<TWord>();
		}
		#endregion

		#region Instance data
		TWord[] mArray;
		int mLength;
		int mVersion;
		#endregion
		public int UnderlyingWordCount { get { return mArray.Length; } }

		/// <summary>Can <see cref="Length"/> be adjusted?</summary>
		public bool FixedLength { get; set; }
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>
		/// IE, the index of the highest addressable bit plus one
		/// 
		/// Note: when downsizing, the underlying storage's size stays the same, but the old
		/// bits will be zeroed and unaddressable. Call <see cref="TrimExcess"/> to optimize
		/// the underlying storage to the minimal size
		/// </remarks>
		public int Length {
			get { return mLength; }
			set {
				Contract.Requires<InvalidOperationException>(!FixedLength);
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0);
				if (value == mLength)
					return;

				int value_in_words = kVectorLengthInT(value);
				#region resize mArray if needed
				if (value_in_words > mArray.Length)
				{
					var new_array = new TWord[value_in_words];
					Array.Copy(mArray, new_array, mArray.Length);
					mArray = new_array;
				}
				#endregion

				#region clear old bits if downsizing
				if (value < mLength)
				{
					int index, bit_offset;
					kVectorBitCursorInT(value, out index, out bit_offset);

					// clear old, now unused, bits in the caboose word
					if (bit_offset != 0)
					{
						var retained_bits_mask = kVectorElementBitMask(value-1);
						// create a mask for all bits below the new length in this word
						retained_bits_mask -= 1;
#pragma warning disable 0162 // comparing const values, could have 'unreachable' code
						// when bitvectors are written MSB->LSB, we have to invert the mask (which begins at the LSB)
						if (Bits.kVectorWordFormat == Shell.EndianFormat.Big)
							retained_bits_mask = ~retained_bits_mask;
#pragma warning restore 0162
						// keep the still-used bits in the vector, while masking out the old ones
						mArray[index] &= retained_bits_mask;
					}
					else // no caboose, 'hack' index so that the loop below clears it
						index--;

					for (int x = index + 1; x < mArray.Length; x++)
						mArray[x] = 0;

					// update the cardinality, if needed
					if (Cardinality > 0)
					{
						Cardinality = 0;
						for (int x = 0; x < value_in_words; x++)
							RecalculateCardinalityRound(x);
					}
				}
				#endregion

				mLength = value;
				mVersion++;
			}
		}
		public int LengthInWords { get { return kVectorLengthInT(mLength); } }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality { get; private set; }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros { get { return Length - Cardinality; } }

		#region Ctor
		#region InitializeArray
		void InitializeArrayWithDefault(int length, bool defaultValue, out int outBitLength)
		{
			outBitLength = length;

			mArray = new TWord[kVectorLengthInT(length)];

			TWord fill_value = defaultValue ? TWord.MaxValue : TWord.MinValue;
			for (int x = 0; x < mArray.Length; x++)
				mArray[x] = fill_value;

			if (defaultValue)
				Cardinality = outBitLength;
		}
		void InitializeArrayFromBytes(byte[] bytes, int index, int length, out int outBitLength)
		{
			outBitLength = length * Bits.kByteBitCount;

			mArray = new TWord[kVectorLengthInT(outBitLength)];

			Buffer.BlockCopy(bytes, index, mArray, 0, length);

			RecalculateCardinality();
		}
		void InitializeArrayFromBools(bool[] values, int index, int length, out int outBitLength)
		{
			outBitLength = length;

			mArray = new TWord[kVectorLengthInT(outBitLength)];

			for (int x = 0; x < length; x++)
				if (values[index + x])
				{
					mArray[kVectorIndexInT(x)] |= kVectorElementBitMask(x);
					Cardinality++;
				}
		}
		#endregion

		public BitSet(int length, bool defaultValue = false, bool fixedLength = true)
		{
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);

			mVersion = 0;
			InitializeArrayWithDefault(length, defaultValue, out mLength);
			FixedLength = fixedLength;
		}

		public BitSet(byte[] bytes, int index, int length, bool fixedLength = true)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < bytes.Length);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index+length) <= bytes.Length);

			mVersion = 0;
			InitializeArrayFromBytes(bytes, index, length, out mLength);
			FixedLength = fixedLength;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		public BitSet(params byte[] bytes) : this(bytes, 0, bytes.Length, true)
		{
		}

		public BitSet(bool[] values, int index, int length, bool fixedLength = true)
		{
			Contract.Requires<ArgumentNullException>(values != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < values.Length);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index + length) <= values.Length);

			mVersion = 0;
			InitializeArrayFromBools(values, index, length, out mLength);
			FixedLength = fixedLength;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		public BitSet(params bool[] values) : this(values, 0, values.Length, true)
		{
		}

		public BitSet(BitSet set)
		{
			Contract.Requires<ArgumentNullException>(set != null);

			mArray = new TWord[set.mArray.Length];
			Array.Copy(set.mArray, mArray, mArray.Length);

			mLength = set.mLength;
			Cardinality = set.Cardinality;
			mVersion = set.mVersion;
			FixedLength = set.FixedLength;
		}
		public object Clone()	{ return new BitSet(this); }
		#endregion

		void RecalculateCardinalityRound(int wordIndex)
		{
			Cardinality += Bits.BitCount(mArray[wordIndex]);
		}
		void RecalculateCardinality()
		{
			Cardinality = 0;
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				RecalculateCardinalityRound(x);
		}

		#region Access
		public bool this[int bitIndex] {
			get {
				Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

				return GetInternal(bitIndex);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

				SetInternal(bitIndex, value);
			}
		}

		/// <summary>Get the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		bool GetInternal(int bitIndex)
		{
			int index = kVectorIndexInT(bitIndex);
			var bitmask = kVectorElementBitMask(bitIndex);

			return (mArray[index] & bitmask) != 0;
		}
		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		public bool Get(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			return GetInternal(bitIndex);
		}

		/// <summary>Set the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		public void SetInternal(int bitIndex, bool value)
		{
			int index = kVectorIndexInT(bitIndex);
			var bitmask = kVectorElementBitMask(bitIndex);

			bool old_value = (mArray[index] & bitmask) != 0;

			if (old_value != value)
			{
				if (value)
				{
					mArray[index] |= bitmask;
					++Cardinality;
				}
				else
				{
					mArray[index] &= (TWord)~bitmask;
					--Cardinality;
				}

				mVersion++;
			}
		}
		/// <summary>Set the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		public void Set(int bitIndex, bool value)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			SetInternal(bitIndex, value);
		}

		/// <summary>Flip the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		public void Toggle(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			// TODO: Not optimal, but no code-dup
			SetInternal(bitIndex, !GetInternal(bitIndex));
		}

		public void SetAll(bool value)
		{
			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			TWord fill_value = value ? TWord.MaxValue : TWord.MinValue;
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				mArray[x] = fill_value;

			if (value)
				Cardinality = Length;
			else
				Cardinality = 0;

			mVersion++;
		}

		int NextBit(int startBitIndex, bool stateFilter)
		{
			int index, bit_offset;
			kVectorBitCursorInT(startBitIndex, out index, out bit_offset);
			var bitmask = TWord.MaxValue >> bit_offset;

			int result_bit_index = -1;
			var word = mArray[index];
			for (	word = (stateFilter == false ? ~word : word) & bitmask;
					result_bit_index == -1;
					word =  stateFilter == false ? ~mArray[index] : mArray[index])
 			{
				if (word != 0)
					result_bit_index = (index * kWordBitCount) + Bits.LeadingZerosCount(word);
				if (++index == mArray.Length)
					break;
 			}

			return result_bit_index < Length ? result_bit_index : -1;
		}
		public int NextClearBit(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < Length);
			return NextBit(startBitIndex, false);
		}
		public int NextSetBit(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < Length);
			return NextBit(startBitIndex, true);
		}

		public EnumeratorWrapper<int> ClearBitIndices { get {
			return new EnumeratorWrapper<int>(new EnumeratorBitState(this, false));
		} }
		public EnumeratorWrapper<int> SetBitIndices { get {
			return new EnumeratorWrapper<int>(new EnumeratorBitState(this, true));
		} }

		public IEnumerator<bool> GetEnumerator() { return new EnumeratorSimple(this); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()	{ return new EnumeratorSimple(this); }
		#endregion

		#region Bit Operations
		/// <summary>Bit AND this set with another</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet And(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (value != this)
			{
				Cardinality = 0;
				for (int x = System.Math.Min(LengthInWords, value.LengthInWords) - 1; x >= 0; x--)
				{
					mArray[x] &= value.mArray[x];
					RecalculateCardinalityRound(x);
				}

				mVersion++;
			}
			return this;
		}
		/// <summary>Clears all of the bits in this set whose corresponding bit is set in the specified BitSet</summary>
		/// <param name="value">set the BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public BitSet AndNot(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (value != this)
			{
				Cardinality = 0;
				for (int x = System.Math.Min(LengthInWords, value.LengthInWords) - 1; x >= 0; x--)
				{
					mArray[x] &= ~value.mArray[x];
					RecalculateCardinalityRound(x);
				}
			}
			else // we're clearing with ourself, just clear all the bits
				Clear();

			mVersion++;
			return this;
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet Or(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (value != this)
			{
				Cardinality = 0;
				for (int x = System.Math.Min(LengthInWords, value.LengthInWords) - 1; x >= 0; x--)
				{
					mArray[x] |= value.mArray[x];
					RecalculateCardinalityRound(x);
				}

				mVersion++;
			}
			return this;
		}
		/// <summary>Bit XOR this set with another</summary>
		/// <param name="value">Set with the bits to XOR with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet Xor(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (value != this)
			{
				Cardinality = 0;
				for (int x = System.Math.Min(LengthInWords, value.LengthInWords) - 1; x >= 0; x--)
				{
					mArray[x] ^= value.mArray[x];
					RecalculateCardinalityRound(x);
				}
			}
			else // we're xoring with ourself, just clear all the bits
				Clear();

			mVersion++;
			return this;
		}

		/// <summary>Inverts all bits in this set</summary>
		/// <returns>Returns the current instance</returns>
		public BitSet Not()
		{
			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				mArray[x] = (TWord)~mArray[x];

			// invert the Cardinality as what was once one is now none!
			Cardinality = Length - Cardinality;

			mVersion++;
			return this;
		}
		#endregion

		#region ICollection<bool> Members
		[NonSerialized] object mSyncRoot;
		public object SyncRoot { get {
			if (mSyncRoot == null)
				System.Threading.Interlocked.CompareExchange(ref mSyncRoot, new object(), null);
			return mSyncRoot;
		} }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int ICollection<bool>.Count					{ get { return Cardinality; } }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int System.Collections.ICollection.Count	{ get { return Cardinality; } }
		public bool IsReadOnly						{ get { return false; } }
		bool System.Collections.ICollection.IsSynchronized { get { return false; } }

		void ICollection<bool>.Add(bool item)		{ throw new NotSupportedException(); }
		bool ICollection<bool>.Contains(bool item)	{ throw new NotSupportedException(); }
		bool ICollection<bool>.Remove(bool item)	{ throw new NotSupportedException(); }
		#endregion

		/// <summary>Resizes the underlying storage to the minimal size needed to represent the current <see cref="Length"/></summary>
		public void TrimExcess()
		{
			int length_in_words = LengthInWords;

			if (mArray.Length > length_in_words)
				Array.Resize(ref mArray, length_in_words);
		}

		/// <summary>Set all the bits to zero; doesn't modify <see cref="Length"/></summary>
		public void Clear() { SetAll(false); }

		public void CopyTo(bool[] array, int arrayIndex)
		{
			foreach(var bit in this)
				array[arrayIndex++] = bit;
		}

		void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
		{
			// TODO: verify 'array' lengths
			if (array is TWord[])
				Array.Copy(mArray, arrayIndex, array, 0, LengthInWords);
			else if (array is byte[])
				Buffer.BlockCopy(mArray, arrayIndex, array, 0, LengthInWords * sizeof(TWord));
			else if (array is bool[])
				CopyTo((bool[])array, arrayIndex);
			else
				throw new ArgumentException(string.Format("Array type unsupported {0}", array.GetType()));
		}

		public override int GetHashCode()
		{
			return Length ^ Cardinality;
		}
		#region IComparable<BitSet> Members
		public int CompareTo(BitSet other)
		{
			if(Length == other.Length)
				return Cardinality - other.Cardinality;

			return Length - other.Length;
		}
		#endregion
		#region IEquatable<BitSet> Members
		public bool Equals(BitSet other)
		{
			return Length == other.Length && Cardinality == other.Cardinality;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			// TODO: needs to write like byte[] in order to be TWord agnostic
			throw new NotImplementedException();
		}

		public void SerializeWords(IO.EndianStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			bool byte_swap = streamedFormat != Bits.kVectorWordFormat;

			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
			{
				if (byte_swap) Bits.BitReverse(ref mArray[x]);
				s.Stream(ref mArray[x]);
				if (byte_swap) Bits.BitReverse(ref mArray[x]);
			}

			if (s.IsReading)
				RecalculateCardinality();
		}
		#endregion

		public void SerializeWords(IO.BitStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			bool byte_swap = streamedFormat != Bits.kVectorWordFormat;

			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
			{
				if (byte_swap) Bits.BitReverse(ref mArray[x]);
				s.Stream(ref mArray[x]);
				if (byte_swap) Bits.BitReverse(ref mArray[x]);
			}

			if (s.IsReading)
				RecalculateCardinality();
		}
#if false
		public delegate int SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext>(
			IO.TagElementStream<TDoc, TCursor, string> s, BitSet bitset, int bitIndex, TContext ctxt)
			where TDoc : class
			where TCursor : class;

		public void Serialize<TDoc, TCursor, TContext>(IO.TagElementStream<TDoc, TCursor, string> s,
			string elementName,
			TContext ctxt, SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext> streamElement,
			int highestBitIndex = -1)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(streamElement != null);
			Contract.Requires(highestBitIndex.IsNoneOrPositive());
			Contract.Requires(highestBitIndex < Length);

			if (highestBitIndex.IsNone()) highestBitIndex = Length - 1;

			if (s.IsReading)
			{
				int bit_index = 0;
				foreach(var node in s.ElementsByName(elementName))
					using(s.EnterCursorBookmark(node))
					{
						bit_index = streamElement(s, this, -1, ctxt);
						this[bit_index] = true;
					}
			}
			else if (s.IsWriting)
			{
				foreach (int bit_index in SetBitIndices)
				{
					if (bit_index > highestBitIndex) break;

					using (s.EnterCursorBookmark(elementName))
						streamElement(s, this, bit_index, ctxt);
				}
			}
		}
#endif
	};
}