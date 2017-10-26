using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	/// <summary>Represents the info needed to compose a specific bit-field</summary>
	public struct BitFieldTraits
	{
		public const int kMaxBitCount = Bits.kInt64BitCount;

		public static readonly BitFieldTraits Empty = new BitFieldTraits();

		#region Fields
		readonly byte mBitCount;
		readonly byte mBitIndex;
		readonly bool mIs32Bit; // false, Is64Bit
		#endregion

		/// <summary>The number of bits this field consumes</summary>
		public int BitCount { get {
			Contract.Ensures(Contract.Result<int>() > 0 && Contract.Result<int>() <= kMaxBitCount);

			return mBitCount;
		} }
		/// <summary>The bit offset where this field begins</summary>
		public int BitIndex { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() < kMaxBitCount);

			return mBitIndex;
		} }

		/// <summary>Does this bit-field require 32-bit words for operations?</summary>
		public bool Is32Bit { get { return mIs32Bit; } }
		/// <summary>Does this bit-field require 64-bit words for operations?</summary>
		public bool Is64Bit { get { return !Is32Bit; } }

		#region Bitmask
		/// <summary>The bitmask for this field, when the bits are shifted all the way right (offset=0)</summary>
		public IntegerUnion Bitmask { get {
			if (Is32Bit)
				return IntegerUnion.FromUInt32(Bits.BitCountToMask32(BitCount));
			else
				return IntegerUnion.FromUInt64(Bits.BitCountToMask64(BitCount));
		} }
		public ushort Bitmask16 { get {
			Contract.Assert(!Is64Bit, "Tried to access a 64-bit based BitField's bitmask as 16-bits");
			Contract.Assert(Bitmask.u32 == (ushort)Bitmask.u32, "Tried to access 32-bit based BitField bitmask as 16-bits");

			return (ushort)Bitmask.u32;
		} }
		public uint Bitmask32 { get {
			Contract.Assert(!Is64Bit, "Tried to access a 64-bit based BitField's bitmask as 32-bits");

			return Bitmask.u32;
		} }
		public ulong Bitmask64 { get {
			return Bitmask.u64;
		} }
		#endregion

		/// <summary>Are these traits invalid?</summary>
		/// <remarks>This would be the case if the default constructor was called (as this is a value type)</remarks>
		public bool IsEmpty { get { return BitCount == 0; } }

		public int NextFieldBitIndex { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() <= kMaxBitCount);

			return BitIndex + BitCount;
		} }

		/// <summary>Get the total number of bits consumed by this field and all the bits before <see cref="BitIndex"/></summary>
		/// <remarks>Mainly a utility for exposing a total "BitCount" for a handle composed of bit-fields</remarks>
		public int FieldsBitCount { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() <= kMaxBitCount);

			return BitIndex + BitCount;
		} }
		/// <summary>Get the bitmask associated with <see cref="FieldsBitCount"/></summary>
		/// <remarks>Mainly a utility for exposing a "Bitmask" for a handle composed of bit-fields</remarks>
		public IntegerUnion FieldsBitmask { get {
			var bitmask = new IntegerUnion();

			var fields_bit_count = FieldsBitCount;
			bool fields_are_32_bit = fields_bit_count <= Bits.kInt32BitCount;

			if (fields_are_32_bit)
				bitmask.u32 = Bits.BitCountToMask32(fields_bit_count);
			else
				bitmask.u64 = Bits.BitCountToMask64(fields_bit_count);

			return bitmask;
		} }

		#region Ctors
		BitFieldTraits(bool dummy, int bitCount, int bitIndex)
		{
			mBitCount = (byte)bitCount;
			mBitIndex = (byte)bitIndex;
			mIs32Bit = bitCount <= Bits.kInt32BitCount;

			Contract.Assert(Is32Bit || Is64Bit);
		}

		public BitFieldTraits(int bitCount)
			: this(false, bitCount, 0)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitCount > 0 && bitCount <= kMaxBitCount);
		}
		public BitFieldTraits(int bitCount, int bitIndex)
			: this(false, bitCount, bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitCount > 0 && bitCount <= kMaxBitCount);
			// less than: b/c bitIndex be one less than the highest bit (else there's no way the field can exist!)
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < kMaxBitCount);
			Contract.Requires<ArgumentException>((bitIndex+bitCount) <= kMaxBitCount);
		}
		public BitFieldTraits(int bitCount, BitFieldTraits prev)
			: this(false, bitCount, prev.NextFieldBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitCount > 0 && bitCount <= kMaxBitCount);
			Contract.Requires<ArgumentException>((prev.NextFieldBitIndex+bitCount) <= kMaxBitCount);
		}
		#endregion

		#region Util ctors
		public static BitFieldTraits For<TUInt>(IEnumBitEncoder<TUInt> enumEncoder)
		{
			Contract.Requires(enumEncoder != null);

			return new BitFieldTraits(enumEncoder.BitCountTrait);
		}
		public static BitFieldTraits For<TUInt>(IEnumBitEncoder<TUInt> enumEncoder, BitFieldTraits prev)
		{
			Contract.Requires(enumEncoder != null);

			return new BitFieldTraits(enumEncoder.BitCountTrait, prev);
		}
		#endregion
	};
}