using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class Bits
	{
		#region kBitCount
		/// <summary>Number of bits in a <see cref="System.Char"/></summary>
		public const int kCharBitCount = sizeof(char) * 8;

		/// <summary>Number of bits in a <see cref="System.Byte"/></summary>
		public const int kByteBitCount = sizeof(byte) * 8;

		/// <summary>Number of bits in a <see cref="System.SByte"/></summary>
		public const int kSByteBitCount = sizeof(sbyte) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt16"/></summary>
		public const int kUInt16BitCount = sizeof(ushort) * 8;

		/// <summary>Number of bits in a <see cref="System.Int16"/></summary>
		public const int kInt16BitCount = sizeof(short) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt32"/></summary>
		public const int kUInt32BitCount = sizeof(uint) * 8;

		/// <summary>Number of bits in a <see cref="System.Int32"/></summary>
		public const int kInt32BitCount = sizeof(int) * 8;

		/// <summary>Number of bits in a <see cref="System.UInt64"/></summary>
		public const int kUInt64BitCount = sizeof(ulong) * 8;

		/// <summary>Number of bits in a <see cref="System.Int64"/></summary>
		public const int kInt64BitCount = sizeof(long) * 8;

		/// <summary>Number of bits in a <see cref="System.Single"/></summary>
		public const int kSingleBitCount = sizeof(float) * 8;

		/// <summary>Number of bits in a <see cref="System.Double"/></summary>
		public const int kDoubleBitCount = sizeof(double) * 8;

		#endregion

		#region kBitShift
		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Byte"/> element</summary>
		public const int kByteBitShift =	3;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.SByte"/> element</summary>
		public const int kSByteBitShift =	3;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt16"/> element</summary>
		public const int kUInt16BitShift =	4;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int16"/> element</summary>
		public const int kInt16BitShift =	4;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt32"/> element</summary>
		public const int kUInt32BitShift =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int32"/> element</summary>
		public const int kInt32BitShift =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.UInt64"/> element</summary>
		public const int kUInt64BitShift =	6;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Int64"/> element</summary>
		public const int kInt64BitShift =	6;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Single"/> element</summary>
		public const int kSingleBitShift =	5;

		/// <summary>Bit shift value for getting the bit count of a an <see cref="System.Double"/> element</summary>
		public const int kDoubleBitShift =	6;

		#endregion

		#region kBitMod
		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Byte"/> elements</summary>
		public const int kByteBitMod = 7;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.SByte"/> elements</summary>
		public const int kSByteBitMod = 7;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt16"/> elements</summary>
		public const int kUInt16BitMod = 15;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int16"/> elements</summary>
		public const int kInt16BitMod = 15;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt32"/> elements</summary>
		public const int kUInt32BitMod = 31;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int32"/> elements</summary>
		public const int kInt32BitMod = 31;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.UInt64"/> elements</summary>
		public const int kUInt64BitMod = 63;

		/// <summary>Bitwise AND value for emulating modulus operations on <see cref="System.Int64"/> elements</summary>
		public const int kInt64BitMod = 63;

		#endregion

		#region kBitmaskLookup
		/// <summary>Bit count to bit-mask look up table for 8-bit words</summary>
		public static readonly byte[] kBitmaskLookup8;

		/// <summary>Bit count to bit-mask look up table for 16-bit words</summary>
		public static readonly ushort[] kBitmaskLookup16;

		/// <summary>Bit count to bit-mask look up table for 32-bit words</summary>
		public static readonly uint[] kBitmaskLookup32;

		/// <summary>Bit count to bit-mask look up table for 64-bit words</summary>
		public static readonly ulong[] kBitmaskLookup64;

		#endregion

		public static bool GetBitConstants(Type integerType,
			out int byteCount, out int bitCount, out int bitShift, out int bitMod)
		{
			Contract.Requires/*<ArgumentNullException>*/(integerType != null);

			byteCount = bitCount = bitShift = bitMod = TypeExtensions.kNoneInt32;

			switch(Type.GetTypeCode(integerType))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					byteCount = sizeof(byte);
					bitCount = kByteBitCount;
					bitShift = kByteBitShift;
					bitMod = kByteBitMod;
					break;

				case TypeCode.UInt16:
				case TypeCode.Int16:
					byteCount = sizeof(ushort);
					bitCount = kUInt16BitCount;
					bitShift = kUInt16BitShift;
					bitMod = kUInt16BitMod;
					break;

				case TypeCode.UInt32:
				case TypeCode.Int32:
					byteCount = sizeof(uint);
					bitCount = kUInt32BitCount;
					bitShift = kUInt32BitShift;
					bitMod = kUInt32BitMod;
					break;

				case TypeCode.UInt64:
				case TypeCode.Int64:
					byteCount = sizeof(ulong);
					bitCount = kUInt64BitCount;
					bitShift = kUInt64BitShift;
					bitMod = kUInt64BitMod;
					break;

				default: return false;
			}

			return true;
		}
	};
}