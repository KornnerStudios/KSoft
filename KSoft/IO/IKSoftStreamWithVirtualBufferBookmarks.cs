
using System;
using System.IO;

namespace KSoft.IO
{
	/// <summary>Forces the stream to seek to the end of the virtual buffer when disposed</summary>
	public struct IKSoftStreamWithVirtualBufferCleanup : IDisposable
	{
		IKSoftStreamWithVirtualBuffer? mStream;
		readonly long mBufferEnd;

		public IKSoftStreamWithVirtualBufferCleanup(IKSoftStreamWithVirtualBuffer stream)
		{
			ArgumentNullException.ThrowIfNull(stream);
			if (stream.VirtualBufferStart < 0 || stream.VirtualBufferLength <= 0)
			{
				throw new ArgumentException("Stream does not have an active virtual buffer.", nameof(stream));
			}
			mStream = stream;
			mBufferEnd = stream.VirtualBufferStart + stream.VirtualBufferLength;
		}

		/// <summary>
		/// If the stream position is still inside the virtual buffer, seeks to the VirtualBuffer 'end'.
		/// Sets the VirtualBuffer properties of the underlying stream to 0.
		/// </summary>
		public void Dispose()
		{
			if (mStream != null)
			{
				long leftovers = mBufferEnd - mStream.BaseStream.Position;
				if (leftovers > 0)
				{
					mStream.BaseStream.Seek(leftovers, SeekOrigin.Current);
				}

				mStream.VirtualBufferStart = mStream.VirtualBufferLength = 0;
				mStream = null;
			}
		}
	};
	/// <summary>Temporarily bookmarks a stream's VirtualBuffer properties</summary>
	public struct IKSoftStreamWithVirtualBufferBookmark : IDisposable
	{
		IKSoftStreamWithVirtualBuffer? mStream;
		readonly long mOldStart, mOldLength;

		public IKSoftStreamWithVirtualBufferBookmark(IKSoftStreamWithVirtualBuffer stream)
		{
			ArgumentNullException.ThrowIfNull(stream);
			mStream = stream;
			mOldStart = stream.VirtualBufferStart;
			mOldLength = stream.VirtualBufferLength;
		}

		/// <summary>Restores the VirtualBuffer properties of the underlying stream to their previous values</summary>
		public void Dispose()
		{
			if (mStream != null)
			{
				mStream.VirtualBufferStart = mOldStart;
				mStream.VirtualBufferLength = mOldLength;
				mStream = null;
			}
		}
	};
	/// <summary>
	/// Temporarily bookmarks a stream's VirtualBuffer properties and sets up a new virtual buffer concept
	/// </summary>
	/// <see cref="IKSoftStreamWithVirtualBufferBookmark"/>
	/// <see cref="IKSoftStreamWithVirtualBufferCleanup"/>
	public struct IKSoftStreamWithVirtualBufferAndBookmark : IDisposable
	{
		IKSoftStreamWithVirtualBufferBookmark mBookmark;
		IKSoftStreamWithVirtualBufferCleanup mCleanup;
		bool mDisposed;

		public IKSoftStreamWithVirtualBufferAndBookmark(IKSoftStreamWithVirtualBuffer stream, long bufferLength)
		{
			ArgumentNullException.ThrowIfNull(stream);
			mBookmark = stream.EnterVirtualBufferBookmark();
			mCleanup = stream.EnterVirtualBuffer(bufferLength);
			mDisposed = false;
		}

		public void Dispose()
		{
			if (!mDisposed)
			{
				mCleanup.Dispose();
				mBookmark.Dispose();
				mDisposed = true;
			}
		}
	};
}
