using System;
using System.IO;
using System.Security.Cryptography;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	public struct StreamHashComputer<T>
		where T : HashAlgorithm
	{
		/// <summary>Max size of the scratch buffer when we don't use a user specified preallocated buffer</summary>
		const int kMaxScratchBufferSize = 0x1000;

		private readonly T mAlgo;
		private readonly Stream mInputStream;
		private byte[] mScratchBuffer;
		private long mStartOffset;
		private long mCount;
		private bool mRestorePosition;

		public Stream InputStream { get { return mInputStream; } }
		public long StartOffset { get { return mStartOffset; } }
		public long Count { get { return mCount; } }
		/// <summary>
		/// Does the input stream's current position get treated as the starting offset?
		/// </summary>
		public bool StartOffsetIsStreamPosition { get { return mStartOffset.IsNone(); } }

		public StreamHashComputer(T algo, Stream inputStream
			, bool restorePosition = false
			, byte[] preallocatedBuffer = null)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentException>(preallocatedBuffer == null || preallocatedBuffer.Length > 0);

			mAlgo = algo;
			mInputStream = inputStream;
			mScratchBuffer = preallocatedBuffer;
			mStartOffset = TypeExtensions.kNone;
			mCount = TypeExtensions.kNone;
			mRestorePosition = restorePosition;

			mAlgo.Initialize();
		}

		public void SetRangeAtCurrentOffset(long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);

			SetRangeAndOffset(TypeExtensions.kNone, count);
		}
		public void SetRangeAndOffset(long offset, long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNoneOrPositive());
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset+count) <= InputStream.Length);

			mStartOffset = offset;
			mCount = count;
		}

		public T Compute()
		{
			Contract.Requires<InvalidOperationException>(StartOffset.IsNoneOrPositive(),
				"You need to call SetRange before calling this");
			Contract.Requires<InvalidOperationException>(Count >= 0,
				"You need to call SetRange before calling this");

			#region prologue
			mAlgo.Initialize();

			int buffer_size;
			byte[] buffer;

			bool uses_preallocated_buffer = mScratchBuffer != null;
			if (!uses_preallocated_buffer)
			{
				buffer_size = System.Math.Min((int)Count, kMaxScratchBufferSize);
				mScratchBuffer = new byte[buffer_size];
			}

			buffer = mScratchBuffer;
			buffer_size = mScratchBuffer.Length;

			long orig_pos = mInputStream.Position;
			if (!StartOffsetIsStreamPosition && StartOffset != orig_pos)
				mInputStream.Seek(StartOffset, SeekOrigin.Begin);
			#endregion

			for (long bytes_remaining = Count; bytes_remaining > 0;)
			{
				long num_bytes_to_read = System.Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = mInputStream.Read(buffer, num_bytes_read, (int)num_bytes_to_read);
					if (n == 0)
						break;

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
					mAlgo.TransformBlock(buffer, 0, num_bytes_read, null, 0);
				else
					break;

				bytes_remaining -= num_bytes_read;
			}

			mAlgo.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			#region epilogue
			if (mRestorePosition)
				mInputStream.Seek(orig_pos, SeekOrigin.Begin);

			if (!uses_preallocated_buffer)
			{
				mScratchBuffer = null;
			}
			#endregion

			return mAlgo;
		}
	};
}