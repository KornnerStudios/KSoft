using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	partial struct StringSegment
	{
		[Serializable]
		public struct Enumerator
			: IEnumerator<char>
		{
			readonly string mData;
			readonly int mStart, mEnd;
			int mCurrent;

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