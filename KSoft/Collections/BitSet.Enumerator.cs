using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	partial class BitSet
	{
		[Serializable]
		abstract class EnumeratorBase<TCurrent> : IEnumerator<TCurrent>, ICloneable
		{
			protected readonly BitSet mSet;
			protected readonly int mLastIndex;
			readonly int mVersion;
			protected int mBitIndex;
			protected TCurrent mCurrent;

			protected EnumeratorBase(BitSet set)
			{
				mSet = set;
				mLastIndex = set.Length - 1;
				mBitIndex = TypeExtensions.kNoneInt32;

				mVersion = set.mVersion;
			}
			public object Clone() { return MemberwiseClone(); }

			[Contracts.ContractInvariantMethod]
			void ObjectInvariant()
			{
				Contract.Invariant(mVersion == mSet.mVersion, "Collection was modified; enumeration operation may not execute.");
			}

			public TCurrent Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > mLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public abstract bool MoveNext();
			public void Reset()			{ mBitIndex = TypeExtensions.kNoneInt32; }

			void IDisposable.Dispose()	{ }
		};

		[Serializable]
		sealed class EnumeratorSimple : EnumeratorBase<bool>
		{
			public EnumeratorSimple(BitSet set) : base(set) {}

			public override bool MoveNext()
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

		[Serializable]
		sealed class EnumeratorBitState : EnumeratorBase<int>
		{
			readonly bool mStateFilter;
			readonly int mStartBitIndex;

			public EnumeratorBitState(BitSet set, bool stateFilter, int startBitIndex = 0) : base(set)
			{
				mStateFilter = stateFilter;
				mStartBitIndex = startBitIndex-1;
			}

			public override bool MoveNext()
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