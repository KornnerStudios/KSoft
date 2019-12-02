using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	partial class ByteSwap
	{
		#region UInt16
		/// <summary>Swaps a <see cref="UInt16" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort SwapUInt16(
			ushort value)
		{
			return
				(ushort)( 
					((value >>  8) & 0x00FF) | 
					((value <<  8) & 0xFF00)
				);
		}
		/// <summary>Swaps a <see cref="UInt16" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref ushort value)
		{
			value =
				(ushort)( 
					((value >>  8) & 0x00FF) | 
					((value <<  8) & 0xFF00)
				);
		}
		/// <summary>Swaps a <see cref="UInt16" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 2</returns>
		public static int SwapUInt16(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ushort) <= buffer.Length);

			byte b0, b1;
			b0 = buffer[offset++];
			b1 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;

			return offset + sizeof(ushort);
		}
		/// <summary>Replaces 2 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 2</returns>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			ushort value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ushort) <= buffer.Length);

			byte b0, b1;
			if (BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
			} else {
				b0 = (byte)(value >>  8);
				b1 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;

			return offset;
		}
		#endregion

		#region Int16
		/// <summary>Swaps a <see cref="Int16" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static short SwapInt16(
			short value)
		{
			return
				(short)( 
					((value >>  8) & 0x00FF) | 
					 (value <<  8)
				);
		}
		/// <summary>Swaps a <see cref="Int16" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref short value)
		{
			value =
				(short)( 
					((value >>  8) & 0x00FF) | 
					 (value <<  8)
				);
		}
		/// <summary>Swaps a <see cref="Int16" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 2</returns>
		public static int SwapInt16(byte[] buffer, int offset)
		{
			return SwapUInt16(buffer, offset);
		}
		/// <summary>Replaces 2 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 2</returns>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			short value)
		{
			return ReplaceBytes(buffer, offset, (ushort)value);
		}
		#endregion

		#region UInt32
		/// <summary>Swaps a <see cref="UInt32" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint SwapUInt32(
			uint value)
		{
			return
				((value >> 24) & 0x000000FF) | 
				((value >>  8) & 0x0000FF00) | 
				((value <<  8) & 0x00FF0000) | 
				((value << 24) & 0xFF000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt32" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref uint value)
		{
			value =
				((value >> 24) & 0x000000FF) | 
				((value >>  8) & 0x0000FF00) | 
				((value <<  8) & 0x00FF0000) | 
				((value << 24) & 0xFF000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt32" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 4</returns>
		public static int SwapUInt32(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(uint) <= buffer.Length);

			byte b0, b1, b2, b3;
			b0 = buffer[offset++];
			b1 = buffer[offset++];
			b2 = buffer[offset++];
			b3 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;
			buffer[--offset] = b2;
			buffer[--offset] = b3;

			return offset + sizeof(uint);
		}
		/// <summary>Replaces 4 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 4</returns>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			uint value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(uint) <= buffer.Length);

			byte b0, b1, b2, b3;
			if (BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
				b2 = (byte)(value >> 16);
				b3 = (byte)(value >> 24);
			} else {
				b0 = (byte)(value >> 24);
				b1 = (byte)(value >> 16);
				b2 = (byte)(value >>  8);
				b3 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;
			buffer[offset++] = b2;
			buffer[offset++] = b3;

			return offset;
		}
		#endregion

		#region Int32
		/// <summary>Swaps a <see cref="Int32" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int SwapInt32(
			int value)
		{
			return
				((value >> 24) & 0x000000FF) | 
				((value >>  8) & 0x0000FF00) | 
				((value <<  8) & 0x00FF0000) | 
				 (value << 24)
				;
		}
		/// <summary>Swaps a <see cref="Int32" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref int value)
		{
			value =
				((value >> 24) & 0x000000FF) | 
				((value >>  8) & 0x0000FF00) | 
				((value <<  8) & 0x00FF0000) | 
				 (value << 24)
				;
		}
		/// <summary>Swaps a <see cref="Int32" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 4</returns>
		public static int SwapInt32(byte[] buffer, int offset)
		{
			return SwapUInt32(buffer, offset);
		}
		/// <summary>Replaces 4 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 4</returns>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			int value)
		{
			return ReplaceBytes(buffer, offset, (uint)value);
		}
		#endregion

		#region UInt64
		/// <summary>Swaps a <see cref="UInt64" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong SwapUInt64(
			ulong value)
		{
			return
				((value >> 56) & 0x00000000000000FF) | 
				((value >> 40) & 0x000000000000FF00) | 
				((value >> 24) & 0x0000000000FF0000) | 
				((value >>  8) & 0x00000000FF000000) | 
				((value <<  8) & 0x000000FF00000000) | 
				((value << 24) & 0x0000FF0000000000) | 
				((value << 40) & 0x00FF000000000000) | 
				((value << 56) & 0xFF00000000000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt64" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref ulong value)
		{
			value =
				((value >> 56) & 0x00000000000000FF) | 
				((value >> 40) & 0x000000000000FF00) | 
				((value >> 24) & 0x0000000000FF0000) | 
				((value >>  8) & 0x00000000FF000000) | 
				((value <<  8) & 0x000000FF00000000) | 
				((value << 24) & 0x0000FF0000000000) | 
				((value << 40) & 0x00FF000000000000) | 
				((value << 56) & 0xFF00000000000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt64" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 8</returns>
		public static int SwapUInt64(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ulong) <= buffer.Length);

			byte b0, b1, b2, b3, b4, b5, b6, b7;
			b0 = buffer[offset++];
			b1 = buffer[offset++];
			b2 = buffer[offset++];
			b3 = buffer[offset++];
			b4 = buffer[offset++];
			b5 = buffer[offset++];
			b6 = buffer[offset++];
			b7 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;
			buffer[--offset] = b2;
			buffer[--offset] = b3;
			buffer[--offset] = b4;
			buffer[--offset] = b5;
			buffer[--offset] = b6;
			buffer[--offset] = b7;

			return offset + sizeof(ulong);
		}
		/// <summary>Replaces 8 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 8</returns>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			ulong value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ulong) <= buffer.Length);

			byte b0, b1, b2, b3, b4, b5, b6, b7;
			if (BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
				b2 = (byte)(value >> 16);
				b3 = (byte)(value >> 24);
				b4 = (byte)(value >> 32);
				b5 = (byte)(value >> 40);
				b6 = (byte)(value >> 48);
				b7 = (byte)(value >> 56);
			} else {
				b0 = (byte)(value >> 56);
				b1 = (byte)(value >> 48);
				b2 = (byte)(value >> 40);
				b3 = (byte)(value >> 32);
				b4 = (byte)(value >> 24);
				b5 = (byte)(value >> 16);
				b6 = (byte)(value >>  8);
				b7 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;
			buffer[offset++] = b2;
			buffer[offset++] = b3;
			buffer[offset++] = b4;
			buffer[offset++] = b5;
			buffer[offset++] = b6;
			buffer[offset++] = b7;

			return offset;
		}
		#endregion

		#region Int64
		/// <summary>Swaps a <see cref="Int64" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static long SwapInt64(
			long value)
		{
			return
				((value >> 56) & 0x00000000000000FF) | 
				((value >> 40) & 0x000000000000FF00) | 
				((value >> 24) & 0x0000000000FF0000) | 
				((value >>  8) & 0x00000000FF000000) | 
				((value <<  8) & 0x000000FF00000000) | 
				((value << 24) & 0x0000FF0000000000) | 
				((value << 40) & 0x00FF000000000000) | 
				 (value << 56)
				;
		}
		/// <summary>Swaps a <see cref="Int64" /> by reference</summary>
		/// <param name="value"></param>
		public static void Swap(
			ref long value)
		{
			value =
				((value >> 56) & 0x00000000000000FF) | 
				((value >> 40) & 0x000000000000FF00) | 
				((value >> 24) & 0x0000000000FF0000) | 
				((value >>  8) & 0x00000000FF000000) | 
				((value <<  8) & 0x000000FF00000000) | 
				((value << 24) & 0x0000FF0000000000) | 
				((value << 40) & 0x00FF000000000000) | 
				 (value << 56)
				;
		}
		/// <summary>Swaps a <see cref="Int64" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 8</returns>
		public static int SwapInt64(byte[] buffer, int offset)
		{
			return SwapUInt64(buffer, offset);
		}
		/// <summary>Replaces 8 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 8</returns>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		public static int ReplaceBytes(byte[] buffer, int offset,
			long value)
		{
			return ReplaceBytes(buffer, offset, (ulong)value);
		}
		#endregion

		public const int kSizeOfUInt24 = sizeof(byte) * 3;
		public const int kSizeOfInt24 = kSizeOfUInt24;

		#region UInt24
		/// <summary>Swaps a <see cref="UInt32" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint SwapUInt24(
			uint value)
		{
			return
				((value >> 16) & 0x000000FF) | 
				((value >>  0) & 0x0000FF00) | 
				((value << 16) & 0x00FF0000)
				;
		}
		/// <summary>Swaps a <see cref="UInt32" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapUInt24(
			ref uint value)
		{
			value =
				((value >> 16) & 0x000000FF) | 
				((value >>  0) & 0x0000FF00) | 
				((value << 16) & 0x00FF0000)
				;
		}
		/// <summary>Swaps a <see cref="UInt32" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 3</returns>
		public static int SwapUInt24(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+kSizeOfInt24 <= buffer.Length);

			byte b0, b1, b2;
			b0 = buffer[offset++];
			b1 = buffer[offset++];
			b2 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;
			buffer[--offset] = b2;

			return offset + kSizeOfInt24;
		}
		/// <summary>Replaces 3 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 3</returns>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		public static int ReplaceBytesUInt24(byte[] buffer, int offset,
			uint value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+kSizeOfInt24 <= buffer.Length);

			byte b0, b1, b2;
			if (BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
				b2 = (byte)(value >> 16);
			} else {
				b0 = (byte)(value >> 16);
				b1 = (byte)(value >>  8);
				b2 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;
			buffer[offset++] = b2;

			return offset;
		}
		#endregion

		// #TODO: verify we don't need any sign-extension magic
		#region Int24
		/// <summary>Swaps a <see cref="Int32" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int SwapInt24(
			int value)
		{
			return
				((value >> 16) & 0x000000FF) | 
				((value >>  0) & 0x0000FF00) | 
				((value << 16) & 0x00FF0000)
				;
		}
		/// <summary>Swaps a <see cref="Int32" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapInt24(
			ref int value)
		{
			value =
				((value >> 16) & 0x000000FF) | 
				((value >>  0) & 0x0000FF00) | 
				((value << 16) & 0x00FF0000)
				;
		}
		/// <summary>Swaps a <see cref="Int32" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 3</returns>
		public static int SwapInt24(byte[] buffer, int offset)
		{
			return SwapUInt24(buffer, offset);
		}
		/// <summary>Replaces 3 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 3</returns>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		public static int ReplaceBytesInt24(byte[] buffer, int offset,
			int value)
		{
			return ReplaceBytesUInt24(buffer, offset, (uint)value);
		}
		#endregion

		public const int kSizeOfUInt40 = sizeof(byte) * 5;
		public const int kSizeOfInt40 = kSizeOfUInt40;

		#region UInt40
		/// <summary>Swaps a <see cref="UInt64" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong SwapUInt40(
			ulong value)
		{
			return
				((value >> 32) & 0x00000000000000FF) | 
				((value >> 16) & 0x000000000000FF00) | 
				((value >>  0) & 0x0000000000FF0000) | 
				((value << 16) & 0x00000000FF000000) | 
				((value << 32) & 0x000000FF00000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt64" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapUInt40(
			ref ulong value)
		{
			value =
				((value >> 32) & 0x00000000000000FF) | 
				((value >> 16) & 0x000000000000FF00) | 
				((value >>  0) & 0x0000000000FF0000) | 
				((value << 16) & 0x00000000FF000000) | 
				((value << 32) & 0x000000FF00000000)
				;
		}
		/// <summary>Swaps a <see cref="UInt64" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 5</returns>
		public static int SwapUInt40(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+kSizeOfInt40 <= buffer.Length);

			byte b0, b1, b2, b3, b4;
			b0 = buffer[offset++];
			b1 = buffer[offset++];
			b2 = buffer[offset++];
			b3 = buffer[offset++];
			b4 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;
			buffer[--offset] = b2;
			buffer[--offset] = b3;
			buffer[--offset] = b4;

			return offset + kSizeOfInt40;
		}
		/// <summary>Replaces 5 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 5</returns>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		public static int ReplaceBytesUInt40(byte[] buffer, int offset,
			ulong value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+kSizeOfInt40 <= buffer.Length);

			byte b0, b1, b2, b3, b4;
			if (BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
				b2 = (byte)(value >> 16);
				b3 = (byte)(value >> 24);
				b4 = (byte)(value >> 32);
			} else {
				b0 = (byte)(value >> 32);
				b1 = (byte)(value >> 24);
				b2 = (byte)(value >> 16);
				b3 = (byte)(value >>  8);
				b4 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;
			buffer[offset++] = b2;
			buffer[offset++] = b3;
			buffer[offset++] = b4;

			return offset;
		}
		#endregion

		// #TODO: verify we don't need any sign-extension magic
		#region Int40
		/// <summary>Swaps a <see cref="Int64" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static long SwapInt40(
			long value)
		{
			return
				((value >> 32) & 0x00000000000000FF) | 
				((value >> 16) & 0x000000000000FF00) | 
				((value >>  0) & 0x0000000000FF0000) | 
				((value << 16) & 0x00000000FF000000) | 
				((value << 32) & 0x000000FF00000000)
				;
		}
		/// <summary>Swaps a <see cref="Int64" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapInt40(
			ref long value)
		{
			value =
				((value >> 32) & 0x00000000000000FF) | 
				((value >> 16) & 0x000000000000FF00) | 
				((value >>  0) & 0x0000000000FF0000) | 
				((value << 16) & 0x00000000FF000000) | 
				((value << 32) & 0x000000FF00000000)
				;
		}
		/// <summary>Swaps a <see cref="Int64" /> at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		/// <returns>offset + 5</returns>
		public static int SwapInt40(byte[] buffer, int offset)
		{
			return SwapUInt40(buffer, offset);
		}
		/// <summary>Replaces 5 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <returns>offset + 5</returns>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		public static int ReplaceBytesInt40(byte[] buffer, int offset,
			long value)
		{
			return ReplaceBytesUInt40(buffer, offset, (ulong)value);
		}
		#endregion

	};
}