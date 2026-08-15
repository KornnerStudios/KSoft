using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace KSoft.Collections
{
	public sealed class InvertedComparer : IComparer
	{
		readonly IComparer mComparer;

		public InvertedComparer(IComparer comparer)
		{
			ArgumentNullException.ThrowIfNull(comparer);

			mComparer = comparer;
		}

		#region IComparer Members
		public int Compare(object? x, object? y)
		{
			return -mComparer.Compare(x, y);
		}
		#endregion
	};

	public sealed class InvertedComparer<T> : IComparer<T>
	{
		readonly IComparer<T> mComparer;

		public InvertedComparer(IComparer<T> comparer)
		{
			ArgumentNullException.ThrowIfNull(comparer);

			mComparer = comparer;
		}

		#region IComparer<T> Members
		public int Compare(T? x, T? y)
		{
			return -mComparer.Compare(x, y);
		}
		#endregion
	};
}