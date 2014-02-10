using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public class XTEA256 : XTEABase
	{
		public void SetKey(byte[] b)
		{
			// round = 6 + 52/_countof(uint[] v)
			const int k_round_count = 32;

			uint[] key = new uint[4];
			for (int i = 0; i < 16; )
			{
				key[i >> 2]=((uint)(b[i++] << 24)) | 
							((uint)(b[i++] << 16)) | 
							((uint)(b[i++] << 8)) | 
							((uint) b[i++]);
			}

			uint[] r = new uint[k_round_count];
			for (int i = 0; i < r.Length; i++)
				r[i] = kDeltas[i] + key[kKeyIndex[i]];

			k0 =  r[0];  k1 =  r[1];  k2 =  r[2];  k3 =  r[3];  k4 =  r[4];  k5 =  r[5];  k6 =  r[6];  k7 =  r[7];
			k8 =  r[8];  k9 =  r[9];  k10 = r[10]; k11 = r[11]; k12 = r[12]; k13 = r[13]; k14 = r[14]; k15 = r[15];
			k16 = r[16]; k17 = r[17]; k18 = r[18]; k19 = r[19]; k20 = r[20]; k21 = r[21]; k22 = r[22]; k23 = r[23];
			k24 = r[24]; k25 = r[25]; k26 = r[26]; k27 = r[27]; k28 = r[28]; k29 = r[29]; k30 = r[30]; k31 = r[31];
		}

		protected override void EncryptBlock(byte[] input, byte[] output, int offset)
		{
		}

		protected override void DecryptBlock(byte[] input, byte[] output, int offset)
		{
			uint y = GetUInt32(input, offset+0);
			uint z = GetUInt32(input, offset+4);

			uint v;
			v = y >> 5;
			v ^= k31;

			z -= (((y >> 5) ^ (y << 4)) + y) ^ k31; y -= (((z << 4) ^ (z >> 5)) + z) ^ k30;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k29; y -= (((z << 4) ^ (z >> 5)) + z) ^ k28;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k27; y -= (((z << 4) ^ (z >> 5)) + z) ^ k26;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k25; y -= (((z << 4) ^ (z >> 5)) + z) ^ k24;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k23; y -= (((z << 4) ^ (z >> 5)) + z) ^ k22;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k21; y -= (((z << 4) ^ (z >> 5)) + z) ^ k20;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k19; y -= (((z << 4) ^ (z >> 5)) + z) ^ k18;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k17; y -= (((z << 4) ^ (z >> 5)) + z) ^ k16;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k15; y -= (((z << 4) ^ (z >> 5)) + z) ^ k14;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k13; y -= (((z << 4) ^ (z >> 5)) + z) ^ k12;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k11; y -= (((z << 4) ^ (z >> 5)) + z) ^ k10;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k9;  y -= (((z << 4) ^ (z >> 5)) + z) ^ k8;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k7;  y -= (((z << 4) ^ (z >> 5)) + z) ^ k6;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k5;  y -= (((z << 4) ^ (z >> 5)) + z) ^ k4;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k3;  y -= (((z << 4) ^ (z >> 5)) + z) ^ k2;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ k1;  y -= (((z << 4) ^ (z >> 5)) + z) ^ k0;

			SetUInt32(output, offset + 0, y);
			SetUInt32(output, offset + 4, z);
		}
	};
}