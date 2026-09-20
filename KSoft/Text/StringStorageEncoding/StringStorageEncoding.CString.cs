using System;
using System.IO;
using KSoft.Memory.Strings;

namespace KSoft.Text;

partial class StringStorageEncoding
{
	/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.CString"/> string</summary>
	/// <param name="byteCount">Base characters byte count</param>
	/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.CString"/> string</returns>
	int CalcByteCountCString(int byteCount)
	{
		return checked(byteCount + mNullCharacterSize);
	}

	/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.CString"/> string</summary>
	/// <param name="byteCount">Raw string's byte count</param>
	/// <returns>Byte count of the actual string data to be transformed into characters</returns>
	int CalcCharByteCountCString(int byteCount)				{ return byteCount - mNullCharacterSize; }

	int EncodeCStringTerminator(Span<byte> bytes)
	{
		bytes[..mNullCharacterSize].Clear();

		return mNullCharacterSize; // number of bytes written into [bytes]
	}

	static void ValidateCStringTerminator(ReadOnlySpan<byte> bytes)
	{
		if (!IsNullStorageUnit(bytes))
		{
			throw new InvalidDataException("The CString terminator is missing.");
		}
	}

	int CalcCharByteCountCString(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length < mNullCharacterSize)
		{
			throw new InvalidDataException("The CString terminator is incomplete.");
		}
		int payloadByteCount = CalcCharByteCountCString(bytes.Length);
		ValidateCStringTerminator(bytes[payloadByteCount..]);
		return payloadByteCount;
	}

	int GetCStringPayloadByteCount(ReadOnlySpan<byte> bytes, int maxLength = -1)
	{
		for (int offset = 0; offset < bytes.Length; offset += mNullCharacterSize)
		{
			if (IsNullStorageUnit(bytes.Slice(offset, mNullCharacterSize)))
			{
				if (maxLength > 0 && offset / mNullCharacterSize > maxLength)
				{
					throw new InvalidDataException("The CString exceeds its maximum character count.");
				}
				return offset;
			}
		}
		throw new InvalidDataException("The fixed CString field has no null terminator.");
	}

	void CopyFixedCStringPayload(ReadOnlySpan<byte> bytes, MemoryStream destination, int maxLength = -1)
	{
		int payloadByteCount = GetCStringPayloadByteCount(bytes, maxLength);
		destination.Write(bytes[..payloadByteCount]);
	}

	/// <summary>Read a single-byte CString from an binary stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	void ReadCStringSingleByte(/*System.IO.BinaryReader*/IO.EndianReader s, System.IO.MemoryStream ms)
	{
		byte character;
		if (!mStorage.IsFixedLength)
		{
			while ((character = s.ReadByte()) != 0)
			{
				ms.WriteByte(character);
			}
		}
		else
		{
			byte[] characters = ReadPayloadBytes(s, mFixedLengthByteLength);
			CopyFixedCStringPayload(characters, ms);
		}
	}

	/// <summary>Read a single-byte CString from a bitstream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	void ReadCStringSingleByte(IO.BitStream s, System.IO.MemoryStream ms, int maxLength)
	{
		byte character;
		if (maxLength > 0 && !mStorage.IsFixedLength)
		{
			int x = 0;
			while ((character = s.ReadByte()) != 0 && ++x <= maxLength)
			{
				ms.WriteByte(character);
			}
			if (character != 0)
			{
				throw new InvalidDataException("The CString exceeds its maximum character count.");
			}
		}
		else if (!mStorage.IsFixedLength)
		{
			while ((character = s.ReadByte()) != 0)
			{
				ms.WriteByte(character);
			}
		}
		else
		{
			byte[] characters = s.ReadBytes(mFixedLengthByteLength);
			CopyFixedCStringPayload(characters, ms, maxLength);
		}
	}

	/// <summary>Read a multi-byte CString from an endian stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	void ReadCStringMultiByte(IO.EndianReader s, System.IO.MemoryStream ms)
	{
		if (!mStorage.IsFixedLength)
		{
			Span<byte> characters = stackalloc byte[mNullCharacterSize];
			characters.Clear();
			while (true)
			{
				s.BaseStream.ReadExactly(characters);
				if (IsNullStorageUnit(characters))
					break;

				ms.Write(characters);
			}
		}
		else
		{
			byte[] characters = ReadPayloadBytes(s, mFixedLengthByteLength);
			CopyFixedCStringPayload(characters, ms);
		}
	}

	/// <summary>Read a multi-byte CString from an endian stream</summary>
	/// <param name="s">Bitstream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	void ReadCStringMultiByte(IO.BitStream s, System.IO.MemoryStream ms, int maxLength)
	{
		if (maxLength > 0 && !mStorage.IsFixedLength)
		{
			int x = 0;
			Span<byte> characters = stackalloc byte[mNullCharacterSize];
			characters.Clear();
			while (true)
			{
				s.Read(characters);
				if (IsNullStorageUnit(characters))
				{
					break;
				}
				if (++x > maxLength)
				{
					throw new InvalidDataException("The CString exceeds its maximum character count.");
				}

				ms.Write(characters);
			}
		}
		else if (!mStorage.IsFixedLength)
		{
			Span<byte> characters = stackalloc byte[mNullCharacterSize];
			characters.Clear();
			while (true)
			{
				s.Read(characters);
				if (IsNullStorageUnit(characters))
				{
					break;
				}

				ms.Write(characters);
			}
		}
		else
		{
			byte[] characters = s.ReadBytes(mFixedLengthByteLength);
			CopyFixedCStringPayload(characters, ms, maxLength);
		}
	}

	/// <summary>Read a CString from an endian stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="length">Optional length specification</param>
	/// <param name="actualCount">On return, the actual character byte count, or -1 if all bytes are valid</param>
	/// <returns>The character's bytes for the string we're reading</returns>
	byte[] ReadStrCString(IO.EndianReader s, int length, out int actualCount)
	{
		byte[] bytes;

		actualCount = TypeExtensions.kNone; // complete string case

		// the user was nice and saved us some CPU trying to feel around for the null
		// because we don't have a fixed length to speed things up
		if (!mStorage.IsFixedLength && length > 0)
		{
			bytes = ReadPayloadBytes(s, GetMaxCleanByteCount(length));
			Span<byte> terminator = stackalloc byte[mNullCharacterSize];
			s.BaseStream.ReadExactly(terminator);
			ValidateCStringTerminator(terminator);
		}
		// NOT NICE: figure out the length ourselves. Or maybe we're a fixed length CString...
		// in which case we'll ignore anything the user tried to tell us about the length
		else
		{
			using (var ms = new System.IO.MemoryStream(!mStorage.IsFixedLength ? 512 : mStorage.FixedLength))
			{
				// The N-byte methods take care of reading past the
				// null character, no need to do it in this case.
				if (mNullCharacterSize == 1)	{ ReadCStringSingleByte(s, ms); }
				else							{ ReadCStringMultiByte(s, ms); }

				// We use ToArray instead of GetArray so all of [ms] can theoretically be disposed of
				bytes = ms.ToArray();
			}
		}

		return bytes;
	}

	/// <summary>Read a CString from a bitstream</summary>
	/// <param name="s">Bitstream to read from</param>
	/// <param name="length">Optional length specification</param>
	/// <param name="actualCount">On return, the actual character byte count, or -1 if all bytes are valid</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	/// <returns>The character's bytes for the string we're reading</returns>
	byte[] ReadStrCString(IO.BitStream s, int length, out int actualCount, int maxLength)
	{
		byte[] bytes;

		actualCount = TypeExtensions.kNone; // complete string case

		// the user was nice and saved us some CPU trying to feel around for the null
		// because we don't have a fixed length to speed things up
		if (!mStorage.IsFixedLength && length > 0)
		{
			bytes = s.ReadBytes(GetMaxCleanByteCount(length));
			Span<byte> terminator = stackalloc byte[mNullCharacterSize];
			s.Read(terminator);
			ValidateCStringTerminator(terminator);
		}
		// NOT NICE: figure out the length ourselves. Or maybe we're a fixed length CString...
		// in which case we'll ignore anything the user tried to tell us about the length
		else
		{
			using (var ms = new System.IO.MemoryStream(!mStorage.IsFixedLength ? 512 : mStorage.FixedLength))
			{
				// The N-byte methods take care of reading past the
				// null character, no need to do it in this case.
				if (mNullCharacterSize == 1)	{ ReadCStringSingleByte(s, ms, maxLength); }
				else							{ ReadCStringMultiByte(s, ms, maxLength); }

				// We use ToArray instead of GetArray so all of [ms] can theoretically be disposed of
				bytes = ms.ToArray();
			}
		}

		return bytes;
	}
}
