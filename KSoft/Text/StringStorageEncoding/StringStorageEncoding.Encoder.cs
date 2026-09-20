using System;
using System.Buffers;

namespace KSoft.Text
{
	using Memory.Strings;

	partial class StringStorageEncoding
	{

		// A fixed-width payload unit has the same byte width as an encoded null.
		int GetSerializedCharacterCount(int byteCount) => byteCount / mNullCharacterSize;

		void ValidateEncodedPayload(int byteCount)
		{
			if (mStorage.HasLengthPrefix)
			{
				GetPascalPrefixByteCount(GetSerializedCharacterCount(byteCount));
			}
			if (mStorage.IsFixedLength &&
				byteCount > mFixedLengthByteLength - (mStorage.Type == StringStorageType.CString ? mNullCharacterSize : 0))
			{
				throw new ArgumentException("The encoded payload exceeds the fixed string field.", "chars");
			}
		}

		#region CalculateByteCount
		/// <summary>Calculate how many additional bytes are needed for encoding a raw string</summary>
		/// <param name="byteCount">Base characters byte count</param>
		/// <param name="validateLength">Whether the payload is exact rather than an upper bound.</param>
		/// <returns>Total byte count needed for encoding a string</returns>
		int CalculateByteCount(int byteCount, bool validateLength = true)
		{
			if (validateLength)
			{
				ValidateEncodedPayload(byteCount);
			}
			if (mStorage.IsFixedLength)
			{
				return mFixedLengthByteLength;
			}

			switch (mStorage.Type)
			{
				case StringStorageType.CString:		byteCount = CalcByteCountCString(byteCount); break;
				case StringStorageType.Pascal:		byteCount = CalcByteCountPascal(byteCount, validateLength); break;
				// CharArray doesn't do anything anyway
				case StringStorageType.CharArray:	/*byteCount = CalcByteCountCharArray(byteCount);*/ break;
				default:
					throw new Debug.UnreachableException(mStorage.Type.ToString());
			}

			return byteCount;
		}
		#endregion

		/// <summary>Return the input character count fitting the serialized field or caller's limit.</summary>
		/// <param name="chars">Managed input characters.</param>
		/// <param name="maxLength">Optional maximum payload length in storage units for fixed-width encodings.</param>
		int ClampCharCount(ReadOnlySpan<char> chars, int maxLength = -1)
		{
			int limit = maxLength > 0 ? maxLength : int.MaxValue;
			if (mStorage.IsFixedLength)
			{
				int capacity = mStorage.FixedLength - (mStorage.Type == StringStorageType.CString ? 1 : 0);
				limit = Math.Min(limit, capacity);
			}
			if (chars.Length <= limit || mStorage.WidthType != StringStorageWidthType.UTF32)
			{
				return Math.Min(chars.Length, limit);
			}

			int charCount = 0;
			for (int units = 0; units < limit && charCount < chars.Length; units++)
			{
				// A complete surrogate pair occupies one UTF-32 storage unit.
				charCount += charCount + 1 < chars.Length && char.IsSurrogatePair(chars[charCount], chars[charCount + 1])
					? 2 : 1;
			}
			return charCount;
		}

		#region Encode StringStorageType Data Prefix
		/// <summary>Encode any prefix related data for the <see cref="StringStorageType"/> into a byte span</summary>
		/// <param name="charCount">The number of characters in the serialized representation.</param>
		/// <param name="bytes">The destination for the resulting sequence of bytes</param>
		/// <returns>Number of prefix bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePrefixData(int charCount, Span<byte> bytes)
		{
			return mStorage.Type switch
			{
				// No prefix for CString
				StringStorageType.CString => 0,
				StringStorageType.Pascal => EncodePascalPrefix(charCount, bytes),
				// CharArray doesn't do anything anyway
				StringStorageType.CharArray => 0,
				_ => throw new Debug.UnreachableException(mStorage.Type.ToString()),
			};
		}
		#endregion

