#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public static partial class Crc32
	{
		public sealed class Definition
		{
			readonly uint mPolynomial;
			readonly uint[] mCrcTable;
			readonly uint mInitialValue;
			readonly uint mXorIn;
			readonly uint mXorOut;

			public uint Polynomial { get { return mPolynomial; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public uint[] CrcTable { get { return mCrcTable; } }
			public uint InitialValue { get { return mInitialValue; } }
			public uint XorIn { get { return mXorIn; } }
			public uint XorOut { get { return mXorOut; } }

			static uint[] BuildCrcTable(uint polynomial)
			{
				var crc_table = new uint[kCrcTableSize];

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

				Contract.Assert(crc_table[1] != 0);
				Contract.Assert(crc_table[crc_table.Length-1] != 0);

				return crc_table;
			}

			public Definition(uint polynomial = kDefaultPolynomial, uint initialValue = uint.MaxValue, uint xorIn = 0, uint xorOut = 0, params uint[] crcTable)
			{
				Contract.Requires(crcTable.IsNullOrEmpty() || crcTable.Length == kCrcTableSize);

				mPolynomial = polynomial;
				mInitialValue = initialValue;
				mXorIn = xorIn;
				mXorOut = xorOut;

				mCrcTable = crcTable.IsNullOrEmpty()
					? BuildCrcTable(Polynomial)
					: crcTable;
			}

			public uint ComputeUpdate(uint crc, uint value)
			{
				value &= 0xFF;
				uint a = (crc >> 8) & 0x00FFFFFF; // don't include the top most byte in case there was somehow any carry
				uint b = CrcTable[((int)crc ^ value) & 0xFF];
				return a ^ b;
			}

			public void ComputeUpdate(uint value, ref uint crc)
			{
				crc = ComputeUpdate(crc, value);
			}

			internal uint HashCore(uint crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					crc = ComputeUpdate(crc, array[index]);
				}

				return crc;
			}
			public uint Crc(ref uint crc, byte[] buffer, int size)
			{
				if (crc == 0)
					crc = InitialValue;

				crc ^= XorIn;

				crc = HashCore(crc, buffer, 0, size);

				crc ^= XorOut;

				return crc;
			}
		};
	};
}
