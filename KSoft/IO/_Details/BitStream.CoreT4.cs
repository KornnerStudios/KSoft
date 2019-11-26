#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TWord = System.UInt32;

namespace KSoft.IO
{
	partial class BitStream
	{
		#region Read\Write uint Impl
		internal void ReadWord(out uint word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				word = (uint)ExtractWordFromCache(bitCount);
				mCacheBitIndex += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (mCacheBitIndex == kWordBitCount && !IsEndOfStream)
					FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// get the word bits (MSB) that are left in the cache
				word = (uint)ExtractWordFromCache(bits_remaining);
				word_bits_remaining -= bits_remaining;
				// adjust the bits to the MSB
				word <<= msb_shift;

				FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord more_bits = ExtractWordFromCache(word_bits_remaining);

				word |= (uint)more_bits;
				mCacheBitIndex = word_bits_remaining;
			}
		}
		internal void WriteWord(uint word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				PutWordInCache((TWord)word, bitCount);
				mCacheBitIndex += bitCount;

				if (mCacheBitIndex == kWordBitCount)
					FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// write the upper (MSB) word bits to the remaining cache bits
				PutWordInCache((TWord)(word >> msb_shift), bits_remaining);
				word_bits_remaining -= bits_remaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				mCacheBitIndex += bits_remaining;
				FlushCache(); // flush the MSB results and reset the cache

				PutWordInCache((TWord)word, word_bits_remaining);
				mCacheBitIndex = word_bits_remaining;
			}
		}
		#endregion

		#region Read\Write ulong Impl
		internal void ReadWord(out ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				word = (ulong)ExtractWordFromCache(bitCount);
				mCacheBitIndex += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (mCacheBitIndex == kWordBitCount && !IsEndOfStream)
					FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// get the word bits (MSB) that are left in the cache
				word = (ulong)ExtractWordFromCache(bits_remaining);
				word_bits_remaining -= bits_remaining;
				// adjust the bits to the MSB
				word <<= msb_shift;

				FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord more_bits = ExtractWordFromCache(word_bits_remaining);

				word |= (ulong)more_bits;
				mCacheBitIndex = word_bits_remaining;
			}
		}
		internal void WriteWord(ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= kWordBitCount);

			int bits_remaining = CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bits_remaining)
			{
				PutWordInCache((TWord)word, bitCount);
				mCacheBitIndex += bitCount;

				if (mCacheBitIndex == kWordBitCount)
					FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int word_bits_remaining = bitCount;

				// will always be negative, so abs it
				int msb_shift = -(bits_remaining - word_bits_remaining);
				// write the upper (MSB) word bits to the remaining cache bits
				PutWordInCache((TWord)(word >> msb_shift), bits_remaining);
				word_bits_remaining -= bits_remaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				mCacheBitIndex += bits_remaining;
				FlushCache(); // flush the MSB results and reset the cache

				PutWordInCache((TWord)word, word_bits_remaining);
				mCacheBitIndex = word_bits_remaining;
			}
		}
		#endregion

	};
}