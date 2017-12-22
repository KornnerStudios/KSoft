using System;
using System.Collections.Generic;
using KSoft.Bitwise;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// the 'implementation word'
using TWord = System.UInt32;

namespace KSoft.Collections
{
	using StateEnumerator = IReadOnlyBitSetEnumerators.StateEnumerator;

	// http://docs.oracle.com/javase/7/docs/api/java/util/BitSet.html

	// NOTE: there are multiple places in this implementation where it ignores specially handling alignment-only bits.
	// Eg, if a BitSet has 33 bits in it, it would be aligned to 64 bits. If you then called SetAll(true) on it, it
	// would end up setting 64 bits to true. If you then set the Length to be 64, those previously alignment-only bits
	// would then retain their true state.
	// ...however, as of 2015, Length and all other places should now be void of this problem (with alignment only bits)

	[System.Diagnostics.DebuggerDisplay("Length = {Length}, Cardinality = {Cardinality}")]
	[Serializable, System.Runtime.InteropServices.ComVisible(true)]
	public sealed partial class BitSet
		: ICollection<bool>, System.Collections.ICollection
		, IReadOnlyBitSet
		, IO.IEndianStreamSerializable
	{
		#region Constants
		static readonly int kWordBitMod;
		/// <summary>Number of bits in the implementation word</summary>
		const int kWordBitCount = sizeof(TWord) * Bits.kByteBitCount;

		const TWord kWordAllBitsClear = TWord.MinValue;
		const TWord kWordAllBitsSet = TWord.MaxValue;

		static readonly Bits.VectorLengthInT kVectorLengthInT;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementBitMask;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementSectionBitMask;
		static readonly Bits.VectorIndexInT kVectorIndexInT;
		static readonly Bits.VectorBitCursorInT kVectorBitCursorInT;

		static readonly Func<TWord, byte> kCountZerosForNextBit;

		static BitSet()
		{
			int word_byte_count;
			int word_bit_count; // we define a const for this, so ignore it
			int word_bit_shift; // unused
			bool success = Bits.GetBitConstants(typeof(TWord),
				out word_byte_count, out word_bit_count, out word_bit_shift, out kWordBitMod);
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
				? (Func<TWord,byte>)Bits.LeadingZerosCount   // Big Endian
				: (Func<TWord,byte>)Bits.TrailingZerosCount; // Little Endian
#pragma warning restore 0162
		}

		/// <summary>
		/// Get the mask needed for a caboose word for masking out its alignment-only (ie, unaddressable) bits
		/// </summary>
		/// <param name="bitLength"></param>
		/// <returns>Mask representing the non-alignment bits in the caboose word, or 0 if there are no alignment-only bits</returns>
		static TWord GetCabooseRetainedBitsMask(int bitLength)
		{
			// if there are no bits left over, then the bit length doesn't require any alignment-only bits
			if ((bitLength & kWordBitMod) == 0)
				return 0;

			TWord retained_bits_mask;
#pragma warning disable 0162 // comparing const values, could have 'unreachable' code
			if (Bits.kVectorWordFormat == Shell.EndianFormat.Big)
			{
				retained_bits_mask = kVectorElementBitMask(bitLength-1);
				// create a mask for all bits below the given length in a caboose word
				retained_bits_mask -= 1;
				// when bitvectors are written MSB->LSB, we have to invert the mask (which begins at the LSB)
				retained_bits_mask = ~retained_bits_mask;
			}
			else
			{
				retained_bits_mask = kVectorElementBitMask(bitLength);
				// create a mask for all bits below the given length in a caboose word
				retained_bits_mask -= 1;
			}
#pragma warning restore 0162

			return retained_bits_mask;
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
						// create a mask for all bits below the new length in this word
						var retained_bits_mask = GetCabooseRetainedBitsMask(value);

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

		/// <summary>Are all the bits in this set currently false?</summary>
		public bool IsAllClear { get { return Cardinality == 0; } }

		int IReadOnlyBitSet.Version { get { return mVersion; } }

		#region Ctor
		#region InitializeArray
		void InitializeArrayWithDefault(int length, bool defaultValue, out int outBitLength)
		{
			outBitLength = length;

			mArray = new TWord[kVectorLengthInT(length)];

			SetAllInternal(defaultValue);

			// the above method doesn't modify anything besides the raw bits,
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

		/// <summary>Creates an empty, growable bit-set</summary>
		public BitSet()
			: this(0, defaultValue:false, fixedLength:false)
		{
		}

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

		#region RecalculateCardinality
		/// <summary>Update <see cref="Cardinality"/> for an individual word in the underlying array</summary>
		/// <param name="wordIndex"></param>
		void RecalculateCardinalityRound(int wordIndex)
		{
			Cardinality += Bits.BitCount(mArray[wordIndex]);
		}
		/// <summary>Undo a previous <see cref="RecalculateCardinalityRound"/> for an individual word in the underlying array</summary>
		/// <param name="wordIndex"></param>
		void RecalculateCardinalityUndoRound(int wordIndex)
		{
			Cardinality -= Bits.BitCount(mArray[wordIndex]);
		}
		void RecalculateCardinalityFinishRounds(int startWordIndex)
		{
			for (int x = startWordIndex, word_count = LengthInWords; x < word_count; x++)
				RecalculateCardinalityRound(x);
		}
		void RecalculateCardinality()
		{
			Cardinality = 0;
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				RecalculateCardinalityRound(x);
		}
		#endregion

		#region ClearAlignmentOnlyBits
		void ClearAlignmentOnlyBits()
		{
			int caboose_word_index = LengthInWords - 1;

			if (caboose_word_index < 0)
				return;

			var retained_bits_mask = GetCabooseRetainedBitsMask(Length);

			if (retained_bits_mask == 0)
				return;

			mArray[caboose_word_index] &= retained_bits_mask;
		}
		void ClearAlignmentOnlyBitsForBitOperation(IReadOnlyBitSet value)
		{
			// if the operation value is longer, it could possibly contain more addressable bits in its caboose word,
			// causing this set's caboose word to have those bits be non-zero in the Bit Operation. So zero them out.
			if (value.Length <= this.Length)
				return;

			var retained_bits_mask = GetCabooseRetainedBitsMask(Length);

			if (retained_bits_mask == 0)
				return;

			int last_word_index = LengthInWords - 1;
			// the Bit Operations below update Cardinality as each word is touched
			// so we'll need to 'undo' the last word's round before we mask out the alignment-only bits, then recalc
			RecalculateCardinalityUndoRound(last_word_index);
			mArray[last_word_index] &= retained_bits_mask;
			RecalculateCardinalityRound(last_word_index);
		}
		#endregion

		#region Access
		public bool this[int bitIndex] {
			get {
				// REMINDER: Contract for bitIndex already specified by IReadOnlyBitSet's contract

				return GetInternal(bitIndex);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

				SetInternal(bitIndex, value);
			}
		}
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		public bool this[int frombitIndex, int toBitIndex] {
			get {
				// REMINDER: Contracts already specified by IReadOnlyBitSet's contract

				int bitCount = toBitIndex - frombitIndex;
				return bitCount > 0 && TestBits(frombitIndex, bitCount);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < Length);
				Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= Length);

				// handle the cases of the set already being all 1's or 0's
				if (value && Cardinality == Length)
					return;
				if (!value && CardinalityZeros == Length)
					return;

				int bitCount = toBitIndex - frombitIndex;
				if (bitCount == 0)
					return;

				if (value)
					SetBits(frombitIndex, bitCount);
				else
					ClearBits(frombitIndex, bitCount);
			}
		}

		bool GetInternal(int bitIndex, out int wordIndex, out TWord bitmask)
		{
			wordIndex = kVectorIndexInT(bitIndex);
			bitmask = kVectorElementBitMask(bitIndex);

			return Flags.Test(mArray[wordIndex], bitmask);
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
				Flags.Add(ref mArray[wordIndex], bitmask);
				++Cardinality;
			}
			else
			{
				Flags.Remove(ref mArray[wordIndex], bitmask);
				--Cardinality;
			}

			mVersion++;
		}
		/// <summary>Set the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		void SetInternal(int bitIndex, bool value)
		{
			// TODO: is it really worth checking that we're not setting a bit to the same state?
			// Yes, currently, as SetInternal updates Cardinality

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
		/// <returns>The bit's new value</returns>
		public bool Toggle(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < Length);

			int index;
			TWord bitmask;
			bool old_value = GetInternal(bitIndex, out index, out bitmask);
			bool new_value = !old_value;

			SetInternal(index, bitmask, new_value);

			return new_value;
		}

