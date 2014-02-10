using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	/// <summary>Pre-defined byte swapping codes</summary>
	[EnumBitEncoderDisable]
	public enum BsCode : short
	{
		/// <summary>Byte-swap code for 8 bits of data</summary>
		Byte = 1,
		/// <summary>Byte-swap code for 16 bits of data</summary>
		Int16 = -2,
		/// <summary>Byte-swap code for 32 bits of data</summary>
		Int32 = -4,
		/// <summary>Byte-swap code for 64 bits of data</summary>
		Int64 = -8,

		/// <summary>
		/// Byte-swap code for the start of a repeated table of byte swap codes.
		/// Next int in the byte swap code list is the amount of times to repeat the codes
		/// </summary>
		ArrayStart = -100,
		/// <summary>Byte-swap code for the end of a repeated table of byte swap codes</summary>
		ArrayEnd = -101,
	};

	public static partial class ByteSwap
	{
		public const int kSizeOfInt24 = sizeof(byte) * 3;

		// ArrayStart, {Count}, {Elements}, ArrayEnd
		internal const int kMinumumNumberOfDefinitionBsCodes = 4;

		struct BsDefinition : IByteSwappable
		{
			readonly string kName;
			public override string ToString()	{ return kName; }
			readonly short[] kBsCodes;
			public short[] ByteSwapCodes		{ get { return kBsCodes; } }
			readonly int kSizeOf;
			public int SizeOf { get { return kSizeOf; } }

			public BsDefinition(string name, int size_of, params short[] bs_codes)
			{
				Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
				Contract.Requires<ArgumentOutOfRangeException>(size_of > 0);
				Contract.Requires<ArgumentNullException>(bs_codes != null);
				Contract.Requires<ArgumentException>(bs_codes.Length >= kMinumumNumberOfDefinitionBsCodes);

				kName = name;
				kSizeOf = size_of;
				kBsCodes = bs_codes;
			}
		};

		public static int SwapData(IByteSwappable definition, byte[] buffer, 
			int startIndex = 0, int count = 1)
		{
			Contract.Requires<ArgumentNullException>(definition != null);
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(count > 0);
			Contract.Requires<ArgumentOutOfRangeException>((count*definition.SizeOf) <= (buffer.Length-startIndex),
				"buffer doesn't have enough data for the given byte swap parameters");
			Contract.Ensures(Contract.Result<int>() >= 0);

			if (count == 0)
				return 0;

			var swap = new Swapper(definition);
			swap.SwapData(buffer, 0);

			return definition.SizeOf;
		}

		#region Int byte swap definitions
		/// <summary>Byte-swap definition for a 16-bit integer</summary>
		public static readonly IByteSwappable kInt16Definition = new BsDefinition("Int16", sizeof(short), 
			(short)BsCode.ArrayStart, 1,
			(short)BsCode.Int16,
			(short)BsCode.ArrayEnd);
		/// <summary>Byte-swap definition for a 32-bit integer</summary>
		public static readonly IByteSwappable kInt32Definition = new BsDefinition("Int32", sizeof(int), 
			(short)BsCode.ArrayStart, 1,
			(short)BsCode.Int32,
			(short)BsCode.ArrayEnd);
		/// <summary>Byte-swap definition for a 64-bit integer</summary>
		public static readonly IByteSwappable kInt64Definition = new BsDefinition("Int64", sizeof(long),
			(short)BsCode.ArrayStart, 1,
			(short)BsCode.Int64,
			(short)BsCode.ArrayEnd);
		#endregion

		#region UInt24
		/// <summary>Swaps a UInt24 and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint SwapUInt24(
			uint value)
		{
			return 
				((value >> 16) & 0x000000FF) |
				((value >>  0) & 0x0000FF00) |
				((value << 16) & 0x00FF0000);
		}
		/// <summary>Swaps a UInt24 by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapUInt24(
			ref uint value)
		{
			value =
				((value >> 16) & 0x000000FF) |
				((value >>  0) & 0x0000FF00) |
				((value << 16) & 0x00FF0000);
		}
		/// <summary>Swaps a UInt24 at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		[Contracts.Pure]
		public static void SwapUInt24(byte[] buffer, int offset)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset >= 0 && offset < buffer.Length);
			Contract.Requires<ArgumentOutOfRangeException>(
				offset+kSizeOfInt24 <= buffer.Length);

			byte 
				b0 = buffer[offset++], 
				b1 = buffer[offset++], 
				b2 = buffer[offset++];

			buffer[--offset] = b0;
			buffer[--offset] = b1;
			buffer[--offset] = b2;
		}
		#endregion
		// TODO: verify we don't need any sign-extension magic
		#region Int24
		/// <summary>Swaps a Int24 and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int SwapInt24(
			int value)
		{
			return 
				((value >> 16) & 0x000000FF) |
				((value >>  0) & 0x0000FF00) |
				((value << 16) & 0x00FF0000);
		}
		/// <summary>Swaps a Int24 by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapInt24(
			ref int value)
		{
			value =
				((value >> 16) & 0x000000FF) |
				((value >>  0) & 0x0000FF00) |
				((value << 16) & 0x00FF0000);
		}
		/// <summary>Swaps a Int24 at a position in a bye array</summary>
		/// <param name="buffer">source array</param>
		/// <param name="offset">offset at which to perform the byte swap</param>
		[Contracts.Pure]
		public static void SwapInt24(byte[] buffer, int offset)
		{
			SwapUInt24(buffer, offset);
		}
		#endregion
		#region Single
		/// <summary>Swaps a <see cref="Single" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static float SwapSingle(
			float value)
		{
			return new SingleUnion(
				SwapUInt32(new SingleUnion(value).Integer)
				).Real;
		}
		/// <summary>Swaps a <see cref="Single" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapSingle(
			ref float value)
		{
			var union = new SingleUnion(value);
			Swap(ref union.Integer);
			value = union.Real;
		}
		/// <summary>Replaces 4 bytes in an array with a floating-point value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			float value)
		{
			ReplaceBytes(buffer, offset, new SingleUnion(value).Integer);
		}

		public static float SingleFromUInt32(uint bits)
		{
			var union = new SingleUnion(bits);

			return union.Real;
		}
		public static uint SingleToUInt32(float value)
		{
			var union = new SingleUnion(value);

			return union.Integer;
		}
		#endregion
		#region Double
		/// <summary>Swaps a <see cref="Double" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static double SwapDouble(
			double value)
		{
			return new DoubleUnion(
				SwapUInt64(new DoubleUnion(value).Integer)
				).Real;
		}
		/// <summary>Swaps a <see cref="Double" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapDouble(
			ref double value)
		{
			var union = new DoubleUnion(value);
			Swap(ref union.Integer);
			value = union.Real;
		}
		/// <summary>Replaces 8 bytes in an array with a floating-point value</summary>
		/// <param name="buffer">byte buffer</param>
		/// <param name="offset">offset in <paramref name="buffer"/> to put the new value</param>
		/// <param name="value">value to replace the buffer's current bytes with</param>
		/// <remarks><paramref name="buffer"/>'s endian order is assumed to be the same as the current operating environment</remarks>
		[Contracts.Pure]
		public static void ReplaceBytes(byte[] buffer, int offset,
			double value)
		{
			ReplaceBytes(buffer, offset, new DoubleUnion(value).Integer);
		}

		public static double DoubleFromUInt64(ulong bits)
		{
			var union = new DoubleUnion(bits);

			return union.Real;
		}
		public static ulong DoubleToUInt64(double value)
		{
			var union = new DoubleUnion(value);

			return union.Integer;
		}
		#endregion
	};
}