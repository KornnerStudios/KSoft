using System;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

#nullable enable

namespace KSoft.Bitwise
{
	/// <summary>Pre-defined byte swapping codes</summary>
	[EnumBitEncoderDisable]
	public enum BsCode : short
	{
		/// <summary>Byte-swap code for 8 bits of data</summary>
		Byte = 1,
		/// <summary>Byte-swap code for 16 bits of data</summary>
//		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Int16 = -2,
		/// <summary>Byte-swap code for 32 bits of data</summary>
//		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
		Int32 = -4,
		/// <summary>Byte-swap code for 64 bits of data</summary>
//		[SuppressMessage("Microsoft.Design", "CA1720:IdentifiersShouldNotContainTypeNames")]
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
		// ArrayStart, {Count}, {Elements}, ArrayEnd
		internal const int kMinumumNumberOfDefinitionBsCodes = 4;

		//[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public readonly struct BsDefinition
			: IByteSwappable
		{
			readonly string kName;
			public readonly override string ToString()	=> kName;
			readonly short[] kBsCodes;
//			[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public readonly short[] ByteSwapCodes	=> kBsCodes;
			readonly int kSizeOf;
			public readonly int SizeOf				=> kSizeOf;

			public BsDefinition(string name, int sizeOf, params short[] bsCodes)
			{
				if (string.IsNullOrEmpty(name))
				{
					throw new ArgumentNullException(nameof(name));
				}
				ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeOf);
				ArgumentNullException.ThrowIfNull(bsCodes);
				if (bsCodes.Length < kMinumumNumberOfDefinitionBsCodes)
				{
					throw new ArgumentException("Codes should include: ArrayStart, {Count}, {Elements}, and ArrayEnd",
						nameof(bsCodes));
				}

				kName = name;
				kSizeOf = sizeOf;
				kBsCodes = bsCodes;
			}
		};

		/// <summary>Byte swap a given structure a number of times over a range of bytes</summary>
		/// <param name="definition">Structure definition in terms of byte swap codes</param>
		/// <param name="buffer">Buffer containing the bytes of an instance of the definition</param>
		/// <param name="startIndex">Where to start processing the definition in the buffer</param>
		/// <param name="count">Number of times to process the definition on the buffer</param>
		/// <returns>Offset in <paramref name="buffer"/> where processing ended</returns>
		public static int SwapData(IByteSwappable definition, byte[] buffer,
			int startIndex = 0, int count = 1)
		{
			Contract.Ensures(Contract.Result<int>() >= 0);

			ArgumentNullException.ThrowIfNull(definition);
			ArgumentNullException.ThrowIfNull(buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, buffer.Length);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
			if ((long)count * definition.SizeOf > buffer.Length - startIndex)
			{
				throw new ArgumentOutOfRangeException(nameof(count),
					"buffer doesn't have enough data for the given byte swap parameters");
			}

			var swap = new Swapper(definition);
			int buffer_index = startIndex;
			for (int elements_remaining = count; elements_remaining > 0; elements_remaining--)
			{
				buffer_index = swap.SwapData(buffer, buffer_index);
			}

			Contract.Assert(buffer_index == (startIndex + (definition.SizeOf * count)));
			return buffer_index;
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

		#region Single
		/// <summary>Swaps a <see cref="Single" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static float SwapSingle(
			float value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter bit helpers.
			return BitConverter.UInt32BitsToSingle(
				SwapUInt32(BitConverter.SingleToUInt32Bits(value)));
		}
		/// <summary>Swaps a <see cref="Single" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapSingle(
			ref float value)
		{
			value = SwapSingle(value);
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
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter.UInt32BitsToSingle.
			return BitConverter.UInt32BitsToSingle(bits);
		}
		public static uint SingleToUInt32(float value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter.SingleToUInt32Bits.
			return BitConverter.SingleToUInt32Bits(value);
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
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter bit helpers.
			return BitConverter.UInt64BitsToDouble(
				SwapUInt64(BitConverter.DoubleToUInt64Bits(value)));
		}
		/// <summary>Swaps a <see cref="Double" /> by reference</summary>
		/// <param name="value"></param>
		[Contracts.Pure]
		public static void SwapDouble(
			ref double value)
		{
			value = SwapDouble(value);
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
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter.UInt64BitsToDouble.
			return BitConverter.UInt64BitsToDouble(bits);
		}
		public static ulong DoubleToUInt64(double value)
		{
			// #VITA_SHIM: Keep KSoft API while callers migrate to BitConverter.DoubleToUInt64Bits.
			return BitConverter.DoubleToUInt64Bits(value);
		}
		#endregion
	};
}
