using System;

namespace KSoft.Collections
{
	public static partial class IReadOnlyBitSetEnumerators
	{
		public partial struct StateEnumerator
		{
			public StateEnumerator(IReadOnlyBitSet bitset)
				: this(Util.ThrowIfNull(bitset), false)
			{
			}

			public bool MoveNext()
			{
				if (mBitIndex < mLastIndex)
				{
					mCurrent = mSet.Get(++mBitIndex);
					return true;
				}

				mBitIndex = mSet.Length;
				return false;
			}
		};

		public partial struct StateFilterEnumerator
		{
			public StateFilterEnumerator(IReadOnlyBitSet bitset, bool stateFilter, int startBitIndex = 0)
				: this(Util.ThrowIfNull(bitset), false)
			{
				ArgumentOutOfRangeException.ThrowIfNegative(startBitIndex);
				if (bitset.Length != 0)
				{
					ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startBitIndex, bitset.Length);
				}

				mStateFilter = stateFilter;
				mStartBitIndex = startBitIndex-1;
			}

			public bool MoveNext()
			{
				if (mBitIndex.IsNone())
				{
					mBitIndex = mStartBitIndex;
				}

				if (mBitIndex < mLastIndex)
				{
					mCurrent = mSet.NextBitIndex(++mBitIndex, mStateFilter);

					if (mCurrent >= 0)
					{
						mBitIndex = mCurrent;
						return true;
					}
				}

				mBitIndex = mSet.Length;
				return false;
			}
		};
	};
}