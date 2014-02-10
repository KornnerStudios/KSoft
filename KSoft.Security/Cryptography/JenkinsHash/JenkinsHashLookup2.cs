using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

			public uint Result { get { return c; } }

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
				c += (uint)length;

				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				c += (uint)length;

				JenkinsHashLookup.FinalFill(ref a, ref b, ref c, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
				c += (uint)length;

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
				Mix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				Mix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				FinalFill(buffer, ref index, length);
				Mix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(seed);
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

			HashState state = new HashState(seed);
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

			HashState state = new HashState(seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}