using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[Contracts.ContractClass(typeof(IBitStreamSerializableContract))]
	public interface IBitStreamSerializable
	{
		void Serialize(BitStream s);
	};

	[Contracts.ContractClassFor(typeof(IBitStreamSerializable))]
	abstract class IBitStreamSerializableContract : IBitStreamSerializable
	{
		public void Serialize(BitStream s)	{ Contract.Requires(s != null); }
	};
}