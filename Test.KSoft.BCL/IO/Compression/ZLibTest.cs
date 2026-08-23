using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Compression.Test
{
	[TestClass]
	public class ZLibTest : BaseTestClass
	{
		static readonly byte[] kSampleData = System.Text.Encoding.ASCII.GetBytes(
			"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaHello Vita compression!");

		static byte[] Compress(byte[] bytes, int level, bool noZlibHeaderOrFooter, out uint adler)
		{
			byte[] compressed_buffer = new byte[bytes.Length + 64];
			return ZLib.LowLevelCompress(bytes, level, out adler, compressed_buffer,
				noZlibHeaderOrFooter: noZlibHeaderOrFooter);
		}

		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void RawDeflateRoundTripsAndReturnsUncompressedAdlerTest()
		{
			byte[] compressed = Compress(kSampleData, 5, noZlibHeaderOrFooter: true, out uint compressAdler);
			var uncompressed = new byte[kSampleData.Length];

			uint decompress_adler = ZLib.LowLevelDecompress(compressed, uncompressed, noHeader: true);

			CollectionAssert.AreEqual(kSampleData, uncompressed);
			Assert.AreEqual(Security.Cryptography.Adler32.Compute(kSampleData), compressAdler);
			Assert.AreEqual(compressAdler, decompress_adler);
		}

		[TestMethod]
		public void FramedZLibRoundTripsAndReturnsUncompressedAdlerTest()
		{
			byte[] compressed = Compress(kSampleData, ZLib.kBestCompression,
				noZlibHeaderOrFooter: false, out uint compressAdler);
			var uncompressed = new byte[kSampleData.Length];

			uint decompress_adler = ZLib.LowLevelDecompress(compressed, uncompressed, noHeader: false);

			CollectionAssert.AreEqual(kSampleData, uncompressed);
			Assert.AreEqual(Security.Cryptography.Adler32.Compute(kSampleData), compressAdler);
			Assert.AreEqual(compressAdler, decompress_adler);
		}

		[TestMethod]
		public void TrimmedCompressCanGrowPastScratchBufferTest()
		{
			byte[] scratch = new byte[1];
			byte[] compressed = ZLib.LowLevelCompress(kSampleData, ZLib.kNoCompression, out uint _,
				scratch, trimCompressedBytes: true);
			var uncompressed = new byte[kSampleData.Length];

			ZLib.LowLevelDecompress(compressed, uncompressed);

			Assert.IsTrue(compressed.Length > scratch.Length);
			CollectionAssert.AreEqual(kSampleData, uncompressed);
		}

		[TestMethod]
		public void BigEndianSizeHeaderRoundTripsTest()
		{
			byte[] compressed = ZLib.LowLevelCompress(kSampleData, Shell.EndianFormat.Big);
			byte[] expected_header = BitConverter.GetBytes(kSampleData.Length);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(expected_header);
			}

			CollectionAssert.AreEqual(expected_header, compressed[0..sizeof(int)]);
			CollectionAssert.AreEqual(kSampleData, ZLib.LowLevelDecompress(compressed, kSampleData.Length));
		}

		[TestMethod]
		public void CustomSkipHeaderLengthRoundTripsTest()
		{
			byte[] compressed = Compress(kSampleData, ZLib.kBestCompression,
				noZlibHeaderOrFooter: false, out uint _);
			byte[] wrapped = new byte[sizeof(uint) * 2 + compressed.Length];
			Array.Copy(compressed, 0, wrapped, sizeof(uint) * 2, compressed.Length);

			byte[] uncompressed = ZLib.LowLevelDecompress(wrapped, kSampleData.Length, sizeof(uint) * 2);

			CollectionAssert.AreEqual(kSampleData, uncompressed);
		}

		[TestMethod]
		public void BufferFromBytesSkipsZLibHeaderTest()
		{
			byte[] compressed = Compress(kSampleData, ZLib.kBestCompression,
				noZlibHeaderOrFooter: false, out uint _);

			CollectionAssert.AreEqual(kSampleData, ZLib.BufferFromBytes(compressed, length: kSampleData.Length));
		}

		[TestMethod]
		public void BufferFromStream_NullStream_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("ms", () => ZLib.BufferFromStream(null!));
		}

		[TestMethod]
		public void BufferFromBytes_NullBytes_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("bytes", () => ZLib.BufferFromBytes(null!));
		}

		[TestMethod]
		public void LowLevelCompress_NullBytes_ThrowsArgumentNullException()
		{
			byte[] scratch = new byte[32];

			AssertThrowsArgumentNull("bytes", () =>
				ZLib.LowLevelCompress(null!, ZLib.kBestCompression, out uint _, scratch));
		}

		[TestMethod]
		public void LowLevelCompress_NullCompressedBytes_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("compressedBytes", () =>
				ZLib.LowLevelCompress(kSampleData, ZLib.kBestCompression, out uint _, null!));
		}

		[TestMethod]
		public void LowLevelDecompress_NullCompressedBytes_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("compressedBytes", () =>
				ZLib.LowLevelDecompress(null!, new byte[kSampleData.Length]));
		}

		[TestMethod]
		public void LowLevelDecompress_NullUncompressedBytes_ThrowsArgumentNullException()
		{
			byte[] compressed = Compress(kSampleData, ZLib.kBestCompression,
				noZlibHeaderOrFooter: true, out uint _);

			AssertThrowsArgumentNull("uncompressedBytes", () =>
				ZLib.LowLevelDecompress(compressed, null!));
		}

		[TestMethod]
		public void LowLevelCompressWithSizeHeader_NullBytes_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("bytes", () =>
				ZLib.LowLevelCompress(null!, Shell.EndianFormat.Little));
		}

		[TestMethod]
		public void LowLevelDecompressWithSizeHeader_NullBytes_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("bytes", () => ZLib.LowLevelDecompress(null!, 0));
		}

		[TestMethod]
		public void LowLevelDecompressWithSizeHeader_NegativeUncompressedSize_ThrowsArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange("uncompressedSize", () =>
				ZLib.LowLevelDecompress(Array.Empty<byte>(), -1));
		}

		[TestMethod]
		public void LowLevelDecompressWithSizeHeader_NegativeSkipHeaderLength_ThrowsArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange("skipHeaderLength", () =>
				ZLib.LowLevelDecompress(Array.Empty<byte>(), 0, -1));
		}

		[TestMethod]
		public void LowLevelDecompressWithSizeHeader_SkipHeaderPastEnd_ThrowsArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange("skipHeaderLength", () =>
				ZLib.LowLevelDecompress(Array.Empty<byte>(), 0, 1));
		}
	}
}
