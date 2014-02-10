using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Bitwise
{
	/// <summary>Stack friendly bit encoder for dealing with handle generation or reading</summary>
	[System.Diagnostics.DebuggerDisplay("Bits = {m64}, BitIndex = {mBitIndex}")]
	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=HandleBitEncoder.kSizeOf)]
	public partial struct HandleBitEncoder
	{
		public const int kSizeOf = sizeof(ulong) + sizeof(int);

		[Interop.FieldOffset(0)] ulong m64;
		[Interop.FieldOffset(0)] uint  m32;
		[Interop.FieldOffset(8)] int   mBitIndex;

		// TODO: Can't use ObjectInvariants with StructLayouts. Their internal '$evaluatingInvariant$' 
		// fields default to FieldOffset(0) and we can't change this
		// http://social.msdn.microsoft.com/Forums/en-US/codecontracts/thread/dd9a1d71-2eac-4f33-9adc-33b38d5f38a4/
// 		[Contracts.ContractInvariantMethod]
// 		void ObjectInvariant()
// 		{
// 			Contract.Invariant(mBitIndex >= 0);
// 			Contract.Invariant(mBitIndex < Bits.kInt64BitCount);
// 		}

		/// <summary>How many bits have actually been consumed by the handle data</summary>
		public int UsedBitCount { get {
			return mBitIndex;
		} }

		/// <summary>Get the entire handle's value represented in 32-bits</summary>
		/// <returns></returns>
		public uint GetCombinedHandle()
		{
			uint hi = IntegerMath.GetHighBits(m64);

			// this order allows a user to XOR again with GetHandle32 to get 
			// the upper 32-bit values of m64
			return hi ^ m32;
		}

		void VerifyBitIndex(int advanceBitCount)
		{
			if (mBitIndex + advanceBitCount >= Bits.kInt64BitCount)
				throw new System.ArgumentOutOfRangeException("bitIndex", mBitIndex + advanceBitCount,
					"bitIndex is or will be greater than or equal to Bits.kInt64BitCount");
		}

		/// <summary>Clear the internal state of the encoder</summary>
		public void Reset()
		{
			m64 = 0;
			m32 = 0; // just to be safe
			mBitIndex = 0;
		}

		#region Overrides
		public override bool Equals(object obj)
		{
			if (obj is HandleBitEncoder)
			{
				var o = (HandleBitEncoder)obj;

				return mBitIndex == o.mBitIndex &&
					m64 == o.m64;
			}

			return false;
		}
		public override int GetHashCode()
		{
			return (int)GetCombinedHandle();
		}
		/// <summary>"[{<see cref="GetHandle64()"/>} @ {CurrentBitIndex}]</summary>
		/// <returns></returns>
		/// <remarks>Handle value is formatted to a 16-character hex string</remarks>
		public override string ToString()
		{
			return string.Format("[{0} @ {1}]", m64.ToString("X16"), mBitIndex.ToString());
		}
		#endregion
	};
}