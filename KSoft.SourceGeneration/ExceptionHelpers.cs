using System;

namespace KSoft.SourceGeneration;

internal static class ExceptionHelpers
{
	public static void ThrowIfNull(object value, string paramName)
	{
		if (value == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}

	public static void ThrowIfNullOrEmpty(string value, string paramName)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("Value cannot be null or empty.", paramName);
		}
	}

	public static void ThrowIfNegative(int value, string paramName)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
		}
	}

	public static void ThrowIfGreaterThan(int value, int other, string paramName)
	{
		if (value > other)
		{
			throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be greater than the allowed value.");
		}
	}
};
