using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace KSoft.Verify;

/// <summary>Provides argument verification helpers for bit-width contracts.</summary>
public static class Bits
{
	/// <summary>Verifies that a bit count does not exceed a maximum value.</summary>
	/// <remarks>This is an upper-bound-only check; it does not reject zero or negative values.</remarks>
	/// <param name="bitCount">The bit count value to validate.</param>
	/// <param name="maximum">The inclusive maximum value allowed for <paramref name="bitCount"/>.</param>
	/// <param name="bitCountParamName">
	/// The caller expression to use when <paramref name="bitCount"/> is greater than <paramref name="maximum"/>.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="bitCount"/> is greater than <paramref name="maximum"/>.
	/// </exception>
	public static void AtMost(int bitCount, int maximum,
		[CallerArgumentExpression(nameof(bitCount))] string? bitCountParamName = null)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(bitCount, maximum, bitCountParamName);
	}
}
