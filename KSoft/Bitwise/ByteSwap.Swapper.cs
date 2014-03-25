using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	partial class ByteSwap
	{
		public struct Swapper
		{
			readonly short[] kCodes;
			readonly int kSizeOf;

			int mCodesIndex;

			public Swapper(int sizeOf, params short[] codes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(sizeOf > 0);
				Contract.Requires<ArgumentNullException>(codes != null);

				kCodes = codes;
				kSizeOf = sizeOf;

				mCodesIndex = 0;
			}
			public Swapper(IByteSwappable definition)
			{
				Contract.Requires<ArgumentNullException>(definition != null);

				kCodes = definition.ByteSwapCodes;
				kSizeOf = definition.SizeOf;

				mCodesIndex = 0;
			}

			public void SwapData(byte[] buffer, int startIndex = 0)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);

				int size_in_bytes, size_in_codes;
				SwapData(buffer, startIndex, out size_in_bytes, out size_in_codes);
			}
			public void SwapData(byte[] buffer, int startIndex,
				out int sizeInBytes, out int sizeInCodes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);

				sizeInBytes = 0;
				sizeInCodes = 0;

				Contract.Assert(kCodes[mCodesIndex] == (int)BsCode.ArrayStart);
				int array_count = kCodes[mCodesIndex + 1]; // array count comes after ArrayStart
				int array_index = 0;

				bool buffer_is_valid = buffer != null;
				bool done;
				for(int buffer_index = startIndex;
					array_index < array_count; 
					array_index++, buffer_index += kSizeOf)
				{
					sizeInCodes = 2;
					done = false;

					while(!done)
					{
						// We don't want to increment sizeInCodes just yet in case this is an ArrayStart. In which case we'll 
						// just recurse into SwapData again and it'll start the chain over again at the ArrayStart's index
						short code = kCodes[mCodesIndex + sizeInCodes];

						switch(code)
						{
							#region Word
							case (int)BsCode.Int16:
								if (buffer_is_valid)
								{
									SwapInt16(buffer, buffer_index);
									buffer_index += sizeof(short);
								}

								sizeInBytes += sizeof(short);
								break;
							#endregion

							#region DWord
							case (int)BsCode.Int32:
								if (buffer_is_valid)
								{
									SwapInt32(buffer, buffer_index);
									buffer_index += sizeof(int);
								}

								sizeInBytes += sizeof(int);
								break;
							#endregion

							#region QWord
							case (int)BsCode.Int64:
								if (buffer_is_valid)
								{
									SwapInt64(buffer, buffer_index);
									buffer_index += sizeof(long);
								}

								sizeInBytes += sizeof(long);
								break;
							#endregion

							case (int)BsCode.ArrayStart:
								int recursive_size_in_bytes, recursive_size_in_codes;

								// This is why we didn't increment sizeInCodes before the 'switch'
								mCodesIndex += sizeInCodes; // enter new byte code region
								SwapData(buffer, startIndex,
									out recursive_size_in_bytes, out recursive_size_in_codes);
								mCodesIndex -= sizeInCodes; // exit new byte code region

								sizeInCodes++; // increment over array count
								break;

							case (int)BsCode.ArrayEnd:
								done = true;
								break;

							#region Skip (default)
							default:
								if (code < 0)
									throw new Debug.UnreachableException();

								sizeInBytes += code;
								buffer_index += code;
								break;
							#endregion
						}

						sizeInCodes++;
					}
				}
			}
		};
	};
}