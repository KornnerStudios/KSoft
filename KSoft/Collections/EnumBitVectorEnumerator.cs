using System;
using System.Collections;
using System.Collections.Generic;

namespace KSoft.Collections;

/// <summary>Enumerates matching declared indices from a fixed-vector value snapshot.</summary>
/// <typeparam name="TBits">The associated bit-index enum.</typeparam>
/// <remarks>Enumerators returned by the fixed vectors visit each matching index once in ascending order. Later mutations of the original vector do not alter the captured state.</remarks>
public struct EnumBitVectorEnumerator<TBits> : IEnumerator<TBits> where TBits : struct, Enum
{
	readonly ulong mInitial;
	ulong mRemaining;
	int mCurrentIndex;

	internal EnumBitVectorEnumerator(ulong word, int width, bool stateFilter)
	{
		mInitial = (stateFilter ? word : ~word) & EnumBitTraits<TBits>.GetMask(width);
		mRemaining = mInitial;
		mCurrentIndex = -1;
	}

	/// <summary>Gets the member at the current declared index.</summary>
	/// <remarks>Valid after a successful <see cref="MoveNext"/> and before exhaustion; enum value-to-string alias selection is not controlled by this enumerator.</remarks>
	/// <exception cref="InvalidOperationException">An enumerator returned by a fixed vector is not positioned on a member.</exception>
	public readonly TBits Current => mCurrentIndex >= 0
		? EnumBitTraits<TBits>.FromIndex(mCurrentIndex)
		: throw new InvalidOperationException("The enumerator is not positioned on a member.");
	readonly object IEnumerator.Current => Current;

	/// <summary>Advances to the next matching declared index in the captured value.</summary>
	/// <returns><see langword="true"/> if positioned on a member; otherwise <see langword="false"/>.</returns>
	public bool MoveNext()
	{
		if (mRemaining == 0)
		{
			mCurrentIndex = -1;
			return false;
		}
		mCurrentIndex = System.Numerics.BitOperations.TrailingZeroCount(mRemaining);
		mRemaining &= mRemaining - 1;
		return true;
	}

	/// <summary>Restores the captured starting state before the first member.</summary>
	public void Reset()
	{
		mRemaining = mInitial;
		mCurrentIndex = -1;
	}

	public readonly void Dispose() { }
}
