using System;
using HashAlgorithm = System.Security.Cryptography.HashAlgorithm;

namespace KSoft.Security.Cryptography
{
	public abstract class BlockHashAlgorithm
		: HashAlgorithm
	{
		readonly byte[] mBlockBuffer;

		protected long TotalBytesProcessed { get; private set; }

		/// <summary>The size in bytes of an individual block.</summary>
		public int BlockSize { get { return mBlockBuffer.Length; } }
		/// <summary>The number of bytes currently in the buffer waiting to be processed.</summary>
		public int BlockBytesRemaining { get; private set; }
		internal byte[] InternalBlockBuffer { get { return mBlockBuffer; } }

		/// <summary>Initializes a new instance of the BlockHashAlgorithm class.</summary>
		/// <param name="blockSize">The size in bytes of an individual block.</param>
		protected BlockHashAlgorithm(int blockSize, int hashSize) : base()
		{
			base.HashSizeValue = hashSize;

			mBlockBuffer = new byte[blockSize];
		}

		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">One or more complete blocks of data to process.</param>
		protected abstract void ProcessBlock(ReadOnlySpan<byte> inputBuffer);

		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		protected abstract byte[] ProcessFinalBlock(Span<byte> inputBuffer, int inputCount);

		#region HashAlgorithm
		/// <summary>Initializes the algorithm.</summary>
		/// <remarks>If this function is overriden in a derived class, the new function should call back to
		/// this function or you could risk garbage being carried over from one calculation to the next.</remarks>
		public override void Initialize()
		{
			Array.Clear(mBlockBuffer, 0, mBlockBuffer.Length);
			BlockBytesRemaining = 0;
			TotalBytesProcessed = 0;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			ReadOnlySpan<byte> input = array.AsSpan(startIndex, count);

			// Use what may already be in the buffer.
			if (BlockBytesRemaining > 0)
			{
				if (input.Length + BlockBytesRemaining < BlockSize)
				{
					// Still don't have enough for a full block, just store it.
					input.CopyTo(mBlockBuffer.AsSpan(BlockBytesRemaining));
					BlockBytesRemaining += input.Length;
					return;
				}
				else
				{
					// Fill out the buffer to make a full block, and then process it.
					int bytesToFill = BlockSize - BlockBytesRemaining;
					input[..bytesToFill].CopyTo(mBlockBuffer.AsSpan(BlockBytesRemaining));
					ProcessBlock(mBlockBuffer);
					TotalBytesProcessed += BlockSize;
					BlockBytesRemaining = 0;
					input = input[bytesToFill..];
				}
			}

			// For as long as we have full blocks, process them.
			int blockBytes = input.Length - (input.Length % BlockSize);
			if (blockBytes > 0)
			{
				ProcessBlock(input[..blockBytes]);
				TotalBytesProcessed += blockBytes;
				input = input[blockBytes..];
			}

			// If we still have some bytes left, store them for later.
			if (!input.IsEmpty)
			{
				input.CopyTo(mBlockBuffer);
				BlockBytesRemaining = input.Length;
			}
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			return ProcessFinalBlock(mBlockBuffer.AsSpan(), BlockBytesRemaining);
		}
		#endregion
	};
}
