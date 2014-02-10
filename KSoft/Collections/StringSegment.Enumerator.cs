using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	partial struct StringSegment
	{
		[Serializable]
		class Enumerator : IEnumerator<char>
		{
			string mData;
			int mCurrent, mStart, mEnd;

			public Enumerator(StringSegment segment)
			{
				mData = segment.mData;
				mStart = segment.mOffset;
				mEnd = mStart + segment.mCount;
				mCurrent = mStart - 1;
			}
			public void Dispose() { }

			#region IEnumerator<char> Members
			public char Current { get {
				if (mCurrent < mStart)	throw new InvalidOperationException("Enumeration has not started");
				if (mCurrent >= mEnd)	throw new InvalidOperationException("Enumeration already finished");

				return mData[mCurrent];
			} }
			object System.Collections.IEnumerator.Current { get { return Current; } }

			public bool MoveNext()
			{
				if (mCurrent < mEnd)
				{
					++mCurrent;
					return mCurrent < mEnd;
				}
				return false;
			}

			public void Reset()
			{
				mCurrent = mStart - 1;
			}
			#endregion
		};
	};
}