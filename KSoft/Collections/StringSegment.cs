using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Collections
{
	using StringSegmentEnumerator = StringSegment.Enumerator;

	// #TODO how is this better or different compared to StringSegment Microsoft.Extensions.Primitives.dll?
	// #REVIEW with .net9, can we replace this with Span<char> and ReadOnlySpan<char>?
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public readonly partial struct StringSegment
		: IReadOnlyList<char>
		, IEquatable<StringSegment>
		, IList<char>
	{
		readonly string mData;
		public readonly string Data => mData;
		readonly int mOffset;
		public readonly int Offset => mOffset;
		readonly int mCount;
		public readonly int Count => mCount;

		#region Ctor
		public StringSegment(string data)
		{
			ArgumentNullException.ThrowIfNull(data);
			mData = data;
			mOffset = 0;
			mCount = data.Length;
		}
		public StringSegment(string data, int offset, int count)
		{
			ArgumentNullException.ThrowIfNull(data);
			ArgumentOutOfRangeException.ThrowIfNegative(offset);
			ArgumentOutOfRangeException.ThrowIfNegative(count);
			if (count >= data.Length - offset)
			{
				throw new ArgumentException(null, nameof(count));
			}

			mData = data;
			mOffset = offset;
			mCount = count;
		}
		#endregion

		readonly void VerifyData()
		{
			if (mData == null)
			{
				throw new InvalidOperationException("String data is null");
			}
		}

		public readonly int IndexOf(char value)
		{
			VerifyData();

			int index = mData.IndexOf(value, mOffset, mCount);
			if (index < 0)
			{
				return TypeExtensions.kNone;
			}

			return index - mOffset;
		}

		public readonly bool Contains(char value)
		{
			VerifyData();

			return mData.IndexOf(value, mOffset, mCount) >= 0;
		}

		public readonly void CopyTo(char[] array, int arrayIndex)
		{
			VerifyData();

			mData.CopyTo(mOffset, array, arrayIndex, mCount);
		}

		#region IReadOnlyList<char> Members
		public readonly char this[int index] { get {
			VerifyData();

			return mData[mOffset + index];
		} }

		char IList<char>.this[int index] {
			readonly get { return this[index]; }
			set { throw new NotImplementedException(); }
		}
		#endregion

		#region IEnumerable<char> Members
		public readonly StringSegmentEnumerator GetEnumerator()
		{
			VerifyData();

			return new StringSegmentEnumerator(this);
		}
		readonly IEnumerator<char> IEnumerable<char>.GetEnumerator()
		{ return GetEnumerator(); }
		readonly System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{ return GetEnumerator(); }
		#endregion

		#region NotImplemented IList<char> Members
		void IList<char>.Insert(int index, char item) { throw new NotImplementedException(); }
		void IList<char>.RemoveAt(int index) { throw new NotImplementedException(); }
		#endregion

		#region NotImplemented ICollection<char> Members
		void ICollection<char>.Add(char item) { throw new NotImplementedException(); }
		void ICollection<char>.Clear() { throw new NotImplementedException(); }
		readonly bool ICollection<char>.IsReadOnly { get { return true; } }
		bool ICollection<char>.Remove(char item) { throw new NotImplementedException(); }
		#endregion

		#region Equatable Members
		public readonly bool Equals(StringSegment other)
		{
			return other.mData == mData && other.mOffset == mOffset && other.mCount == mCount;
		}
		public override readonly bool Equals(object obj)
		{
			return obj is StringSegment segment && Equals(segment);
		}

		public static bool operator ==(StringSegment lhs, StringSegment rhs)
		{
			return lhs.Equals(rhs);
		}
		public static bool operator !=(StringSegment lhs, StringSegment rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion

		public override readonly int GetHashCode()
		{
			if (mData != null)
			{
				return (mData.GetHashCode() ^ mOffset) ^ mCount;
			}

			return 0;
		}
	};
}
