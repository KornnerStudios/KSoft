using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	partial class Crc16
	{
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