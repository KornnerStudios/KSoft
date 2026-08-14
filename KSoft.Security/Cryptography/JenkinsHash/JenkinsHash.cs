using System;

#nullable enable

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

		public static uint Hash(byte[] buffer, int index = 0, int length = -1)
		{
			ArgumentNullException.ThrowIfNull(buffer);

			if (length == -1)
			{
				length = buffer.Length - index;
			}

			uint hash = 0;

			for (int x = 0; x < length; x++)
			{
				hash = HashChar(hash, (char)buffer[index + x]);
			}

			return HashEnd(hash);
		}

		public static uint Hash(char[] buffer, int index = 0, int length = -1)
		{
			ArgumentNullException.ThrowIfNull(buffer);

			if (length == -1)
			{
				length = buffer.Length - index;
			}

			uint hash = 0;

			for (int x = 0; x < length; x++)
			{
				hash = HashChar(hash, buffer[index + x]);
			}

			return HashEnd(hash);
		}

		public static uint Hash(string buffer)
		{
			ArgumentException.ThrowIfNullOrEmpty(buffer);

			uint hash = 0;

			for (int x = 0; x < buffer.Length; x++)
			{
				hash = HashChar(hash, buffer[x]);
			}

			return HashEnd(hash);
		}
	};
}