		#region Encode StringStorageType Data Postfix
		/// <summary>Encode any additional <see cref="StringStorageType"/> related data into a byte span</summary>
		/// <param name="bytes">The destination for the resulting sequence of postfix bytes</param>
		/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePostfixData(Span<byte> bytes)
		{
			return mStorage.Type switch
			{
				StringStorageType.CString => EncodeCStringTerminator(bytes),
				// No postfix for Pascal
				StringStorageType.Pascal => 0,
				// CharArray doesn't do anything anyway
				StringStorageType.CharArray => 0,
				_ => throw new Debug.UnreachableException(mStorage.Type.ToString()),
			};
		}
		#endregion

		/// <summary>Converts a set of characters into a sequence of bytes.</summary>
		class Encoder : System.Text.Encoder
		{
			readonly StringStorageEncoding mEncoding;
			readonly System.Text.Encoder mEnc;
			public Encoder(StringStorageEncoding enc) { mEncoding = enc; mEnc = enc.mBaseEncoding.GetEncoder(); }

			/// <summary>
			/// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
			/// A parameter indicates whether to clear the internal state of the encoder after the calculation.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="index">The index of the first character to encode</param>
			/// <param name="count">The number of characters to encode</param>
			/// <param name="flush"><b>true</b> to simulate clearing the internal state of the encoder after the calculation; otherwise, <b>false</b></param>
			/// <returns>The number of bytes produced by encoding the specified characters and any characters in the internal buffer</returns>
			/// <seealso cref="System.Text.Encoder.GetByteCount(Char[], Int32, Int32, Boolean) "/>
			public override int GetByteCount(char[] chars, int index, int count, bool flush)
			{
				Verify.Buffers.OffsetAndLengthWithinLength(chars, index, count);
				count = mEncoding.ClampCharCount(chars.AsSpan(index, count));
				int byte_count = mEnc.GetByteCount(chars, index, count, mEncoding.DontAlwaysFlush ? flush : true);

				byte_count = mEncoding.CalculateByteCount(byte_count); // Add our String Storage calculations

				return byte_count;
			}

			/// <summary>
			/// Encodes a set of characters from the specified character array and any characters in the internal buffer into the specified byte array.
			/// A parameter indicates whether to clear the internal state of the encoder after the conversion.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="charIndex">The index of the first character to encode</param>
			/// <param name="charCount">The number of characters to encode</param>
			/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
			/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
			/// <param name="flush">true to clear the internal state of the encoder after the conversion; otherwise, false</param>
			/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
			/// <seealso cref="System.Text.Encoder.GetBytes(Char[], Int32, Int32, Byte[], Int32, Boolean) "/>
			public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
			{
				Verify.Buffers.OffsetAndLengthWithinLength(chars, charIndex, charCount);
				Verify.Buffers.StartIndexWithinLength(bytes, byteIndex);
				charCount = mEncoding.ClampCharCount(chars.AsSpan(charIndex, charCount));
				int serializedCount = 0;
				if (mEncoding.HasCountedPayload)
				{
					int byteCount = mEnc.GetByteCount(chars, charIndex, charCount, mEncoding.DontAlwaysFlush ? flush : true);
					mEncoding.ValidateEncodedPayload(byteCount);
					serializedCount = mEncoding.GetSerializedCharacterCount(byteCount);
				}
				// Add our String Storage calculations
				int bytes_written = mEncoding.EncodeStringStorageTypePrefixData(
					serializedCount, bytes.AsSpan(byteIndex));

				bytes_written += mEnc.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytes_written, mEncoding.DontAlwaysFlush ? flush : true);

				// Add our String Storage calculations
				bytes_written += mEncoding.EncodeStringStorageTypePostfixData(
					bytes.AsSpan(byteIndex + bytes_written));

				return bytes_written;
			}

			/// <summary>Sets the encoder back to its initial state</summary>
			public override void Reset()	{ mEnc.Reset(); }
		};

