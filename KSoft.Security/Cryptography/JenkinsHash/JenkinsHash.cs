using System;

namespace KSoft.Security.Cryptography
{
	/// <summary>Jenkins One-at-a-time hash</summary>
	/// <remarks>http://en.wikipedia.org/wiki/Jenkins_hash_function#one-at-a-time</remarks>
	public static class JenkinsHash
	{
		static uint HashChar(uint hash, char c)
		{
			c = char.ToLowerInvariant(c);
			if (c == '\\')
			{
				c = '/';
			}

			hash += (byte)c;
			hash += hash << 10;
			hash ^= hash >> 6;

			return hash;
		}
		static uint HashEnd(uint hash)
		{
			hash += hash << 3;
			hash ^= hash >> 11;
			hash += hash << 15;

			if (hash < 2)
			{
				hash += 2;
			}

			return hash;
		}

		static uint HashCore(ReadOnlySpan<byte> buffer)
		{
			uint hash = 0;

			for (int x = 0; x < buffer.Length; x++)
			{
				hash = HashChar(hash, (char)buffer[x]);
			}

			return HashEnd(hash);
		}
		static uint HashCore(ReadOnlySpan<char> buffer)
		{
			uint hash = 0;

			for (int x = 0; x < buffer.Length; x++)
			{
				hash = HashChar(hash, buffer[x]);
			}

			return HashEnd(hash);
		}

		public static uint Hash(ReadOnlySpan<byte> buffer)
		{
			return HashCore(buffer);
		}

		public static uint Hash(ReadOnlySpan<char> buffer)
		{
			return HashCore(buffer);
		}
	};
}
