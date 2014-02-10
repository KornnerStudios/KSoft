using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	/// <summary>Utility class for bit level manipulation</summary>
	// Reference: http://graphics.stanford.edu/~seander/bithacks.html
	// Also, http://stackoverflow.com/questions/746171/best-algorithm-for-bit-reversal-from-msb-lsb-to-lsb-msb-in-c
	// http://corner.squareup.com/2013/07/reversing-bits-on-arm.html
	public static partial class Bits
	{
		/// <summary>Number of logical bits in a <see cref="System.SByte"/></summary>
		public const int kBooleanBitCount = 1;

		[Contracts.Pure]
		static int BitmaskLookUpTableGetLength(int wordBitSize)
		{
			// first element in the LUT is zero, followed by a mask for each range of bits up until wordBitSize
			return 1 + wordBitSize;
		}

		#region MultiplyDeBruijnBitPosition
		static readonly int[]	kMultiplyDeBruijnBitPositionHighestBitSet32,
								kMultiplyDeBruijnBitPositionLeadingZeros32,
								kMultiplyDeBruijnBitPositionTrailingZeros32;
		#endregion

		#region Contract messages
		const string kBitSwap_StartBitIndexNotGreaterThanZero =
			"Doesn't make sense to bit swap 1 bit. Or to start at a negative index";

		const string kGetMaxEnumBits_MaxValueOutOfRangeMessage = "There is no point in this if '0' is the only option";

		const string kGetBitmaskEnum_MaxValueOutOfRangeMessage = kGetMaxEnumBits_MaxValueOutOfRangeMessage;
		const string kGetBitmaskFlag_MaxValueOutOfRangeMessage = kGetMaxEnumBits_MaxValueOutOfRangeMessage;
		#endregion

		static Bits()
		{
			#region kBitmaskLookup
			BitmaskLookUpTableGenerate(Bits.kByteBitCount,  out kBitmaskLookup8);
			BitmaskLookUpTableGenerate(Bits.kInt16BitCount, out kBitmaskLookup16);
			BitmaskLookUpTableGenerate(Bits.kInt32BitCount, out kBitmaskLookup32);
			BitmaskLookUpTableGenerate(Bits.kInt64BitCount, out kBitmaskLookup64);
			#endregion

			#region MultiplyDeBruijnBitPosition
			kMultiplyDeBruijnBitPositionHighestBitSet32 = new int[kInt32BitCount]
			{
				0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
				8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
			};

			kMultiplyDeBruijnBitPositionLeadingZeros32 = new int[kInt32BitCount];
			for (int x = 0; x < kMultiplyDeBruijnBitPositionLeadingZeros32.Length; x++)
				kMultiplyDeBruijnBitPositionLeadingZeros32[x] = kMultiplyDeBruijnBitPositionHighestBitSet32[x]+1;

			kMultiplyDeBruijnBitPositionTrailingZeros32 = new int[kInt32BitCount]
			{
				0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
				31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
			};
			#endregion
		}

		#region Memory/ArrayCopy
		public struct MemoryCopier<TDst, TSrc>
			where TDst : struct
			where TSrc : struct
		{
			// As long as no one uses the default constructor, the cctor should be ran before instance code
			// http://stackoverflow.com/a/3246817/444977
			static MemoryCopier()
			{
				var dst_type = typeof(TDst);
				var src_type = typeof(TSrc);

				if (!dst_type.IsPrimitive)
					throw new ArgumentException("Destination type isn't a primitive type",
						dst_type.FullName);

				if (!src_type.IsPrimitive)
					throw new ArgumentException("Source type isn't a primitive type",
						src_type.FullName);
			}

			readonly int mDstTypeSize;
			readonly int mSrcTypeSize;

			public int DestinationTypeSize { get { return mDstTypeSize; } }
			public int SourceTypeSize { get { return mSrcTypeSize; } }

			public MemoryCopier(bool dummy)
			{
				mDstTypeSize = System.Runtime.InteropServices.Marshal.SizeOf<TDst>();
				mSrcTypeSize = System.Runtime.InteropServices.Marshal.SizeOf<TSrc>();
			}

			internal void CopyInternal(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				Contract.Assert(DestinationTypeSize != 0 && SourceTypeSize != 0,
					"somebody used MemoryCopier's default constructor!");

				if (srcCopyCount == 0)
					return;

				// Get the available size of the buffers
				int dst_buffer_local_size_in_bytes = (dst.Length - dstOffset) * mDstTypeSize;
				int src_buffer_local_size_in_bytes = (src.Length - srcOffset) * mSrcTypeSize;

				// Size, in bytes, of the src elements to copy. Could be smaller than src_buffer_size
				int src_copy_count_in_bytes = mSrcTypeSize * srcCopyCount;

				if (src_copy_count_in_bytes > dst_buffer_local_size_in_bytes)
					throw new ArgumentOutOfRangeException("srcCopyCount", srcCopyCount,
						"total source memory to copy exceeds the memory available in destination");

				Buffer.BlockCopy(src, srcOffset,
					dst, dstOffset,
					src_copy_count_in_bytes);
			}

			public void Copy(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				Contract.Requires<ArgumentNullException>(dst != null);
				Contract.Requires<ArgumentNullException>(src != null);
				Contract.Requires<ArgumentOutOfRangeException>(srcCopyCount >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(
					dstOffset >= 0 && dstOffset < dst.Length);
				Contract.Requires<ArgumentOutOfRangeException>(
					srcOffset >= 0 && srcOffset < src.Length);

				Contract.Requires<ArgumentOutOfRangeException>(
					(srcOffset+srcCopyCount) <= src.Length);

				CopyInternal(dst, dstOffset, src, srcOffset, srcCopyCount);
			}

			public void Copy(TDst[] dst, TSrc[] src,
				int srcCopyCount)
			{
				Contract.Requires<ArgumentNullException>(dst != null);
				Contract.Requires<ArgumentNullException>(src != null);
				Contract.Requires<ArgumentOutOfRangeException>(srcCopyCount >= 0);

				CopyInternal(dst, 0, src, 0, srcCopyCount);
			}
		};

		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		[Contracts.Pure]
		public static bool ArrayCopyFromBytesBoundsValidate(byte[] src, int srcOffset, Array dst, int dstOffset, int count, int elementSize)
		{
			int src_index_end = srcOffset + count;
			int dst_index_end = dstOffset + (count / elementSize);
			int copy_leftovers = count % elementSize;

			return src_index_end <= src.Length && dst_index_end <= dst.Length && copy_leftovers == 0;
		}
		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		[Contracts.Pure]
		public static bool ArrayCopyToBytesBoundsValidate(Array src, int srcOffset, byte[] dst, int dstOffset, int count, int elementSize)
		{
			int src_index_end = srcOffset + count;
			int dst_index_end = dstOffset + (count * elementSize);

			return src_index_end <= src.Length && dst_index_end <= dst.Length;
		}
#if false // TODO
		/// <summary>
		/// Copies a range of elements from a source array into the element memory of a destination array
		/// </summary>
		/// <typeparam name="TSrc">Source element type. Must be a primitive type</typeparam>
		/// <typeparam name="TDst">Destination element type. Must be a primitive type</typeparam>
		/// <param name="sourceArray">Memory to copy from</param>
		/// <param name="sourceIndex">Element index to start the copy from</param>
		/// <param name="length">Number of source elements to copy</param>
		/// <param name="destinationArray">Memory to copy to</param>
		/// <param name="destinationIndex">Element index to start the copy at</param>
		/// <returns>True if the memcpy operation was successful</returns>
		/// <remarks>Unlike <see cref="System.Buffer.BlockCopy"/> (which is more like memmove), this doesn't guard against overlap</remarks>
		public static bool MemoryCopy<TSrc, TDst>(TSrc[] sourceArray, int sourceIndex, int length, TDst[] destinationArray, int destinationIndex)
			where TSrc : struct
			where TDst : struct
		{
			const string k_type_not_primitive_msg_postfix = " must be a primitive type";

			Contract.Requires<ArgumentNullException>(sourceArray != null);
			Contract.Requires<ArgumentOutOfRangeException>(sourceIndex >= 0);
			Contract.Requires<ArgumentException>(typeof(TSrc).IsPrimitive, "TSrc" + k_type_not_primitive_msg_postfix);
			Contract.Requires<ArgumentNullException>(destinationArray != null);
			Contract.Requires<ArgumentOutOfRangeException>(destinationIndex >= 0);
			Contract.Requires<ArgumentException>(typeof(TDst).IsPrimitive, "TDst" + k_type_not_primitive_msg_postfix);

			// LowLevel's Memcpy takes destinationArray first, then sourceArray, like C's memcpy
			return LowLevel.Util.ValueTypeBitConverter.Memcpy(	destinationArray, destinationIndex, 
																sourceArray, sourceIndex, length, 
																true); // check that array types are primitives
		}
#endif
		#endregion

		#region BitReverse
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte BitReverse(byte x)
		{
			uint v = x;
			v = ((v & 0xAA) >> 1) | ((v & 0x55) << 1); // swap odd and even bits
			v = ((v & 0xCC) >> 2) | ((v & 0x33) << 2); // swap consecutive pairs
			v = ((v & 0xF0) >> 4) | ((v & 0x0F) << 4); // swap nibbles

			return (byte)v;
		}
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort BitReverse(ushort x)
		{
			uint v = x;
			v = ((v & 0xAAAA) >> 1) | ((v & 0x5555) << 1); // swap odd and even bits
			v = ((v & 0xCCCC) >> 2) | ((v & 0x3333) << 2); // swap consecutive pairs
			v = ((v & 0xF0F0) >> 4) | ((v & 0x0F0F) << 4); // swap nibbles
			v = ((v & 0xFF00) >> 8) | ((v & 0x00FF) << 8); // swap bytes

			return (ushort)v;
		}
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint BitReverse(uint x)
		{
			x = ((x & 0xAAAAAAAA) >>  1) | ((x & 0x55555555) << 1); // swap odd and even bits
			x = ((x & 0xCCCCCCCC) >>  2) | ((x & 0x33333333) << 2); // swap consecutive pairs
			x = ((x & 0xF0F0F0F0) >>  4) | ((x & 0x0F0F0F0F) << 4); // swap nibbles
			x = ((x & 0xFF00FF00) >>  8) | ((x & 0x00FF00FF) << 8); // swap bytes
			x = ((x             ) >> 16) | ((x             ) << 16);// swap halfs

			return x;
		}
		/// <summary>Get the bit-reversed equivalent of an unsigned integer</summary>
		/// <param name="x">Integer to bit-reverse</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong BitReverse(ulong x)
		{
			x = ((x & 0xAAAAAAAAAAAAAAAA) >>  1) | ((x & 0x5555555555555555) <<  1); // swap odd and even bits
			x = ((x & 0xCCCCCCCCCCCCCCCC) >>  2) | ((x & 0x3333333333333333) <<  2); // swap consecutive pairs
			x = ((x & 0xF0F0F0F0F0F0F0F0) >>  4) | ((x & 0x0F0F0F0F0F0F0F0F) <<  4); // swap nibbles
			x = ((x & 0xFF00FF00FF00FF00) >>  8) | ((x & 0x00FF00FF00FF00FF) <<  8); // swap bytes
			x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16); // swap halfs
			x = ((x                     ) >> 32) | ((x                     ) << 32); // swap words

			return x;
		}
		#endregion

		#region HighestBitSetIndex
		[Contracts.Pure]
		public static int IndexOfHighestBitSet(uint value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() < kInt32BitCount);

			value |= value >> 1; // first round down to one less than a power of 2 
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			uint index = (value * 0x07C4ACDDU) >> 27;
			return kMultiplyDeBruijnBitPositionHighestBitSet32[index];
		}
		[Contracts.Pure]
		public static int IndexOfHighestBitSet(ulong value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() < kInt64BitCount);

			int index = 0;
			uint high = IntegerMath.GetHighBits(value);
			if(high != 0)
				index = IndexOfHighestBitSet(high) + kInt32BitCount;
			else
				index = IndexOfHighestBitSet(IntegerMath.GetLowBits(value));

			return index;
		}
		#endregion

		#region LeadingZerosCount
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int LeadingZerosCount(byte value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kByteBitCount);
			return LeadingZerosCount((uint)value) - (kByteBitCount * 3);
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int LeadingZerosCount(ushort value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kInt16BitCount);
			return LeadingZerosCount((uint)value) - (kByteBitCount * 2);
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int LeadingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kInt32BitCount);
			if (value == 0)
				return kInt32BitCount;

			value |= value >> 1; // first round down to one less than a power of 2 
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			// subtract the log base 2 from the number of bits in the integer
			uint index = (value * 0x07C4ACDDU) >> 27;
			return kInt32BitCount - kMultiplyDeBruijnBitPositionLeadingZeros32[index];
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int LeadingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kInt64BitCount);

			int count = LeadingZerosCount(IntegerMath.GetHighBits(value));
			// The high bits were all zero, continue checking low bits
			if (count == kInt32BitCount)
				count +=LeadingZerosCount(IntegerMath.GetLowBits(value));

			return count;
		}
		#endregion

		#region TrailingZerosCount
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int TrailingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kInt32BitCount);
			if (value == 0)
				return kInt32BitCount;

			// instead of (value & -value), where the op result is a long, we do this to keep it all 32-bit
			uint ls1b = (~value) + 1; // two's complement
			ls1b = value & ls1b; // least significant 1 bit
			uint index = (ls1b * 0x077CB531U) >> 27;
			return kMultiplyDeBruijnBitPositionTrailingZeros32[index];
		}
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int TrailingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= kInt64BitCount);

			int count = TrailingZerosCount(IntegerMath.GetLowBits(value));
			// The low bits were all zero, continue checking high bits
			if (count == kInt32BitCount)
				count +=TrailingZerosCount(IntegerMath.GetHighBits(value));

			return count;
		}
		#endregion

		// look at this http://www.df.lth.se/~john_e/gems/gem002d.html
		#region BitCount
		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(byte bits)
		{
			uint x = bits;
			x = (x & 0x55) + ((x & 0xAA) >> 1);
			x = (x & 0x33) + ((x & 0xCC) >> 2);
			x = (x & 0x0F) + ((x & 0xF0) >> 4);

			return (int)x;
		}
		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(ushort bits)
		{
			uint x = bits;
			x = (x & 0x5555) + ((x & 0xAAAA) >>  1);
			x = (x & 0x3333) + ((x & 0xCCCC) >>  2);
			x = (x & 0x0F0F) + ((x & 0xF0F0) >>  4);
			x = (x & 0x00FF) + ((x & 0xFF00) >>  8);

			return (int)x;
		}
		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(uint bits)
		{
			uint x = bits;
			x = (x & 0x55555555) + ((x & 0xAAAAAAAA) >>  1);
			x = (x & 0x33333333) + ((x & 0xCCCCCCCC) >>  2);
			x = (x & 0x0F0F0F0F) + ((x & 0xF0F0F0F0) >>  4);
			x = (x & 0x00FF00FF) + ((x & 0xFF00FF00) >>  8);
			x = (x & 0x0000FFFF) + ((x & 0xFFFF0000) >> 16);

			return (int)x;
		}
		/// <summary>Count the number of 'on' bits in an unsigned integer</summary>
		/// <param name="bits">Integer whose bits to count</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int BitCount(ulong bits)
		{
			ulong x = bits;
			x = (x & 0x5555555555555555) + ((x & 0xAAAAAAAAAAAAAAAA) >>  1);
			x = (x & 0x3333333333333333) + ((x & 0xCCCCCCCCCCCCCCCC) >>  2);
			x = (x & 0x0F0F0F0F0F0F0F0F) + ((x & 0xF0F0F0F0F0F0F0F0) >>  4);
			x = (x & 0x00FF00FF00FF00FF) + ((x & 0xFF00FF00FF00FF00) >>  8);
			x = (x & 0x0000FFFF0000FFFF) + ((x & 0xFFFF0000FFFF0000) >> 16);
			x = (x & 0x00000000FFFFFFFF) + ((x & 0xFFFFFFFF00000000) >> 32);

			return (int)x;
		}

		/// <summary>Calculate the bit-mask needed for a number of bits</summary>
		/// <param name="bitCount">Number of bits needed for the mask</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint BitCountToMask32(int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount >= 0 && bitCount <= kInt32BitCount);

			return uint.MaxValue >> (kInt32BitCount-bitCount);
		}
		/// <summary>Calculate the bit-mask needed for a number of bits</summary>
		/// <param name="bitCount">Number of bits needed for the mask</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong BitCountToMask64(int bitCount)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(bitCount >= 0 && bitCount <= kInt64BitCount);

			return ulong.MaxValue >> (kInt64BitCount-bitCount);
		}
		#endregion
	};
}