using System;
using System.IO;

namespace KSoft.Text
{
	using Memory.Strings;

	partial class StringStorageEncoding
	{
		static byte[] ReadPayloadBytes(IO.EndianReader s, int byteCount)
		{
			if (s.BaseStream.CanSeek && byteCount > s.BaseStream.Length - s.BaseStream.Position)
			{
				throw new EndOfStreamException("The string payload is incomplete.");
			}
			byte[] bytes = s.ReadBytes(byteCount);
			if (bytes.Length != byteCount)
			{
				throw new EndOfStreamException("The string payload is incomplete.");
			}
			return bytes;
		}

		void ValidateBitStreamRead(int maxLength, int prefixBitLength)
		{
			if (mStorage.WidthType.IsVariableWidth() && maxLength > 0)
			{
				throw new NotSupportedException("A bounded CString bitstream read requires fixed-width storage.");
			}
			if (!mStorage.HasLengthPrefix)
			{
				if (!prefixBitLength.IsNone())
				{
					throw new ArgumentException("Only Pascal storage has a length prefix.", nameof(prefixBitLength));
				}
				return;
			}
			ValidatePascalBitStreamPrefix(prefixBitLength);
		}

		#region CalculateCharByteCount
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
			Verify.Buffers.OffsetAndLengthWithinLength(buffer, byteIndex, byteCount);
			switch (mStorage.Type)
			{
				case StringStorageType.CString:
					byteCount = CalcCharByteCountCString(buffer.AsSpan(byteIndex, byteCount));
					break;
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
			readonly StringStorageEncoding mEncoding;
			readonly System.Text.Decoder mDec;
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
		static bool IsNullStorageUnit(ReadOnlySpan<byte> bytes) => !bytes.ContainsAnyExcept((byte)0);

		/// <summary>Read a string from an endian stream using <see cref="Storage"/>'s specifications</summary>
		/// <param name="s">Endian stream to read from</param>
		/// <param name="length">Optional length specification</param>
		/// <returns></returns>
		internal string ReadString(IO.EndianReader s, int length)
		{
			ArgumentNullException.ThrowIfNull(s);
			Verify.StringStorage.ForStreaming(mStorage, length);

			if (length < 0) // Not <= because FixedLength might just be zero itself, resulting in a redundant expression
			{
				length = mStorage.FixedLength;
			}

			int actual_count;
			byte[] bytes = mStorage.Type switch
			{
				// Type streamers should set actual_count to -1 if we're to assume all the bytes are characters.
				// Otherwise, set actual_count to a byte count for padded string cases (where we don't want to
				// include null characters in the result string)

				StringStorageType.CString	=> ReadStrCString(s, length, out actual_count),
				StringStorageType.Pascal	=> ReadStrPascal(s, out actual_count),
				StringStorageType.CharArray	=> ReadStrCharArray(s, length, out actual_count),
				_ => throw new Debug.UnreachableException(),
			};
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
			ArgumentNullException.ThrowIfNull(s);
			Verify.StringStorage.ForStreaming(mStorage, length);
			ValidateBitStreamRead(maxLength, prefixBitLength);

			if (length < 0) // Not <= because FixedLength might just be zero itself, resulting in a redundant expression
			{
				length = mStorage.FixedLength;
			}

			int actual_count;
			byte[] bytes = mStorage.Type switch
			{
				// Type streamers should set actual_count to -1 if we're to assume all the bytes are characters.
				// Otherwise, set actual_count to a byte count for padded string cases (where we don't want to
				// include null characters in the result string)

				StringStorageType.CString	=> ReadStrCString(s, length, out actual_count, maxLength),
				StringStorageType.Pascal	=> ReadStrPascal(s, out actual_count, prefixBitLength),
				StringStorageType.CharArray	=> ReadStrCharArray(s, length, out actual_count),
				_ => throw new Debug.UnreachableException(),
			};
			return new string(actual_count != -1
				? mBaseEncoding.GetChars(bytes, 0, actual_count)// for padded string cases
				: mBaseEncoding.GetChars(bytes));				// for complete string cases
		}
		#endregion
	};
}
