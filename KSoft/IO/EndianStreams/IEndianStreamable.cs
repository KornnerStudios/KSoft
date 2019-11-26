using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>Interfaces object serialization with the endian streams</summary>
	[Contracts.ContractClass(typeof(IEndianStreamableContract))]
	public interface IEndianStreamable
	{
		/// <summary>Reads the object from the endian stream object</summary>
		/// <param name="s"></param>
		void Read(EndianReader s);
		/// <summary>Writes the object to the endian stream object</summary>
		/// <param name="s"></param>
		void Write(EndianWriter s);
	};

	[Contracts.ContractClassFor(typeof(IEndianStreamable))]
	abstract class IEndianStreamableContract : IEndianStreamable
	{
		public void Read(EndianReader s)	{ Contract.Requires(s != null); }
		public void Write(EndianWriter s)	{ Contract.Requires(s != null); }
	};
}