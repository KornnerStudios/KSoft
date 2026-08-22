using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft
{
	partial class EnumBitEncoder32<TEnum>
	{
		static void ValidateBitIndex(int bitIndex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);
			if (bitIndex >= Bits.kInt64BitCount || (bitIndex+kBitCount) >= Bits.kInt64BitCount)
			{
				throw new ArgumentOutOfRangeException(nameof(bitIndex));
			}
		}

		// Only added this really to ease the coding of HandleBitEncoder
		/// <summary>Bit encode an enumeration value into an unsigned integer</summary>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="bits">Bit data as an unsigned integer</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start encoding at</param>
		/// <remarks>
		/// On return <paramref name="bits"/> has <paramref name="value"/> encoded into it and
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		public void BitEncode(TEnum value, ref ulong bits, ref int bitIndex)
		{
			ValidateBitIndex(bitIndex);

			ulong v = Reflection.EnumValue<TEnum>.ToUInt32(value);
			if (kHasNone)
			{
				v++;
			}

			if (v > kMaxValue)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Encoded enum value must be <= {0}; actual value is {1}.",
					kMaxValue, v));
			}
			bits = Reflection.EnumUtil<TEnum>.IsFlags
				? Bits.BitEncodeFlags(v, bits, bitIndex, kBitmask)
				: Bits.BitEncodeEnum (v, bits, bitIndex, kBitmask);

			bitIndex += kBitCount;
		}

		// Only added this really to ease the coding of HandleBitEncoder
		/// <summary>Bit decode an enumeration value from an unsigned integer</summary>
		/// <param name="bits">Unsigned integer to decode from</param>
		/// <param name="bitIndex">Index in <paramref name="bits"/> to start decoding at</param>
		/// <returns>The enumeration value as it stood before it was ever encoded into <paramref name="bits"/></returns>
		/// <remarks>
		/// <paramref name="bitIndex"/> is incremented by the bit count of the underlying enumeration
		/// </remarks>
		public TEnum BitDecode(ulong bits, ref int bitIndex)
		{
			ValidateBitIndex(bitIndex);

			ulong v = Bits.BitDecode(bits, bitIndex, kBitmask);
			if (kHasNone)
			{
				v--;
			}

			bitIndex += kBitCount;

			if (v > kMaxValue && (!kHasNone || v != ulong.MaxValue))
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Decoded enum value must be <= {0}; actual value is {1}.",
					kMaxValue, v));
			}
			return Reflection.EnumValue<TEnum>.FromUInt64(v);
		}

		public ushort BitEncode(TEnum value, ushort bits, Bitwise.BitFieldTraits traits)
		{
			return (ushort)BitEncode(value, bits, traits.BitIndex);
		}
		public TEnum BitEncode(uint bits, Bitwise.BitFieldTraits traits)
		{
			return BitDecode(bits, traits.BitIndex);
		}
	};

	public sealed class EnumBitEncoder<TEnum>
		where TEnum : struct, Enum
	{
		//[SuppressMessage("Microsoft.Design", "CA1823:AvoidUnusedPrivateFields",
		//	Justification = "x32 could probably just be wrapped in #if DEBUG...but what if you don't ever run debug?")]
		static readonly EnumBitEncoder32<TEnum> x32 = new();
		static readonly EnumBitEncoder64<TEnum> x64 = new();

		public bool IsFlags { get => x64.IsFlags; }
		public bool HasNone { get => x64.HasNone; }
		/// <see cref="kBitmask"/>
		public ulong MaxValueTrait { get => x64.MaxValueTrait; }
		/// <see cref="kBitmask"/>
		public ulong BitmaskTrait { get => x64.BitmaskTrait; }
		/// <see cref="kBitCount"/>
		public int BitCountTrait { get => x64.BitCountTrait; }
	};
}
