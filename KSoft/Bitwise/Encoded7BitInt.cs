using System;

namespace KSoft.Bitwise
{
	/// <summary>Utility class for interacting with encoded 7-bit integers</summary>
	/// <remarks>Encoded 7-bit integers can only consume a max of 4 bytes</remarks>
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
		const int kMaxEncodedByteCount = 4;

		static void ValidateValue(int value)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(value);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(value, kMaxValue4Bytes);
		}

		/// <summary>Calculate how many bytes it would take to encode a value into a 7-bit integer</summary>
		/// <param name="value">Value to encode</param>
		/// <returns>Number of bytes it would take to encode <paramref name="value"/>, from 1 through 4.</returns>
		public static int CalculateSize(int value)
		{

			ValidateValue(value);

			int size = 0;
			for (uint num = (uint)value; num >= 0x80; size++)
			{
				num >>= 7;
			}

			return ++size;
		}
		/// <summary>Decode a 7-bit integer payload byte count from the beginning of a span</summary>
		/// <param name="buffer">The complete readable range, beginning with the encoded count and followed by its payload bytes</param>
		/// <param name="bytesRead">
		/// The relative number of prefix bytes consumed, from 1 through 4, or
		/// <see cref="TypeExtensions.kNone"/> if the data is incomplete or corrupt.
		/// </param>
		/// <returns>
		/// The decoded payload byte count, or <see cref="TypeExtensions.kNone"/> if the data is incomplete or corrupt.
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
		/// <returns>The relative number of bytes written, from 1 through 4.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the supported range.</exception>
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
			uint v = (uint)value;
			for (; v >= 0x80; v >>= 7)
			{
				buffer[bytesWritten++] = (byte)(v | 0x80);
			}

			buffer[bytesWritten++] = (byte)v;

			return bytesWritten;
		}
	};
}