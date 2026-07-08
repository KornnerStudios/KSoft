using System;
using System.Numerics;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		[Contracts.Pure]
		public static byte RotateLeft(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kByteBitCount);

			// #VITA_KEEP: byte/ushort rotates are width-specific; BitOperations exposes 32/64-bit rotates.
			return (byte)( (x << shift) | (x >> (kByteBitCount - shift)) );
		}
		[Contracts.Pure]
		public static byte RotateRight(byte x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kByteBitCount);

			// #VITA_KEEP: byte/ushort rotates are width-specific; BitOperations exposes 32/64-bit rotates.
			return (byte)( (x >> shift) | (x << (kByteBitCount - shift)) );
		}

		[Contracts.Pure]
		public static ushort RotateLeft(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt16BitCount);

			// #VITA_KEEP: byte/ushort rotates are width-specific; BitOperations exposes 32/64-bit rotates.
			return (ushort)( (x << shift) | (x >> (kInt16BitCount - shift)) );
		}
		[Contracts.Pure]
		public static ushort RotateRight(ushort x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt16BitCount);

			// #VITA_KEEP: byte/ushort rotates are width-specific; BitOperations exposes 32/64-bit rotates.
			return (ushort)( (x >> shift) | (x << (kInt16BitCount - shift)) );
		}

		[Contracts.Pure]
		public static uint RotateLeft(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt32BitCount);

			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.RotateLeft.
			return BitOperations.RotateLeft(x, shift);
		}
		[Contracts.Pure]
		public static uint RotateRight(uint x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt32BitCount);

			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.RotateRight.
			return BitOperations.RotateRight(x, shift);
		}

		[Contracts.Pure]
		public static ulong RotateLeft(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt64BitCount);

			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.RotateLeft.
			return BitOperations.RotateLeft(x, shift);
		}
		[Contracts.Pure]
		public static ulong RotateRight(ulong x, int shift)
		{
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(shift < kInt64BitCount);

			// #VITA_SHIM: Keep KSoft API while callers migrate to BitOperations.RotateRight.
			return BitOperations.RotateRight(x, shift);
		}

	};
}