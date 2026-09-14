using System;

namespace KSoft.Bitwise
{
	/// <summary>Utility class for interacting with encoded 7-bit integers</summary>
	/// <remarks>Uses the Int32 bit representation of BinaryReader.Read7BitEncodedInt and BinaryWriter.Write7BitEncodedInt: one through five bytes, including five bytes for negative values.</remarks>
	public static class Encoded7BitInt
	{
		/// <summary>Maximum value that can be stored in 1 encoded 7-bit integer</summary>
		/// <remarks>0x80 - 1</remarks>
		public const int kMaxValue1Bytes = 0x0000007F;
		/// <summary>Maximum value that can be stored in 2 encoded 7-bit integer</summary>
		/// <remarks>(0x80 &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue2Bytes = 0x00003FFF;
		/// <summary>Maximum value that can be stored in 3 encoded 7-bit integer</summary>
		/// <remarks>((0x80 &lt;&lt; 7) &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue3Bytes = 0x001FFFFF;
		/// <summary>Maximum value that can be stored in 4 encoded 7-bit integer</summary>
		/// <remarks>(((0x80 &lt;&lt; 7) &lt;&lt; 7) &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue4Bytes = 0x0FFFFFFF;
		const int kMaxEncodedByteCount = 5;

		/// <summary>Calculate how many bytes it would take to encode a value into a 7-bit integer</summary>
		/// <param name="value">Value to encode</param>
		/// <returns>Number of bytes it would take to encode <paramref name="value"/>, from 1 through 5.</returns>
		public static int CalculateSize(int value)
		{
			int size = 0;
			for (uint num = unchecked((uint)value); num >= 0x80; size++)
			{
				num >>= 7;
			}

			return ++size;
		}
		/// <summary>Decode a 7-bit integer from the beginning of a span.</summary>
		/// <remarks>Retains the legacy payload-capacity check: a nonnegative value must fit within the bytes following its prefix. This is not a prefix-only API.</remarks>
		/// <param name="buffer">The complete readable range, including the payload bytes for a nonnegative count.</param>
		/// <param name="bytesRead">
		/// The relative number of prefix bytes consumed, from 1 through 5, or
		/// <see cref="TypeExtensions.kNone"/> if the data is incomplete or corrupt.
		/// </param>
		/// <returns>
		/// The decoded integer, or <see cref="TypeExtensions.kNone"/> on failure. Use <paramref name="bytesRead"/> to distinguish failure from an encoded -1.
		/// </returns>
		public static int Read(ReadOnlySpan<byte> buffer, out int bytesRead)
		{
			bytesRead = TypeExtensions.kNone;

			int size = 0; // size (bytes) of the encoded int
			int count = 0;
			int shift = 0;
			byte b;
			do
			{
				// Either the prefix is corrupt or the buffer is incomplete.
				if (size >= kMaxEncodedByteCount || size >= buffer.Length)
				{
					return TypeExtensions.kNone;
				}

				b = buffer[size++];
				// Only four bits remain in the fifth byte of an Int32.
				if (size == kMaxEncodedByteCount && b > 0x0F)
				{
					return TypeExtensions.kNone;
				}
				count |= (b & 0x7F) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);

			// either buffer is incomplete or
			// this isn't even data with a 7-bit integer.
			if (count > buffer.Length - size)
			{
				return TypeExtensions.kNone;
			}

			bytesRead = size;

			return count;
		}
		/// <summary>Encode a value at the beginning of a span</summary>
		/// <param name="buffer">The complete writable range beginning where the encoded value should be written</param>
		/// <param name="value">The value to encode into <paramref name="buffer"/></param>
		/// <returns>The relative number of bytes written, from 1 through 5.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="buffer"/> is too small for the encoded value. No bytes are written.
		/// </exception>
		public static int Write(Span<byte> buffer, int value)
		{
			int encodedByteCount = CalculateSize(value);
			if (encodedByteCount > buffer.Length)
			{
				throw new ArgumentException("Destination buffer is too small.", nameof(buffer));
			}

			// Write out an int 7 bits at a time.  The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			int bytesWritten = 0;
			uint v = unchecked((uint)value);
			for (; v >= 0x80; v >>= 7)
			{
				buffer[bytesWritten++] = unchecked((byte)(v | 0x80));
			}

			buffer[bytesWritten++] = (byte)v;

			return bytesWritten;
		}
	};
}