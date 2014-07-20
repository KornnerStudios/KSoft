﻿using System;
using System.Collections.Generic;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Text
{
	using Memory.Strings;

	partial class StringStorageEncoding
	{
		#region CalculateCharByteCount
		/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.CString"/> string</summary>
		/// <param name="byteCount">Raw string's byte count</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		int CalcCharByteCountCString(int byteCount)				{ return byteCount - mNullCharacterSize; }
		#region Pascal
		/// <summary>Calculate the estimated character byte count of a raw <see cref="StringStorageLengthPrefix.Int7"/> string</summary>
		/// <param name="byteCount">Raw string's byte count</param>
		/// <returns>Estimated byte count of the actual string data to be transformed into characters</returns>
		int CalcCharByteCountPascalInt7(int byteCount)
		{
			// HACK: It is possible the underlying encoding doesn't actually have a 
			// fixed character size for one, some, or all of its characters
			int char_count = byteCount / mNullCharacterSize;

				 if ((char_count - 1) <= Bitwise.Encoded7BitInt.kMaxValue1Bytes)	return byteCount - 1;
			else if ((char_count - 2) <= Bitwise.Encoded7BitInt.kMaxValue2Bytes)	return byteCount - 2;
			else if ((char_count - 3) <= Bitwise.Encoded7BitInt.kMaxValue3Bytes)	return byteCount - 3;
			else if ((char_count - 4) <= Bitwise.Encoded7BitInt.kMaxValue4Bytes)	return byteCount - 4;
			else
				throw new Debug.UnreachableException(char_count.ToString());
		}
		/// <summary>Calculate the estimated character byte count of a raw <see cref="StringStorageType.Pascal"/> string</summary>
		/// <param name="byteCount">Raw string's byte count</param>
		/// <returns>Estimated byte count of the actual string data to be transformed into characters</returns>
		int CalcCharByteCountPascal(int byteCount)
		{
			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int7: return CalcCharByteCountPascalInt7(byteCount);
				case StringStorageLengthPrefix.Int8: return byteCount - sizeof(byte);
				case StringStorageLengthPrefix.Int16:return byteCount - sizeof(short);
				case StringStorageLengthPrefix.Int32:return byteCount - sizeof(int);
				default:
					throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
			}
		}
		#endregion
		/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.CharArray"/> string</summary>
		/// <param name="byteCount">Raw string's byte count</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		static int CalcCharByteCountCharArray(int byteCount)	{ return byteCount; }
		/// <summary>Calculate the true character byte count of a raw string's characters when decoding them</summary>
		/// <param name="byteCount">Raw string's byte count</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		/// <remarks>For <see cref="StringStorageType.Pascal"/> cases, the value is a best-guess estimate</remarks>
		int CalculateCharByteCount(int byteCount)
		{
			switch (mStorage.Type)
			{
				case StringStorageType.CString:		byteCount = CalcCharByteCountCString(byteCount); break;
				case StringStorageType.Pascal:		byteCount = CalcCharByteCountPascal(byteCount); break;
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	/*byteCount = CalcCharByteCountCharArray(byteCount);*/ break;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}

			return byteCount;
		}
		#region Pascal
		/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageLengthPrefix.Int7"/> string</summary>
		/// <param name="buffer">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">
		/// In: The index of the first byte to decode.
		/// Out: The index of the first character's byte(s).
		/// </param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		static int CalcCharByteCountPascalInt7(byte[] buffer, ref int byteIndex, int byteCount)
		{
			return Bitwise.Encoded7BitInt.Read(buffer, byteIndex, byteCount, out byteIndex);
		}
		/// <summary>Calculate the true character byte count of a raw <see cref="StringStorageType.Pascal"/> string</summary>
		/// <param name="buffer">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">
		/// In: The index of the first byte to decode.
		/// Out: The index of the first character's byte(s).
		/// </param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		int CalcCharByteCountPascal(byte[] buffer, ref int byteIndex, int byteCount)
		{
			int result = 0;

			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int7: result = CalcCharByteCountPascalInt7(buffer, ref byteIndex, byteCount); break;
				case StringStorageLengthPrefix.Int8: result = buffer[byteIndex]; byteIndex += sizeof(byte); break;
				case StringStorageLengthPrefix.Int16:result = BitConverter.ToInt16(buffer, byteIndex); byteIndex += sizeof(short); break;
				case StringStorageLengthPrefix.Int32:result = BitConverter.ToInt32(buffer, byteIndex); byteIndex += sizeof(int); break;
				default:
					throw new Debug.UnreachableException(mStorage.LengthPrefix.ToString());
			}

			return result;
		}
		#endregion
		/// <summary>Calculate the true character byte count of a raw string's characters when decoding them</summary>
		/// <param name="buffer">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">
		/// In: The index of the first byte to decode.
		/// Out: The index of the first character's byte(s).
		/// </param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>Byte count of the actual string data to be transformed into characters</returns>
		int CalculateCharByteCount(byte[] buffer, ref int byteIndex, int byteCount)
		{
			switch (mStorage.Type)
			{
				case StringStorageType.CString:		byteCount = CalcCharByteCountCString(byteCount); break;
				case StringStorageType.Pascal:		byteCount = CalcCharByteCountPascal(buffer, ref byteIndex, byteCount); break;
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	/*byteCount = CalcCharByteCountCharArray(byteCount);*/ break;
				default: throw new Debug.UnreachableException();
			}

			return byteCount;
		}
		#endregion
		
		/// <summary>Converts a sequence of encoded bytes into a set of characters.</summary>
		class Decoder : System.Text.Decoder
		{
			StringStorageEncoding mEncoding;
			System.Text.Decoder mDec;
			public Decoder(StringStorageEncoding enc) { mEncoding = enc; mDec = enc.mBaseEncoding.GetDecoder(); }

			/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array</summary>
			/// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
			/// <param name="index">The index of the first byte to decode.</param>
			/// <param name="count">The number of bytes to decode.</param>
			/// <returns>The number of characters produced by decoding the specified sequence of bytes and any bytes in the internal buffer.</returns>
			/// <remarks>This method does not affect the state of the decoder.</remarks>
			/// <seealso cref="System.Text.Decoder.GetCharCount(Byte[], Int32, Int32)"/>
			public override int GetCharCount(byte[] bytes, int index, int count)
			{
				count = mEncoding.CalculateCharByteCount(bytes, ref index, count); // Remove our String Storage calculations

				int char_count = mDec.GetCharCount(bytes, index, count);

				return char_count;
			}
			/// <summary>
			/// Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array. 
			/// A parameter indicates whether to clear the internal state of the decoder after the calculation.
			/// </summary>
			/// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
			/// <param name="index">The index of the first byte to decode.</param>
			/// <param name="count">The number of bytes to decode.</param>
			/// <param name="flush"><b>true</b> to simulate clearing the internal state of the encoder after the calculation; otherwise, <b>false</b></param>
			/// <returns>The number of characters produced by decoding the specified sequence of bytes and any bytes in the internal buffer</returns>
			/// <seealso cref="System.Text.Decoder.GetCharCount(Byte[], Int32, Int32, Boolean)"/>
			public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
			{
				count = mEncoding.CalculateCharByteCount(bytes, ref index, count); // Remove our String Storage calculations

				int char_count = mDec.GetCharCount(bytes, index, count, mEncoding.DontAlwaysFlush ? flush : true);

				return char_count;
			}

			/// <summary>
			/// Decodes a sequence of bytes from the specified byte array and any bytes in the internal buffer into the specified character array.
			/// </summary>
			/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
			/// <param name="byteIndex">The index of the first byte to decode</param>
			/// <param name="byteCount">The number of bytes to decode</param>
			/// <param name="chars">The character array to contain the resulting set of characters</param>
			/// <param name="charIndex">The index at which to start writing the resulting set of characters</param>
			/// <returns>The actual number of characters written into <paramref name="chars"/></returns>
			/// <seealso cref="System.Text.Decoder.GetChars(Byte[], Int32, Int32, Char[], Int32)"/>
			public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
			{
				byteCount = mEncoding.CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

				int chars_written = mDec.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

				return chars_written;
			}
			/// <summary>
			/// Decodes a sequence of bytes from the specified byte array and any bytes in the internal buffer into the specified character array. 
			/// A parameter indicates whether to clear the internal state of the decoder after the conversion.
			/// </summary>
			/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
			/// <param name="byteIndex">The index of the first byte to decode</param>
			/// <param name="byteCount">The number of bytes to decode</param>
			/// <param name="chars">The character array to contain the resulting set of characters</param>
			/// <param name="charIndex">The index at which to start writing the resulting set of characters</param>
			/// <param name="flush"><b>true</b> to clear the internal state of the decoder after the conversion; otherwise, <b>false</b></param>
			/// <returns>The actual number of characters written into the <paramref name="chars"/> parameter</returns>
			/// <seealso cref="System.Text.Decoder.GetChars(Byte[], Int32, Int32, Char[], Int32, Boolean)"/>
			public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
			{
				byteCount = mEncoding.CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

				int chars_written = mDec.GetChars(bytes, byteIndex, byteCount, chars, charIndex, mEncoding.DontAlwaysFlush ? flush : true);

				return chars_written;
			}

			/// <summary>Sets the decoder back to its initial state</summary>
			public override void Reset()	{ mDec.Reset(); }
		};

		#region ReadString
		/// <summary>Determine if the multi-byte character is a null character or not</summary>
		/// <param name="byteOrder">Byte order of the character data</param>
		/// <param name="characters">Buffer containing the character's bytes</param>
		/// <param name="offset">offset to start the null comparison at</param>
		/// <returns>True if <paramref name="characters"/> is null; all zeros</returns>
		/// <remarks>
		/// If <paramref name="byteOrder"/> is different from <see cref="Storage.ByteOrder"/>, this will 
		/// byte-swap the bytes before returning
		/// </remarks>
		bool ReadStringMultiByteIsNull(Shell.EndianFormat byteOrder, byte[] characters, int offset)
		{
			bool result = false;

			if (mNullCharacterSize == sizeof(uint))
			{
				if (byteOrder != mStorage.ByteOrder)
					Bitwise.ByteSwap.SwapInt32(characters, offset);

				result =  characters[offset+3] == 0;
				result &= characters[offset+2] == 0;
				result &= characters[offset+1] == 0;
				result &= characters[offset  ] == 0;
			}
			else if (mNullCharacterSize == sizeof(ushort))
			{
				if (byteOrder != mStorage.ByteOrder)
					Bitwise.ByteSwap.SwapInt16(characters, offset);

				result =  characters[offset+1] == 0;
				result &= characters[offset  ] == 0;
			}
			else
				throw new Debug.UnreachableException(mNullCharacterSize.ToString());

			return result;
		}

		#region CString
		/// <summary>Read a single-byte CString from an binary stream</summary>
		/// <param name="s">Endian stream to read from</param>
		/// <param name="ms">Stream to write the character's bytes to</param>
		void ReadCStringSingleByte(System.IO.BinaryReader s, System.IO.MemoryStream ms)
		{
			byte character;
			if (!mStorage.IsFixedLength)
				while((character = s.ReadByte()) != 0)
					ms.WriteByte(character);
			else
			{
				byte[] characters = s.ReadBytes(mFixedLengthByteLength);

				int x;
				for (x = 0; x < characters.Length; x++)
					if (characters[x] == 0)
						break;

				ms.Write(characters, 0, x);
			}
		}
		/// <summary>Read a single-byte CString from a bitstream</summary>
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
					ms.WriteByte(character);
			}
			else if (!mStorage.IsFixedLength)
				while((character = s.ReadByte()) != 0)
					ms.WriteByte(character);
			else
			{
				byte[] characters = s.ReadBytes(mFixedLengthByteLength);

				int x;
				for (x = 0; x < characters.Length; x++)
					if (characters[x] == 0)
						break;

				ms.Write(characters, 0, x);
			}
		}
		/// <summary>Read a multi-byte CString from an endian stream</summary>
		/// <param name="s">Endian stream to read from</param>
		/// <param name="ms">Stream to write the character's bytes to</param>
		void ReadCStringMultiByte(IO.EndianReader s, System.IO.MemoryStream ms)
		{
			byte[] characters;
			if (!mStorage.IsFixedLength)
			{
				characters = new byte[mNullCharacterSize];
				while (!ReadStringMultiByteIsNull(s.ByteOrder, s.Read(characters), 0))
					ms.Write(characters, 0, characters.Length);
			}
			else
			{
				characters = s.ReadBytes(mFixedLengthByteLength);

				int x;
				for (x = 0; x < characters.Length - mNullCharacterSize; x += mNullCharacterSize)
					if (ReadStringMultiByteIsNull(s.ByteOrder, characters, x))
						break;

				ms.Write(characters, 0, x);
			}
		}
		/// <summary>Read a multi-byte CString from an endian stream</summary>
		/// <param name="s">Bitstream to read from</param>
		/// <param name="ms">Stream to write the character's bytes to</param>
		/// <param name="maxLength">Optional maximum length of this specific string</param>
		void ReadCStringMultiByte(IO.BitStream s, System.IO.MemoryStream ms, int maxLength)
		{
			byte[] characters;
			if (maxLength > 0)
			{
				int x = 0;
				characters = new byte[mNullCharacterSize];
				while (!ReadStringMultiByteIsNull(mStorage.ByteOrder, s.Read(characters), 0) && ++x <= maxLength)
					ms.Write(characters, 0, characters.Length);
			}
			else if (!mStorage.IsFixedLength)
			{
				characters = new byte[mNullCharacterSize];
				while (!ReadStringMultiByteIsNull(mStorage.ByteOrder, s.Read(characters), 0))
					ms.Write(characters, 0, characters.Length);
			}
			else
			{
				characters = s.ReadBytes(mFixedLengthByteLength);

				int x;
				for (x = 0; x < characters.Length - mNullCharacterSize; x += mNullCharacterSize)
					if (ReadStringMultiByteIsNull(mStorage.ByteOrder, characters, x))
						break;

				ms.Write(characters, 0, x);
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
				bytes = s.ReadBytes(GetMaxCleanByteCount(length));
				s.Seek(mNullCharacterSize, System.IO.SeekOrigin.Current);
			}
			// FUCK: figure out the length ourselves. Or maybe we're a fixed length CString...
			// in which case we'll ignore anything the user tried to tell us about the length
			else
			{
				using (var ms = new System.IO.MemoryStream(!mStorage.IsFixedLength ? 512 : mStorage.FixedLength))
				{
					// The N-byte methods take care of reading past the 
					// null character, no need to do it in this case.
					if (mNullCharacterSize == 1)	ReadCStringSingleByte(s, ms);
					else							ReadCStringMultiByte(s, ms);

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
				s.ReadUInt32(mNullCharacterSize * Bits.kByteBitCount);
			}
			// FUCK: figure out the length ourselves. Or maybe we're a fixed length CString...
			// in which case we'll ignore anything the user tried to tell us about the length
			else
			{
				using (var ms = new System.IO.MemoryStream(!mStorage.IsFixedLength ? 512 : mStorage.FixedLength))
				{
					// The N-byte methods take care of reading past the 
					// null character, no need to do it in this case.
					if (mNullCharacterSize == 1)	ReadCStringSingleByte(s, ms, maxLength);
					else							ReadCStringMultiByte(s, ms, maxLength);

					// We use ToArray instead of GetArray so all of [ms] can theoretically be disposed of
					bytes = ms.ToArray();
				}
			}

			return bytes;
		}
		#endregion
		#region Pascal
		byte[] ReadStrPascal(IO.EndianReader s, out int actualCount)
		{
			actualCount = TypeExtensions.kNone;

			int length;
			// One would think that the length prefix would be of the same endian as the stream, but just in case...
			using(s.BeginEndianSwitch(mStorage.ByteOrder))
				switch (mStorage.LengthPrefix)
				{
					case StringStorageLengthPrefix.Int7: length = s.Read7BitEncodedInt(); break;
					case StringStorageLengthPrefix.Int8: length = s.ReadByte(); break;
					case StringStorageLengthPrefix.Int16:length = s.ReadInt16(); break;
					case StringStorageLengthPrefix.Int32:length = s.ReadInt32(); break;
					default: throw new Debug.UnreachableException();
				}

			return s.ReadBytes(GetMaxCleanByteCount(length));
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

			int length;
			switch (mStorage.LengthPrefix)
			{
				case StringStorageLengthPrefix.Int7: throw new NotSupportedException();
				case StringStorageLengthPrefix.Int8: length = s.ReadByte(prefixBitLength); break;
				case StringStorageLengthPrefix.Int16:length = s.ReadInt16(prefixBitLength); break;
				case StringStorageLengthPrefix.Int32:length = s.ReadInt32(prefixBitLength); break;
				default: throw new Debug.UnreachableException();
			}

			return s.ReadBytes(GetMaxCleanByteCount(length));
		}
		#endregion
		#region CharArray
		int ReadStrCharArrayGetRealCountSingleByte(byte[] bytes)
		{
			if(bytes[bytes.Length-1] == 0) // padded string case
			{
				// find the first last index which isn't null
				for (int x = bytes.Length - 2; x > 0; x--)
					if (bytes[x] != 0) return x+1;

				return 0; // wtf! no characters, not cool, what a waste
			}
			else
				return TypeExtensions.kNone; // complete string case
		}
		int ReadStrCharArrayGetRealCountMultiByte(Shell.EndianFormat byteOrder, byte[] bytes)
		{
			if (ReadStringMultiByteIsNull(byteOrder, bytes, bytes.Length - mNullCharacterSize)) // padded string case
			{
				// find the first last index which isn't null
				for (int x = bytes.Length - (mNullCharacterSize * 2); x > mNullCharacterSize; x -= mNullCharacterSize)
					if (!ReadStringMultiByteIsNull(byteOrder, bytes, x)) return x+mNullCharacterSize;

				return 0; // wtf! no characters, not cool, what a waste
			}
			else
				return TypeExtensions.kNone; // complete string case
		}
		byte[] ReadStrCharArray(IO.EndianReader s, int length, out int actualCount)
		{
			byte[] bytes = s.ReadBytes(mStorage.IsFixedLength
				? mFixedLengthByteLength
				: GetMaxCleanByteCount(length));

			actualCount = mNullCharacterSize == sizeof(byte)
				? ReadStrCharArrayGetRealCountSingleByte(bytes)
				: ReadStrCharArrayGetRealCountMultiByte(s.ByteOrder, bytes);

			return bytes;
		}
		byte[] ReadStrCharArray(IO.BitStream s, int length, out int actualCount)
		{
			byte[] bytes = s.ReadBytes(mStorage.IsFixedLength ?
				mFixedLengthByteLength :
				GetMaxCleanByteCount(length));

			actualCount = mNullCharacterSize == sizeof(byte)
				? ReadStrCharArrayGetRealCountSingleByte(bytes)
				: ReadStrCharArrayGetRealCountMultiByte(mStorage.ByteOrder, bytes);

			return bytes;
		}
		#endregion
		/// <summary>Read a string from an endian stream using <see cref="Storage"/>'s specifications</summary>
		/// <param name="s">Endian stream to read from</param>
		/// <param name="length">Optional length specification</param>
		/// <returns></returns>
		internal string ReadString(IO.EndianReader s, int length)
		{
			Contract.Requires(s != null);

			if (length < 0) // Not <= because FixedLength might just be zero itself, resulting in a redundant expression
				length = mStorage.FixedLength;

			byte[] bytes = null;
			int actual_count = 0;
			switch (mStorage.Type)
			{
				// Type streamers should set actual_count to -1 if we're to assume all the bytes are characters.
				// Otherwise, set actual_count to a byte count for padded string cases (where we don't want to 
				// include null characters in the result string)

				case StringStorageType.CString:		bytes = ReadStrCString(s, length, out actual_count); break;
				case StringStorageType.Pascal:		bytes = ReadStrPascal(s, out actual_count); break;
				case StringStorageType.CharArray:	bytes = ReadStrCharArray(s, length, out actual_count); break;
				default:							throw new Debug.UnreachableException();
			}

			return new string(actual_count != -1
				? mBaseEncoding.GetChars(bytes, 0, actual_count)// for padded string cases
				: mBaseEncoding.GetChars(bytes));				// for complete string cases
		}
		/// <summary>Read a string from an bitstream using <see cref="Storage"/>'s specifications</summary>
		/// <param name="s">Endian stream to read from</param>
		/// <param name="length">Optional length specification</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string</param>
		/// <param name="prefixBitLength">Pascal only: Number of bits in the prefix count</param>
		/// <returns></returns>
		internal string ReadString(IO.BitStream s, int length, int maxLength = -1, int prefixBitLength = -1)
		{
			Contract.Requires(s != null);

			if (length < 0) // Not <= because FixedLength might just be zero itself, resulting in a redundant expression
				length = mStorage.FixedLength;

			byte[] bytes = null;
			int actual_count = 0;
			switch (mStorage.Type)
			{
				// Type streamers should set actual_count to -1 if we're to assume all the bytes are characters.
				// Otherwise, set actual_count to a byte count for padded string cases (where we don't want to 
				// include null characters in the result string)

				case StringStorageType.CString:		bytes = ReadStrCString(s, length, out actual_count, maxLength); break;
				case StringStorageType.Pascal:		bytes = ReadStrPascal(s, out actual_count, prefixBitLength); break;
				case StringStorageType.CharArray:	bytes = ReadStrCharArray(s, length, out actual_count); break;
				default:							throw new Debug.UnreachableException();
			}

			return new string(actual_count != -1
				? mBaseEncoding.GetChars(bytes, 0, actual_count)// for padded string cases
				: mBaseEncoding.GetChars(bytes));				// for complete string cases
		}
		#endregion
	};
}