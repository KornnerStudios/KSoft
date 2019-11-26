using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>Exposes the concept of a virtual buffer inside a stream</summary>
	/// <remarks>No bytes are IO'd. Purely position based</remarks>
	[Contracts.ContractClass(typeof(IKSoftStreamWithVirtualBufferContract))]
	public interface IKSoftStreamWithVirtualBuffer
	{
		Stream BaseStream { get; }

		/// <summary>Absolute position of the start of the virtual buffer</summary>
		long VirtualBufferStart { get; set; }
		/// <summary>How many bytes compose the virtual buffer</summary>
		long VirtualBufferLength { get; set; }
	};

	[Contracts.ContractClassFor(typeof(IKSoftStreamWithVirtualBuffer))]
	abstract class IKSoftStreamWithVirtualBufferContract : IKSoftStreamWithVirtualBuffer
	{
		public abstract Stream BaseStream { get; }

		public long VirtualBufferStart {
			get {
				Contract.Ensures(Contract.Result<long>() >= 0);

				throw new NotImplementedException();
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0);

				throw new NotImplementedException();
			}
		}
		public long VirtualBufferLength {
			get {
				Contract.Ensures(Contract.Result<long>() >= 0);

				throw new NotImplementedException();
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0);

				throw new NotImplementedException();
			}
		}
	};
}