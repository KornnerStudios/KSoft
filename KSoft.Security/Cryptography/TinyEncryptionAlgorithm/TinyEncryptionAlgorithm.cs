
namespace KSoft.Security.Cryptography
{
	/// <summary>T(iny) E(ncryption) (A)lgorithm</summary>
	public static class TEA
	{
		/// <summary>Encrypt routine</summary>
		/// <param name="v"></param>
		/// <param name="k">key data</param>
		public static void RoundEncrypt(uint[] v, uint[] k)
		{
			const uint delta = 0x9E3779B9;
			uint y = v[0], z = v[1];

			for (uint sum = 0, n = 32; n > 0; n--)
			{
				y += (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
				sum += delta;
				z += (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
			}

			v[0] = y;
			v[1] = z;
		}
		/// <summary>Decrypt routine</summary>
		/// <param name="v"></param>
		/// <param name="k">key data</param>
		public static void RoundDecrypt(uint[] v, uint[] k)
		{
			const uint delta = 0x9E3779B9;
			uint y = v[0], z = v[1];

			for (uint sum = delta << 5, n = 32; n > 0; n--)
			{
				z -= (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
				sum -= delta;
				y -= (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
			}

			v[0] = y;
			v[1] = z;
		}
	};
}