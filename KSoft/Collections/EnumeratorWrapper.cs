using System.Collections.Generic;

namespace KSoft
{
	/// <summary>Wraps an IEnumerator object so it can be used in an foreach expression</summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks>
	/// Use this lightweight wrapper (it's a value type) when a type already has a provided IEnumerable
	/// implementation, but the type has other data/properties which can be enumerated with a foreach.
	/// </remarks>
	/// <see cref="Collections.BitSet.ClearBitIndices"/>
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