using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	partial class Adler32
	{
		public struct BitComputer
		{
			uint s1, s2;

			public BitComputer(uint adler32)
			{
				s1 = adler32 & 0xFFFF; s2 = adler32 >> 16;
			}

			public static BitComputer New { get { return new BitComputer(1); } }

			public uint ComputeFinish()
			{
				return Adler32.ComputeFinish(s1, s2);
			}

			#region Compute 16-bits
			public void ComputeLE(ushort value)
			{
				ComputeUpdate((value & 0x00FFU) >> 0, ref s1, ref s2);
				ComputeUpdate((value & 0xFF00U) >> 8, ref s1, ref s2);
			}
			public void ComputeBE(ushort value)
			{
				ComputeUpdate((value & 0xFF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((value & 0x00FFU) >> 0, ref s1, ref s2);
			}
			#endregion

			#region Compute 32-bits
			public void ComputeLE(uint value, uint adler32 = 1)
			{
				ComputeUpdate((value & 0x000000FFU) >> 0, ref s1, ref s2);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((value & 0xFF000000U) >> 24, ref s1, ref s2);
			}
			public void ComputeBE(uint value, uint adler32 = 1)
			{
				ComputeUpdate((value & 0xFF000000U) >> 24, ref s1, ref s2);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((value & 0x000000FFU) >> 0, ref s1, ref s2);
			}
			#endregion

			#region Compute 64-bits
			public void ComputeLE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = lo;
				ComputeUpdate((_value & 0x000000FFU) >> 0, ref s1, ref s2);
				ComputeUpdate((_value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((_value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((_value & 0xFF000000U) >> 24, ref s1, ref s2);

				_value = hi;
				ComputeUpdate((_value & 0x000000FFU) >> 0, ref s1, ref s2);
				ComputeUpdate((_value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((_value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((_value & 0xFF000000U) >> 24, ref s1, ref s2);
			}
			public void ComputeBE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = hi;
				ComputeUpdate((_value & 0xFF000000U) >> 24, ref s1, ref s2);
				ComputeUpdate((_value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((_value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((_value & 0x000000FFU) >> 0, ref s1, ref s2);

				_value = lo;
				ComputeUpdate((_value & 0xFF000000U) >> 24, ref s1, ref s2);
				ComputeUpdate((_value & 0x00FF0000U) >> 16, ref s1, ref s2);
				ComputeUpdate((_value & 0x0000FF00U) >> 8, ref s1, ref s2);
				ComputeUpdate((_value & 0x000000FFU) >> 0, ref s1, ref s2);
			}
			#endregion
		};
	};
}