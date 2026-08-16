using System;

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
}
