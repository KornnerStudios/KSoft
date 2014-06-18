using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// the 'implementation word'
using TWord = System.UInt32;

namespace KSoft.Collections
{
	// http://docs.oracle.com/javase/1.4.2/docs/api/java/util/BitSet.html
	
	// NOTE: there are multiple places in this implementation where it ignores specially handling alignment-only bits.
	// Eg, if a BitSet has 33 bits in it, it would be aligned to 64 bits. If you then called SetAll(true) on it, it 
	// would end up setting 64 bits to true. If you then set the Length to be 64, those previously alignment-only bits
	// would then retain their true state.

	[System.Diagnostics.DebuggerDisplay("Length = {Length}, Cardinality = {Cardinality}")]
	[Serializable, System.Runtime.InteropServices.ComVisible(true)]
	public partial class BitSet : ICloneable,
		ICollection<bool>, System.Collections.ICollection,
		IComparable<IReadOnlyBitSet>, IEquatable<IReadOnlyBitSet>,
		IReadOnlyBitSet,
		IO.IEndianStreamSerializable
	{
		#region Constants
		/// <summary>Number of bytes in the implementation word</summary>
		static readonly int kWordByteCount;
		/// <summary>Number of bits in the implementation word</summary>
		const int kWordBitCount = sizeof(TWord) * Bits.kByteBitCount;

		static readonly Bits.VectorLengthInT kVectorLengthInT;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementBitMask;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementSectionBitMask;
		static readonly Bits.VectorIndexInT kVectorIndexInT;
		static readonly Bits.VectorBitCursorInT kVectorBitCursorInT;

		static readonly Func<TWord, int> kCountZerosForNextBit;

		static BitSet()
		{
			int word_bit_count; // we define a const for this, so ignore it
			int word_bit_shift, word_bit_mod; // unused
			bool success = Bits.GetBitConstants(typeof(TWord),
				out kWordByteCount, out word_bit_count, out word_bit_shift, out word_bit_mod);
			Contract.Assert(success, "TWord is an invalid type for BitSet");

			kVectorLengthInT = Bits.GetVectorLengthInT<TWord>();
			Bits.GetVectorElementBitMaskInT(out kVectorElementBitMask);
			Bits.GetVectorElementSectionBitMaskInT(out kVectorElementSectionBitMask);
			kVectorIndexInT = Bits.GetVectorIndexInT<TWord>();
			kVectorBitCursorInT = Bits.GetVectorBitCursorInT<TWord>();

#pragma warning disable 0429 // Unreachable expression code detected
			// Big:    Bits go from MSB->LSB, so we want to count the 'left most' zeros
			// Little: Bits go from LSB->MSB, so we want to count the 'right most' zeros
			kCountZerosForNextBit = Bits.kVectorWordFormat == Shell.EndianFormat.Big
				? (Func<TWord,int>)Bits.LeadingZerosCount   // Big Endian
				: (Func<TWord,int>)Bits.TrailingZerosCount; // Little Endian
#pragma warning restore 0162
		}
		#endregion

		#region Instance data
		TWord[] mArray;
		int mLength;
		int mVersion;
		#endregion

		/// <summary>Size of a single implementation word, in bytes, used in the internal array to represent this bit set</summary>
		public int UnderlyingWordSize { get { return sizeof(TWord); } }
		/// <summary>Number of implementation words <b>used</b> in the internal array to represent this bit set</summary>
		/// <remarks>
		/// This differs from <see cref="LengthInWords"/> as the internal array can be larger than needed due to downsizing
		/// without calling <see cref="TrimExcess"/>
		/// </remarks>
		public int UnderlyingWordCount { get { return mArray.Length; } }

		/// <summary>Can <see cref="Length"/> be adjusted?</summary>
		public bool FixedLength { get; set; }
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>
		/// IE, the index of the highest addressable bit plus one
		/// 
		/// Note: when downsizing, the underlying storage's size stays the same, but the old bits will be zeroed and
		/// unaddressable. Call <see cref="TrimExcess"/> to optimize the underlying storage to the minimal size
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
						TWord retained_bits_mask;
#pragma warning disable 0162 // comparing const values, could have 'unreachable' code
						if (Bits.kVectorWordFormat == Shell.EndianFormat.Big)
						{
							retained_bits_mask = kVectorElementBitMask(value-1);
							// create a mask for all bits below the new length in this word
							retained_bits_mask -= 1;
							// when bitvectors are written MSB->LSB, we have to invert the mask (which begins at the LSB)
							retained_bits_mask = ~retained_bits_mask;
						}
						else
						{
							retained_bits_mask = kVectorElementBitMask(value);
							// create a mask for all bits below the new length in this word
							retained_bits_mask -= 1;
						}
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
		/// <summary>Number of implementation words <b>needed</b> to represent this bit set</summary>
		/// <remarks>
		/// This differs from <see cref="UnderlyingWordCount"/> as it only considers the absolute least amounts of words needed
		/// and ignores any extra space that may have been accumulated from length downsizing without a call to <see cref="TrimExcess"/>
		/// </remarks>
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

		/// <summary>Update <see cref="Cardinality"/> for an individual word in the underlying array</summary>
		/// <param name="wordIndex"></param>
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
				return GetInternal(bitIndex);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

				SetInternal(bitIndex, value);
			}
		}

		bool GetInternal(int bitIndex, out int wordIndex, out TWord bitmask)
		{
			wordIndex = kVectorIndexInT(bitIndex);
			bitmask = kVectorElementBitMask(bitIndex);

			return (mArray[wordIndex] & bitmask) != 0;
		}
		/// <summary>Get the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		bool GetInternal(int bitIndex)
		{
			int index;
			TWord bitmask;

			return GetInternal(bitIndex, out index, out bitmask);
		}
		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		public bool Get(int bitIndex)
		{
			return GetInternal(bitIndex);
		}

		void SetInternal(int wordIndex, TWord bitmask, bool value)
		{
			if (value)
			{
				mArray[wordIndex] |= bitmask;
				++Cardinality;
			}
			else
			{
				mArray[wordIndex] &= (TWord)~bitmask;
				--Cardinality;
			}

			mVersion++;
		}
		/// <summary>Set the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		void SetInternal(int bitIndex, bool value)
		{
			int index;
			TWord bitmask;
			bool old_value = GetInternal(bitIndex, out index, out bitmask);

			if (old_value != value)
				SetInternal(index, bitmask, value);
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

			int index;
			TWord bitmask;
			bool old_value = GetInternal(bitIndex, out index, out bitmask);

			SetInternal(index, bitmask, !old_value);
		}

		public void SetAll(bool value)
		{
			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			var fill_value = value ? TWord.MaxValue : TWord.MinValue;
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				mArray[x] = fill_value;

			if (value)
				Cardinality = Length;
			else
				Cardinality = 0;

			mVersion++;
		}

		int NextBitIndex(int startBitIndex, bool stateFilter)
		{
			int index, bit_offset;
			kVectorBitCursorInT(startBitIndex, out index, out bit_offset);

			// get a mask for the the bits that start at bit_offset, thus ignoring bits that came before startBitIndex
			var bitmask = kVectorElementSectionBitMask(bit_offset);

			int result_bit_index = TypeExtensions.kNoneInt32;
			var word = mArray[index];
			for (	word = (stateFilter == false ? ~word : word) & bitmask;
					result_bit_index == -1;
					word =  stateFilter == false ? ~mArray[index] : mArray[index])
 			{
				// word will be 0 if it contains bits that are NOT stateFilter, thus we want to ignore such elements.
				// count the number of zeros (representing bits in the undesired state) leading up to the bit with 
				// the desired state, then add the the index in which it appears at within the overall BitSet
				if (word != 0)
					result_bit_index = kCountZerosForNextBit(word) + (index * kWordBitCount);

				// I perform the increment and loop condition here to keep the for() statement simple
				if (++index == mArray.Length)
					break;
 			}

			// If we didn't find a next bit, result will be -1 and thus less than Length, which is desired behavior
			// else, the result is a valid index of the next bit with the desired state
			return result_bit_index < Length 
				? result_bit_index 
				: TypeExtensions.kNoneInt32;
		}
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(int startBitIndex = 0)
		{
			return NextBitIndex(startBitIndex, false);
		}
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(int startBitIndex = 0)
		{
			return NextBitIndex(startBitIndex, true);
		}

		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		public EnumeratorWrapper<int> ClearBitIndices { get {
			return new EnumeratorWrapper<int>(new EnumeratorBitState(this, false));
		} }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
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

			if (!object.ReferenceEquals(value, this))
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

			if (!object.ReferenceEquals(value, this))
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

			if (!object.ReferenceEquals(value, this))
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

			if (!object.ReferenceEquals(value, this))
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
		int IReadOnlyCollection<bool>.Count			{ get { return Cardinality; } }
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
		#region IComparable<IReadOnlyBitSet> Members
		public int CompareTo(IReadOnlyBitSet other)
		{
			if(Length == other.Length)
				return Cardinality - other.Cardinality;

			return Length - other.Length;
		}
		#endregion
		#region IEquatable<IReadOnlyBitSet> Members
		public bool Equals(IReadOnlyBitSet other)
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
	};
}