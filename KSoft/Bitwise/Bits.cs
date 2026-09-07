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
			// #VITA_KEEP: BitOperations exposes 32/64-bit counts; adjust the result to byte width.
			return (byte)( BitOperations.LeadingZeroCount((uint)value) - (kByteBitCount * 3) );
		}
		/// <summary>Count the "leftmost" consecutive zero bits (leading) in an unsigned integer</summary>
		/// <param name="value"></param>
		/// <returns>Number of leading zeros, from 0 through <see cref="kInt16BitCount"/>.</returns>
		public static byte LeadingZerosCount(ushort value)
		{
			// #VITA_KEEP: BitOperations exposes 32/64-bit counts; adjust the result to ushort width.
			return (byte)( BitOperations.LeadingZeroCount((uint)value) - (kByteBitCount * 2) );
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
