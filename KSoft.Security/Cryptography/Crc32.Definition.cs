using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public static partial class Crc32
	{
		public class Definition
		{
			public uint Polynomial { get; private set; }
			public uint[] CrcTable { get; private set; }
			public uint InitialValue { get; private set; }
			public uint XorOut { get; private set; }

			uint[] BuildCrcTable()
			{
				uint[] crc_table = new uint[kCrcTableSize];

				for (uint index = 0; index < crc_table.Length; index++)
				{
					uint crc = index;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 1) == 1)
							crc = (crc >> 1) ^ Polynomial;
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

				Polynomial = polynomial;
				InitialValue = initialValue;
				XorOut = xorOut;

				CrcTable = crcTable.IsNullOrEmpty() ? BuildCrcTable() : crcTable;
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