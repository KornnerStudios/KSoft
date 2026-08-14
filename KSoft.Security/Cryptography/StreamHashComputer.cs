using System;
using System.IO;
using System.Security.Cryptography;

#nullable enable

namespace KSoft.Security.Cryptography
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		"CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct StreamHashComputer<T>
		where T : HashAlgorithm
	{
		/// <summary>Max size of the scratch buffer when we don't use a user specified preallocated buffer</summary>
		const int kMaxScratchBufferSize = 0x1000;

		private readonly T mAlgo;
		private readonly Stream mInputStream;
		private byte[]? mScratchBuffer;
		private long mStartOffset;
		private long mCount;
		private readonly bool mRestorePosition;

		public readonly Stream InputStream { get { return mInputStream; } }
		public readonly long StartOffset { get { return mStartOffset; } }
		public readonly long Count { get { return mCount; } }
		/// <summary>
		/// Does the input stream's current position get treated as the starting offset?
		/// </summary>
		public readonly bool StartOffsetIsStreamPosition { get { return mStartOffset.IsNone(); } }

		public StreamHashComputer(T algo,
			Stream inputStream,
			bool restorePosition = false,
			byte[]? preallocatedBuffer = null)
		{
			ArgumentNullException.ThrowIfNull(algo);
			ArgumentNullException.ThrowIfNull(inputStream);
			if (!inputStream.CanSeek)
			{
				throw new ArgumentException("Input stream must support seeking.", nameof(inputStream));
			}
			if (preallocatedBuffer != null && preallocatedBuffer.Length == 0)
			{
				throw new ArgumentException("Preallocated buffer must not be empty.", nameof(preallocatedBuffer));
			}

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
			ArgumentOutOfRangeException.ThrowIfNegative(count);

			SetRangeAndOffset(TypeExtensions.kNone, count);
		}
		public void SetRangeAndOffset(long offset, long count)
		{
			if (!offset.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
			ArgumentOutOfRangeException.ThrowIfNegative(count);
			if (!offset.IsNone())
			{
				if (offset > InputStream.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(offset));
				}
				if (count > InputStream.Length - offset)
				{
					throw new ArgumentOutOfRangeException(nameof(count));
				}
			}

			mStartOffset = offset;
			mCount = count;
		}

		public T Compute()
		{
			const string set_range_required_message = "You need to call SetRange before calling this";
			if (!StartOffset.IsNoneOrPositive())
			{
				throw new InvalidOperationException(set_range_required_message);
			}
			if (Count < 0)
			{
				throw new InvalidOperationException(set_range_required_message);
			}

			#region prologue
			mAlgo.Initialize();

			int buffer_size;
			byte[]? scratch_buffer = mScratchBuffer;
			bool uses_preallocated_buffer = scratch_buffer != null;
			if (scratch_buffer == null)
			{
				buffer_size = System.Math.Min((int)Count, kMaxScratchBufferSize);
				scratch_buffer = new byte[buffer_size];
				mScratchBuffer = scratch_buffer;
			}

			byte[] buffer = scratch_buffer;
			buffer_size = buffer.Length;

			long orig_pos = mInputStream.Position;
			if (!StartOffsetIsStreamPosition && StartOffset != orig_pos)
			{
				mInputStream.Seek(StartOffset, SeekOrigin.Begin);
			}
			#endregion

			for (long bytes_remaining = Count; bytes_remaining > 0;)
			{
				long num_bytes_to_read = System.Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = mInputStream.Read(buffer, num_bytes_read, (int)num_bytes_to_read);
					if (n == 0)
					{
						break;
					}

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
				{
					mAlgo.TransformBlock(buffer, 0, num_bytes_read, null, 0);
				}
				else
				{
					break;
				}

				bytes_remaining -= num_bytes_read;
			}

			// Yes, 0 bytes; all bytes should have been taken care of already.
			mAlgo.TransformFinalBlock(buffer, 0, 0);

			#region epilogue
			if (mRestorePosition)
			{
				mInputStream.Seek(orig_pos, SeekOrigin.Begin);
			}

			if (!uses_preallocated_buffer)
			{
				mScratchBuffer = null;
			}
			#endregion

			return mAlgo;
		}
	};
}
