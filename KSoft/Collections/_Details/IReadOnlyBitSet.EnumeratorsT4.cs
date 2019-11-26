using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	static partial class IReadOnlyBitSetEnumerators
	{
		[Serializable]
		partial struct StateEnumerator
			: IEnumerator< bool >
		{
			readonly IReadOnlyBitSet mSet;
			readonly int mLastIndex;
			readonly int mVersion;
			int mBitIndex;
			bool mCurrent;

			StateEnumerator(IReadOnlyBitSet bitset, bool dummy)
				: this()
			{
				mSet = bitset;
				mLastIndex = bitset.Length - 1;
				mVersion = bitset.Version;
				mBitIndex = TypeExtensions.kNone;
			}

			void VerifyVersion()
			{
				if (mVersion != mSet.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public bool Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > mLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				VerifyVersion();
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }
		};

		[Serializable]
		partial struct StateFilterEnumerator
			: IEnumerator< int >
		{
			readonly IReadOnlyBitSet mSet;
			readonly int mLastIndex;
			readonly int mVersion;
			int mBitIndex;
			int mCurrent;
			// defined here to avoid: CS0282: There is no defined ordering between fields in multiple declarations of partial class or struct
			readonly bool mStateFilter;
			readonly int mStartBitIndex;

			StateFilterEnumerator(IReadOnlyBitSet bitset, bool dummy)
				: this()
			{
				mSet = bitset;
				mLastIndex = bitset.Length - 1;
				mVersion = bitset.Version;
				mBitIndex = TypeExtensions.kNone;
			}

			void VerifyVersion()
			{
				if (mVersion != mSet.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public int Current { get {
				if (mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (mBitIndex > mLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public void Reset()
			{
				VerifyVersion();
				mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }
		};

	};
}