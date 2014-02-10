using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public static partial class TypeExtensions
	{
		public static byte[] ComputeHash(this System.Security.Cryptography.HashAlgorithm algo,
			System.IO.Stream inputStream, long offset, long count, bool restorePosition = false)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((offset+count) <= inputStream.Length);

			int buffer_size = (int)System.Math.Min(count, 0x1000);
			byte[] buffer = new byte[buffer_size];

			long orig_pos = inputStream.Position;
			if (offset != orig_pos)
				inputStream.Seek(offset, System.IO.SeekOrigin.Begin);


			int read_count = 0;
			do {
				read_count = inputStream.Read(buffer, 0, System.Math.Min((int)count, buffer_size));
				if (read_count > 0)
					algo.TransformBlock(buffer, 0, read_count, null, 0);

				count -= read_count;
			} while(read_count > 0);

			algo.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			if(restorePosition)
				inputStream.Seek(orig_pos, System.IO.SeekOrigin.Begin);

			return algo.Hash;
		}
	};
}