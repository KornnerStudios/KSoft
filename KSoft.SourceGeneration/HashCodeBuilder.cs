using System;

namespace KSoft.SourceGeneration;

// System.HashCode is not available to the netstandard2.0 analyzer target, so keep the fallback combiner centralized
// instead of scattering one-off prime-based GetHashCode implementations across descriptor and option value types.
internal static class HashCodeBuilder
{
	private const int kSeed = 17;
	private const int kMultiplier = 31;

	public static int Combine<T1, T2>(T1 value1, T2 value2)
	{
		unchecked
		{
			int hash = kSeed;
			hash = Add(hash, value1);
			hash = Add(hash, value2);
			return hash;
		}
	}

	public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
	{
		unchecked
		{
			int hash = kSeed;
			hash = Add(hash, value1);
			hash = Add(hash, value2);
			hash = Add(hash, value3);
			hash = Add(hash, value4);
			return hash;
		}
	}

	public static int Add<T>(int hash, T value)
		=> unchecked((hash * kMultiplier) + (value?.GetHashCode() ?? 0));

	public static int AddOrdinalString(int hash, string value)
	{
		ExceptionHelpers.ThrowIfNull(value, nameof(value));

		// Option names use ordinal equality, so their hash contribution must use the same comparison semantics.
		return unchecked((hash * kMultiplier) + StringComparer.Ordinal.GetHashCode(value));
	}
};
