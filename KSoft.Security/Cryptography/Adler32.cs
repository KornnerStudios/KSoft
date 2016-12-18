using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public static partial class Adler32
	{
		// http://www.opensource.apple.com/source/xnu/xnu-1504.3.12/libkern/zlib/arm/adler32vec.s

		const uint kAdlerMod = 65521;

		public static uint Compute(byte[] buffer, int offset, int length, uint adler32 = 1)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset+length <= buffer.Length);

			uint s1 = adler32 & 0xFFFF, s2 = adler32 >> 16;

			int buflen = length;
			for (int blocklen = buflen % 5552; buflen > 0; buflen -= blocklen, blocklen = 5552)
			{
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
				buflen -= blocklen;
			}

			return (s2 << 16) + s1;
		}
		public static uint Compute(byte[] buffer, uint adler32 = 1)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);

			return Compute(buffer, 0, buffer.Length, adler32);
		}

		public static uint Compute(System.IO.Stream stream, int length, uint adler32 = 1)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<InvalidOperationException>(stream.CanRead);

			long prev_position = stream.CanSeek
				? stream.Position
				: -1;

			// #TODO inefficient, support buffering
			byte[] stream_bytes = new byte[length];
			for (int bytes_read = 0; bytes_read < length; )
			{
				bytes_read += stream.Read(stream_bytes, bytes_read, length - bytes_read);
			}

			if (prev_position != -1)
				stream.Seek(prev_position, System.IO.SeekOrigin.Begin);

			adler32 = Compute(stream_bytes, 0, stream_bytes.Length, adler32);

			return adler32;
		}


		static void ComputeUpdate(uint value, ref uint s1, ref uint s2)
		{
			s1 += value & 0xFFU;
			s2 += s1;
		}
		static uint ComputeFinish(uint s1, uint s2)
		{
			s1 %= kAdlerMod; s2 %= kAdlerMod;

			return (s2 << 16) + s1;
		}
		public static uint Compute(byte value, uint adler32 = 1)
		{
			uint s1 = adler32 & 0xFFFF, s2 = adler32 >> 16;

			ComputeUpdate(value, ref s1, ref s2);

			return ComputeFinish(s1, s2);
		}

		#region Compute 16-bits
		public static uint ComputeLE(ushort value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(ushort value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion

		#region Compute 32-bits
		public static uint ComputeLE(uint value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(uint value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion

		#region Compute 64-bits
		public static uint ComputeLE(ulong value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(ulong value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion
	};
}