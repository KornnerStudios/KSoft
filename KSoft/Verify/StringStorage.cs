using System;
using System.IO;


namespace KSoft.Verify;

/// <summary>Provides verification helpers for string-storage streaming contracts.</summary>
public static class StringStorage
{
	/// <summary>Verifies that a string storage definition has enough length information for streaming.</summary>
	/// <param name="storage">The string storage definition to validate.</param>
	/// <param name="length">The caller-provided string length.</param>
	/// <exception cref="InvalidDataException">
	/// <paramref name="storage"/> is an unfixed character-array storage definition and <paramref name="length"/> is not
	/// positive.
	/// </exception>
	public static void ForStreaming(Memory.Strings.StringStorage storage, int length)
	{
		if (storage.Type == Memory.Strings.StringStorageType.CharArray && !storage.IsFixedLength && length <= 0)
		{
			throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
				"Provided string storage and length is invalid for Endian streaming: {0}, {1}",
				storage.ToString(), length.ToString(Util.InvariantCultureInfo)));
		}
	}
}