		void SetAllInternal(bool value)
		{
			int word_count = LengthInWords;

			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			var fill_value = value
				? kWordAllBitsSet
				: kWordAllBitsClear;
			for (int x = 0; x < word_count; x++)
				mArray[x] = fill_value;

			if (value) // so if any exist, zero them out
				ClearAlignmentOnlyBits();

			// intentionally don't update Cardinality or mVersion here
		}
		public void SetAll(bool value)
		{
			SetAllInternal(value);

			Cardinality = value
				? Length
				: 0;

			mVersion++;
		}

		public int NextBitIndex(int startBitIndex, bool stateFilter)
		{
			int index, bit_offset;
			kVectorBitCursorInT(startBitIndex, out index, out bit_offset);

			// get a mask for the the bits that start at bit_offset, thus ignoring bits that came before startBitIndex
			var bitmask = kVectorElementSectionBitMask(bit_offset);

			int result_bit_index = TypeExtensions.kNone;
			var word = mArray[index];
			for (	word = (stateFilter == false ? ~word : word) & bitmask;
					result_bit_index.IsNone();
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
				: TypeExtensions.kNone;
		}

		public StateEnumerator GetEnumerator()
		{ return new StateEnumerator(this); }
		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{ return new StateEnumerator(this); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{ return new StateEnumerator(this); }
		#endregion

		#region Bit Operations
		/// <summary>Bit AND this set with another</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet And(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (!object.ReferenceEquals(value, this) && value.Length > 0)
			{
				Cardinality = 0;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					mArray[word_index] &= value.mArray[word_index];
					RecalculateCardinalityRound(word_index);
				}

				// NOTE: we don't do ClearAlignmentOnlyBitsForBitOperation here as a larger BitSet won't introduce
				// new TRUE-bits in a And() operation

				RecalculateCardinalityFinishRounds(word_index);

				mVersion++;
			}
			return this;
		}
		/// <summary>Clears all of the bits in this set whose corresponding bit is set in the specified BitSet</summary>
		/// <param name="value">BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public BitSet AndNot(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			// we're clearing with ourself, just clear all the bits
			if (object.ReferenceEquals(value, this))
			{
				Clear(); // will increment mVersion
			}
			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this will be cleared.
			else if (value.Cardinality > 0)
			{
				Cardinality = 0;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					mArray[word_index] &= ~value.mArray[word_index];
					RecalculateCardinalityRound(word_index);
				}

