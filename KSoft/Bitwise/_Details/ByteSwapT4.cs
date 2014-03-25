using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapUInt16(byte[] buffer, int offset)
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
		}
		/// <summary>Replaces 2 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			ushort value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ushort) <= buffer.Length);

			byte b0, b1;
			if(BitConverter.IsLittleEndian) {
				b0 = (byte)(value >>  0);
				b1 = (byte)(value >>  8);
			} else {
				b0 = (byte)(value >>  8);
				b1 = (byte)(value >>  0);
			}

			buffer[offset++] = b0;
			buffer[offset++] = b1;
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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapInt16(byte[] buffer, int offset)
		{
			SwapUInt16(buffer, offset);
		}
		/// <summary>Replaces 2 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			short value)
		{
			ReplaceBytes(buffer, offset, (ushort)value);
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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapUInt32(byte[] buffer, int offset)
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
		}
		/// <summary>Replaces 4 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			uint value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(uint) <= buffer.Length);

			byte b0, b1, b2, b3;
			if(BitConverter.IsLittleEndian) {
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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapInt32(byte[] buffer, int offset)
		{
			SwapUInt32(buffer, offset);
		}
		/// <summary>Replaces 4 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			int value)
		{
			ReplaceBytes(buffer, offset, (uint)value);
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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapUInt64(byte[] buffer, int offset)
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
		}
		/// <summary>Replaces 8 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks>
		/// <paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment.
		/// Uses <see cref="BitConverter.IsLittleEndian" /> to determine <paramref name="value"/>'s byte ordering
		/// when written to the buffer
		/// </remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			ulong value)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+sizeof(ulong) <= buffer.Length);

			byte b0, b1, b2, b3, b4, b5, b6, b7;
			if(BitConverter.IsLittleEndian) {
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
		[Contracts.Pure]
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
		[Contracts.Pure]
		public static void SwapInt64(byte[] buffer, int offset)
		{
			SwapUInt64(buffer, offset);
		}
		/// <summary>Replaces 8 bytes in an array with a integer value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			long value)
		{
			ReplaceBytes(buffer, offset, (ulong)value);
		}
		#endregion

	};
}