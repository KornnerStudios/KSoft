using System;
using KSoft.Memory.Strings;

namespace KSoft.Text;

partial class StringStorageEncoding
{
	/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.CharArray"/> string</summary>
	/// <param name="byteCount">Base characters byte count</param>
	/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.CharArray"/> string</returns>
	static int CalcByteCountCharArray(int byteCount)
	{
		return byteCount;
	}

	/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.CharArray"/> string</summary>
	/// <param name="byteCount">Raw string's byte count</param>
	/// <returns>Byte count of the actual string data to be transformed into characters</returns>
	static int CalcCharByteCountCharArray(int byteCount)	{ return byteCount; }

	static int ReadStrCharArrayGetRealCountSingleByte(ReadOnlySpan<byte> bytes)
	{
		if (bytes[bytes.Length-1] == 0) // padded string case
		{
			// find the first last index which isn't null
			for (int x = bytes.Length - 2; x >= 0; x--)
			{
				if (bytes[x] != 0)
				{
					return x + 1;
				}
			}

			return 0; // wtf! no characters, not cool, what a waste
		}
		else
		{
			return TypeExtensions.kNone; // complete string case
		}
	}

	int GetCharArraySuffixByteCount(ReadOnlySpan<byte> bytes)
	{
		ReadOnlySpan<byte> unit = bytes[^mNullCharacterSize..];
		if (IsNullStorageUnit(unit)) // padded string case
		{
			// find the first last index which isn't null
			for (int x = bytes.Length - (mNullCharacterSize * 2); x >= 0; x -= mNullCharacterSize)
			{
				unit = bytes.Slice(x, mNullCharacterSize);
				if (!IsNullStorageUnit(unit))
				{
					return x+mNullCharacterSize;
				}
			}

			return 0; // wtf! no characters, not cool, what a waste
		}
		else
		{
			return TypeExtensions.kNone; // complete string case
		}
	}

	string ReadStrCharArray(IO.EndianReader s, int length)
	{
		int byteCount = mStorage.IsFixedLength
			? mFixedLengthByteLength
			: GetMaxCleanByteCount(length);
		return ReadKnownPayload(s, byteCount, KnownPayloadKind.CharArray);
	}

	string ReadStrCharArray(IO.BitStream s, int length)
	{
		int byteCount = mStorage.IsFixedLength
			? mFixedLengthByteLength
			: GetMaxCleanByteCount(length);
		return ReadKnownPayload(s, byteCount, KnownPayloadKind.CharArray);
	}
}