				ClearAlignmentOnlyBitsForBitOperation(value);

				RecalculateCardinalityFinishRounds(word_index);

				mVersion++;
			}

			return this;
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet Or(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this would get modified.
			if (!object.ReferenceEquals(value, this) && value.Cardinality > 0)
			{
				Cardinality = 0;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					mArray[word_index] |= value.mArray[word_index];
					RecalculateCardinalityRound(word_index);
				}

				ClearAlignmentOnlyBitsForBitOperation(value);

				RecalculateCardinalityFinishRounds(word_index);

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

			// we're clearing with ourself, just clear all the bits
			if (object.ReferenceEquals(value, this))
			{
				Clear(); // will increment mVersion
			}
			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this would get modified.
			else if (value.Cardinality > 0)
			{
				Cardinality = 0;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					mArray[word_index] ^= value.mArray[word_index];
					RecalculateCardinalityRound(word_index);
				}

				ClearAlignmentOnlyBitsForBitOperation(value);

				RecalculateCardinalityFinishRounds(word_index);

				mVersion++;
			}

			return this;
		}

		/// <summary>Inverts all bits in this set</summary>
		/// <returns>Returns the current instance</returns>
		public BitSet Not()
		{
			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			for (int x = 0, word_count = LengthInWords; x < word_count; x++)
				mArray[x] = (TWord)~mArray[x];
			// so reset that data
			ClearAlignmentOnlyBits();

			// invert the Cardinality as what was once one is now none!
			Cardinality = CardinalityZeros;

			mVersion++;
			return this;
		}
		#endregion

		/// <summary>Compare the words of this set with another</summary>
		/// <param name="other">The other set, that is equal or greater in length, to compare bits with</param>
		/// <param name="bitsCount">Number of bits to compare</param>
		/// <returns></returns>
		bool BitwiseEquals(BitSet other, int bitsCount)
		{
			if (object.ReferenceEquals(other, this))
				return true;

			int word_index = 0;
			int word_count = kVectorLengthInT(bitsCount) - 1;
			Contract.Assume((word_index+word_count) <= this.LengthInWords);
			Contract.Assume((word_index+word_count) <= other.LengthInWords);

			for (; word_index < word_count; word_index++, bitsCount -= kWordBitCount)
			{
				if (mArray[word_index] != other.mArray[word_index])
					return false;
			}

			var last_word_mask = GetCabooseRetainedBitsMask(bitsCount);

			return
				( this.mArray[word_index] & last_word_mask) ==
				(other.mArray[word_index] & last_word_mask);
		}
		#region ISet-like interfaces
		/// <summary>This set is included in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSubsetOf(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			// THIS is a subset of OTHER if it contains the same set bits as OTHER
			// If THIS is larger, then OTHER couldn't contain the bits of THIS
			return Length <= other.Length &&
				// verify all our bits exist in OTHER
				((BitSet)other).BitwiseEquals(this, this.Length);
		}
		/// <summary>This set includes all of other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSupersetOf(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			// THIS is a superset of OTHER if it contains the same set bits as OTHER
			// If THIS is shorter, then THIS couldn't contain the bits of OTHER
			return Length >= other.Length &&
				// verify all of OTHER's bits exist in THIS
				this.BitwiseEquals((BitSet)other, other.Length);
		}
		/// <summary>This set's bits match 1+ bits in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Overlaps(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			if (object.ReferenceEquals(other, this))
				return true;

			if (Length != 0)
			{
				// NOTE: this algorithm doesn't play nice with auto-aligned arrays where a Bit Operation
				// has tweaked alignment-only data
				var value = (BitSet)other;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				for (int x = 0; x < length_in_words; x++)
				{
					var lhs = mArray[x];
					var rhs = value.mArray[x];
					// if each array has similar bits active only.
					// negate to test for matching FALSE-bits
					if ((lhs & rhs) != 0 || (~lhs & ~rhs) != 0)
						return true;
				}
			}

			return false;
		}
		/// <summary>This set's TRUE-bits match 1+ bits in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool OverlapsSansZeros(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			if (object.ReferenceEquals(other, this))
				return true;

			if (Length != 0)
			{
				// NOTE: this algorithm doesn't play nice with auto-aligned arrays where a Bit Operation
				// has tweaked alignment-only data
				var value = (BitSet)other;
				int length_in_words = System.Math.Min(LengthInWords, value.LengthInWords);
				for (int x = 0; x < length_in_words; x++)
				{
					var lhs = mArray[x];
					var rhs = value.mArray[x];
					// if each array has similar bits active only. FALSE-bits are ignored
					if ((lhs & rhs) != 0)
						return true;
				}
			}

			return false;
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
		[System.Diagnostics.DebuggerStepThrough]
		public void Clear() { SetAll(false); }

		#region CopyTo
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
		#endregion

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash*23 + Length.GetHashCode();
				hash = hash*23 + Cardinality.GetHashCode();
				return hash;
			}
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
			// TODO: this also needs to check BitwiseEquals
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

		#region Enum interfaces
		private void ValidateBit<TEnum>(TEnum bit, int bitIndex)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (bitIndex < 0 || bitIndex >= this.Length)
			{
				throw new ArgumentOutOfRangeException("bit", bit,
					"Enum member is out of range for indexing");
			}
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool Test<TEnum>(TEnum bit)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			ValidateBit(bit, bitIndex);

			return this[bitIndex];
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public BitSet Set<TEnum>(TEnum bit, bool value = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			ValidateBit(bit, bitIndex);

			this.Set(bitIndex, value);
			return this;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public string ToString<TEnum>(TEnum maxCount
			, string valueSeperator = TypeExtensions.kDefaultArrayValueSeperator
			, bool stateFilter = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Cardinality == 0)
				return "";

			int maxCountValue = maxCount.ToInt32(null);
			if (maxCountValue < 0 || maxCountValue >= Length)
			{
				throw new ArgumentOutOfRangeException("maxCount", string.Format("{0}/{1} is invalid",
					maxCount, maxCountValue));
			}

			if (valueSeperator == null)
				valueSeperator = "";

			var enumType = typeof(TEnum);
			var enumMembers = (TEnum[])Enum.GetValues(enumType);

			// Find the member which represents bit-0
			int memberIndex = 0;
			while (memberIndex < enumMembers.Length && memberIndex < maxCountValue && enumMembers[memberIndex].ToInt32(null) != 0)
				memberIndex++;

			var sb = new System.Text.StringBuilder();
			var bitsInDesiredState = stateFilter
				? SetBitIndices
				: ClearBitIndices;
			foreach (int bitIndex in bitsInDesiredState)
			{
				if (bitIndex >= maxCountValue)
					break;

				if (sb.Length > 0)
					sb.Append(valueSeperator);

				sb.Append(enumMembers[memberIndex+bitIndex].ToString());
			}

			return sb.ToString();
		}

		/// <summary>Interprets the provided separated strings as Enum members and sets their corresponding bits</summary>
		/// <returns>True if all strings were parsed successfully, false if there were some strings that failed to parse</returns>
		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool TryParseFlags<TEnum>(string line
			, string valueSeperator = TypeExtensions.kDefaultArrayValueSeperator
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			// LINQ stmt allows there to be whitespace around the commas
			return TryParseFlags<TEnum>(
				KSoft.Util.Trim(System.Text.RegularExpressions.Regex.Split(line, valueSeperator)),
				errorsOutput);
		}

		/// <summary>Interprets the provided strings as Enum members and sets their corresponding bits</summary>
		/// <returns>True if all strings were parsed successfully, false if there were some strings that failed to parse</returns>
		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool TryParseFlags<TEnum>(IEnumerable<string> collection
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (collection == null)
			{
				return false;
			}

			bool success = true;
			foreach (string flagStr in collection)
			{
				var parsed = TryParseFlag<TEnum>(flagStr, errorsOutput);
				if (parsed.HasValue == false)
					continue;
				else if (parsed.Value == false)
					success = false;
			}

			return success;
		}

		private bool? TryParseFlag<TEnum>(string flagStr
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			const bool ignore_case = true;

			// Enum.TryParse will call Trim on the value anyway, so don't add yet another allocation when we can check for whitespace
			if (string.IsNullOrWhiteSpace(flagStr))
				return null;

			TEnum flag;
			if (!Enum.TryParse<TEnum>(flagStr, ignore_case, out flag))
			{
				if (errorsOutput != null)
				{
					errorsOutput.AddFormat("Couldn't parse '{0}' as a {1} flag",
						flagStr, typeof(TEnum));
				}
				return false;
			}

			int bitIndex = flag.ToInt32(null);
			if (bitIndex < 0 || bitIndex > Length)
			{
				if (errorsOutput != null)
				{
					errorsOutput.AddFormat("Member '{0}'={1} in enum {2} can't be used as a bit index",
						flag, bitIndex, typeof(TEnum));
				}
				return false;
			}

			this.Set(bitIndex, true);
			return true;
		}
		#endregion
	};
}