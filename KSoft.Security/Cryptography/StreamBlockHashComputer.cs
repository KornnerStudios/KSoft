using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct StreamBlockHashComputer<T>
		where T : BlockHashAlgorithm
	{
		private readonly T mAlgo;
		private readonly Stream mInputStream;
		private long mStartOffset;
		private long mCount;
		private readonly bool mRestorePosition;

		public Stream InputStream { get { return mInputStream; } }
		public long StartOffset { get { return mStartOffset; } }
		public long Count { get { return mCount; } }
		/// <summary>
		/// Does the input stream's current position get treated as the starting offset?
		/// </summary>
		public bool StartOffsetIsStreamPosition { get { return mStartOffset.IsNone(); } }

		public StreamBlockHashComputer(T algo, Stream inputStream
			, bool restorePosition = false)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);

			mAlgo = algo;
			mInputStream = inputStream;
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
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset + count) <= InputStream.Length);

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

			byte[] buffer = mAlgo.InternalBlockBuffer;
			int buffer_size = mAlgo.BlockSize;

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
			#endregion

			return mAlgo;
		}
	};
}
