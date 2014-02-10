using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	// http://burtleburtle.net/bob/c/lookup3.c
	// TODO: hashlittle2 support (two results)?
	public abstract class JenkinsHashLookup3 : JenkinsHashLookup
	{
		const uint kGoldenRatio = 0xDEADBEEF;
		const int kBlockSize = 12;

		static uint rot(uint x, int k)
		{
			return (x << k) | (x >> (Bits.kInt32BitCount-k));
		}

		struct HashState
		{
			uint a, b, c;

			public uint Result { get { return c; } }

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

			void Fill(byte[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref a, ref b, ref c, data, ref i);
			}

			void Fill(char[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref a, ref b, ref c, data, ref i);
			}

			void Fill(string data, ref int i)
			{
				JenkinsHashLookup.Fill(ref a, ref b, ref c, data, ref i);
			}

			void FinalFill(byte[] data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			public void ProcessBlock(byte[] buffer, ref int index)
			{
				Fill(buffer, ref index);
				Mix();
			}

			public void ProcessBlock(char[] buffer, ref int index)
			{
				Fill(buffer, ref index);
				Mix();
			}

			public void ProcessBlock(string buffer, ref int index)
			{
				Fill(buffer, ref index);
				Mix();
			}

			public void ProcessFinalBlock(byte[] buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				if(length > 0) FinalMix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				if (length > 0) FinalMix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				if (length > 0) FinalMix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(char[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(string buffer, uint seed = 0)
		{
			Contract.Requires(buffer != null);

			int length = buffer.Length;
			int index = 0;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}