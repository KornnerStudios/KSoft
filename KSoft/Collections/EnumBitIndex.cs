using System;

namespace KSoft.Collections;

internal static class EnumBitIndex
{
	internal static int ToIndex<TEnum>(TEnum bit, int length, string paramName)
		where TEnum : struct, Enum
	{
		// Negative values also compare out of range after the full-width unsigned conversion.
		ulong index = Reflection.EnumValue<TEnum>.ToUInt64(bit);
		if (index >= (ulong)length)
		{
			throw new ArgumentOutOfRangeException(paramName, bit, "Enum member is out of range for indexing");
		}

		return (int)index;
	}
}
