using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	partial class Crc16
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BitComputer
		{
			Definition mDefinition;
			ushort mCrc;

			public BitComputer(Definition definition)
			{
				Contract.Requires(definition != null);

				mDefinition = definition;
				mCrc = mDefinition.InitialValue;
			}
			public BitComputer(Definition definition, ushort initialValue)
			{
				Contract.Requires(definition != null);

				mDefinition = definition;
				mCrc = initialValue;
			}

			public void ComputeBegin()
			{
				mCrc ^= mDefinition.XorIn;
			}

			public ushort ComputeFinish()
			{
				mCrc ^= mDefinition.XorOut;
				return mCrc;
			}

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset+length <= buffer.Length);

				for (int x = 0; x < length; x++)
					mDefinition.ComputeUpdate(buffer[offset+x], ref mCrc);
			}

			public void Compute(byte value)
			{
				mDefinition.ComputeUpdate(value, ref mCrc);
			}

			#region Compute 16-bits
			public void ComputeLE(ushort value)
			{
				mDefinition.ComputeUpdate((value & 0x00FFU) >> 0, ref mCrc);
				mDefinition.ComputeUpdate((value & 0xFF00U) >> 8, ref mCrc);
			}
			public void ComputeBE(ushort value)
			{
				mDefinition.ComputeUpdate((value & 0xFF00U) >> 8, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x00FFU) >> 0, ref mCrc);
			}
			#endregion

			#region Compute 32-bits
			public void Compute(Shell.EndianFormat byteOrder, uint value)
			{
				if (byteOrder == Shell.EndianFormat.Little)
					ComputeLE(value);
				else
					ComputeBE(value);
			}
			public void ComputeLE(uint value)
			{
				mDefinition.ComputeUpdate((value & 0x000000FFU) >> 0, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x0000FF00U) >> 8, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x00FF0000U) >> 16, ref mCrc);
				mDefinition.ComputeUpdate((value & 0xFF000000U) >> 24, ref mCrc);
			}
			public void ComputeBE(uint value)
			{
				mDefinition.ComputeUpdate((value & 0xFF000000U) >> 24, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x00FF0000U) >> 16, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x0000FF00U) >> 8, ref mCrc);
				mDefinition.ComputeUpdate((value & 0x000000FFU) >> 0, ref mCrc);
			}
			#endregion

			#region Compute 64-bits
			public void ComputeLE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = lo;
				ComputeLE(_value);

				_value = hi;
				ComputeLE(_value);
			}
			public void ComputeBE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = hi;
				ComputeBE(_value);

				_value = lo;
				ComputeBE(_value);
			}
			#endregion
		};
	};
}
