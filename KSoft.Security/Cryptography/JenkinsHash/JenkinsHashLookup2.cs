using System;

namespace KSoft.Security.Cryptography
{
	/// <remarks>http://bretm.home.comcast.net/~bretm/hash/7.html</remarks>
	// http://burtleburtle.net/bob/c/lookup2.c
	public abstract class JenkinsHashLookup2 : JenkinsHashLookup
	{
		const uint kGoldenRatio = 0x9E3779B9;
		const int kBlockSize = 12; // 96 bits

		struct HashState
		{
			uint a, b, c;

			public readonly uint Result { get { return c; } }

			public HashState(uint seed)
			{
				a = b = kGoldenRatio;
				c = seed;
			}

			void Mix()
			{
				a -= b; a -= c; a ^= (c >> 13);
				b -= c; b -= a; b ^= (a <<  8);
				c -= a; c -= b; c ^= (b >> 13);
				a -= b; a -= c; a ^= (c >> 12);
				b -= c; b -= a; b ^= (a << 16);
				c -= a; c -= b; c ^= (b >>  5);
				a -= b; a -= c; a ^= (c >>  3);
				b -= c; b -= a; b ^= (a << 10);
				c -= a; c -= b; c ^= (b >> 15);
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
				c += (uint)length;

				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(ReadOnlySpan<char> data, ref int i, int length)
			{
				c += (uint)length;

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
				Mix();
			}

			public void ProcessFinalBlock(ReadOnlySpan<char> buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				Mix();
			}
		};

		static uint HashCore(ReadOnlySpan<byte> buffer, uint seed)
		{
			var state = new HashState(seed);
			int index = 0;
			for (; index + kBlockSize <= buffer.Length; )
			{
				state.ProcessBlock(buffer, ref index);
			}

			state.ProcessFinalBlock(buffer, ref index, buffer.Length);

			return state.Result;
		}
		static uint HashCore(ReadOnlySpan<char> buffer, uint seed)
		{
			var state = new HashState(seed);
			int index = 0;
			for (; index + kBlockSize <= buffer.Length; )
			{
				state.ProcessBlock(buffer, ref index);
			}

			state.ProcessFinalBlock(buffer, ref index, buffer.Length);

			return state.Result;
		}

		public static uint Hash(ReadOnlySpan<byte> buffer, uint seed = 0)
		{
			return HashCore(buffer, seed);
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(ReadOnlySpan<char> buffer, uint seed = 0)
		{
			return HashCore(buffer, seed);
		}
	};
}