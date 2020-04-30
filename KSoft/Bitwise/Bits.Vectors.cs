using System;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		/// <summary>Order in which bits are enumerated (first to last) in bitvectors</summary>
		/// <remarks>Currently MSB to LSB</remarks>
		public const Shell.EndianFormat kVectorWordFormat = Shell.EndianFormat.Big;


		/// <summary>Calculates how many elements (T) are needed to hold a bit vector of a certain length</summary>
		/// <param name="bitsCount">Number of bits to be hosted in the vector</param>
		/// <returns></returns>
		public delegate int VectorLengthInT(int bitsCount);
		#region Bit Vector length calculations
		[Contracts.Pure]
		[SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public static VectorLengthInT GetVectorLengthInT<T>()
			where T : struct
		{
			Contract.Ensures(Contract.Result<VectorLengthInT>() != null);

			TypeCode c = Type.GetTypeCode(typeof(T));

			switch(c)
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
					return VectorLengthInBytes;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					return VectorLengthInInt16;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					return VectorLengthInInt32;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					return VectorLengthInInt64;

				default: throw new ArgumentException(c.ToString(), nameof(T));
			}
		}
		#endregion

		/// <summary>Get the mask for a specific bit in a vector, relative to the vector's element size</summary>
		/// <typeparam name="T">Underlying bit vector's element type</typeparam>
		/// <param name="bitIndex">Bit index to get the mask for</param>
		/// <returns></returns>
		public delegate T VectorElementBitMask<out T>(int bitIndex)
			where T : struct;

		public delegate void VectorElementFromBuffer<T>(byte[] buffer, int index, ref T element)
			where T : struct;

		/// <summary>Get the vector index of a bit index, for a vector represented by a specific element type</summary>
		/// <param name="bitIndex">Index of the bit which we want the vector index of</param>
		/// <returns>Index of an element (T) which holds the bit in question</returns>
		public delegate int VectorIndexInT(int bitIndex);
		#region Bit Vector bitIndex to vector_index
		[Contracts.Pure]
		[SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public static VectorIndexInT GetVectorIndexInT<T>()
			where T : struct
		{
			Contract.Ensures(Contract.Result<VectorIndexInT>() != null);

			TypeCode c = Type.GetTypeCode(typeof(T));

			switch(c)
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
					return VectorIndexInBytes;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					return VectorIndexInInt16;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					return VectorIndexInInt32;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					return VectorIndexInInt64;

				default: throw new ArgumentException(c.ToString(), nameof(T));
			}
		}
		#endregion

		/// <summary>Calculates the bit position of a vector cursor based on a specific element (T) type</summary>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		/// <returns></returns>
		public delegate int VectorBitIndexInT(int index, int bitOffset);
		#region Bit Vector cursor to bitIndex
		[Contracts.Pure]
		[SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public static VectorBitIndexInT GetVectorBitIndexInT<T>()
			where T : struct
		{
			Contract.Ensures(Contract.Result<VectorBitIndexInT>() != null);

			TypeCode c = Type.GetTypeCode(typeof(T));

			switch (c)
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
					return VectorBitIndexInBytes;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					return VectorBitIndexInInt16;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					return VectorBitIndexInInt32;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					return VectorBitIndexInInt64;

				default: throw new ArgumentException(c.ToString(), nameof(T));
			}
		}
		#endregion

		/// <summary>Calculates the vector cursor based on a bit index in a specific element (T) vector</summary>
		/// <param name="bitIndex">Index to translate into a cursor</param>
		/// <param name="index">Element index of the cursor</param>
		/// <param name="bitOffset">Element bit offset of the current</param>
		public delegate void VectorBitCursorInT(int bitIndex, out int index, out int bitOffset);
		#region Bit Vector cursor from bitIndex
		[Contracts.Pure]
		[SuppressMessage("Microsoft.Design", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public static VectorBitCursorInT GetVectorBitCursorInT<T>()
			where T : struct
		{
			Contract.Ensures(Contract.Result<VectorBitCursorInT>() != null);

			TypeCode c = Type.GetTypeCode(typeof(T));

			switch (c)
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
					return VectorBitCursorInBytes;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					return VectorBitCursorInInt16;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					return VectorBitCursorInInt32;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					return VectorBitCursorInInt64;

				default: throw new ArgumentException(c.ToString(), nameof(T));
			}
		}
		#endregion
	};
}
