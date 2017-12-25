using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public static partial class Crc16
	{
		public sealed class Definition
		{
			readonly ushort mPolynomial;
			readonly ushort[] mCrcTable;
			readonly ushort mInitialValue;
			readonly ushort mXorIn;
			readonly ushort mXorOut;

			public ushort Polynomial { get { return mPolynomial; } }
			public ushort[] CrcTable { get { return mCrcTable; } }
			public ushort InitialValue { get { return mInitialValue; } }
			public ushort XorIn { get { return mXorIn; } }
			public ushort XorOut { get { return mXorOut; } }

			static ushort[] BuildCrcTable(uint polynomial)
			{
				var crc_table = new ushort[kCrcTableSize];

				for (uint index = 0; index < crc_table.Length; index++)
				{
					uint crc = index << 8;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 0x8000) != 0)
							crc = (crc << 1) ^ polynomial;
						else
							crc <<= 1;
					}
					crc_table[index] = (ushort)crc;
				}

				Contract.Assert(crc_table[1] != 0);
				Contract.Assert(crc_table[crc_table.Length-1] != 0);

				return crc_table;
			}

			public Definition(ushort polynomial = kDefaultPolynomial, ushort initialValue = ushort.MaxValue, ushort xorIn = 0, ushort xorOut = 0, params ushort[] crcTable)
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

			public ushort ComputeUpdate(ushort crc, uint value)
			{
				value &= 0xFF;
				ushort a = (ushort) (crc << 8);
				ushort b = (ushort)((crc >> 8) & 0x00FFFFFF); // don't include the top most byte in case there was somehow any carry
				ushort c = CrcTable[(b ^ value) & 0xFF];
				return (ushort)(a ^ c);
			}

			public void ComputeUpdate(uint value, ref ushort crc)
			{
				crc = ComputeUpdate(crc, value);
			}

			internal ushort HashCore(ushort crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					crc = ComputeUpdate(crc, array[index]);
				}

				return crc;
			}
			public ushort Crc(ref ushort crc, byte[] buffer, int size)
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