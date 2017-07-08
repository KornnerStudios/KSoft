using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	[System.Diagnostics.DebuggerDisplay("Data = {mWord}, Cardinality = {Cardinality}")]
	public struct BitVector32
		: IComparable<BitVector32>
		, IEquatable<BitVector32>
	{
		const int kNumberOfBits = Bits.kInt32BitCount;
		// for Enumerators impl
		const int kLastIndex = kNumberOfBits - 1;
		const Shell.EndianFormat kVectorWordFormat = Shell.EndianFormat.Little;

		uint mWord;

		public BitVector32(uint bits)
		{
			mWord = bits;
		}
		public BitVector32(int bits)
		{
			mWord = (uint)bits;
		}

		public int Data { get { return (int)mWord; } }

		/// <summary>Length in bits. Always returns 32</summary>
		public int Length			{ get { return kNumberOfBits; } }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality		{ get { return Bits.BitCount(mWord); } }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros	{ get { return Length - Cardinality; } }

		/// <summary>Are all the bits in this set currently false?</summary>
		public bool IsAllClear	{ get { return mWord == uint.MinValue; } }
		/// <summary>Are all the bits in this set currently true?</summary>
		public bool IsAllSet	{ get { return mWord == uint.MaxValue; } }

		public int TrailingZerosCount	{ get { return Bits.TrailingZerosCount(mWord); } }
		public int IndexOfHighestBitSet	{ get { return Bits.IndexOfHighestBitSet(mWord); } }

		#region Overrides
		public bool Equals(BitVector32 other)
		{
			return mWord == other.mWord;
		}
		public override bool Equals(object o)
		{
			if (!(o is BitVector32))
				return false;

			return Equals((BitVector32)o);
		}
		public static bool operator ==(BitVector32 x, BitVector32 y)
		{
			return x.Equals(y);
		}
		public static bool operator !=(BitVector32 x, BitVector32 y)
		{
			return !x.Equals(y);
		}

		public override int GetHashCode()
		{
			return mWord.GetHashCode();
		}

		public static string ToString(BitVector32 value)
		{
			const int k_msb = 1 << (kNumberOfBits-1);

			var sb = new System.Text.StringBuilder(/*"BitVector32{".Length*/12 + kNumberOfBits + /*"}".Length"*/1);
			sb.Append("BitVector32{");
			var word = value.Data;
			for (int i = 0; i < kNumberOfBits; i++)
			{
				sb.Append((word & k_msb) != 0
					? "1"
					: "0");

				word <<= 1;
			}
			sb.Append("}");
			return sb.ToString();
		}
		public override string ToString()
		{
			return BitVector32.ToString(this);
		}
		#endregion

		#region Access
		public bool this[int bitIndex]
		{
			get
			{
				Contract.Requires(bitIndex >= 0 && bitIndex < Bits.kInt32BitCount);

				return Bitwise.Flags.Test(mWord, ((uint)1) << bitIndex);
			}
			set
			{
				Contract.Requires(bitIndex >= 0 && bitIndex < Bits.kInt32BitCount);

				var flag = ((uint)1) << bitIndex;

				Bitwise.Flags.Modify(value, ref mWord, flag);
			}
		}
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		public bool this[int frombitIndex, int toBitIndex] {
			get {
				Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < Length);
				Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= Length);

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

		[Contracts.Pure]
		public int NextBitIndex(
			int prevBitIndex = TypeExtensions.kNone, bool stateFilter = true)
		{
			Contract.Requires(prevBitIndex.IsNoneOrPositive() && prevBitIndex < Bits.kInt32BitCount);

			for (int bit_index = prevBitIndex+1; bit_index < kNumberOfBits; bit_index++)
			{
				if (this[bit_index] == stateFilter)
					return bit_index;
			}

			return TypeExtensions.kNone;
		}
		#endregion

		#region Access (ranged)
		public void ClearBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt32(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt32(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Remove(ref mWord, mask);
		}

		public void SetBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt32(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt32(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Add(ref mWord, mask);
		}

		public void ToggleBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt32(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt32(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Toggle(ref mWord, mask);
		}

		[Contracts.Pure]
		public bool TestBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return false;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt32(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt32(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			return Bitwise.Flags.TestAny(mWord, mask);
		}

		#endregion

		#region Bit Operations
		/// <summary>Bit AND this vector with another</summary>
		/// <param name="vector">Vector with the bits to AND with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector32 And(BitVector32 vector)
		{
			return new BitVector32(mWord & vector.mWord);
		}
		/// <summary>Clears all of the bits in this vector whose corresponding bit is set in the specified vector</summary>
		/// <param name="vector">vector with which to mask this vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector32 AndNot(BitVector32 vector)
		{
			return new BitVector32(Bitwise.Flags.Remove(mWord, vector.mWord));
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="vector">Vector with the bits to OR with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector32 Or(BitVector32 vector)
		{
			return new BitVector32(mWord | vector.mWord);
		}
		/// <summary>Bit XOR this vector with another</summary>
		/// <param name="vector">Vector with the bits to XOR with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector32 Xor(BitVector32 vector)
		{
			return new BitVector32(Bitwise.Flags.Toggle(mWord, vector.mWord));
		}

		/// <summary>Inverts all bits in this vector</summary>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector32 Not()
		{
			return new BitVector32(~mWord);
		}
		#endregion

		/// <summary>Set all the bits to zero</summary>
		public void Clear()
		{
			mWord = 0;
		}

		public void SetAll(bool value)
		{
			var fill_value = value
				? uint.MaxValue
				: uint.MinValue;

			mWord = fill_value;
		}

		public int CompareTo(BitVector32 other)
		{
			return mWord.CompareTo(other.mWord);
		}

		#region Math operators
		public static BitVector32 operator &(BitVector32 lhs, BitVector32 rhs)
		{
			return new BitVector32(lhs.mWord & rhs.mWord);
		}
		public static BitVector32 operator |(BitVector32 lhs, BitVector32 rhs)
		{
			return new BitVector32(lhs.mWord | rhs.mWord);
		}
		public static BitVector32 operator ^(BitVector32 lhs, BitVector32 rhs)
		{
			return new BitVector32(lhs.mWord ^ rhs.mWord);
		}

		public static BitVector32 operator ~(BitVector32 value)
		{
			return new BitVector32(~value.mWord);
		}
		#endregion

		#region Enumerators
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(int startBitIndex = -1)
		{
			return NextBitIndex(startBitIndex, false);
		}
		/// <summary>Enumeration of bit indexes in this vector which are 0 (clear)</summary>
		public EnumeratorWrapper<int, StateFilterEnumerator> ClearBitIndices { get {
			return new EnumeratorWrapper<int, StateFilterEnumerator>(new StateFilterEnumerator(this, false));
		} }

		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(int startBitIndex = -1)
		{
			return NextBitIndex(startBitIndex, true);
		}
		/// <summary>Enumeration of bit indexes in this vector which are 1 (set)</summary>
		public EnumeratorWrapper<int, StateFilterEnumerator> SetBitIndices { get {
			return new EnumeratorWrapper<int, StateFilterEnumerator>(new StateFilterEnumerator(this, true));
		} }

		#endregion

		#region Enumerators impls
		[Serializable]
		public struct StateEnumerator
			: IEnumerator< bool >
		{
			readonly BitVector32 mVector;
			int mBitIndex;
			bool mCurrent;

			public StateEnumerator(BitVector32 vector
				)
			{
				mVector = vector;
				mBitIndex = TypeExtensions.kNone;
				mCurrent = default(bool);
			}

			public bool Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > kLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }

			public bool MoveNext()
			{
				if (mBitIndex < kLastIndex)
				{
					mCurrent = mVector[++mBitIndex];
					return true;
				}

				mBitIndex = kNumberOfBits;
				return false;
			}
		};

		[Serializable]
		public struct StateFilterEnumerator
			: IEnumerator< int >
		{
			readonly BitVector32 mVector;
			int mBitIndex;
			int mCurrent;
			readonly bool mStateFilter;
			readonly int mStartBitIndex;

			public StateFilterEnumerator(BitVector32 vector
				, bool stateFilter, int startBitIndex = 0
				)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < vector.Length);

				mStateFilter = stateFilter;
				mStartBitIndex = startBitIndex-1;
				mVector = vector;
				mBitIndex = TypeExtensions.kNone;
				mCurrent = default(int);
			}

			public int Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > kLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }

			public bool MoveNext()
			{
				if (mBitIndex.IsNone())
					mBitIndex = mStartBitIndex;

				if (mBitIndex < kLastIndex)
				{
					mCurrent = mVector.NextBitIndex(++mBitIndex, mStateFilter);

					if (mCurrent >= 0)
					{
						mBitIndex = mCurrent;
						return true;
					}
				}

				mBitIndex = kNumberOfBits;
				return false;
			}
		};

		#endregion

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

			var flag = ((uint)1) << bitIndex;

			return Bitwise.Flags.Test(mWord, flag);
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public BitVector32 Set<TEnum>(TEnum bit, bool value = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			ValidateBit(bit, bitIndex);

			var flag = ((uint)1) << bitIndex;

			Bitwise.Flags.Modify(value, ref mWord, flag);
			return this;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public List<string> ToStrings<TEnum>(TEnum maxCount
			, string valueSeperator = ","
			, bool stateFilter = true
			, List<string> results = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (results == null)
				results = new List<string>(Cardinality);

			if (Cardinality == 0)
				return results;

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

			var bitsInDesiredState = stateFilter
				? SetBitIndices
				: ClearBitIndices;
			foreach (int bitIndex in bitsInDesiredState)
			{
				if (bitIndex >= maxCountValue)
					break;

				results.Add(enumMembers[memberIndex+bitIndex].ToString());
			}

			return results;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public string ToString<TEnum>(TEnum maxCount
			, string valueSeperator = ","
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
			, string valueSeperator = ","
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
				if (parsed.HasValue==false)
					continue;
				else if (parsed.Value==false)
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

			this[bitIndex] = true;
			return true;
		}
		#endregion
	};

	[System.Diagnostics.DebuggerDisplay("Data = {mWord}, Cardinality = {Cardinality}")]
	public struct BitVector64
		: IComparable<BitVector64>
		, IEquatable<BitVector64>
	{
		const int kNumberOfBits = Bits.kInt64BitCount;
		// for Enumerators impl
		const int kLastIndex = kNumberOfBits - 1;
		const Shell.EndianFormat kVectorWordFormat = Shell.EndianFormat.Little;

		ulong mWord;

		public BitVector64(ulong bits)
		{
			mWord = bits;
		}
		public BitVector64(long bits)
		{
			mWord = (ulong)bits;
		}

		public long Data { get { return (long)mWord; } }

		/// <summary>Length in bits. Always returns 64</summary>
		public int Length			{ get { return kNumberOfBits; } }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality		{ get { return Bits.BitCount(mWord); } }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros	{ get { return Length - Cardinality; } }

		/// <summary>Are all the bits in this set currently false?</summary>
		public bool IsAllClear	{ get { return mWord == ulong.MinValue; } }
		/// <summary>Are all the bits in this set currently true?</summary>
		public bool IsAllSet	{ get { return mWord == ulong.MaxValue; } }

		public int TrailingZerosCount	{ get { return Bits.TrailingZerosCount(mWord); } }
		public int IndexOfHighestBitSet	{ get { return Bits.IndexOfHighestBitSet(mWord); } }

		#region Overrides
		public bool Equals(BitVector64 other)
		{
			return mWord == other.mWord;
		}
		public override bool Equals(object o)
		{
			if (!(o is BitVector64))
				return false;

			return Equals((BitVector64)o);
		}
		public static bool operator ==(BitVector64 x, BitVector64 y)
		{
			return x.Equals(y);
		}
		public static bool operator !=(BitVector64 x, BitVector64 y)
		{
			return !x.Equals(y);
		}

		public override int GetHashCode()
		{
			return mWord.GetHashCode();
		}

		public static string ToString(BitVector64 value)
		{
			const long k_msb = 1 << (kNumberOfBits-1);

			var sb = new System.Text.StringBuilder(/*"BitVector64{".Length*/12 + kNumberOfBits + /*"}".Length"*/1);
			sb.Append("BitVector64{");
			var word = value.Data;
			for (int i = 0; i < kNumberOfBits; i++)
			{
				sb.Append((word & k_msb) != 0
					? "1"
					: "0");

				word <<= 1;
			}
			sb.Append("}");
			return sb.ToString();
		}
		public override string ToString()
		{
			return BitVector64.ToString(this);
		}
		#endregion

		#region Access
		public bool this[int bitIndex]
		{
			get
			{
				Contract.Requires(bitIndex >= 0 && bitIndex < Bits.kInt64BitCount);

				return Bitwise.Flags.Test(mWord, ((ulong)1) << bitIndex);
			}
			set
			{
				Contract.Requires(bitIndex >= 0 && bitIndex < Bits.kInt64BitCount);

				var flag = ((ulong)1) << bitIndex;

				Bitwise.Flags.Modify(value, ref mWord, flag);
			}
		}
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		public bool this[int frombitIndex, int toBitIndex] {
			get {
				Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < Length);
				Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= Length);

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

		[Contracts.Pure]
		public int NextBitIndex(
			int prevBitIndex = TypeExtensions.kNone, bool stateFilter = true)
		{
			Contract.Requires(prevBitIndex.IsNoneOrPositive() && prevBitIndex < Bits.kInt64BitCount);

			for (int bit_index = prevBitIndex+1; bit_index < kNumberOfBits; bit_index++)
			{
				if (this[bit_index] == stateFilter)
					return bit_index;
			}

			return TypeExtensions.kNone;
		}
		#endregion

		#region Access (ranged)
		public void ClearBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt64(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt64(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Remove(ref mWord, mask);
		}

		public void SetBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt64(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt64(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Add(ref mWord, mask);
		}

		public void ToggleBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt64(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt64(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			Bitwise.Flags.Toggle(ref mWord, mask);
		}

		[Contracts.Pure]
		public bool TestBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);

			if (bitCount <= 0)
				return false;

			var from_word_mask = Bits.VectorElementSectionBitMaskInInt64(startBitIndex, kVectorWordFormat);
//			var last_word_mask = Bits.VectorElementBitMaskInInt64(startBitIndex+bitCount, kVectorWordFormat);
			// create a mask for all bits below the given length in a caboose word
//			last_word_mask -= 1;

			var mask = from_word_mask;// & last_word_mask;
			return Bitwise.Flags.TestAny(mWord, mask);
		}

		#endregion

		#region Bit Operations
		/// <summary>Bit AND this vector with another</summary>
		/// <param name="vector">Vector with the bits to AND with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector64 And(BitVector64 vector)
		{
			return new BitVector64(mWord & vector.mWord);
		}
		/// <summary>Clears all of the bits in this vector whose corresponding bit is set in the specified vector</summary>
		/// <param name="vector">vector with which to mask this vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector64 AndNot(BitVector64 vector)
		{
			return new BitVector64(Bitwise.Flags.Remove(mWord, vector.mWord));
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="vector">Vector with the bits to OR with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector64 Or(BitVector64 vector)
		{
			return new BitVector64(mWord | vector.mWord);
		}
		/// <summary>Bit XOR this vector with another</summary>
		/// <param name="vector">Vector with the bits to XOR with</param>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector64 Xor(BitVector64 vector)
		{
			return new BitVector64(Bitwise.Flags.Toggle(mWord, vector.mWord));
		}

		/// <summary>Inverts all bits in this vector</summary>
		/// <returns></returns>
		[Contracts.Pure]
		public BitVector64 Not()
		{
			return new BitVector64(~mWord);
		}
		#endregion

		/// <summary>Set all the bits to zero</summary>
		public void Clear()
		{
			mWord = 0;
		}

		public void SetAll(bool value)
		{
			var fill_value = value
				? ulong.MaxValue
				: ulong.MinValue;

			mWord = fill_value;
		}

		public int CompareTo(BitVector64 other)
		{
			return mWord.CompareTo(other.mWord);
		}

		#region Math operators
		public static BitVector64 operator &(BitVector64 lhs, BitVector64 rhs)
		{
			return new BitVector64(lhs.mWord & rhs.mWord);
		}
		public static BitVector64 operator |(BitVector64 lhs, BitVector64 rhs)
		{
			return new BitVector64(lhs.mWord | rhs.mWord);
		}
		public static BitVector64 operator ^(BitVector64 lhs, BitVector64 rhs)
		{
			return new BitVector64(lhs.mWord ^ rhs.mWord);
		}

		public static BitVector64 operator ~(BitVector64 value)
		{
			return new BitVector64(~value.mWord);
		}
		#endregion

		#region Enumerators
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(int startBitIndex = -1)
		{
			return NextBitIndex(startBitIndex, false);
		}
		/// <summary>Enumeration of bit indexes in this vector which are 0 (clear)</summary>
		public EnumeratorWrapper<int, StateFilterEnumerator> ClearBitIndices { get {
			return new EnumeratorWrapper<int, StateFilterEnumerator>(new StateFilterEnumerator(this, false));
		} }

		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(int startBitIndex = -1)
		{
			return NextBitIndex(startBitIndex, true);
		}
		/// <summary>Enumeration of bit indexes in this vector which are 1 (set)</summary>
		public EnumeratorWrapper<int, StateFilterEnumerator> SetBitIndices { get {
			return new EnumeratorWrapper<int, StateFilterEnumerator>(new StateFilterEnumerator(this, true));
		} }

		#endregion

		#region Enumerators impls
		[Serializable]
		public struct StateEnumerator
			: IEnumerator< bool >
		{
			readonly BitVector64 mVector;
			int mBitIndex;
			bool mCurrent;

			public StateEnumerator(BitVector64 vector
				)
			{
				mVector = vector;
				mBitIndex = TypeExtensions.kNone;
				mCurrent = default(bool);
			}

			public bool Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > kLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }

			public bool MoveNext()
			{
				if (mBitIndex < kLastIndex)
				{
					mCurrent = mVector[++mBitIndex];
					return true;
				}

				mBitIndex = kNumberOfBits;
				return false;
			}
		};

		[Serializable]
		public struct StateFilterEnumerator
			: IEnumerator< int >
		{
			readonly BitVector64 mVector;
			int mBitIndex;
			int mCurrent;
			readonly bool mStateFilter;
			readonly int mStartBitIndex;

			public StateFilterEnumerator(BitVector64 vector
				, bool stateFilter, int startBitIndex = 0
				)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < vector.Length);

				mStateFilter = stateFilter;
				mStartBitIndex = startBitIndex-1;
				mVector = vector;
				mBitIndex = TypeExtensions.kNone;
				mCurrent = default(int);
			}

			public int Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > kLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }

			public bool MoveNext()
			{
				if (mBitIndex.IsNone())
					mBitIndex = mStartBitIndex;

				if (mBitIndex < kLastIndex)
				{
					mCurrent = mVector.NextBitIndex(++mBitIndex, mStateFilter);

					if (mCurrent >= 0)
					{
						mBitIndex = mCurrent;
						return true;
					}
				}

				mBitIndex = kNumberOfBits;
				return false;
			}
		};

		#endregion

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

			var flag = ((ulong)1) << bitIndex;

			return Bitwise.Flags.Test(mWord, flag);
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public BitVector64 Set<TEnum>(TEnum bit, bool value = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			ValidateBit(bit, bitIndex);

			var flag = ((ulong)1) << bitIndex;

			Bitwise.Flags.Modify(value, ref mWord, flag);
			return this;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public List<string> ToStrings<TEnum>(TEnum maxCount
			, string valueSeperator = ","
			, bool stateFilter = true
			, List<string> results = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (results == null)
				results = new List<string>(Cardinality);

			if (Cardinality == 0)
				return results;

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

			var bitsInDesiredState = stateFilter
				? SetBitIndices
				: ClearBitIndices;
			foreach (int bitIndex in bitsInDesiredState)
			{
				if (bitIndex >= maxCountValue)
					break;

				results.Add(enumMembers[memberIndex+bitIndex].ToString());
			}

			return results;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public string ToString<TEnum>(TEnum maxCount
			, string valueSeperator = ","
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
			, string valueSeperator = ","
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
				if (parsed.HasValue==false)
					continue;
				else if (parsed.Value==false)
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

			this[bitIndex] = true;
			return true;
		}
		#endregion
	};

}