using Contracts = System.Diagnostics.Contracts;

namespace KSoft
{
	public static partial class IntegerMath
	{
		/// <summary>Represents 1024 bytes, or 1KB</summary>
		public const int kKilo = 1024;
		/// <summary>Represents 1024KB, or 1MB</summary>
		public const int kMega = 1024 * kKilo;
		/// <summary>Maximum accepted alignment bit value in integer math functions</summary>
		public const int kMaxAlignmentBit = 24; // 16MB

		/// <summary>16-bit alignment in log2</summary>
		public const int kInt16AlignmentBit = 1;
		/// <summary>32-bit alignment in log2</summary>
		public const int kInt32AlignmentBit = 2;
		/// <summary>64-bit alignment in log2</summary>
		public const int kInt64AlignmentBit = 3;
		/// <summary>128-bit alignment in log2</summary>
		public const int k16ByteAlignmentBit = 4;
		/// <summary>256-bit alignment in log2</summary>
		public const int k32ByteAlignmentBit = 5;

		/// <summary>1KB alignment in log2</summary>
		public const int kKiloAlignmentBit = 10;
		/// <summary>4KB alignment in log2</summary>
		public const int kFourKiloAlignmentBit = 12;


		/// <summary>Round up <paramref name="value"/> to nearest multiple of <paramref name="mult"/></summary>
		/// <param name="value"></param>
		/// <param name="mult"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint RoundUpUInt32(uint value, uint mult)
		{
			if (mult == 0)
				return value;

			return value - (value-1) % mult + (mult-1);
		}
	};
}