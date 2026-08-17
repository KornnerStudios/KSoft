using System.IO;

namespace KSoft.IO
{
	/// <summary>Exposes the concept of a virtual buffer inside a stream</summary>
	/// <remarks>No bytes are IO'd. Purely position based</remarks>
	public interface IKSoftStreamWithVirtualBuffer
	{
		Stream BaseStream { get; }

		/// <summary>Absolute position of the start of the virtual buffer</summary>
		long VirtualBufferStart { get; set; }
		/// <summary>How many bytes compose the virtual buffer</summary>
		long VirtualBufferLength { get; set; }
	};
}
