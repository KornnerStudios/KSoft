using System;

namespace KSoft.IO
{
	public interface IKSoftEndianStream : IKSoftBinaryStream
	{
		/// <summary>The assumed byte order of the stream</summary>
		/// <remarks>Use <see cref="ChangeByteOrder"/> to properly change this property</remarks>
		Shell.EndianFormat ByteOrder { get; }

		/// <summary>Change the order in which bytes are ordered to/from the stream</summary>
		/// <param name="newOrder">The new byte order to switch to</param>
		/// <remarks>If <paramref name="newOrder"/> is the same as <see cref="ByteOrder"/> nothing will happen</remarks>
		void ChangeByteOrder(Shell.EndianFormat newOrder);

		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		IDisposable BeginEndianSwitch();
		/// <summary>Convenience method for C# "using" statements. Temporarily inverts the current byte order which is used for read/writes.</summary>
		/// <param name="switchTo">Byte order to switch to</param>
		/// <returns>Object which when Disposed will return this stream to its original <see cref="Shell.EndianFormat"/> state</returns>
		/// <remarks>
		/// If <paramref name="switchTo"/> is the same as <see cref="EndianStream.ByteOrder"/>
		/// then no actual order state change will happen. However, this construct
		/// will continue to be usable and will Dispose of properly with no error
		/// </remarks>
		IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo);
	};
}