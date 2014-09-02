﻿using System;
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
		/// <summary>Number of logical bits in a <see cref="System.Boolean"/></summary>
		public const int kBooleanBitCount = 1;

		[Contracts.Pure]
		static int BitmaskLookUpTableGetLength(int wordBitSize)
		{
			// first element in the LUT is zero, followed by a mask for each range of bits up until wordBitSize
			return 1 + wordBitSize;
		}

		#region MultiplyDeBruijnBitPosition
		static readonly byte[]	kMultiplyDeBruijnBitPositionHighestBitSet32,
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
			kMultiplyDeBruijnBitPositionHighestBitSet32 = new byte[kInt32BitCount]
			{
				0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
				8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
			};

			kMultiplyDeBruijnBitPositionLeadingZeros32 = new byte[kInt32BitCount];
			for (int x = 0; x < kMultiplyDeBruijnBitPositionLeadingZeros32.Length; x++)
				kMultiplyDeBruijnBitPositionLeadingZeros32[x] = (byte)(kMultiplyDeBruijnBitPositionHighestBitSet32[x]+1);

			kMultiplyDeBruijnBitPositionTrailingZeros32 = new byte[kInt32BitCount]
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
				mDstTypeSize = LowLevel.Util.Unmanaged.SizeOf<TDst>();
				mSrcTypeSize = LowLevel.Util.Unmanaged.SizeOf<TSrc>();
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

		#region Get high/low bits
		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the high-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetHighBitsSigned(uint value)	{ return (int)((value >> 16) & 0xFFFFFFFF); }
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the low-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static int GetLowBitsSigned(uint value)	{ return (int)(value & 0xFFFFFFFF); }

		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the high-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static uint GetHighBits(ulong value)	{ return (uint)((value >> 32) & 0xFFFFFFFF); }
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the low-bits in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static uint GetLowBits(ulong value)	{ return (uint)(value & 0xFFFFFFFF); }
		#endregion

		#region HighestBitSetIndex
		[Contracts.Pure]
		public static byte IndexOfHighestBitSet(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() < kInt32BitCount);

			value |= value >> 1; // first round down to one less than a power of 2 
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			uint index = (value * 0x07C4ACDDU) >> 27;
			return kMultiplyDeBruijnBitPositionHighestBitSet32[index];
		}
		[Contracts.Pure]
		public static byte IndexOfHighestBitSet(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() < kInt64BitCount);

			int index = 0;
			uint high = GetHighBits(value);
			if(high != 0)
				index = IndexOfHighestBitSet(high) + kInt32BitCount;
			else
				index = IndexOfHighestBitSet(GetLowBits(value));

			Contract.Assume(index >= 0);
			return (byte)index;
		}
		#endregion

		#region LeadingZerosCount
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(byte value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kByteBitCount);
			return (byte)( LeadingZerosCount((uint)value) - (kByteBitCount * 3) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(ushort value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kInt16BitCount);
			return (byte)( LeadingZerosCount((uint)value) - (kByteBitCount * 2) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kInt32BitCount);
			if (value == 0)
				return kInt32BitCount;

			value |= value >> 1; // first round down to one less than a power of 2 
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			// subtract the log base 2 from the number of bits in the integer
			uint index = (value * 0x07C4ACDDU) >> 27;
			return (byte)(kInt32BitCount - kMultiplyDeBruijnBitPositionLeadingZeros32[index]);
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte LeadingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kInt64BitCount);

			byte count = LeadingZerosCount(GetHighBits(value));
			// The high bits were all zero, continue checking low bits
			if (count == kInt32BitCount)
				count += LeadingZerosCount(GetLowBits(value));

			return count;
		}
		#endregion

		#region TrailingZerosCount
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte TrailingZerosCount(uint value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kInt32BitCount);
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
		public static byte TrailingZerosCount(ulong value)
		{
			Contract.Ensures(Contract.Result<byte>() <= kInt64BitCount);

			byte count = TrailingZerosCount(GetLowBits(value));
			// The low bits were all zero, continue checking high bits
			if (count == kInt32BitCount)
				count += TrailingZerosCount(GetHighBits(value));

			return count;
		}
		#endregion
	};
}