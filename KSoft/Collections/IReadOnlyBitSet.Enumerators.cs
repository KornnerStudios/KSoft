using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public static partial class IReadOnlyBitSetEnumerators
	{
		public partial struct StateEnumerator
		{
			public StateEnumerator(IReadOnlyBitSet bitset)
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
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
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < bitset.Length || bitset.Length == 0);

				mStateFilter = stateFilter;
				mStartBitIndex = startBitIndex-1;
			}

			public bool MoveNext()
			{
				if (mBitIndex.IsNone())
					mBitIndex = mStartBitIndex;

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