﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	[Contracts.ContractClass(typeof(IByteSwappableContract))]
	public interface IByteSwappable
	{
		int SizeOf { get; }

		short[] ByteSwapCodes { get; }
	};
	[Contracts.ContractClassFor(typeof(IByteSwappable))]
	abstract class IByteSwappableContract : IByteSwappable
	{
		#region IByteSwappable Members
		public int SizeOf { get {
			Contract.Ensures(Contract.Result<int>() > 0, 
				"You can't define a zero-byte struct, so what's the point of this definition?");
			
			throw new NotImplementedException();
		} }

		public short[] ByteSwapCodes { get {
			Contract.Ensures(Contract.Result<short[]>() != null);
			Contract.Ensures(Contract.Result<short[]>().Length >= ByteSwap.kMinumumNumberOfDefinitionBsCodes,
				"Codes should include: ArrayStart, {Count}, {Elements}, and ArrayEnd");
			
			throw new NotImplementedException();
		} }

		#endregion
	};
}