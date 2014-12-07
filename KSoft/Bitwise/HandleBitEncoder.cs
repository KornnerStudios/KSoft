using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	/// <summary>Stack friendly bit encoder for dealing with handle generation or reading</summary>
	/// <remarks>Bits are written from the LSB to the MSB</remarks>
	[System.Diagnostics.DebuggerDisplay("Bits = {m64}, BitIndex = {mBitIndex}")]
	public partial struct HandleBitEncoder
	{
		IntegerUnion mBits;
		int mBitIndex;

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(mBitIndex >= 0);
			Contract.Invariant(mBitIndex <= Bits.kInt64BitCount);
		}

		/// <summary>How many bits have actually been consumed by the handle data</summary>
		public int UsedBitCount { get {
			return mBitIndex;
		} }

		/// <summary>Get the entire handle's value represented in 32-bits</summary>
		/// <returns></returns>
		public uint GetCombinedHandle()
		{
			uint hi = Bits.GetHighBits(mBits.u64);

			// this order allows a user to XOR again with GetHandle32 to get 
			// the upper 32-bit values of m64
			return hi ^ mBits.u32;
		}

		void VerifyBitIndex(int advanceBitCount)
		{
			if (mBitIndex + advanceBitCount > Bits.kInt64BitCount)
				throw new System.ArgumentOutOfRangeException("bitIndex", mBitIndex + advanceBitCount,
					"bitIndex is or will be greater than to Bits.kInt64BitCount");
		}

		/// <summary>Clear the internal state of the encoder</summary>
		public void Reset()
		{
			mBits = new IntegerUnion();
			mBitIndex = 0;
		}

		#region Overrides
		public override bool Equals(object obj)
		{
			if (obj is HandleBitEncoder)
			{
				var o = (HandleBitEncoder)obj;

				return mBitIndex == o.mBitIndex &&
					mBits.u64 == o.mBits.u64;
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
			return string.Format("[{0} @ {1}]", mBits.u64.ToString("X16"), mBitIndex.ToString());
		}
		#endregion
	};
}