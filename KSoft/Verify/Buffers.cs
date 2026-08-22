using System;
using System.Runtime.CompilerServices;


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

	/// <summary>Verifies that a buffer start index is non-negative and can point at the end of the buffer.</summary>
	/// <param name="value">The array value to validate.</param>
	/// <param name="startIndex">The starting index into <paramref name="value"/>.</param>
	/// <param name="valueParamName">The caller expression to use when <paramref name="value"/> is null.</param>
	/// <param name="startIndexParamName">
	/// The caller expression to use when <paramref name="startIndex"/> is negative or greater than the buffer length.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="startIndex"/> is negative or greater than the length of <paramref name="value"/>.
	/// </exception>
	public static void StartIndexWithinLength(Array? value, int startIndex,
		[CallerArgumentExpression(nameof(value))] string? valueParamName = null,
		[CallerArgumentExpression(nameof(startIndex))] string? startIndexParamName = null)
	{
		ArgumentNullException.ThrowIfNull(value, valueParamName);
		ArgumentOutOfRangeException.ThrowIfNegative(startIndex, startIndexParamName);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, value.Length, startIndexParamName);
	}

	/// <summary>Verifies that an offset and length describe a valid array slice.</summary>
	/// <param name="value">The array value to validate.</param>
	/// <param name="offset">The starting offset into <paramref name="value"/>.</param>
	/// <param name="length">The number of elements in the slice.</param>
	/// <param name="valueParamName">The caller expression to use when <paramref name="value"/> is null.</param>
	/// <param name="offsetParamName">
	/// The caller expression to use when <paramref name="offset"/> is negative or greater than the buffer length.
	/// </param>
	/// <param name="lengthParamName">
	/// The caller expression to use when <paramref name="length"/> is negative or does not fit after
	/// <paramref name="offset"/>.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="offset"/> or <paramref name="length"/> is negative, or the slice exceeds the buffer length.
	/// </exception>
	public static void OffsetAndLengthWithinLength(Array? value, int offset, int length,
		[CallerArgumentExpression(nameof(value))] string? valueParamName = null,
		[CallerArgumentExpression(nameof(offset))] string? offsetParamName = null,
		[CallerArgumentExpression(nameof(length))] string? lengthParamName = null)
	{
		ArgumentNullException.ThrowIfNull(value, valueParamName);
		ArgumentOutOfRangeException.ThrowIfNegative(offset, offsetParamName);
		ArgumentOutOfRangeException.ThrowIfNegative(length, lengthParamName);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, value.Length, offsetParamName);
		if (length > value.Length - offset)
		{
			throw new ArgumentOutOfRangeException(lengthParamName);
		}
	}
}
