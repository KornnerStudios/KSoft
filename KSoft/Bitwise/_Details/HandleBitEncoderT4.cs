using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	partial struct HandleBitEncoder
	{
		public HandleBitEncoder(uint initialBits)
		{
			m64 = 0;
			m32 = 0;
			mBitIndex = 0;

			m32 = initialBits;
		}
		public HandleBitEncoder(ulong initialBits)
		{
			m64 = 0;
			m32 = 0;
			mBitIndex = 0;

			m64 = initialBits;
		}

		/// <summary>Get the 32-bit handle value</summary>
		/// <returns></returns>
		public uint GetHandle32()
		{
			return m32;
		}
		/// <summary>Get the 64-bit handle value</summary>
		/// <returns></returns>
		public ulong GetHandle64()
		{
			return m64;
		}

		#region Encode
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Encode32<TEnum>(TEnum value, EnumBitEncoder32<TEnum> encoder)
			where TEnum : struct
		{
			Contract.Requires<System.ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref m64, ref mBitIndex);
		}
		/// <summary>Encode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to encode</typeparam>
		/// <param name="value">Enumeration value to encode</param>
		/// <param name="encoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Encode64<TEnum>(TEnum value, EnumBitEncoder64<TEnum> encoder)
			where TEnum : struct
		{
			Contract.Requires<System.ArgumentNullException>(encoder != null);

			encoder.BitEncode(value, ref m64, ref mBitIndex);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Encode32(uint value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref m64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable32(int value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref m64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Encode64(ulong value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			Bits.BitEncodeEnum(value, ref m64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit encode a none-able value into this handle</summary>
		/// <param name="value">Value to encode</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void EncodeNoneable64(long value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);
			Contract.Requires<System.ArgumentOutOfRangeException>(value.IsNoneOrPositive());

			Bits.BitEncodeEnum((ulong)(value+1), ref m64, ref mBitIndex, bitMask);
		}

		#endregion

		#region Decode
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode32<TEnum>(out TEnum value, EnumBitEncoder32<TEnum> decoder)
			where TEnum : struct
		{
			Contract.Requires<System.ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(m64, ref mBitIndex);
		}
		/// <summary>Decode an enumeration value using an enumeration encoder object</summary>
		/// <typeparam name="TEnum">Enumeration type to decode</typeparam>
		/// <param name="value">Enumeration value decoded from this handle</param>
		/// <param name="decoder">Encoder for <typeparamref name="TEnum"/> objects</param>
		public void Decode64<TEnum>(out TEnum value, EnumBitEncoder64<TEnum> decoder)
			where TEnum : struct
		{
			Contract.Requires<System.ArgumentNullException>(decoder != null);

			value = decoder.BitDecode(m64, ref mBitIndex);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode32(out uint value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (uint)Bits.BitDecode(m64, ref mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable32(out int value, uint bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (int)Bits.BitDecodeNoneable(m64, ref mBitIndex, bitMask);
		}

		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void Decode64(out ulong value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (ulong)Bits.BitDecode(m64, ref mBitIndex, bitMask);
		}
		/// <summary>Bit decode a value from this handle</summary>
		/// <param name="value">Value decoded from this handle</param>
		/// <param name="bitMask">Masking value for <paramref name="value"/></param>
		public void DecodeNoneable64(out long value, ulong bitMask)
		{
			Contract.Requires<System.ArgumentException>(bitMask != 0);

			value = (long)Bits.BitDecodeNoneable(m64, ref mBitIndex, bitMask);
		}

		#endregion
	};
}