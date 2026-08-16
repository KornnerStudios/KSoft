using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace KSoft.Verify;

/// <summary>Provides verification helpers for group-tag contracts.</summary>
public static class GroupTags
{
	/// <summary>Verifies that a character-array group tag has the expected exact length.</summary>
	/// <param name="tag">The group-tag character array to validate.</param>
	/// <param name="expectedLength">The exact required tag length.</param>
	/// <param name="parameterName">The parameter name to report when validation fails.</param>
	/// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="tag"/> does not have <paramref name="expectedLength"/> characters.
	/// </exception>
	public static void ExactLength(char[]? tag, int expectedLength, string parameterName)
	{
		if (tag == null)
		{
			throw new ArgumentNullException(parameterName);
		}
		if (tag.Length != expectedLength)
		{
			throw new ArgumentOutOfRangeException(parameterName, "Tag lengths mismatch");
		}
	}

	/// <summary>Verifies that a string group tag has the expected exact length.</summary>
	/// <param name="tagString">The group-tag string to validate.</param>
	/// <param name="expectedLength">The exact required tag length.</param>
	/// <param name="parameterName">The parameter name to report when validation fails.</param>
	/// <exception cref="ArgumentNullException"><paramref name="tagString"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="tagString"/> is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="tagString"/> does not have <paramref name="expectedLength"/> characters.
	/// </exception>
	public static void ExactLength(string? tagString, int expectedLength, string parameterName)
	{
		ArgumentException.ThrowIfNullOrEmpty(tagString, parameterName);
		if (tagString.Length != expectedLength)
		{
			throw new ArgumentOutOfRangeException(parameterName, "Tag lengths mismatch");
		}
	}

	/// <summary>Verifies a group-tag constructor's tag, display name, and expected tag length.</summary>
	/// <param name="groupTag">The group-tag string to validate.</param>
	/// <param name="name">The group-tag display name to validate.</param>
	/// <param name="expectedLength">The exact required tag length.</param>
	/// <param name="groupTagParamName">The parameter name to report when <paramref name="groupTag"/> fails.</param>
	/// <param name="nameParamName">The parameter name to report when <paramref name="name"/> fails.</param>
	/// <param name="expectedLengthParamName">
	/// The parameter name to report when <paramref name="expectedLength"/> fails.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="groupTag"/> or <paramref name="name"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="groupTag"/> or <paramref name="name"/> is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="expectedLength"/> is not positive or <paramref name="groupTag"/> does not have
	/// <paramref name="expectedLength"/> characters.
	/// </exception>
	public static void NameAndExactLength(string? groupTag, string? name, int expectedLength,
		[CallerArgumentExpression(nameof(groupTag))] string? groupTagParamName = null,
		[CallerArgumentExpression(nameof(name))] string? nameParamName = null,
		[CallerArgumentExpression(nameof(expectedLength))] string? expectedLengthParamName = null)
	{
		ArgumentException.ThrowIfNullOrEmpty(groupTag, groupTagParamName);
		ArgumentException.ThrowIfNullOrEmpty(name, nameParamName);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedLength, expectedLengthParamName);
		if (groupTag.Length != expectedLength)
		{
			throw new ArgumentOutOfRangeException(groupTagParamName);
		}
	}

	/// <summary>Verifies a major/minor group-tag pair and display name for composite tag construction.</summary>
	/// <param name="major">The major group tag to validate.</param>
	/// <param name="minor">The minor group tag to validate.</param>
	/// <param name="name">The composite group-tag display name to validate.</param>
	/// <param name="majorParamName">The parameter name to report when <paramref name="major"/> fails.</param>
	/// <param name="minorParamName">The parameter name to report when <paramref name="minor"/> fails.</param>
	/// <param name="nameParamName">The parameter name to report when <paramref name="name"/> fails.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="major"/>, <paramref name="minor"/>, or <paramref name="name"/> is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> is empty, or <paramref name="major"/> or <paramref name="minor"/> is the null group tag.
	/// </exception>
	public static void CompositePair(Values.GroupTagData32? major, Values.GroupTagData32? minor, string? name,
		[CallerArgumentExpression(nameof(major))] string? majorParamName = null,
		[CallerArgumentExpression(nameof(minor))] string? minorParamName = null,
		[CallerArgumentExpression(nameof(name))] string? nameParamName = null)
	{
		ArgumentNullException.ThrowIfNull(major, majorParamName);
		ArgumentNullException.ThrowIfNull(minor, minorParamName);
		ArgumentException.ThrowIfNullOrEmpty(name, nameParamName);
		if (major == Values.GroupTagData32.Null)
		{
			throw new ArgumentException("Major group tag must not be the null group tag.", majorParamName);
		}
		if (minor == Values.GroupTagData32.Null)
		{
			throw new ArgumentException("Minor group tag must not be the null group tag.", minorParamName);
		}
	}
}
