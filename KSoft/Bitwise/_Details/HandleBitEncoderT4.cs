using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	partial struct HandleBitEncoder
	{
		public HandleBitEncoder(uint initialBits)
		{
			mBits.u64 = 0;
			mBits.u32 = 0;
			mBitIndex = 0;

			mBits.u32 = initialBits;
		}
		public HandleBitEncoder(ulong initialBits)
		{
			mBits.u64 = 0;
			mBits.u32 = 0;
			mBitIndex = 0;

			mBits.u64 = initialBits;
		}

		/// <summary>Get the 32-bit handle value</summary>
		/// <returns></returns>
		public uint GetHandle32()
		{
			return mBits.u32;
		}
		/// <summary>Get the 64-bit handle value</summary>
		/// <returns></returns>
		public ulong GetHandle64()
		{
			return mBits.u64;
		}

		#region Encode
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Encode32<TEnum>(TEnum value, EnumBitEncoder32<TEnum> encoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<System.ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref mBits.u64, ref mBitIndex);
		}
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Encode64<TEnum>(TEnum value, EnumBitEncoder64<TEnum> encoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<System.ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref mBits.u64, ref mBitIndex);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Encode32(uint value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable32(int value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void Encode32(uint value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, traits.Bitmask32);
		}
		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void EncodeNoneable32(int value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, traits.Bitmask32);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Encode64(ulong value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable64(long value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void Encode64(ulong value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			Bits.BitEncodeEnum(value, ref mBits.u64, ref mBitIndex, traits.Bitmask64);
		}
		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="traits"></param>
		public void EncodeNoneable64(long value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref mBits.u64, ref mBitIndex, traits.Bitmask64);
		}

		#endregion

		#region Decode
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode32<TEnum>(out TEnum value, EnumBitEncoder32<TEnum> decoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<System.ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(mBits.u64, ref mBitIndex);
		}
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode64<TEnum>(out TEnum value, EnumBitEncoder64<TEnum> decoder)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires<System.ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(mBits.u64, ref mBitIndex);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode32(out uint value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (uint)Bits.BitDecode(mBits.u64, ref mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable32(out int value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (int)Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void Decode32(out uint value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			value = (uint)Bits.BitDecode(mBits.u64, ref mBitIndex, traits.Bitmask32);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void DecodeNoneable32(out int value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			value = (int)Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, traits.Bitmask32);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode64(out ulong value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (ulong)Bits.BitDecode(mBits.u64, ref mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable64(out long value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (long)Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void Decode64(out ulong value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			value = (ulong)Bits.BitDecode(mBits.u64, ref mBitIndex, traits.Bitmask64);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="traits"></param>
		public void DecodeNoneable64(out long value, Bitwise.BitFieldTraits traits)
		{
			Contract.Requires<System.ArgumentException>(!traits.IsEmpty);

			value = (long)Bits.BitDecodeNoneable(mBits.u64, ref mBitIndex, traits.Bitmask64);
		}

		#endregion
	};
}