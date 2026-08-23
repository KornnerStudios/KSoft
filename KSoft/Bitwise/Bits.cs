using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;


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

		static int BitmaskLookUpTableGetLength(int wordBitSize)
		{
			// first element in the LUT is zero, followed by a mask for each range of bits up until wordBitSize
			return 1 + wordBitSize;
		}

		// https://en.wikipedia.org/wiki/De_Bruijn_sequence

		#region MultiplyDeBruijnBitPosition
		static readonly byte[] kMultiplyDeBruijnBitPositionHighestBitSet32 = GenerateMultiplyDeBruijnBitPositionHighestBitSet32();

		static byte[] GenerateMultiplyDeBruijnBitPositionHighestBitSet32()
		{
			return /*new byte[kInt32BitCount]*/
			[
				0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
				8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
			];
		}
		#endregion

		#region Guard messages
		const string kBitSwap_StartBitIndexNotGreaterThanZero =
			"Doesn't make sense to bit swap 1 bit. Or to start at a negative index";

		const string kGetMaxEnumBits_MaxValueOutOfRangeMessage = "There is no point in this if '0' is the only option";

		const string kGetBitmaskEnum_MaxValueOutOfRangeMessage = kGetMaxEnumBits_MaxValueOutOfRangeMessage;
		const string kGetBitmaskFlag_MaxValueOutOfRangeMessage = kGetMaxEnumBits_MaxValueOutOfRangeMessage;
		#endregion

		#region Memory/ArrayCopy
		// #REVIEW: Does #DOTNET5 enable us to change this to a class and use stackalloc?
		//[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Provides copy operations over unmanaged values.")]
		public readonly struct MemoryCopier<TDst, TSrc>
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
				{
					Debug.Trace.LowLevel.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
						nameof(MemoryCopier<TDst, TSrc>) + ": Destination type is not a primitive type",
						dst_type.FullName);
				}

				if (!src_type.IsPrimitive)
				{
					Debug.Trace.LowLevel.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
						nameof(MemoryCopier<TDst, TSrc>) + ": Source type is not a primitive type",
						src_type.FullName);
				}
			}

			readonly int mDstTypeSize;
			readonly int mSrcTypeSize;

			public readonly int DestinationTypeSize => mDstTypeSize;
			public readonly int SourceTypeSize => mSrcTypeSize;

			public MemoryCopier(
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
				bool dummy)
			{
				mDstTypeSize = LowLevel.Util.Unmanaged.SizeOf<TDst>();
				mSrcTypeSize = LowLevel.Util.Unmanaged.SizeOf<TSrc>();
			}

			internal void CopyInternal(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				if (DestinationTypeSize == 0 || SourceTypeSize == 0)
				{
					throw new InvalidOperationException("Somebody used MemoryCopier's default constructor.");
				}

				if (srcCopyCount == 0)
				{
					return;
				}

				// Get the available size of the buffers
				int dst_buffer_local_size_in_bytes = (dst.Length - dstOffset) * mDstTypeSize;
#if DEBUG
				int src_buffer_local_size_in_bytes = (src.Length - srcOffset) * mSrcTypeSize;
#endif

				// Size, in bytes, of the src elements to copy. Could be smaller than src_buffer_size
				int src_copy_count_in_bytes = mSrcTypeSize * srcCopyCount;

				if (src_copy_count_in_bytes > dst_buffer_local_size_in_bytes)
				{
					throw new ArgumentOutOfRangeException(nameof(srcCopyCount), srcCopyCount,
						"total source memory to copy exceeds the memory available in destination");
				}

				Buffer.BlockCopy(src, srcOffset,
					dst, dstOffset,
					src_copy_count_in_bytes);
			}

			public void Copy(TDst[] dst, int dstOffset,
				TSrc[] src, int srcOffset,
				int srcCopyCount)
			{
				ArgumentNullException.ThrowIfNull(dst);
				ArgumentNullException.ThrowIfNull(src);
				ArgumentOutOfRangeException.ThrowIfNegative(srcCopyCount);
				ArgumentOutOfRangeException.ThrowIfNegative(dstOffset);
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(dstOffset, dst.Length);
				ArgumentOutOfRangeException.ThrowIfNegative(srcOffset);
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(srcOffset, src.Length);
				ArgumentOutOfRangeException.ThrowIfGreaterThan(srcCopyCount, src.Length - srcOffset);

				CopyInternal(dst, dstOffset, src, srcOffset, srcCopyCount);
			}

			public void Copy(TDst[] dst, TSrc[] src,
				int srcCopyCount)
			{
				ArgumentNullException.ThrowIfNull(dst);
				ArgumentNullException.ThrowIfNull(src);
				ArgumentOutOfRangeException.ThrowIfNegative(srcCopyCount);

				CopyInternal(dst, 0, src, 0, srcCopyCount);
			}
		};

		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		public static bool ArrayCopyFromBytesBoundsValidate(byte[] src, int srcOffset, Array dst, int dstOffset, int count, int elementSize)
		{
			if (count < 0)
			{
				return false;
			}

			int src_index_end = srcOffset + count;
			int dst_index_end = dstOffset + (count / elementSize);
			//int copy_leftovers = count % elementSize;

			if (src_index_end > src.Length ||
				dst_index_end > dst.Length)
			{
				return false;
			}

			//if (copy_leftovers != 0)
			//	return false;

			return true;
		}
		/// <remarks>Declared as public as it's used in code contracts. Caller responsible for null and index-positive checks</remarks>
		public static bool ArrayCopyToBytesBoundsValidate(Array src, int srcOffset, byte[] dst, int dstOffset, int count, int elementSize)
		{
			if (count < 0)
			{
				return false;
			}

			int src_index_end = srcOffset + count;
			int dst_index_end = dstOffset + (count * elementSize);

			if (src_index_end > src.Length ||
				dst_index_end > dst.Length)
			{
				return false;
			}

			return true;
		}
		#endregion

		#region Get high/low bits
		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the high-bits in <paramref name="value"/></returns>
		public static int GetHighBitsSigned(uint value) => (int)((value >> 16) & 0xFFFFFFFF);
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Signed representation of the low-bits in <paramref name="value"/></returns>
		public static int GetLowBitsSigned(uint value) => (int)(value & 0xFFFFFFFF);

		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the high-bits in <paramref name="value"/></returns>
		public static uint GetHighBits(ulong value) => (uint)((value >> 32) & 0xFFFFFFFF);
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the low-bits in <paramref name="value"/></returns>
		public static uint GetLowBits(ulong value) => (uint)(value & 0xFFFFFFFF);

		/// <summary>Convenience function for getting the high order bits (LSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the high-bits in <paramref name="value"/></returns>
		public static int GetHighBitsSigned(ulong value) => (int)((value >> 32) & 0xFFFFFFFF);
		/// <summary>Convenience function for getting the low order bits (MSB) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Unsigned representation of the low-bits in <paramref name="value"/></returns>
		public static int GetLowBitsSigned(ulong value) => (int)(value & 0xFFFFFFFF);
		#endregion

		#region HighestBitSetIndex
		/// <summary>Find the zero-based index of the highest set bit in a 32-bit unsigned integer.</summary>
		/// <param name="value">Value to inspect.</param>
		/// <returns>A value from 0 through <see cref="kInt32BitCount"/> - 1. For zero, returns 0.</returns>
		public static byte IndexOfHighestBitSet(uint value)
		{

			value |= value >> 1; // first round down to one less than a power of 2
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;

			uint index = (value * 0x07C4ACDDU) >> 27;
			return kMultiplyDeBruijnBitPositionHighestBitSet32[index];
		}
		/// <summary>Find the zero-based index of the highest set bit in a 64-bit unsigned integer.</summary>
		/// <param name="value">Value to inspect.</param>
		/// <returns>A value from 0 through <see cref="kInt64BitCount"/> - 1. For zero, returns 0.</returns>
		public static byte IndexOfHighestBitSet(ulong value)
		{

			int index;
			uint high = GetHighBits(value);
			if (high != 0)
			{
				index = IndexOfHighestBitSet(high) + kInt32BitCount;
			}
			else
			{
				index = IndexOfHighestBitSet(GetLowBits(value));
			}

			System.Diagnostics.Debug.Assert(index >= 0);
			return (byte)index;
		}
		#endregion

		#region LeadingZerosCount
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of leading zeros, from 0 through <see cref="kByteBitCount"/>.</returns>
		public static byte LeadingZerosCount(byte value)
		{
			// #VITA_SHIM: Keep KSoft's byte-width result while using the BCL 32-bit primitive.
			return (byte)( BitOperations.LeadingZeroCount((uint)value) - (kByteBitCount * 3) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of leading zeros, from 0 through <see cref="kInt16BitCount"/>.</returns>
		public static byte LeadingZerosCount(ushort value)
		{
			// #VITA_SHIM: Keep KSoft's ushort-width result while using the BCL 32-bit primitive.
			return (byte)( BitOperations.LeadingZeroCount((uint)value) - (kByteBitCount * 2) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of leading zeros, from 0 through <see cref="kInt32BitCount"/>.</returns>
		public static byte LeadingZerosCount(uint value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.LeadingZeroCount.
			return (byte)BitOperations.LeadingZeroCount(value);
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of leading zeros, from 0 through <see cref="kInt64BitCount"/>.</returns>
		public static byte LeadingZerosCount(ulong value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.LeadingZeroCount.
			return (byte)BitOperations.LeadingZeroCount(value);
		}
		#endregion

		#region TrailingZerosCount
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of trailing zeros, from 0 through <see cref="kInt32BitCount"/>.</returns>
		public static byte TrailingZerosCount(uint value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.TrailingZeroCount.
			return (byte)BitOperations.TrailingZeroCount(value);
		}
		/// <summary>Count the "rightmost" consecutive zero bits (trailing) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of trailing zeros, from 0 through <see cref="kInt64BitCount"/>.</returns>
		public static byte TrailingZerosCount(ulong value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.TrailingZeroCount.
			return (byte)BitOperations.TrailingZeroCount(value);
		}
		#endregion

		#region BitDecode 16
		static void ValidateUInt16BitFieldTraits(Bitwise.BitFieldTraits traits)
		{
			if (traits.IsEmpty)
			{
				throw new ArgumentException("Traits must not be empty.", nameof(traits));
			}
			if (traits.BitIndex >= kInt16BitCount || traits.BitIndex+traits.BitCount > kInt16BitCount)
			{
				throw new ArgumentOutOfRangeException(nameof(traits));
			}
		}

		/// <summary>Bit decode an enumeration or flags from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="traits"></param>
		/// <returns>The enumeration\flags value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		public static ushort BitDecode(ushort bits, Bitwise.BitFieldTraits traits)
		{
			ValidateUInt16BitFieldTraits(traits);

			return (ushort)((bits >> traits.BitIndex) & traits.Bitmask16);
		}
		#endregion
		#region BitEncode 16
		/// <summary>Bit encode a value into an unsigned integer, removing the original data in the value's range</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="traits"></param>
		/// <returns><paramref name="bits"/> with <paramref name="value"/> encoded into it</returns>
		/// <remarks>
		/// Clears the bit-space between <paramref name="bitIndex"/> + <paramref name="bitMask"/>
		/// so any existing values will be lost after <paramref name="value"/> is added
		/// </remarks>
		public static ushort BitEncode(ushort value, ushort bits, Bitwise.BitFieldTraits traits)
		{
			ValidateUInt16BitFieldTraits(traits);

			var bitmask = (uint)traits.Bitmask16;
			// Use the bit mask's invert so we can get all of the non-value bits
			return (ushort)BitEncodeFlags(value, bits & (~bitmask), traits.BitIndex, bitmask);
		}
		#endregion
	};
}
