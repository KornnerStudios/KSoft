#nullable enable

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace KSoft.Text
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1724:TypeNamesShouldNotMatchNamespaces",
		Justification="I don't care about System.Web.Util")]
	partial class Util
	{
		/// <summary>Determine the string <see cref="Encoding"/> based on the byte-order-marks in a buffer</summary>
		/// <param name="buffer">Buffer containing the BOMs</param>
		/// <param name="index">Start index of the BOMs</param>
		/// <returns>The respected <see cref="Encoding"/> to use or <see cref="Encoding.Default"/> if this was unable to determine</returns>
		public static Encoding DetermineStringEncoding(byte[] buffer
			, int index = 0)
		{
			ArgumentNullException.ThrowIfNull(buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(index);

			Encoding? enc = null;
			int length = buffer.Length - index;

			/*
				FF FE			UTF-16, little-endian
				FE FF			UTF-16, big-endian
				2B 2F 76		UTF-7
				EF BB BF		UTF-8
				FF FE 00 00		UTF-32, little-endian
				00 00 FE FF		UTF-32, big-endian
			 */

			if (length >= 4)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1], b2 = buffer[index+2], b3 = buffer[index+3];

				if (b0 == 0x00 && b1 == 0x00 && b2 == 0xFE && b3 == 0xFF)
				{
					enc = Encoding.UTF32;
				}
			}
			if (enc == null && length >= 3)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1], b2 = buffer[index+2];

				// NOTE: Our UTF-7 detection doesn't test a 4th byte for 0x38,0x39,0x2B or 0x2F.
				// One of those are suppose to follow in UTF7 BOMs
				if (b0 == 0x2B && b1 == 0x2F && b2 == 0x76)
				{
//					enc = Encoding.UTF7;
					throw new NotSupportedException("UTF7 is not supported with the move to dotnet");
				}
				else if (b0 == 0xEF && b1 == 0xBB && b2 == 0xBF)
				{
					enc = Encoding.UTF8;
				}
			}
			if (enc == null && length >= 2)
			{
				byte b0 = buffer[index+0], b1 = buffer[index+1];

				if (b0 == 0xFF && b1 == 0xFE)
				{
					enc = Encoding.Unicode;
				}
				else if (b0 == 0xFE && b1 == 0xFF)
				{
					enc = Encoding.BigEndianUnicode;
				}
			}
			if (enc == null)
			{
				enc = Encoding.Default;
			}

			return enc;
		}

		#region Byte arrays
		public const int kDefaultHexDigitsPerLine = 16;
		const int kHexStreamBytesPerChunk = 256;
		private static readonly Type[] kTextWriterSpanWriteParameterTypes = [typeof(ReadOnlySpan<char>)];

		// TextWriter's base Write(ReadOnlySpan<char>) implementation does not delegate through Write(string).
		// Some legacy/custom writers only override Write(string), which worked with the old per-byte ToString path.
		// Detect span-aware writers so common writers avoid chunk strings, but fall back to bounded string chunks to
		// preserve output behavior for string-only writers.
		private static bool TextWriterOverridesSpanWrite(TextWriter stream)
		{
			MethodInfo? spanWriteMethod = stream.GetType().GetMethod(nameof(TextWriter.Write),
				kTextWriterSpanWriteParameterTypes);

			return spanWriteMethod != null && spanWriteMethod.DeclaringType != typeof(TextWriter);
		}
		private static void ValidateStartIndex(int length, int startIndex, string paramName = "startIndex")
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException(paramName, startIndex, "Start index must not be negative.");
			}
			if (startIndex >= length)
			{
				throw new ArgumentOutOfRangeException(paramName, startIndex, "Start index must be inside the source.");
			}
		}
		private static void ValidateRange(int length, int startIndex, int count)
		{
			ValidateStartIndex(length, startIndex);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
			if (count > length - startIndex)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count, "Count must fit within source length.");
			}
		}
		private static void ValidateEvenCharacterCount(int count, string paramName)
		{
			if ((count % 2) != 0)
			{
				throw new ArgumentException("Can't byte-ify a string that's not even!", paramName);
			}
		}
		private static void ValidateDestinationLength(byte[] bytes, int requiredLength)
		{
			if (bytes.Length < requiredLength)
			{
				throw new ArgumentException("Destination buffer is too small.", nameof(bytes));
			}
		}
		private static void ValidateDigitsPerLine(int digitsPerLine)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(digitsPerLine, 2);
			if ((digitsPerLine % 2) != 0)
			{
				throw new ArgumentException("Digits per line must be even.", nameof(digitsPerLine));
			}
		}

		#region ByteArrayToString (byte[] to string)
		/// <summary>Converts an array of bytes to a hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <param name="count">Number of bytes to convert</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static string ByteArrayToString(byte[] data, int startIndex, int count)
		{
			ArgumentNullException.ThrowIfNull(data);
			ValidateRange(data.Length, startIndex, count);

			// #VITA_SHIM: Preserve KSoft's uppercase hex string API while routing exact-format output through the BCL.
			return Convert.ToHexString(data, startIndex, count);
		}
		/// <summary>Converts an array of bytes to a hex string and outputs it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="stream">Stream to output the hex string to</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <param name="count">Number of bytes to convert</param>
		/// <example>"1337BEEF"</example>
		public static void ByteArrayToStream(byte[] data, TextWriter stream, int startIndex, int count)
		{
			ArgumentNullException.ThrowIfNull(data);
			ArgumentNullException.ThrowIfNull(stream);
			ValidateRange(data.Length, startIndex, count);

			// #VITA_SHIM: TextWriter output uses BCL hex conversion in bounded stack chunks to avoid a hidden full string.
			ReadOnlySpan<byte> source = data.AsSpan(startIndex, count);
			Span<char> buffer = stackalloc char[kHexStreamBytesPerChunk * 2];
			bool canWriteSpan = TextWriterOverridesSpanWrite(stream);
			while (!source.IsEmpty)
			{
				int chunkLength = Math.Min(source.Length, kHexStreamBytesPerChunk);
				ReadOnlySpan<byte> chunk = source.Slice(0, chunkLength);
				if (!Convert.TryToHexString(chunk, buffer, out int charsWritten))
				{
					throw new InvalidOperationException("Hex conversion failed for a pre-sized destination buffer.");
				}

				ReadOnlySpan<char> text = buffer.Slice(0, charsWritten);
				if (canWriteSpan)
				{
					stream.Write(text);
				}
				else
				{
					stream.Write(text.ToString());
				}
				source = source.Slice(chunkLength);
			}
		}
		/// <summary>Converts an array of bytes to a hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static string ByteArrayToString(byte[] data
			, int startIndex = 0)
		{
			ArgumentNullException.ThrowIfNull(data);
			ValidateStartIndex(data.Length, startIndex);

			return ByteArrayToString(data, startIndex, data.Length-startIndex);
		}
		/// <summary>Converts an array of bytes to a hex string and outputs it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="startIndex">Index in <paramref name="data"/> to start the conversion</param>
		/// <example>"1337BEEF"</example>
		/// <returns></returns>
		public static void ByteArrayToStream(byte[] data, TextWriter stream
			, int startIndex = 0)
		{
			ArgumentNullException.ThrowIfNull(data);
			ArgumentNullException.ThrowIfNull(stream);
			ValidateStartIndex(data.Length, startIndex);

			ByteArrayToStream(data, stream, startIndex, data.Length-startIndex);
		}
		#endregion

		#region ByteStringToArray (string to byte[])
		private static bool IsBclHexString(string data, int startIndex, int count)
		{
			int endIndex = startIndex + count;
			for (int x = startIndex; x < endIndex; x++)
			{
				char c = data[x];
				if (c > 0x7F || !CharIsDigit(c))
				{
					return false;
				}
			}

			return true;
		}

		private static void ConvertHexStringToArray(byte[] bytes, string data, int startIndex, int count)
		{
			System.Buffers.OperationStatus status = Convert.FromHexString(data.AsSpan(startIndex, count),
				bytes.AsSpan(0, count / 2), out int charsConsumed, out int bytesWritten);
			if (status != System.Buffers.OperationStatus.Done || charsConsumed != count || bytesWritten != (count / 2))
			{
				throw new InvalidOperationException("Hex conversion failed for a pre-validated source string.");
			}
		}

		public static byte[] ByteStringToArray(byte[] bytes, string data, int startIndex, int count)
		{
			ArgumentException.ThrowIfNullOrEmpty(data);
			ValidateRange(data.Length, startIndex, count);
			ValidateEvenCharacterCount(count, nameof(count));
			ArgumentNullException.ThrowIfNull(bytes);
			ValidateDestinationLength(bytes, count / 2);

			Array.Clear(bytes, 0, bytes.Length);

			// #VITA_SHIM: Strict hex input can use the BCL converter; legacy non-hex digit behavior falls back below.
			if (IsBclHexString(data, startIndex, count))
			{
				ConvertHexStringToArray(bytes, data, startIndex, count);
				return bytes;
			}

			for ( int x = startIndex, index = 0
				; x < (startIndex+count)
				; x+=2, index++)
			{
				bytes[index] = (byte)CharsToByte(NumeralBase.Hex, data, x);
			}

			return bytes;
		}

		public static byte[] ByteStringToArray(byte[] bytes, string data
			, int startIndex = 0)
		{
			ArgumentException.ThrowIfNullOrEmpty(data);
			ValidateStartIndex(data.Length, startIndex);
			int count = data.Length - startIndex;
			ValidateEvenCharacterCount(count, nameof(data));
			ArgumentNullException.ThrowIfNull(bytes);
			ValidateDestinationLength(bytes, count / 2);

			return ByteStringToArray(bytes, data, startIndex, count);
		}

		/// <summary>Converts a string containing hex values into a byte array</summary>
		/// <param name="data">String of hex digits to convert</param>
		/// <param name="startIndex">Character index in <paramref name="data"/> to start the conversion at</param>
		/// <param name="count">Number of characters to convert</param>
		/// <returns></returns>
		public static byte[] ByteStringToArray(string data, int startIndex, int count)
		{
			ArgumentException.ThrowIfNullOrEmpty(data);
			ValidateRange(data.Length, startIndex, count);
			ValidateEvenCharacterCount(count, nameof(count));

			// #VITA_SHIM: Strict hex input can allocate directly through the BCL without changing the public result shape.
			if (IsBclHexString(data, startIndex, count))
			{
				return Convert.FromHexString(data.AsSpan(startIndex, count));
			}

			byte[] bytes = new byte[count / 2];
			return ByteStringToArray(bytes, data, startIndex, count);
		}
		/// <summary>Converts a string containing hex values into a byte array</summary>
		/// <param name="data">String of hex digits to convert</param>
		/// <param name="startIndex">Character index in <paramref name="data"/> to start the conversion at</param>
		/// <returns></returns>
		public static byte[] ByteStringToArray(string data
			, int startIndex = 0)
		{
			ArgumentException.ThrowIfNullOrEmpty(data);
			ValidateStartIndex(data.Length, startIndex);
			ValidateEvenCharacterCount(data.Length - startIndex, nameof(data));

			return ByteStringToArray(data, startIndex, data.Length-startIndex);
		}
		#endregion

		/// <summary>Convert an array of bytes into a formatted hex string</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="padding">Padding string to appear before each line of hex characters</param>
		/// <param name="digitsPerLine">Number of hex characters per line</param>
		/// <returns></returns>
		/// <remarks>Uses <see cref="System.Environment.NewLine"/> for line termination</remarks>
		public static string ByteArrayToAlignedString(byte[] data
			, string padding = ""
			, int digitsPerLine = kDefaultHexDigitsPerLine)
		{
			ArgumentNullException.ThrowIfNull(data);
			ArgumentNullException.ThrowIfNull(padding);
			ValidateDigitsPerLine(digitsPerLine);

			string new_line = Environment.NewLine;

			int blocks = data.Length / digitsPerLine;
			int leftovers = data.Length % digitsPerLine;

			var sb = new StringBuilder(
				(data.Length * 2) +
				(new_line.Length * blocks) + // calculate how many new line characters we'll need
				(padding.Length * (leftovers == 0 ? blocks : blocks + 1)) // calculate how many characters the padding on each line will take
			);

			int index = 0;
			for (int b = 0; b < blocks; b++, index+=digitsPerLine)
			{
				sb.AppendFormat(KSoft.Util.InvariantCultureInfo, "{0}{1}{2}", padding, ByteArrayToString(data, index, digitsPerLine), new_line);
			}

			if (leftovers > 0)
			{
				sb.AppendFormat(KSoft.Util.InvariantCultureInfo, "{0}{1}{2}", padding, ByteArrayToString(data, index), new_line);
			}

			return sb.ToString();
		}
		/// <summary>Convert an array of bytes into a formatted hex string and output it to the stream</summary>
		/// <param name="data">Buffer of bytes to convert</param>
		/// <param name="output">Stream to output the hex strings to</param>
		/// <param name="padding">Padding string to appear before each line of hex characters. Can be null.</param>
		/// <param name="digitsPerLine">Number of hex characters per line</param>
		public static void ByteArrayToAlignedOutput(byte[] data, TextWriter output
			, string? padding = null
			, int digitsPerLine = kDefaultHexDigitsPerLine)
		{
			ArgumentNullException.ThrowIfNull(data);
			ValidateDigitsPerLine(digitsPerLine);

			int blocks = data.Length / digitsPerLine;
			int leftovers = data.Length % digitsPerLine;

			int index = 0;
			for (int b = 0; b < blocks; b++, index += digitsPerLine)
			{
				if (!string.IsNullOrEmpty(padding))
				{
					output.Write(padding);
				}

				output.WriteLine(ByteArrayToString(data, index, digitsPerLine));
			}

			if (leftovers > 0)
			{
				if (!string.IsNullOrEmpty(padding))
				{
					output.Write(padding);
				}

				output.WriteLine(ByteArrayToString(data, index));
			}
		}
		#endregion

		#region Char arrays
		/// <summary>Convert a radix into an acceptable NumberBase usable with the CharToX converters</summary>
		/// <param name="radix">Base we're converting from. Can be up to 36.</param>
		/// <returns>0 if <paramref name="radix"/> can't be converted</returns>
		public static NumeralBase ToAcceptableNumberBase(int radix) =>
			radix > 36 || radix < 0 ? 0 : (NumeralBase)radix;

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-36</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharToAnyDigit(char c)	=> kCharToByteLookup36[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-36</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static bool CharIsAnyDigit(char c)	=> kCharIsDigitLookup62[(byte)c];

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-62</param>
		/// <returns></returns>
		/// <remarks>"Extended" char digits map different int values for upper ('A') and lower ('a') case</remarks>
		public static int CharToAnyDigitExtended(char c)	=> kCharToByteLookup62[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-62</param>
		/// <returns></returns>
		/// <remarks>"Extended" char digits map different int values for upper ('A') and lower ('a') case</remarks>
		public static bool CharIsAnyDigitExtended(char c)	=> kCharIsDigitLookup62[(byte)c];

		/// <summary>Convert the byte digit character to the byte value it represents</summary>
		/// <param name="c">Byte digit char up to base-16</param>
		/// <returns></returns>
		public static int CharToDigit(char c)		=> kCharToByteLookup16[(byte)c];
		/// <summary>Checks if the byte digit character is any valid representable value</summary>
		/// <param name="c">Byte digit char up to base-16</param>
		/// <returns></returns>
		public static bool CharIsDigit(char c)		=> kCharIsDigitLookup16[(byte)c];

		/// <summary>Convert a character into the byte digit it represents</summary>
		/// <param name="c">Character representing the byte digit</param>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="place">base-0 position of the digit in the number string</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharToInt(char c, NumeralBase radix, int place)
		{
			int digit = CharToAnyDigit(c);
			int from_base = (int)radix;
			int multiplier = (int)System.Math.Pow(from_base, place);

			return digit * multiplier;
		}
		/// <summary>Convert a character into the byte digit it represents</summary>
		/// <param name="c">Character representing the byte digit</param>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="place">base-0 position of the digit in the number string</param>
		/// <returns></returns>
		/// <remarks>
		/// "Extended" char digits map different int values for upper ('A') and lower ('a') case.
		/// Does not validate that <paramref name="c"/> is within <paramref name="radix"/>'s range
		/// </remarks>
		public static int CharToIntExtended(char c, NumeralBase radix, int place)
		{
			int digit = CharToAnyDigitExtended(c);
			int from_base = (int)radix;
			int multiplier = (int)System.Math.Pow(from_base, place);

			return digit * multiplier;
		}

		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="c2">Character in the 2nd position (when reading right to left)</param>
		/// <param name="c1">Character in the 1st position (when reading right to left)</param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		/// <example>
		/// int b = CharsToByte(NumeralBase.Hex, '3', 'F');
		/// b == 63;
		/// </example>
		public static int CharsToByte(NumeralBase radix, char c2, char c1)
		{
			int value = 0;

			if (CharIsAnyDigit(c2) && CharIsAnyDigit(c1))
			{
				value = CharToInt(c2, radix, 1) + CharToInt(c1, radix, 0);
			}

			// Someone could supply a radix value that isn't technically a member of NumeralBase (eg, 36)
			// So we clamp it to a byte here
			return value > byte.MaxValue ? 0 : value;
		}

		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="data">Buffer that holds the byte digit character pair</param>
		/// <param name="index">Index to start processing in <paramref name="data"/></param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		public static int CharsToByte(NumeralBase radix, char[] data, int index = 0)
		{
			ArgumentNullException.ThrowIfNull(data);
			ValidateStartIndex(data.Length, index, nameof(index));

			return CharsToByte(radix, data[index], data[index+1]);
		}
		/// <summary>Convert a byte digit character pair to the byte they represent</summary>
		/// <param name="radix">The base we're converting from</param>
		/// <param name="data">Buffer that holds the byte digit character pair</param>
		/// <param name="index">Index to start processing in <paramref name="data"/></param>
		/// <returns></returns>
		/// <remarks>Upper ('A') and lower ('a') case char digits map to the same int values</remarks>
		/// <example>
		/// int b = CharsToByte(NumeralBase.Hex, "3F");
		/// b == 63;
		/// </example>
		public static int CharsToByte(NumeralBase radix, string data, int index = 0)
		{
			ArgumentNullException.ThrowIfNull(data);
			ValidateStartIndex(data.Length, index, nameof(index));

			return CharsToByte(radix, data[index], data[index+1]);
		}
		#endregion
	};
}
