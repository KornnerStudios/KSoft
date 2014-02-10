using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public interface IKSoftBinaryStream : IKSoftStream, IDisposable
	{
		/// <summary>Base address used for simulating pointers in the stream</summary>
		Values.PtrHandle BaseAddress { get; set; }

		#region Seek
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		void Seek32(uint offset);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		void Seek32(uint offset, SeekOrigin origin);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		void Seek32(int offset);
		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		void Seek32(int offset, SeekOrigin origin);

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to the beginning of the stream</summary>
		/// <param name="offset">Offset to seek to</param>
		void Seek(long offset);

		/// <summary>Moves the stream cursor to <paramref name="offset"/> relative to <paramref name="origin"/></summary>
		/// <param name="offset">Offset to seek to</param>
		/// <param name="origin">Origin to base seek operation</param>
		void Seek(long offset, SeekOrigin origin);
		#endregion

		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		bool AlignToBoundry(int alignmentBit);

		#region VirtualAddressTranslation
		/// <summary>Initialize the VAT with a specific handle size and initial table capacity</summary>
		/// <param name="vaSize">Handle size</param>
		/// <param name="translationCapacity">The initial table capacity</param>
		void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0);
		/// <summary>Push a PA into to the VAT table, setting the current PA in the process</summary>
		/// <param name="physicalAddress">PA to push and to use as the VAT's current address</param>
		void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress);
		/// <summary>Push the stream's position (as a physical address) into the VAT table</summary>
		void VirtualAddressTranslationPushPosition();
		/// <summary>Increase the current address (PA) by a relative offset</summary>
		/// <param name="relativeOffset">Offset, relative to the current address</param>
		void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset);
		/// <summary>Pop and return the current address (PA) in the VAT table</summary>
		/// <returns>The VAT's current address value before this call</returns>
		Values.PtrHandle VirtualAddressTranslationPop();
		#endregion

		#region PositionPtr
		/// <summary>Get the current position as a <see cref="Data.PtrHandle"/></summary>
		/// <param name="ptrSize">Pointer size to use for the result handle</param>
		/// <returns></returns>
		Values.PtrHandle GetPositionPtr(Shell.ProcessorSize ptrSize);
		/// <summary>Current position as a <see cref="Data.PtrHandle"/></summary>
		/// <remarks>Pointer traits\info is inherited from <see cref="BaseAddress"/></remarks>
		Values.PtrHandle PositionPtr { get; }
		#endregion
	};
}