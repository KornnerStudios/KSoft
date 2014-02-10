using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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

		/// <summary>Begin the concept of a virtual buffer</summary>
		/// <param name="bufferLength">Virtual buffer's byte length</param>
		/// <returns></returns>
		IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer(long bufferLength);
		/// <summary>Begin the concept of a virtual buffer</summary>
		/// <returns></returns>
		IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer();
		/// <summary>Temporarily bookmark this stream's VirtualBuffer properties</summary>
		/// <returns></returns>
		IKSoftStreamWithVirtualBufferBookmark EnterVirtualBufferBookmark();
		/// <summary>
		/// Temporarily bookmark this stream's VirtualBuffer properties and begin the concept of a virtual buffer
		/// </summary>
		/// <returns></returns>
		IKSoftStreamWithVirtualBufferAndBookmark EnterVirtualBufferWithBookmark(long bufferLength);
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

		public IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer(long bufferLength)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bufferLength > 0);

			throw new NotImplementedException();
		}

		public abstract IKSoftStreamWithVirtualBufferCleanup EnterVirtualBuffer();
		public abstract IKSoftStreamWithVirtualBufferBookmark EnterVirtualBufferBookmark();

		public IKSoftStreamWithVirtualBufferAndBookmark EnterVirtualBufferWithBookmark(long bufferLength)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bufferLength > 0);

			throw new NotImplementedException();
		}
	};
}