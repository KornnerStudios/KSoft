using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace KSoft.Verify;

/// <summary>Provides argument verification helpers for array buffer contracts.</summary>
public static class Buffers
{
	/// <summary>Verifies that a counted buffer argument is non-null and that the count fits within its length.</summary>
	/// <param name="value">The array value to validate.</param>
	/// <param name="count">The number of elements that will be read, written, or streamed.</param>
	/// <param name="valueParamName">The caller expression to use when <paramref name="value"/> is null.</param>
	/// <param name="countParamName">
	/// The caller expression to use when <paramref name="count"/> is negative or greater than the buffer length.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="count"/> is negative or greater than the length of <paramref name="value"/>.
	/// </exception>
	public static void CountWithinLength(Array? value, int count,
		[CallerArgumentExpression(nameof(value))] string? valueParamName = null,
		[CallerArgumentExpression(nameof(count))] string? countParamName = null)
	{
		ArgumentNullException.ThrowIfNull(value, valueParamName);
		ArgumentOutOfRangeException.ThrowIfNegative(count, countParamName);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(count, value.Length, countParamName);
	}
}
