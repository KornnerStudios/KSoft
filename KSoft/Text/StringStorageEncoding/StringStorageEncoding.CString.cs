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

	/// <summary>Read an unfixed single-byte CString from a binary stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	void ReadCStringSingleByte(/*System.IO.BinaryReader*/IO.EndianReader s, System.IO.MemoryStream ms)
	{
		byte character;
		while ((character = s.ReadByte()) != 0)
		{
			ms.WriteByte(character);
		}
	}

	/// <summary>Read an unfixed single-byte CString from a bitstream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	void ReadCStringSingleByte(IO.BitStream s, System.IO.MemoryStream ms, int maxLength)
	{
		byte character;
		if (maxLength > 0)
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
		else
		{
			while ((character = s.ReadByte()) != 0)
			{
				ms.WriteByte(character);
			}
		}
	}

	/// <summary>Read an unfixed multi-byte CString from an endian stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	void ReadCStringMultiByte(IO.EndianReader s, System.IO.MemoryStream ms)
	{
		Span<byte> characters = stackalloc byte[mNullCharacterSize];
		while (true)
		{
			s.BaseStream.ReadExactly(characters);
			if (IsNullStorageUnit(characters))
				break;

			ms.Write(characters);
		}
	}

	/// <summary>Read an unfixed multi-byte CString from a bitstream</summary>
	/// <param name="s">Bitstream to read from</param>
	/// <param name="ms">Stream to write the character's bytes to</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	void ReadCStringMultiByte(IO.BitStream s, System.IO.MemoryStream ms, int maxLength)
	{
		if (maxLength > 0)
		{
			int x = 0;
			Span<byte> characters = stackalloc byte[mNullCharacterSize];
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
		else
		{
			Span<byte> characters = stackalloc byte[mNullCharacterSize];
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
	}

	/// <summary>Read a CString from an endian stream</summary>
	/// <param name="s">Endian stream to read from</param>
	/// <param name="length">Optional length specification</param>
	/// <param name="actualCount">On return, the payload byte count to decode.</param>
	/// <returns>The character's bytes for the string we're reading</returns>
	byte[] ReadStrCString(IO.EndianReader s, int length, out int actualCount)
	{
		byte[] bytes;

		// the user was nice and saved us some CPU trying to feel around for the null
		// because we don't have a fixed length to speed things up
		if (!mStorage.IsFixedLength && length > 0)
		{
			bytes = ReadPayloadBytes(s, GetMaxCleanByteCount(length));
			Span<byte> terminator = stackalloc byte[mNullCharacterSize];
			s.BaseStream.ReadExactly(terminator);
			ValidateCStringTerminator(terminator);
			actualCount = bytes.Length;
		}
		else if (mStorage.IsFixedLength)
		{
			bytes = ReadPayloadBytes(s, mFixedLengthByteLength);
			actualCount = GetCStringPayloadByteCount(bytes);
		}
		// Scan an unfixed CString when the caller does not know its length.
		else
		{
			using (var ms = new System.IO.MemoryStream(512))
			{
				// The N-byte methods take care of reading past the
				// null character, no need to do it in this case.
				if (mNullCharacterSize == 1)	{ ReadCStringSingleByte(s, ms); }
				else							{ ReadCStringMultiByte(s, ms); }

				bytes = ms.GetBuffer();
				actualCount = checked((int)ms.Length);
			}
		}

		return bytes;
	}

	/// <summary>Read a CString from a bitstream</summary>
	/// <param name="s">Bitstream to read from</param>
	/// <param name="length">Optional length specification</param>
	/// <param name="actualCount">On return, the payload byte count to decode.</param>
	/// <param name="maxLength">Optional maximum length of this specific string</param>
	/// <returns>The character's bytes for the string we're reading</returns>
	byte[] ReadStrCString(IO.BitStream s, int length, out int actualCount, int maxLength)
	{
		byte[] bytes;

		// the user was nice and saved us some CPU trying to feel around for the null
		// because we don't have a fixed length to speed things up
		if (!mStorage.IsFixedLength && length > 0)
		{
			bytes = s.ReadBytes(GetMaxCleanByteCount(length));
			Span<byte> terminator = stackalloc byte[mNullCharacterSize];
			s.Read(terminator);
			ValidateCStringTerminator(terminator);
			actualCount = bytes.Length;
		}
		else if (mStorage.IsFixedLength)
		{
			bytes = s.ReadBytes(mFixedLengthByteLength);
			actualCount = GetCStringPayloadByteCount(bytes, maxLength);
		}
		// Scan an unfixed CString when the caller does not know its length.
		else
		{
			using (var ms = new System.IO.MemoryStream(512))
			{
				// The N-byte methods take care of reading past the
				// null character, no need to do it in this case.
				if (mNullCharacterSize == 1)	{ ReadCStringSingleByte(s, ms, maxLength); }
				else							{ ReadCStringMultiByte(s, ms, maxLength); }

				bytes = ms.GetBuffer();
				actualCount = checked((int)ms.Length);
			}
		}

		return bytes;
	}
}
