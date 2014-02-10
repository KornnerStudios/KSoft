using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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