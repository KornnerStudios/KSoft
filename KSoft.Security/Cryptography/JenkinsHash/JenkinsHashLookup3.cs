using System;

namespace KSoft.Security.Cryptography
{
	// http://burtleburtle.net/bob/c/lookup3.c
	// #REVIEW: hashlittle2 support (two results)?
	public abstract class JenkinsHashLookup3 : JenkinsHashLookup
	{
		const uint kGoldenRatio = 0xDEADBEEF;
		const int kBlockSize = 12;

#pragma warning disable IDE1006 // Naming Styles
		static uint rot(uint x, int k)
#pragma warning restore IDE1006 // Naming Styles
		{
			return (x << k) | (x >> (Bits.kInt32BitCount - k));
		}

		struct HashState
		{
			uint a, b, c;

			public readonly uint Result { get { return c; } }

			public HashState(int length, uint seed)
			{
				a = b = c = kGoldenRatio + (uint)length + seed;
			}

			void Mix()
			{
				a -= c; a ^= rot(c, 4); c += b;
				b -= a; b ^= rot(a, 6); a += c;
				c -= b; c ^= rot(b, 8); b += a;
				a -= c; a ^= rot(c,16); c += b;
				b -= a; b ^= rot(a,19); a += c;
				c -= b; c ^= rot(b, 4); b += a;
			}

			void FinalMix()
			{
				c ^= b; c -= rot(b, 14);
				a ^= c; a -= rot(c, 11);
				b ^= a; b -= rot(a, 25);
				c ^= b; c -= rot(b, 16);
				a ^= c; a -= rot(c, 4);
				b ^= a; b -= rot(a, 14);
				c ^= b; c -= rot(b, 24);
			}

			void Fill(ReadOnlySpan<byte> data, ref int i)
			{
				JenkinsHashLookup.Fill(ref a, ref b, ref c, data, ref i);
			}

			void Fill(ReadOnlySpan<char> data, ref int i)
			{
				JenkinsHashLookup.Fill(ref a, ref b, ref c, data, ref i);
			}

			void FinalFill(ReadOnlySpan<byte> data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(ReadOnlySpan<char> data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			public void ProcessBlock(ReadOnlySpan<byte> buffer, ref int index)
			{
				Fill(buffer, ref index);
				Mix();
			}

			public void ProcessBlock(ReadOnlySpan<char> buffer, ref int index)
			{
				Fill(buffer, ref index);
				Mix();
			}

			public void ProcessFinalBlock(ReadOnlySpan<byte> buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				if (length > 0) { FinalMix(); }
			}

			public void ProcessFinalBlock(ReadOnlySpan<char> buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				if (length > 0) { FinalMix(); }
			}
		};

		static uint HashCore(ReadOnlySpan<byte> buffer, uint seed, int index, int length)
		{
			var state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
			{
				state.ProcessBlock(buffer, ref index);
			}

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
		static uint HashCore(ReadOnlySpan<char> buffer, uint seed, int index, int length)
		{
			var state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
			{
				state.ProcessBlock(buffer, ref index);
			}

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			ArgumentNullException.ThrowIfNull(buffer);

			if (length.IsNone())
			{
				length = buffer.Length - index;
			}

			return HashCore(buffer.AsSpan(), seed, index, length);
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(char[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			ArgumentNullException.ThrowIfNull(buffer);

			if (length.IsNone())
			{
				length = buffer.Length - index;
			}

			return HashCore(buffer.AsSpan(), seed, index, length);
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(string buffer, uint seed = 0)
		{
			ArgumentNullException.ThrowIfNull(buffer);

			return HashCore(buffer.AsSpan(), seed, 0, buffer.Length);
		}
	};
}