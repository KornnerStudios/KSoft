using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public static partial class Crc32
	{
		public class Definition
		{
			readonly uint mPolynomial;
			readonly uint[] mCrcTable;
			readonly uint mInitialValue;
			readonly uint mXorOut;

			public uint Polynomial { get { return mPolynomial; } }
			public uint[] CrcTable { get { return mCrcTable; } }
			public uint InitialValue { get { return mInitialValue; } }
			public uint XorOut { get { return mXorOut; } }

			static uint[] BuildCrcTable(uint polynomial)
			{
				uint[] crc_table = new uint[kCrcTableSize];

				for (uint index = 0; index < crc_table.Length; index++)
				{
					uint crc = index;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 1) == 1)
							crc = (crc >> 1) ^ polynomial;
						else
							crc >>= 1;
					}
					crc_table[index] = crc;
				}

				return crc_table;
			}

			public Definition(uint polynomial = kDefaultPolynomial, uint initialValue = uint.MaxValue, uint xorOut = 0, params uint[] crcTable)
			{
				Contract.Requires(crcTable.IsNullOrEmpty() || crcTable.Length == kCrcTableSize);

				mPolynomial = polynomial;
				mInitialValue = initialValue;
				mXorOut = xorOut;

				mCrcTable = crcTable.IsNullOrEmpty() 
					? BuildCrcTable(Polynomial) 
					: crcTable;
			}

			internal uint HashCore(uint crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					uint a = (crc >> 8) & 0x00FFFFFF; // don't include the top most byte in case there was somehow any carry
					uint b = CrcTable[(int)crc ^ array[index] & 0xFF];
					crc = a ^ b;
				}

				return crc;
			}
			public uint Crc(ref uint crc, byte[] buffer, int size)
			{
				if (crc == 0)
					crc = InitialValue;

				crc = HashCore(crc, buffer, 0, size);

				return crc ^ XorOut;
			}
		};
	};
}