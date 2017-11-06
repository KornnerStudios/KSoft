
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	partial class IntegerMath
	{
		#region Align
		/// <summary>Takes <paramref name="value"/> and returns what it would be if it were aligned to <paramref name="align_size"/> bytes</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns><paramref name="value"/> aligned to the next <paramref name="alignmentBit"/> boundary, if it isn't already</returns>
		[Contracts.Pure]
		public static uint Align(int alignmentBit, uint value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			uint align_size = 1U << alignmentBit;

			return (value + (align_size-1)) & ~(align_size-1);
		}

		/// <summary>Takes <paramref name="value"/> and returns what it would be if it were aligned to <paramref name="align_size"/> bytes</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns><paramref name="value"/> aligned to the next <paramref name="alignmentBit"/> boundary, if it isn't already</returns>
		[Contracts.Pure]
		public static int Align(int alignmentBit, int value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			Contract.Requires<System.ArgumentOutOfRangeException>(value >= 0);
			Contract.Ensures(Contract.Result<int>() >= 0);
			int align_size = 1 << alignmentBit;

			return (value + (align_size-1)) & ~(align_size-1);
		}

		/// <summary>Takes <paramref name="value"/> and returns what it would be if it were aligned to <paramref name="align_size"/> bytes</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns><paramref name="value"/> aligned to the next <paramref name="alignmentBit"/> boundary, if it isn't already</returns>
		[Contracts.Pure]
		public static ulong Align(int alignmentBit, ulong value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			ulong align_size = 1UL << alignmentBit;

			return (value + (align_size-1)) & ~(align_size-1);
		}

		/// <summary>Takes <paramref name="value"/> and returns what it would be if it were aligned to <paramref name="align_size"/> bytes</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns><paramref name="value"/> aligned to the next <paramref name="alignmentBit"/> boundary, if it isn't already</returns>
		[Contracts.Pure]
		public static long Align(int alignmentBit, long value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			Contract.Requires<System.ArgumentOutOfRangeException>(value >= 0);
			Contract.Ensures(Contract.Result<long>() >= 0);
			long align_size = 1L << alignmentBit;

			return (value + (align_size-1)) & ~(align_size-1);
		}

		#endregion

		#region PaddingRequired
		/// <summary>Calculate the number of padding bytes, if any, needed to align a value</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns>Bytes needed to align <paramref name="value"/> to the next <paramref name="alignmentBit"/> boundary, or zero if it is already aligned</returns>
		[Contracts.Pure]
		public static int PaddingRequired(int alignmentBit, uint value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);

			return (int)(Align(alignmentBit, value) - value);
		}

		/// <summary>Calculate the number of padding bytes, if any, needed to align a value</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns>Bytes needed to align <paramref name="value"/> to the next <paramref name="alignmentBit"/> boundary, or zero if it is already aligned</returns>
		[Contracts.Pure]
		public static int PaddingRequired(int alignmentBit, int value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			Contract.Requires<System.ArgumentOutOfRangeException>(value >= 0);
			Contract.Ensures(Contract.Result<int>() >= 0);

			return (int)(Align(alignmentBit, value) - value);
		}

		/// <summary>Calculate the number of padding bytes, if any, needed to align a value</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns>Bytes needed to align <paramref name="value"/> to the next <paramref name="alignmentBit"/> boundary, or zero if it is already aligned</returns>
		[Contracts.Pure]
		public static int PaddingRequired(int alignmentBit, ulong value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);

			return (int)(Align(alignmentBit, value) - value);
		}

		/// <summary>Calculate the number of padding bytes, if any, needed to align a value</summary>
		/// <param name="alignmentBit">Alignment size in log2 form</param>
		/// <param name="value">Value to align</param>
		/// <returns>Bytes needed to align <paramref name="value"/> to the next <paramref name="alignmentBit"/> boundary, or zero if it is already aligned</returns>
		[Contracts.Pure]
		public static int PaddingRequired(int alignmentBit, long value)
		{
			Contract.Requires<System.ArgumentOutOfRangeException>(alignmentBit <= kMaxAlignmentBit);
			Contract.Requires<System.ArgumentOutOfRangeException>(value >= 0);
			Contract.Ensures(Contract.Result<int>() >= 0);

			return (int)(Align(alignmentBit, value) - value);
		}

		#endregion

		#region FloorLog2 - Unsigned Integer
		/// <summary>Get the largest power of 2 that is less than or equal to the input (positive) number</summary>
		/// <param name="n">Positive number's log2 to deduce</param>
		/// <returns>
		/// The floor form of log2(<paramref name="n"/>).
		///
		/// Or -1 if <paramref name="n"/> is 0.
		/// </returns>
		[Contracts.Pure]
		public static int FloorLog2(byte n)
		{
			if (n == 0)			return -1;
			int pos = 0;
			if (n >= 1 << 4)	{ n >>= 4;	pos += 4; }
			if (n >= 1 << 2)	{ n >>= 2;	pos += 2; }
			if (n >= 1 << 1)	pos += 1;

			return pos;
		}

		/// <summary>Get the largest power of 2 that is less than or equal to the input (positive) number</summary>
		/// <param name="n">Positive number's log2 to deduce</param>
		/// <returns>
		/// The floor form of log2(<paramref name="n"/>).
		///
		/// Or -1 if <paramref name="n"/> is 0.
		/// </returns>
		[Contracts.Pure]
		public static int FloorLog2(ushort n)
		{
			if (n == 0)			return -1;
			int pos = 0;
			if (n >= 1 << 8)	{ n >>= 8;	pos += 8; }
			if (n >= 1 << 4)	{ n >>= 4;	pos += 4; }
			if (n >= 1 << 2)	{ n >>= 2;	pos += 2; }
			if (n >= 1 << 1)	pos += 1;

			return pos;
		}

		/// <summary>Get the largest power of 2 that is less than or equal to the input (positive) number</summary>
		/// <param name="n">Positive number's log2 to deduce</param>
		/// <returns>
		/// The floor form of log2(<paramref name="n"/>).
		///
		/// Or -1 if <paramref name="n"/> is 0.
		/// </returns>
		[Contracts.Pure]
		public static int FloorLog2(uint n)
		{
			if (n == 0)			return -1;
			int pos = 0;
			if (n >= 1 << 16)	{ n >>= 16;	pos += 16; }
			if (n >= 1 << 8)	{ n >>= 8;	pos += 8; }
			if (n >= 1 << 4)	{ n >>= 4;	pos += 4; }
			if (n >= 1 << 2)	{ n >>= 2;	pos += 2; }
			if (n >= 1 << 1)	pos += 1;

			return pos;
		}

		/// <summary>Get the largest power of 2 that is less than or equal to the input (positive) number</summary>
		/// <param name="n">Positive number's log2 to deduce</param>
		/// <returns>
		/// The floor form of log2(<paramref name="n"/>).
		///
		/// Or -1 if <paramref name="n"/> is 0.
		/// </returns>
		[Contracts.Pure]
		public static int FloorLog2(ulong n)
		{
			if (n == 0)			return -1;
			int pos = 0;
			if (n >= 1 << 32)	{ n >>= 32;	pos += 32; }
			if (n >= 1 << 16)	{ n >>= 16;	pos += 16; }
			if (n >= 1 << 8)	{ n >>= 8;	pos += 8; }
			if (n >= 1 << 4)	{ n >>= 4;	pos += 4; }
			if (n >= 1 << 2)	{ n >>= 2;	pos += 2; }
			if (n >= 1 << 1)	pos += 1;

			return pos;
		}

		#endregion

		#region IsSigned
		/// <summary>Tests to see if the given value's sign-bit is on</summary>
		/// <param name="value">Value to test</param>
		/// <returns>True if the sign-bit is set</returns>
		[Contracts.Pure]
		public static bool IsSigned(byte value)
		{
			return ((value >>  0) & 0x80) != 0;
		}

		/// <summary>Tests to see if the given value's sign-bit is on</summary>
		/// <param name="value">Value to test</param>
		/// <returns>True if the sign-bit is set</returns>
		[Contracts.Pure]
		public static bool IsSigned(ushort value)
		{
			return ((value >>  8) & 0x80) != 0;
		}

		/// <summary>Tests to see if the given value's sign-bit is on</summary>
		/// <param name="value">Value to test</param>
		/// <returns>True if the sign-bit is set</returns>
		[Contracts.Pure]
		public static bool IsSigned(uint value)
		{
			return ((value >> 24) & 0x80) != 0;
		}

		/// <summary>Tests to see if the given value's sign-bit is on</summary>
		/// <param name="value">Value to test</param>
		/// <returns>True if the sign-bit is set</returns>
		[Contracts.Pure]
		public static bool IsSigned(ulong value)
		{
			return ((value >> 56) & 0x80) != 0;
		}

		#endregion

		#region SetSignBit
		/// <summary>Set the sign-bit in the value given</summary>
		/// <param name="value">Value to return with its sign-bit set</param>
		/// <returns><paramref name="value"/> with its sign-bit set</returns>
		[Contracts.Pure]
		public static byte SetSignBit(byte value)
		{
			return (byte) (
				value | (0x80   <<  0)
			);
		}

		/// <summary>Set the sign-bit in the value given</summary>
		/// <param name="value">Value to return with its sign-bit set</param>
		/// <returns><paramref name="value"/> with its sign-bit set</returns>
		[Contracts.Pure]
		public static ushort SetSignBit(ushort value)
		{
			return (ushort) (
				value | (0x80   <<  8)
			);
		}

		/// <summary>Set the sign-bit in the value given</summary>
		/// <param name="value">Value to return with its sign-bit set</param>
		/// <returns><paramref name="value"/> with its sign-bit set</returns>
		[Contracts.Pure]
		public static uint SetSignBit(uint value)
		{
			return
				value | (0x80U  << 24)
			;
		}

		/// <summary>Set the sign-bit in the value given</summary>
		/// <param name="value">Value to return with its sign-bit set</param>
		/// <returns><paramref name="value"/> with its sign-bit set</returns>
		[Contracts.Pure]
		public static ulong SetSignBit(ulong value)
		{
			return
				value | (0x80UL << 56)
			;
		}

		#endregion
	};
}
