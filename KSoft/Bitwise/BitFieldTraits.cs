using System;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

#nullable enable

namespace KSoft.Bitwise
{
	/// <summary>Represents the info needed to compose a specific bit-field</summary>
	//[SuppressMessage("Microsoft.Design", "CA1815")]
	public struct BitFieldTraits
	{
		public const int kMaxBitCount = Bits.kInt64BitCount;

		public static readonly BitFieldTraits Empty = new();

		#region Fields
		readonly byte mBitCount;
		readonly byte mBitIndex;
		readonly bool mIs32Bit; // false, Is64Bit
		#endregion

		/// <summary>The number of bits this field consumes</summary>
		public readonly int BitCount { get {
			Contract.Ensures(Contract.Result<int>() > 0 && Contract.Result<int>() <= kMaxBitCount);

			return mBitCount;
		} }
		/// <summary>The bit offset where this field begins</summary>
		public readonly int BitIndex { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() < kMaxBitCount);

			return mBitIndex;
		} }

		/// <summary>Does this bit-field require 32-bit words for operations?</summary>
		public readonly bool Is32Bit => mIs32Bit;
		/// <summary>Does this bit-field require 64-bit words for operations?</summary>
		public readonly bool Is64Bit => !Is32Bit;

		#region Bitmask
		/// <summary>The bitmask for this field, when the bits are shifted all the way right (offset=0)</summary>
		public readonly IntegerUnion Bitmask { get {
			if (Is32Bit)
			{
				return IntegerUnion.FromUInt32(Bits.BitCountToMask32(BitCount));
			}
			else
			{
				return IntegerUnion.FromUInt64(Bits.BitCountToMask64(BitCount));
			}
		} }
		public readonly ushort Bitmask16 { get {
			Contract.Assert(!Is64Bit, "Tried to access a 64-bit based BitField's bitmask as 16-bits");
			Contract.Assert(Bitmask.u32 == (ushort)Bitmask.u32, "Tried to access 32-bit based BitField bitmask as 16-bits");

			return (ushort)Bitmask.u32;
		} }
		public readonly uint Bitmask32 { get {
			Contract.Assert(!Is64Bit, "Tried to access a 64-bit based BitField's bitmask as 32-bits");

			return Bitmask.u32;
		} }
		public readonly ulong Bitmask64 => Bitmask.u64;
		#endregion

		/// <summary>Are these traits invalid?</summary>
		/// <remarks>This would be the case if the default constructor was called (as this is a value type)</remarks>
		public readonly bool IsEmpty => BitCount == 0;

		public readonly int NextFieldBitIndex { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() <= kMaxBitCount);

			return BitIndex + BitCount;
		} }

		/// <summary>
		/// Get the total number of bits consumed by this field and all the bits before <see cref="BitIndex"/>.
		/// </summary>
		/// <remarks>Mainly a utility for exposing a total "BitCount" for a handle composed of bit-fields</remarks>
		public readonly int FieldsBitCount { get {
			Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() <= kMaxBitCount);

			return BitIndex + BitCount;
		} }
		/// <summary>Get the bitmask associated with <see cref="FieldsBitCount"/></summary>
		/// <remarks>Mainly a utility for exposing a "Bitmask" for a handle composed of bit-fields</remarks>
		public readonly IntegerUnion FieldsBitmask { get {
			var bitmask = new IntegerUnion();

			var fields_bit_count = FieldsBitCount;
			bool fields_are_32_bit = fields_bit_count <= Bits.kInt32BitCount;

			if (fields_are_32_bit)
			{
				bitmask.u32 = Bits.BitCountToMask32(fields_bit_count);
			}
			else
			{
				bitmask.u64 = Bits.BitCountToMask64(fields_bit_count);
			}

			return bitmask;
		} }

		#region Ctors
		BitFieldTraits(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
			bool dummy, int bitCount, int bitIndex)
		{
			mBitCount = (byte)bitCount;
			mBitIndex = (byte)bitIndex;
			mIs32Bit = bitCount <= Bits.kInt32BitCount;

			Contract.Assert(Is32Bit || Is64Bit);
		}

		static int ValidateBitCount(int bitCount)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitCount);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, kMaxBitCount);

			return bitCount;
		}
		static int ValidateBitIndexAndRange(int bitCount, int bitIndex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, kMaxBitCount);

			if (bitCount > kMaxBitCount - bitIndex)
			{
				throw new ArgumentException("Bit field extends past the maximum bit count.", nameof(bitCount));
			}

			return bitIndex;
		}
		static int ValidateNextFieldBitIndex(BitFieldTraits prev, int bitCount)
		{
			var bitIndex = prev.NextFieldBitIndex;
			if (bitCount > kMaxBitCount - bitIndex)
			{
				throw new ArgumentException("Bit field extends past the maximum bit count.", nameof(bitCount));
			}

			return bitIndex;
		}

		public BitFieldTraits(int bitCount)
			: this(false, ValidateBitCount(bitCount), 0)
		{
		}
		public BitFieldTraits(int bitCount, int bitIndex)
			: this(false, ValidateBitCount(bitCount), ValidateBitIndexAndRange(bitCount, bitIndex))
		{
		}
		public BitFieldTraits(int bitCount, BitFieldTraits prev)
			: this(false, ValidateBitCount(bitCount), ValidateNextFieldBitIndex(prev, bitCount))
		{
		}
		#endregion

		#region Util ctors
		public static BitFieldTraits For<TUInt>(IEnumBitEncoder<TUInt> enumEncoder)
		{
			ArgumentNullException.ThrowIfNull(enumEncoder);

			return new BitFieldTraits(enumEncoder.BitCountTrait);
		}
		public static BitFieldTraits For<TUInt>(IEnumBitEncoder<TUInt> enumEncoder, BitFieldTraits prev)
		{
			ArgumentNullException.ThrowIfNull(enumEncoder);

			return new BitFieldTraits(enumEncoder.BitCountTrait, prev);
		}
		#endregion
	};
}
