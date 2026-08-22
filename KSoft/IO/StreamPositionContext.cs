using System;
using System.IO;

namespace KSoft.IO
{
	/// <summary>
	/// Records the current position of the stream, and returns the stream's cursor to that position when the context object is disposed
	/// </summary>
	public struct StreamPositionContext : IDisposable
	{
		readonly long mPosition;
		Stream? mStream;

		#region Ctor
		public StreamPositionContext(Stream baseStream)
		{
			ArgumentNullException.ThrowIfNull(baseStream);
			if (!baseStream.CanSeek)
			{
				throw new InvalidOperationException();
			}

			mPosition = baseStream.Position;
			mStream = baseStream;
		}

		public StreamPositionContext(BinaryReader stream) : this(Util.ThrowIfNull(stream).BaseStream)
		{
		}
		public StreamPositionContext(BinaryWriter stream) : this(Util.ThrowIfNull(stream).BaseStream)
		{
		}

		public StreamPositionContext(StreamReader stream) : this(Util.ThrowIfNull(stream).BaseStream)
		{
		}
		public StreamPositionContext(StreamWriter stream) : this(Util.ThrowIfNull(stream).BaseStream)
		{
		}
		#endregion

		public void Dispose()
		{
			if (mStream != null)
			{
				mStream.Seek(mPosition, SeekOrigin.Begin);
				mStream = null;
			}
		}
	};
}
