using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;


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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Describes byte-swap operations.")]
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

			ArgumentNullException.ThrowIfNull(definition);
			Verify.Buffers.StartIndexWithinLength(buffer, startIndex);
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

			int expected_buffer_index = startIndex + (definition.SizeOf * count);
			if (buffer_index != expected_buffer_index)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Byte swap consumed {0} bytes; expected {1}.",
					buffer_index - startIndex, expected_buffer_index - startIndex));
			}
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
		public static float SwapSingle(
			float value)
		{
			return BitConverter.UInt32BitsToSingle(
				BinaryPrimitives.ReverseEndianness(BitConverter.SingleToUInt32Bits(value)));
		}
		/// <summary>Swaps a <see cref="Single" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapSingle(
			ref float value)
		{
			value = SwapSingle(value);
		}
		#endregion
		#region Double
		/// <summary>Swaps a <see cref="Double" /> and returns the result</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double SwapDouble(
			double value)
		{
			return BitConverter.UInt64BitsToDouble(
				BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToUInt64Bits(value)));
		}
		/// <summary>Swaps a <see cref="Double" /> by reference</summary>
		/// <param name="value"></param>
		public static void SwapDouble(
			ref double value)
		{
			value = SwapDouble(value);
		}
		#endregion
	};
}
