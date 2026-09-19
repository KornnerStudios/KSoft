using System;
using System.Buffers.Binary;
using KSoft.Memory.Strings;

namespace KSoft.Text;

partial class StringStorageEncoding
{
	int MaxPascalLength => mStorage.LengthPrefix switch
	{
		StringStorageLengthPrefix.Int7 => int.MaxValue,
		StringStorageLengthPrefix.Int8 => byte.MaxValue,
		StringStorageLengthPrefix.Int16 => short.MaxValue,
		StringStorageLengthPrefix.Int32 => int.MaxValue,
		_ => throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString()),
	};

	int GetPascalPrefixByteCount(int charCount)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(charCount);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(charCount, MaxPascalLength);
		return mStorage.LengthPrefix switch
		{
			StringStorageLengthPrefix.Int7 => Bitwise.Encoded7BitInt.CalculateSize(charCount),
			StringStorageLengthPrefix.Int8 => sizeof(byte),
			StringStorageLengthPrefix.Int16 => sizeof(short),
			StringStorageLengthPrefix.Int32 => sizeof(int),
			_ => throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString()),
		};
	}

	int GetPascalPayloadByteCount(int charCount)
	{
		GetPascalPrefixByteCount(charCount);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(charCount, int.MaxValue / mNullCharacterSize);
		return charCount * mNullCharacterSize;
	}

	void ValidatePascalBitStreamPrefix(int prefixBitLength)
	{
		if (mStorage.LengthPrefix == StringStorageLengthPrefix.Int7)
		{
			throw new NotSupportedException("BitStream does not support Int7 Pascal prefix reads.");
		}

		int prefixWidth = GetPascalPrefixByteCount(0) * Bits.kByteBitCount;
		if (!prefixBitLength.IsNone())
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(prefixBitLength);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(prefixBitLength, prefixWidth);
			prefixWidth = prefixBitLength;
		}
		if (prefixWidth > Bits.kByteBitCount && mStorage.ByteOrder == Shell.EndianFormat.Little)
		{
			throw new NotSupportedException("BitStream Pascal prefix reads wider than eight bits require big-endian storage.");
		}
	}

	/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.Pascal"/> string</summary>
	/// <param name="byteCount">Base characters byte count</param>
	/// <param name="validateLength">Whether the payload is exact rather than an upper bound.</param>
	/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.Pascal"/> string</returns>
	int CalcByteCountPascal(int byteCount, bool validateLength)
	{
		int charCount = GetSerializedCharacterCount(byteCount);
		if (!validateLength)
		{
			charCount = Math.Min(charCount, MaxPascalLength);
		}
		return checked(byteCount + GetPascalPrefixByteCount(charCount));
	}

	/// <summary>Calculate the estimated character byte count of a raw <see cref="StringStorageLengthPrefix.Int7"/> string</summary>
	/// <param name="byteCount">Raw string's byte count</param>
	/// <returns>Estimated byte count of the actual string data to be transformed into characters</returns>
	int CalcCharByteCountPascalInt7(int byteCount)
	{
		// Without reading the prefix, use the fixed payload-unit width to estimate its size.
		int char_count = byteCount / mNullCharacterSize;

			 if ((char_count - 1) <= Bitwise.Encoded7BitInt.kMaxValue1Bytes)	{ return byteCount - 1; }
		else if ((char_count - 2) <= Bitwise.Encoded7BitInt.kMaxValue2Bytes)	{ return byteCount - 2; }
		else if ((char_count - 3) <= Bitwise.Encoded7BitInt.kMaxValue3Bytes)	{ return byteCount - 3; }
		else if ((char_count - 4) <= Bitwise.Encoded7BitInt.kMaxValue4Bytes)	{ return byteCount - 4; }
		else
		{
			return byteCount - 5;
		}
	}

	/// <summary>Calculate the estimated character byte count of a raw <see cref="StringStorageType.Pascal"/> string</summary>
	/// <param name="byteCount">Raw string's byte count</param>
	/// <returns>Estimated byte count of the actual string data to be transformed into characters</returns>
	int CalcCharByteCountPascal(int byteCount)
	{
		return mStorage.LengthPrefix switch
		{
			StringStorageLengthPrefix.Int7  => CalcCharByteCountPascalInt7(byteCount),
			StringStorageLengthPrefix.Int8  => byteCount - sizeof(byte),
			StringStorageLengthPrefix.Int16 => byteCount - sizeof(short),
			StringStorageLengthPrefix.Int32 => byteCount - sizeof(int),
			_ => throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString()),
		};
	}

	int EncodePascalPrefix(int charCount, Span<byte> bytes)
	{
		GetPascalPrefixByteCount(charCount);
		int prefix_bytes;
		switch (mStorage.LengthPrefix)
		{
			case StringStorageLengthPrefix.Int7:	prefix_bytes = Bitwise.Encoded7BitInt.Write(bytes, charCount); break;
			case StringStorageLengthPrefix.Int8:	bytes[0] = (byte)charCount;
				prefix_bytes = sizeof(byte); break;
			case StringStorageLengthPrefix.Int16:	BitConverter.TryWriteBytes(
														bytes[..sizeof(short)], (short)charCount);
													if (!mStorage.ByteOrder.IsSameAsRuntime())
														bytes[..sizeof(short)].Reverse();
				prefix_bytes = sizeof(short); break;
			case StringStorageLengthPrefix.Int32:	BitConverter.TryWriteBytes(
														bytes[..sizeof(int)], charCount);
													if (!mStorage.ByteOrder.IsSameAsRuntime())
														bytes[..sizeof(int)].Reverse();
				prefix_bytes = sizeof(int); break;
			default:
				throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
		}

		return prefix_bytes;
	}

	/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageLengthPrefix.Int7"/> string</summary>
	/// <param name="buffer">The span containing the sequence of bytes to decode</param>
	/// <param name="byteIndex">
	/// In: The index of the first byte to decode.
	/// Out: The index of the first character's byte(s).
	/// </param>
	/// <param name="byteCount">The number of bytes to decode</param>
	/// <returns>Byte count of the actual string data to be transformed into characters</returns>
	static int CalcCharByteCountPascalInt7(ReadOnlySpan<byte> buffer, ref int byteIndex, int byteCount)
	{
		int startIndex = byteIndex;
		int result = Bitwise.Encoded7BitInt.Read(buffer.Slice(byteIndex, byteCount), out int bytesRead);
		if (bytesRead.IsNone())
		{
			throw new ArgumentOutOfRangeException(nameof(byteCount), "The Int7 Pascal prefix or payload is incomplete or invalid.");
		}
		byteIndex = startIndex + bytesRead;

		return result;
	}

	/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.Pascal"/> string</summary>
	/// <param name="buffer">The span containing the sequence of bytes to decode</param>
	/// <param name="byteIndex">
	/// In: The index of the first byte to decode.
	/// Out: The index of the first character's byte(s).
	/// </param>
	/// <param name="byteCount">The number of bytes to decode</param>
	/// <returns>Byte count of the actual string data to be transformed into characters</returns>
	int CalcCharByteCountPascal(ReadOnlySpan<byte> buffer, ref int byteIndex, int byteCount)
	{
		int startIndex = byteIndex;
		int prefixBytes = GetPascalPrefixByteCount(0);
		if (byteCount < prefixBytes)
		{
			throw new ArgumentOutOfRangeException(nameof(byteCount), "The Pascal prefix is incomplete.");
		}
		ReadOnlySpan<byte> bytes = buffer.Slice(byteIndex, byteCount);
		int result;

		switch (mStorage.LengthPrefix)
		{
			case StringStorageLengthPrefix.Int7: result = CalcCharByteCountPascalInt7(buffer, ref byteIndex, byteCount); break;
			case StringStorageLengthPrefix.Int8: result = buffer[byteIndex]; byteIndex += sizeof(byte); break;
			case StringStorageLengthPrefix.Int16:
				result = mStorage.ByteOrder == Shell.EndianFormat.Big
					? BinaryPrimitives.ReadInt16BigEndian(bytes)
					: BinaryPrimitives.ReadInt16LittleEndian(bytes);
				byteIndex += sizeof(short); break;
			case StringStorageLengthPrefix.Int32:
				result = mStorage.ByteOrder == Shell.EndianFormat.Big
					? BinaryPrimitives.ReadInt32BigEndian(bytes)
					: BinaryPrimitives.ReadInt32LittleEndian(bytes);
				byteIndex += sizeof(int); break;
			default:
				throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
		}

		int payloadBytes = GetPascalPayloadByteCount(result);
		if (payloadBytes > byteCount - (byteIndex - startIndex))
		{
			throw new ArgumentOutOfRangeException(nameof(byteCount), "The Pascal character count exceeds the supplied payload.");
		}
		return payloadBytes;
	}

	byte[] ReadStrPascal(IO.EndianReader s, out int actualCount)
	{
		actualCount = TypeExtensions.kNone;

		int length;
		// One would think that the length prefix would be of the same endian as the stream, but just in case...
		using (s.BeginEndianSwitch(mStorage.ByteOrder))
		{
			length = mStorage.LengthPrefix switch
			{
				StringStorageLengthPrefix.Int7 => s.Read7BitEncodedInt(),
				StringStorageLengthPrefix.Int8 => s.ReadByte(),
				StringStorageLengthPrefix.Int16 => s.ReadInt16(),
				StringStorageLengthPrefix.Int32 => s.ReadInt32(),
				_ => throw new Debug.UnreachableException(),
			};
		}

		return ReadPayloadBytes(s, GetPascalPayloadByteCount(length));
	}

	byte[] ReadStrPascal(IO.BitStream s, out int actualCount, int prefixBitLength)
	{
		actualCount = TypeExtensions.kNone;

		if (prefixBitLength.IsNone())
		{
			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int8: prefixBitLength = Bits.kByteBitCount; break;
				case StringStorageLengthPrefix.Int16:prefixBitLength = Bits.kInt16BitCount; break;
				case StringStorageLengthPrefix.Int32:prefixBitLength = Bits.kInt32BitCount; break;
			}
		}

		int length = mStorage.LengthPrefix switch
		{
			StringStorageLengthPrefix.Int7 => throw new NotSupportedException(),
			StringStorageLengthPrefix.Int8 => s.ReadByte(prefixBitLength),
			StringStorageLengthPrefix.Int16 => s.ReadInt16(prefixBitLength),
			StringStorageLengthPrefix.Int32 => s.ReadInt32(prefixBitLength),
			_ => throw new Debug.UnreachableException(),
		};
		return s.ReadBytes(GetPascalPayloadByteCount(length));
	}
}
