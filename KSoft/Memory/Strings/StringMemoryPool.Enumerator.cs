using System;
using System.Collections.Generic;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Memory.Strings
{
	partial class StringMemoryPool
	{
		/// <summary>
		/// Enumerates the strings in a <see cref="StringMemoryPool"/> in key-value pairs 
		/// with the <b>address</b> being the <i>key</i> and the <b>string</b> being the <i>value</i>
		/// </summary>
		protected struct KeyValueEnumerator : IEnumerator<KeyValuePair<Values.PtrHandle, string>>
		{
			const int kBlankIndexState = -2;

			StringMemoryPool mPool;
			int mCurrentIndex;

			public KeyValueEnumerator(StringMemoryPool pool)
			{
				mPool = pool;
				mCurrentIndex = kBlankIndexState;
				mCurrent = new KeyValuePair<Values.PtrHandle, string>(Values.PtrHandle.Null32, null);
			}

			#region IEnumerator<T> Members
			KeyValuePair<Values.PtrHandle, string> mCurrent;
			/// <summary>Get the current element in the enumeration</summary>
			public KeyValuePair<Values.PtrHandle, string> Current { get { return mCurrent; } }
			#endregion

			#region IDisposable Members
			void IDisposable.Dispose() { }
			#endregion

			#region IEnumerator Members
			/// <summary>Get the current element in the enumeration</summary>
			object System.Collections.IEnumerator.Current { get { return mCurrent; } }

			/// <summary>Advances the enumerator to the next address\string pair</summary>
			/// <returns></returns>
			public bool MoveNext()
			{
				// for supporting state Resets
				if (mCurrentIndex == kBlankIndexState)
					mCurrentIndex = 0;

				if (mCurrentIndex >= 0 && mCurrentIndex < mPool.Count)
				{
					mCurrent = new KeyValuePair<Values.PtrHandle, string>(
						mPool.mReferences[mCurrentIndex], mPool.mPool[mCurrentIndex]);

					mCurrentIndex++;
				}
				// when we've past the end of the pool
				else
					mCurrentIndex = TypeExtensions.kNoneInt32;

				return mCurrentIndex >= 0 && mCurrentIndex < mPool.Count;
			}

			public void Reset() { mCurrentIndex = kBlankIndexState; }
			#endregion
		};
	};
}