using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	/// <summary>
	/// Utilities for converting between 32-bit and 24-bit single-precision floating point values.
	/// </summary>
	/// <remarks>
	/// 24-bit: s1e6m17, meaning: 1 sign bit, 6 exponent bits, 17 mantissa bits
	/// 32-bit: s1e8m23, meaning: 1 sign bit, 8 exponent bits, 23 mantissa bits
	/// </remarks>
	public static class Single24
	{
		public const float MinValue = kMin;
		public const float MaxValue = kMax;

		// s1e8m23
		#region Single bit definitions
		static class Single32
		{
			internal const int kMantissaBitIndex = 0;
			internal const int kMantissaBitCount = 23;
			/// <summary>0x007FFFFF = (1 LHS 23) - 1</summary>
			internal const uint kMantissaBitMask = (1U<<kMantissaBitCount)-1;

			/// <summary>23</summary>
			internal const int kExponentBitIndex = kMantissaBitIndex + kMantissaBitCount;
			internal const int kExponentBitCount = 8;
			/// <summary>0x7F800000 = ((1 LHS 8) - 1) LHS 23</summary>
			internal const uint kExponentBitMask = ((1U<<kExponentBitCount)-1) << kExponentBitIndex;

			/// <summary>31</summary>
			internal const int kSignBitIndex = kExponentBitIndex + kExponentBitCount;
			internal const int kSignBitCount = 1;
			/// <summary>0x80000000 = 1 LHS 31</summary>
			internal const uint kSignBitMask = 1U << kSignBitIndex;

			/// <summary>0x80000000</summary>
			internal const uint kSignBit = kSignBitMask;

			/// <summary>127</summary>
			internal const int kExponentBias = (1 << (kExponentBitCount-1)) - 1;
		};
		#endregion

		// s1e6m17
		#region Single24 bit definitions
		const int kMantissaBitIndex = 0;
		const int kMantissaBitCount = 17;
		/// <summary>0x0001FFFF = (1 LHS 17) - 1</summary>
		const uint kMantissaBitMask = (1U << kMantissaBitCount) - 1;

		/// <summary>17</summary>
		const int kExponentBitIndex = kMantissaBitIndex + kMantissaBitCount;
		const int kExponentBitCount = 6;
		/// <summary>63 = (1 LHS 6) - 1</summary>
		const uint kExponentMaxValue = (1U << kExponentBitCount) - 1;
		/// <summary>32 = (1 LHS 6) RHS 1</summary>
		const uint kExponentMidpoint = (1U << kExponentBitCount) >> 1;
		/// <summary>0x007E0000 = ((1 LHS 6) - 1) LHS 17</summary>
		const uint kExponentBitMask = ((1U << kExponentBitCount) - 1) << kExponentBitIndex;

		/// <summary>23</summary>
		const int kSignBitIndex = kExponentBitIndex + kExponentBitCount;
		const int kSignBitCount = 1;
		/// <summary>0x00800000 = 1 LHS 23</summary>
		const uint kSignBitMask = 1U << kSignBitIndex;

		/// <summary>0x00800000</summary>
		const uint kSignBit = kSignBitMask;

		/// <summary>31</summary>
		const int kExponentBias = (1 << (kExponentBitCount - 1)) - 1;
		/// <summary>96 = 127 - 31</summary>
		const int kExponentBiasDiff = Single32.kExponentBias - Single24.kExponentBias;

		/// <summary>6 = 23 - 17</summary>
		const int kMantissaBitDiff = Single32.kMantissaBitCount - kMantissaBitCount;
		/// <summary>64 = 1 LHS 6</summary>
		const uint kMantissaBitDiffMaxValue = 1U << kMantissaBitDiff;
		/// <summary>63 = (1 LHS 6) - 1</summary>
		const uint kMantissaBitDiffBitMask = (1U << kMantissaBitDiff) - 1;
		/// <remarks>Exponent is encoded in offset-binary, so we can't just shift the bits like mantissa</remarks>
		[Obsolete]
		const int kExponentBitDiff = Single32.kExponentBitCount - kExponentBitCount;

		/// <summary>0xFFFFFF = 0x00800000 | 0x007E0000 | 0x0001FFFF</summary>
		const uint kBitMask = kSignBitMask | kExponentBitMask | kMantissaBitMask;
		#endregion

		#region Min\Max
		// min\max values for a signed single
		internal const uint kMinInt = 0xFFFFFF;
		internal const uint kMaxInt = 0x7FFFFF;
		const float kMin = -8.589902E+09F;
		const float kMax = 8.589902E+09F;
		#endregion

		/// <summary>
		/// Is the given 32-bit float value in range of what Single24 can roughly represent?
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool InRange(float value)
			=> value >= kMin && value <= kMax;

		public static bool TryFromSingle(float singleValue, out uint encodedBits)
		{
			encodedBits = 0;

			uint singleValueAsUInt = Bitwise.ByteSwap.SingleToUInt32(singleValue);
			uint mantissa = (singleValueAsUInt & Single32.kMantissaBitMask) >> Single32.kMantissaBitIndex;
			uint exponentBits = (singleValueAsUInt & Single32.kExponentBitMask) >> Single32.kExponentBitIndex;
			uint sign = (singleValueAsUInt & Single32.kSignBitMask) >> Single32.kSignBitIndex;

			// handle zero / denormalized values
			if (exponentBits == 0)
			{
				encodedBits = sign == 1
					? kSignBit // -0.0
					: 0; // 0.0
				return true;
			}

			// first get the true Single32 exponent
			uint exponent = exponentBits - Single32.kExponentBias;
			// then put the exponent into the Single24 format
			uint newExponent = exponent + Single24.kExponentBias;
			// it should not actually be possible to get here with an exponent of 0
			// as we already handled that case above, checking exponentBits
			if (newExponent == 0 || newExponent > Single24.kExponentMaxValue)
			{
				return false;
			}

			// the fractional remainder of the 6 bits which are going to get discarded
			uint remainder = mantissa & Single24.kMantissaBitDiffBitMask;
			// clear bottom 5 bits of the original 17-bit mantissa
			// to get the initial rounded-down value
			uint newMantissa = mantissa & ~Single24.kMantissaBitDiffBitMask;

			// round-to-nearest-even ("banker's rounding") to match IEEE 754
			if (remainder >= Single24.kExponentMidpoint)
			{
				bool roundUp = false;
				// check for tie-breaker
				if (remainder == Single24.kExponentMidpoint)
				{
					// do we need to round toward even?
					if ((newMantissa & kMantissaBitDiffMaxValue) != 0)
					{
						roundUp = true;
					}
				}
				else
				{
					roundUp = false;
				}

				if (roundUp)
				{
					// same as doing += kMantissaBitDiffMaxValue
					newMantissa |= (1U << kMantissaBitDiff);
					newMantissa += kMantissaBitDiffMaxValue;
					// check for overflow
					if (newMantissa >= Single32.kMantissaBitMask)
					{
						newMantissa = 0;
						newExponent++;
						if (newExponent > Single24.kExponentMaxValue)
						{
							return false;
						}
					}
				}
			}
			else
			{
				// keep newMantissa as-is (round down)
			}

			newMantissa >>= kMantissaBitDiff;

			Contract.Assert(newExponent>=1 && newExponent<=Single24.kExponentMaxValue);
			Contract.Assert(newMantissa <= Single24.kMantissaBitMask);
			uint v = newMantissa;
			v |= newExponent << Single24.kExponentBitIndex;
			v |= sign == 1 ? Single24.kSignBit : 0U;
			Contract.Assert(v <= Single24.kBitMask);

			encodedBits = v;
			return true;
		}

		[System.Obsolete($"Use {nameof(TryFromSingle)}")]
		public static uint FromSingle(float singleValue)
		{
			uint data = Bitwise.ByteSwap.SingleToUInt32(singleValue);
			uint mantissa = (data & Single32.kMantissaBitMask) >> Single32.kMantissaBitIndex;
			uint exponent = (data & Single32.kExponentBitMask) >> Single32.kExponentBitIndex;
			uint sign = (data & Single32.kSignBitMask) >> Single32.kSignBitIndex;
			uint v;

			if (exponent == 0)
			{
				v = kSignBit;
			}
			else
			{
				sign = sign == 1 ? kSignBit : 0U;

				exponent -= kExponentBiasDiff;
				exponent <<= kExponentBitIndex;

				mantissa >>= kMantissaBitDiff;
				mantissa <<= kMantissaBitIndex;

				v = exponent | mantissa;
				v |= sign;
			}

			return v;
		}
		public static float ToSingle(uint data)
		{
			uint mantissa = (data & kMantissaBitMask) >> kMantissaBitIndex;
			uint exponentBits = (data & kExponentBitMask) >> kExponentBitIndex;
			uint sign = (data & kSignBitMask) >> kSignBitIndex;
			uint v;

			if (exponentBits == 0)
			{
				v = sign == 1
					? Single32.kSignBit // -0.0
					: 0;
			}
			else
			{
				sign = sign == 1
					? Single32.kSignBit
					: 0;

				// We used to have this code, but it isn't as clear what is taking place
				// so break it into exponent and newExponent
				//exponentBits += Single24.kExponentBiasDiff;
				uint exponent = exponentBits - Single24.kExponentBias;
				uint newExponent = exponent + Single32.kExponentBias;

				// lowest bits are the mantissa
				// account for the larger amount of mantissa bits in Single32 vs 24
				v  = mantissa << Single24.kMantissaBitDiff;
				// shift over the exponent after the preceding mantissa bits
				v |= newExponent << Single32.kMantissaBitCount;
				v |= sign;
			}

			return Bitwise.ByteSwap.SingleFromUInt32(v);
		}
	};
}
