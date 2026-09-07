using System;
using System.IO;
using System.IO.Compression;

namespace KSoft.IO.Compression
{
	public static class ZLib
	{
		const int kSizeOfHeader = sizeof(ushort);

		public const int kNoCompression = 0;
		public const int kBestCompression = 9;

		static ZLibCompressionOptions CreateCompressionOptions(int level)
		{
			return new ZLibCompressionOptions { CompressionLevel = level };
		}

		static Stream CreateCompressionStream(Stream stream, int level, bool noZlibHeaderOrFooter, bool leaveOpen)
		{
			var options = CreateCompressionOptions(level);

			return noZlibHeaderOrFooter
				? new DeflateStream(stream, options, leaveOpen)
				: new ZLibStream(stream, options, leaveOpen);
		}

		static Stream CreateDecompressionStream(Stream stream, bool noHeader, bool leaveOpen)
		{
			return noHeader
				? new DeflateStream(stream, CompressionMode.Decompress, leaveOpen)
				: new ZLibStream(stream, CompressionMode.Decompress, leaveOpen);
		}

		static uint ComputeAdler32(byte[] bytes)
		{
			return Security.Cryptography.Adler32.Compute(bytes.AsSpan());
		}

		static byte[] BufferFromStream(DeflateStream dec, int offset, int length, bool skipHeader)
		{
			byte[] result;

			// adjust for zlib header
			if (skipHeader)
			{
				dec.BaseStream.Seek(offset + kSizeOfHeader, System.IO.SeekOrigin.Begin);
			}

			// decompress the data and fill in the result array
			result = new byte[length];
			dec.ReadExactly(result);

			return result;
		}

		public static byte[] BufferFromStream(MemoryStream ms,
			int offset = TypeExtensions.kNoneInt32, int length = TypeExtensions.kNoneInt32,
			bool skipHeader = true)
		{
			ArgumentNullException.ThrowIfNull(ms);

			if (offset.IsNone()) { offset = 0; }
			if (length.IsNone()) { length = (int)ms.Length; }

			using (var dec = new DeflateStream(ms, CompressionMode.Decompress, true))
			{
				return BufferFromStream(dec, offset, length, skipHeader);
			}
		}
		public static byte[] BufferFromBytes(byte[] bytes,
			int offset = TypeExtensions.kNoneInt32, int length = TypeExtensions.kNoneInt32,
			bool skipHeader = true)
		{
			ArgumentNullException.ThrowIfNull(bytes);

			if (offset.IsNone()) { offset = 0; }
			if (length.IsNone()) { length = bytes.Length; }

			using (var ms = new MemoryStream(bytes))
			using (var dec = new DeflateStream(ms, CompressionMode.Decompress, false))
			{
				return BufferFromStream(dec, offset, length, skipHeader);
			}
		}


		public static byte[] LowLevelCompress(byte[] bytes, int level,
			out uint adler, byte[] compressedBytes,
			bool trimCompressedBytes = true, bool noZlibHeaderOrFooter = true)
		{
			ArgumentNullException.ThrowIfNull(bytes);
			ArgumentNullException.ThrowIfNull(compressedBytes);

			adler = ComputeAdler32(bytes);

			if (trimCompressedBytes)
			{
				using (var compressed_stream = new MemoryStream())
				{
					using (var zip = CreateCompressionStream(compressed_stream, level, noZlibHeaderOrFooter,
						leaveOpen: true))
					{
						zip.Write(bytes, 0, bytes.Length);
					}

					return compressed_stream.ToArray();
				}
			}

			using (var fixed_stream = new MemoryStream(compressedBytes, writable: true))
			using (var zip = CreateCompressionStream(fixed_stream, level, noZlibHeaderOrFooter, leaveOpen: true))
			{
				zip.Write(bytes, 0, bytes.Length);
			}
			return compressedBytes;
		}
		public static uint LowLevelDecompress(byte[] compressedBytes, byte[] uncompressedBytes,
			bool noHeader = true)
		{
			ArgumentNullException.ThrowIfNull(compressedBytes);
			ArgumentNullException.ThrowIfNull(uncompressedBytes);

			using (var compressed_stream = new MemoryStream(compressedBytes, writable: false))
			using (var zip = CreateDecompressionStream(compressed_stream, noHeader, leaveOpen: false))
			{
				zip.ReadExactly(uncompressedBytes);
			}
			return ComputeAdler32(uncompressedBytes);
		}

		public static byte[] LowLevelCompress(byte[] bytes, Shell.EndianFormat byteOrder)
		{
			ArgumentNullException.ThrowIfNull(bytes);

			byte[] result = new byte[sizeof(int)];
			// Setup the decompressed size header
			byte[] size_bytes = BitConverter.GetBytes(bytes.Length);
			if (!byteOrder.IsSameAsRuntime())
			{
				size_bytes.AsSpan(0, sizeof(int)).Reverse();
			}
			Array.Copy(size_bytes, result, size_bytes.Length);

			using (var ms = new MemoryStream())
			{
				ms.Write(size_bytes, 0, size_bytes.Length);
				using (var zip = CreateCompressionStream(ms, kBestCompression,
					noZlibHeaderOrFooter: false, leaveOpen: true))
				{
					zip.Write(bytes, 0, bytes.Length);
				}

				result = ms.ToArray();
			}
			return result;
		}
		public static byte[] LowLevelDecompress(byte[] bytes, int uncompressedSize,
			int skipHeaderLength = sizeof(uint))
		{
			ArgumentNullException.ThrowIfNull(bytes);
			ArgumentOutOfRangeException.ThrowIfNegative(uncompressedSize);
			ArgumentOutOfRangeException.ThrowIfNegative(skipHeaderLength);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(skipHeaderLength, bytes.Length);

			byte[] result = new byte[uncompressedSize];
			using (var compressed_stream = new MemoryStream(bytes, skipHeaderLength,
				bytes.Length - skipHeaderLength, writable: false))
			using (var zip = new ZLibStream(compressed_stream, CompressionMode.Decompress, leaveOpen: false))
			{
				zip.ReadExactly(result);
			}
			return result;
		}
	};
}
