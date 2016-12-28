using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.WPF
{
	[Contracts.ContractClass(typeof(IBitVectorUserInterfaceDataContract))]
	public interface IBitVectorUserInterfaceData
	{
		int NumberOfBits { get; }
		string GetDisplayName(int bitIndex);
		string GetDescription(int bitIndex);
		bool IsVisible(int bitIndex);
	};

	[Contracts.ContractClassFor(typeof(IBitVectorUserInterfaceData))]
	abstract class IBitVectorUserInterfaceDataContract : IBitVectorUserInterfaceData
	{
		public int NumberOfBits { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }

		public string GetDisplayName(int bitIndex)
		{
			Contract.Requires(bitIndex >= 0 && bitIndex < NumberOfBits);
			Contract.Ensures(Contract.Result<string>() != null);

			throw new NotImplementedException();
		}

		public string GetDescription(int bitIndex)
		{
			Contract.Requires(bitIndex >= 0 && bitIndex < NumberOfBits);
			Contract.Ensures(Contract.Result<string>() != null);

			throw new NotImplementedException();
		}

		public bool IsVisible(int bitIndex)
		{
			Contract.Requires(bitIndex >= 0 && bitIndex < NumberOfBits);

			throw new NotImplementedException();
		}
	};
}