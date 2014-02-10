using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	/// <summary>
	/// Records the current position of the stream, and returns the stream's cursor to that position when the context object is disposed
	/// </summary>
	public struct StreamPositionContext : IDisposable
	{
		readonly long mPosition;
		Stream mStream;

		#region Ctor
		public StreamPositionContext(Stream baseStream)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<InvalidOperationException>(baseStream.CanSeek);

			mPosition = baseStream.Position;
			mStream = baseStream;
		}

		public StreamPositionContext(BinaryReader stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}
		public StreamPositionContext(BinaryWriter stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}

		public StreamPositionContext(StreamReader stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}
		public StreamPositionContext(StreamWriter stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
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