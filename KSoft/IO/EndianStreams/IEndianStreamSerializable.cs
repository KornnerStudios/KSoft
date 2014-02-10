using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	[Contracts.ContractClass(typeof(IEndianStreamSerializableContract))]
	public interface IEndianStreamSerializable
	{
		void Serialize(EndianStream s);
	};

	[Contracts.ContractClassFor(typeof(IEndianStreamSerializable))]
	abstract class IEndianStreamSerializableContract : IEndianStreamSerializable
	{
		public void Serialize(EndianStream s)	{ Contract.Requires(s != null); }
	};
}