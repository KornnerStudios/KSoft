using System;

#nullable enable

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
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
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
						{
							crc = (crc << 1) ^ polynomial;
						}
						else
						{
							crc <<= 1;
						}
					}
					crc_table[index] = (ushort)crc;
				}

				System.Diagnostics.Debug.Assert(crc_table[1] != 0);
				System.Diagnostics.Debug.Assert(crc_table[crc_table.Length - 1] != 0);

				return crc_table;
			}

			public Definition(ushort polynomial = kDefaultPolynomial,
				ushort initialValue = ushort.MaxValue,
				ushort xorIn = 0,
				ushort xorOut = 0,
				params ushort[]? crcTable)
			{
				if (crcTable != null && crcTable.Length != 0 && crcTable.Length != kCrcTableSize)
				{
					throw new ArgumentException("CRC tables must contain 256 entries.", nameof(crcTable));
				}

				mPolynomial = polynomial;
				mInitialValue = initialValue;
				mXorIn = xorIn;
				mXorOut = xorOut;

				mCrcTable = crcTable == null || crcTable.Length == 0
					? BuildCrcTable(Polynomial)
					: crcTable;
			}

			public ushort ComputeUpdate(ushort crc, uint value)
			{
				value &= 0xFF;
				ushort a = (ushort) (crc << 8);
				// Don't include the topmost byte in case there was somehow any carry.
				ushort b = (ushort)((crc >> 8) & 0x00FFFFFF);
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
				{
					crc = InitialValue;
				}

				crc ^= XorIn;

				crc = HashCore(crc, buffer, 0, size);

				crc ^= XorOut;

				return crc;
			}
		};
	};
}
