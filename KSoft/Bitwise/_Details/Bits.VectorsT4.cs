using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	partial class Bits
	{
		#region Bit Vector length calculations
		/// <summary>Calculates how many <see cref="System.Byte"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInBytes(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (kByteBitCount-1)) >> kByteBitShift;
		}

		/// <summary>Calculates how many <see cref="System.Int16"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt16(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (kInt16BitCount-1)) >> kInt16BitShift;
		}

		/// <summary>Calculates how many <see cref="System.Int32"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt32(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (kInt32BitCount-1)) >> kInt32BitShift;
		}

		/// <summary>Calculates how many <see cref="System.Int64"/>s are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorLengthInInt64(int bitsCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitsCount >= 0);

			return (bitsCount + (kInt64BitCount-1)) >> kInt64BitShift;
		}

		#endregion

		#region Bit Vector element bitmask (kVectorWordFormat dependent)
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementBitMaskInBytesLE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const byte k_one = 1;

			return (byte)(k_one << (bitIndex % kByteBitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementBitMaskInBytesBE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const byte k_one = 1;
			const byte k_most_significant_bit = k_one << (kByteBitCount - 1);

			return (byte)(k_most_significant_bit >> (bitIndex % kByteBitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte VectorElementBitMaskInBytes(int bitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementBitMaskInBytesBE(bitIndex) :
					VectorElementBitMaskInBytesLE(bitIndex);
		}
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<byte> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementBitMaskInBytesBE;
			else
				proc = VectorElementBitMaskInBytesLE;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementBitMaskInInt16LE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ushort k_one = 1;

			return (ushort)(k_one << (bitIndex % kInt16BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementBitMaskInInt16BE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ushort k_one = 1;
			const ushort k_most_significant_bit = k_one << (kInt16BitCount - 1);

			return (ushort)(k_most_significant_bit >> (bitIndex % kInt16BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort VectorElementBitMaskInInt16(int bitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementBitMaskInInt16BE(bitIndex) :
					VectorElementBitMaskInInt16LE(bitIndex);
		}
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<ushort> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementBitMaskInInt16BE;
			else
				proc = VectorElementBitMaskInInt16LE;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementBitMaskInInt32LE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const uint k_one = 1;

			return (uint)(k_one << (bitIndex % kInt32BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementBitMaskInInt32BE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const uint k_one = 1;
			const uint k_most_significant_bit = k_one << (kInt32BitCount - 1);

			return (uint)(k_most_significant_bit >> (bitIndex % kInt32BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint VectorElementBitMaskInInt32(int bitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementBitMaskInInt32BE(bitIndex) :
					VectorElementBitMaskInInt32LE(bitIndex);
		}
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<uint> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementBitMaskInInt32BE;
			else
				proc = VectorElementBitMaskInInt32LE;
		}

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementBitMaskInInt64LE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ulong k_one = 1;

			return (ulong)(k_one << (bitIndex % kInt64BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementBitMaskInInt64BE(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);
			const ulong k_one = 1;
			const ulong k_most_significant_bit = k_one << (kInt64BitCount - 1);

			return (ulong)(k_most_significant_bit >> (bitIndex % kInt64BitCount));
		}
		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong VectorElementBitMaskInInt64(int bitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementBitMaskInInt64BE(bitIndex) :
					VectorElementBitMaskInInt64LE(bitIndex);
		}
		public static void GetVectorElementBitMaskInT(out VectorElementBitMask<ulong> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementBitMaskInInt64BE;
			else
				proc = VectorElementBitMaskInInt64LE;
		}

		#endregion

		#region Bit Vector element section bitmask (kVectorWordFormat dependent)
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementSectionBitMaskInBytesLE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (byte)(byte.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static byte VectorElementSectionBitMaskInBytesBE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (byte)(byte.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Byte"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static byte VectorElementSectionBitMaskInBytes(int startBitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementSectionBitMaskInBytesBE(startBitIndex) :
					VectorElementSectionBitMaskInBytesLE(startBitIndex);
		}
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<byte> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementSectionBitMaskInBytesBE;
			else
				proc = VectorElementSectionBitMaskInBytesLE;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementSectionBitMaskInInt16LE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ushort)(ushort.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ushort VectorElementSectionBitMaskInInt16BE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ushort)(ushort.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int16"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ushort VectorElementSectionBitMaskInInt16(int startBitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementSectionBitMaskInInt16BE(startBitIndex) :
					VectorElementSectionBitMaskInInt16LE(startBitIndex);
		}
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<ushort> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementSectionBitMaskInInt16BE;
			else
				proc = VectorElementSectionBitMaskInInt16LE;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementSectionBitMaskInInt32LE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (uint)(uint.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static uint VectorElementSectionBitMaskInInt32BE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (uint)(uint.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int32"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static uint VectorElementSectionBitMaskInInt32(int startBitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementSectionBitMaskInInt32BE(startBitIndex) :
					VectorElementSectionBitMaskInInt32LE(startBitIndex);
		}
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<uint> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementSectionBitMaskInInt32BE;
			else
				proc = VectorElementSectionBitMaskInInt32LE;
		}

		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementSectionBitMaskInInt64LE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ulong)(ulong.MaxValue << startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <returns></returns>
		[Contracts.Pure]
		/*public*/ static ulong VectorElementSectionBitMaskInInt64BE(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return (ulong)(ulong.MaxValue >> startBitIndex);
		}
		/// <summary>Get the mask for a section of bits in a vector, relative to the vector's element size (<see cref="System.Int64"/>)</summary>
		/// <param name="startBitIndex">Bit index to begin the mask at</param>
		/// <param name="byteOrder">Order in which bits are enumerated (first to last)</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static ulong VectorElementSectionBitMaskInInt64(int startBitIndex,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);

			return byteOrder == Shell.EndianFormat.Big ?
					VectorElementSectionBitMaskInInt64BE(startBitIndex) :
					VectorElementSectionBitMaskInInt64LE(startBitIndex);
		}
		public static void GetVectorElementSectionBitMaskInT(out VectorElementBitMask<ulong> proc,
			Shell.EndianFormat byteOrder = kVectorWordFormat)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			if (byteOrder == Shell.EndianFormat.Big)
				proc = VectorElementSectionBitMaskInInt64BE;
			else
				proc = VectorElementSectionBitMaskInInt64LE;
		}

		#endregion

		#region Bit Vector element from byte[]
		[Contracts.Pure]
		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref byte element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(byte) <= buffer.Length);

			element = buffer[index];
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<byte> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		[Contracts.Pure]
		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref ushort element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(ushort) <= buffer.Length);

			element = BitConverter.ToUInt16(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<ushort> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		[Contracts.Pure]
		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref uint element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(uint) <= buffer.Length);

			element = BitConverter.ToUInt32(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<uint> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		[Contracts.Pure]
		public static void VectorElementFromBufferInT(byte[] buffer, int index, ref ulong element)
		{
			Contract.Requires/*<ArgumentNullException>*/(buffer != null);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index >= 0);
			Contract.Requires/*<ArgumentOutOfRangeException>*/(index+sizeof(ulong) <= buffer.Length);

			element = BitConverter.ToUInt64(buffer, index);
		}
		public static void GetVectorElementFromBufferInT(out VectorElementFromBuffer<ulong> proc)
		{
			Contract.Ensures(Contract.ValueAtReturn(out proc) != null);

			proc = VectorElementFromBufferInT;
		}

		#endregion

		#region Bit Vector bitIndex to vector_index
		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Byte"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Byte"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInBytes(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> kByteBitShift;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int16"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int16"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt16(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> kInt16BitShift;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int32"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int32"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt32(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> kInt32BitShift;
		}

		/// <summary>Get the vector index of a bit index, for a vector represented in <see cref="System.Int64"/>s</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>The index of a <see cref="System.Int64"/> which holds the bit in question</returns>
		[Contracts.Pure]
		public static int VectorIndexInInt64(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			return bitIndex >> kInt64BitShift;
		}

		#endregion

		#region Bit Vector cursor to bitIndex
		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Byte"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInBytes(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << kByteBitShift) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int16"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt16(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << kInt16BitShift) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int32"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt32(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << kInt32BitShift) + bitOffset;
		}

		/// <summary>Calculates the bit position of a vector cursor based on <see cref="System.Int64"/> elements</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static int VectorBitIndexInInt64(int index, int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(bitOffset >= 0);

			return (index << kInt64BitShift) + bitOffset;
		}

		#endregion

		#region Bit Vector cursor from bitIndex
		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Byte"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		[Contracts.Pure]
		public static void VectorBitCursorInBytes(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInBytes(bitIndex);
			bitOffset = bitIndex & kByteBitMod;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int16"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		[Contracts.Pure]
		public static void VectorBitCursorInInt16(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt16(bitIndex);
			bitOffset = bitIndex & kInt16BitMod;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int32"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		[Contracts.Pure]
		public static void VectorBitCursorInInt32(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt32(bitIndex);
			bitOffset = bitIndex & kInt32BitMod;
		}

		/// <summary>Calculates the vector cursor based on a bit index in a <see cref="System.Int64"/> vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		[Contracts.Pure]
		public static void VectorBitCursorInInt64(int bitIndex, out int index, out int bitOffset)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0);

			index = VectorIndexInInt64(bitIndex);
			bitOffset = bitIndex & kInt64BitMod;
		}

		#endregion
	};
}