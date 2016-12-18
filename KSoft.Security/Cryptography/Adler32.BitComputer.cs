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

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset + length <= buffer.Length);

				int buflen = length;
				for (int blocklen; buflen > 0; buflen -= blocklen)
				{
					blocklen = buflen < kBlockMax
						? buflen
						: kBlockMax;

					int x;
					for (x = 0; x + 7 < blocklen; x += 8, offset += 8)
					{
						s1 += buffer[offset + 0]; s2 += s1;
						s1 += buffer[offset + 1]; s2 += s1;
						s1 += buffer[offset + 2]; s2 += s1;
						s1 += buffer[offset + 3]; s2 += s1;
						s1 += buffer[offset + 4]; s2 += s1;
						s1 += buffer[offset + 5]; s2 += s1;
						s1 += buffer[offset + 6]; s2 += s1;
						s1 += buffer[offset + 7]; s2 += s1;
					}

					for (; x < blocklen; x++, offset++)
					{
						s1 += buffer[offset]; s2 += s1;
					}

					s1 %= kAdlerMod; s2 %= kAdlerMod;
				}
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