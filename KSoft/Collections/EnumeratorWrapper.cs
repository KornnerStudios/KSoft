using System.Collections.Generic;

namespace KSoft
{
	public struct EnumeratorWrapper<T> : IEnumerable<T>
	{
		IEnumerator<T> mEnumerator;

		public EnumeratorWrapper(IEnumerator<T> enumerator) { mEnumerator = enumerator; }

		#region IEnumerable<T> Members
		public IEnumerator<T> GetEnumerator() { return mEnumerator; }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return mEnumerator; }
		#endregion
	};
}