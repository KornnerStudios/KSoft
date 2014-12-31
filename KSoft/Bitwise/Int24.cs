﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	public static class Int24
	{
		public const uint MaxValue = kDataBitMask;

		#region Data\Number Access
		internal const int kDataBitIndex = 0;
		const int kDataBitCount = 23;
		internal const uint kDataBitMask = (1 << kDataBitCount) - 1;

		public static uint GetNumber(uint data)
		{
			return data & kDataBitMask;
		}
		#endregion

		#region Sign bit access
		internal const int kSignBitIndex = kDataBitIndex + kDataBitCount;
		const int kSignBitCount = 1;
		internal const int kSignBitMask = 1 << kSignBitIndex;

		public static bool IsSigned(uint data)
		{
			return Bitwise.Flags.Test(data, kSignBitMask);
		}
		public static uint SetSigned(uint data, bool isSigned)
		{
			return Bitwise.Flags.Modify(isSigned, data, kSignBitMask);
		}
		#endregion

		public static bool InRange(uint v)
		{
			return v <= kDataBitMask;
		}
	};
}