		#region WriteString
		internal readonly record struct StringEncodingPlan(
			int CharacterCount, int PayloadByteCount, int SerializedByteCount);

		internal StringEncodingPlan GetEncodingPlan(ReadOnlySpan<char> chars, int maxLength = -1)
		{
			int characterCount = ClampCharCount(chars, maxLength);
			int payloadByteCount = mBaseEncoding.GetByteCount(chars[..characterCount]);
			int serializedByteCount = CalculateByteCount(payloadByteCount);
			return new StringEncodingPlan(characterCount, payloadByteCount, serializedByteCount);
		}

		void EncodeStringCore(ReadOnlySpan<char> chars, Span<byte> bytes,
			StringEncodingPlan plan, out int encodedPayloadByteCount)
		{
			chars = chars[..plan.CharacterCount];
			Span<byte> record = bytes[..plan.SerializedByteCount];
			if (mStorage.IsFixedLength)
			{
				record.Clear();
			}

			int serializedCount = mStorage.HasLengthPrefix
				? GetSerializedCharacterCount(plan.PayloadByteCount)
				: 0;
			int bytesWritten = EncodeStringStorageTypePrefixData(serializedCount, record);

			encodedPayloadByteCount = mBaseEncoding.GetBytes(chars, record[bytesWritten..]);
			bytesWritten += encodedPayloadByteCount;
			_ = EncodeStringStorageTypePostfixData(record[bytesWritten..]);
		}

		internal int EncodeString(ReadOnlySpan<char> chars, Span<byte> bytes, out int payloadByteCount)
		{
			var plan = GetEncodingPlan(chars);
			return EncodeString(chars, bytes, plan, out payloadByteCount);
		}

		internal int EncodeString(ReadOnlySpan<char> chars, Span<byte> bytes,
			StringEncodingPlan plan, out int payloadByteCount)
		{
			if (bytes.Length < plan.SerializedByteCount)
			{
				throw new ArgumentException("The destination is too small for the serialized string.", nameof(bytes));
			}

			EncodeStringCore(chars, bytes, plan, out payloadByteCount);
			return plan.SerializedByteCount;
		}

		void WriteString(System.IO.Stream stream, ReadOnlySpan<char> chars,
			StringEncodingPlan plan)
		{
			byte[]? rented = null;
			Span<byte> bytes = plan.SerializedByteCount <= kStackBufferThreshold
				? stackalloc byte[plan.SerializedByteCount]
				: (rented = ArrayPool<byte>.Shared.Rent(plan.SerializedByteCount));
			try
			{
				int extent = EncodeString(chars, bytes, plan, out _);
				stream.Write(bytes[..extent]);
			}
			finally
			{
				if (rented != null)
				{
					ArrayPool<byte>.Shared.Return(rented, clearArray: true);
				}
			}
		}

		internal void WriteString(System.IO.Stream stream, ReadOnlySpan<char> chars)
		{
			ArgumentNullException.ThrowIfNull(stream);
			WriteString(stream, chars, GetEncodingPlan(chars));
		}

		internal void WriteString(IO.BitStream s, string value, int maxLength = -1, int prefixBitLength = -1)
		{
			if (prefixBitLength > 0)
			{
				throw new NotSupportedException("Currently don't support unnatural bit lengths for prefixes on writes");
			}

			var plan = GetEncodingPlan(value.AsSpan(), maxLength);
			byte[]? rented = null;
			Span<byte> bytes = plan.SerializedByteCount <= kStackBufferThreshold
				? stackalloc byte[plan.SerializedByteCount]
				: (rented = ArrayPool<byte>.Shared.Rent(plan.SerializedByteCount));
			try
			{
				int extent = EncodeString(value, bytes, plan, out _);
				s.Write(bytes[..extent]);
			}
			finally
			{
				if (rented != null)
				{
					ArrayPool<byte>.Shared.Return(rented, clearArray: true);
				}
			}
		}
		#endregion
	};
}
