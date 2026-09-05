using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class Adler32Test : BaseTestClass
	{
		static readonly byte[] kWikipediaBytes = Encoding.ASCII.GetBytes("Wikipedia");

		[TestMethod]
		public void Compute_SpanKnownBytes_ReturnsAdler32()
		{
			const uint expected = 0x11E60398;

			Assert.AreEqual(expected, Adler32.Compute((ReadOnlySpan<byte>)kWikipediaBytes));
			Assert.AreEqual(expected, Adler32.Compute(kWikipediaBytes.AsSpan()));
		}

		[TestMethod]
		public void Compute_SlicedSpanWithSentinelBytes_ReturnsAdler32()
		{
			const uint expected = 0x11E60398;

			byte[] padded = new byte[kWikipediaBytes.Length + 4];
			padded[0] = 0xEE;
			padded[1] = 0xEE;
			Array.Copy(kWikipediaBytes, 0, padded, 2, kWikipediaBytes.Length);
			padded[padded.Length - 2] = 0xEE;
			padded[padded.Length - 1] = 0xEE;

			Assert.AreEqual(expected, Adler32.Compute(padded.AsSpan(2, kWikipediaBytes.Length)));
		}

		[TestMethod]
		public void Compute_EmptySpan_ReturnsDefaultSeed()
		{
			Assert.AreEqual(1u, Adler32.Compute(ReadOnlySpan<byte>.Empty));
			Assert.AreEqual(1u, Adler32.Compute(Array.Empty<byte>().AsSpan()));
		}

		[TestMethod]
		public void Compute_SpanWithNonDefaultSeed_ChainsAcrossSlices()
		{
			const uint expected = 0x11E60398;

			uint seedAfterFirstByte = Adler32.Compute(kWikipediaBytes.AsSpan(0, 1));
			uint actual = Adler32.Compute(kWikipediaBytes.AsSpan(1), seedAfterFirstByte);

			Assert.AreEqual(expected, actual);
			Assert.AreNotEqual(expected, Adler32.Compute(kWikipediaBytes.AsSpan(1)));
		}

		[TestMethod]
		public void Compute_SpanLargerThanBlockMaximum_MatchesScalarUpdates()
		{
			const uint seed = 0x12345678;
			byte[] bytes = new byte[6000];
			for (int x = 0; x < bytes.Length; x++)
				bytes[x] = (byte)x;

			uint expected = seed;
			foreach (byte value in bytes)
				expected = Adler32.Compute(value, expected);

			Assert.AreEqual(expected, Adler32.Compute(bytes.AsSpan(), seed));

			var computer = new Adler32.BitComputer(seed);
			computer.Compute(bytes.AsSpan());
			Assert.AreEqual(expected, computer.ComputeFinish());
		}

		[TestMethod]
		public void BitComputerCompute_SpanKnownBytes_ReturnsAdler32()
		{
			const uint expected = 0x11E60398;

			var computer = Adler32.BitComputer.New;
			computer.Compute((ReadOnlySpan<byte>)kWikipediaBytes);

			Assert.AreEqual(expected, computer.ComputeFinish());
		}

		[TestMethod]
		public void BitComputerCompute_EmptySpan_ReturnsDefaultSeed()
		{
			var computer = Adler32.BitComputer.New;
			computer.Compute(ReadOnlySpan<byte>.Empty);

			Assert.AreEqual(1u, computer.ComputeFinish());
		}

		[TestMethod]
		public void Compute_Stream_ReturnsAdler32AndRestoresPosition()
		{
			using var stream = new MemoryStream(kWikipediaBytes);
			stream.Position = 1;
			uint expected = Adler32.Compute(kWikipediaBytes.AsSpan(1, kWikipediaBytes.Length - 1));

			uint actual = Adler32.Compute(stream, kWikipediaBytes.Length - 1, restorePosition: true);

			Assert.AreEqual(expected, actual);
			Assert.AreEqual(1, stream.Position);
		}

		[TestMethod]
		public void Compute_InvalidStreamArguments_Throw()
		{
			AssertThrowsArgumentNull("stream", () => Adler32.Compute(null!, 0));
			AssertThrowsArgumentOutOfRange("length", () =>
				Adler32.Compute(new MemoryStream(kWikipediaBytes), -1));
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				Adler32.Compute(new NonReadableStream(), 0));
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				Adler32.Compute(new NonSeekableStream(kWikipediaBytes), 0, restorePosition: true));
		}

		sealed class NonReadableStream : MemoryStream
		{
			public override bool CanRead => false;
		}

		sealed class NonSeekableStream : MemoryStream
		{
			public NonSeekableStream(byte[] buffer) : base(buffer)
			{
			}

			public override bool CanSeek => false;
		}
	}
}
