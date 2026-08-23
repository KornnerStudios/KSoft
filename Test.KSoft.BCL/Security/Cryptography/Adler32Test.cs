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
		public void Compute_KnownBytes_ReturnsAdler32()
		{
			const uint expected = 0x11E60398;

			Assert.AreEqual(expected, Adler32.Compute(kWikipediaBytes));
			Assert.AreEqual(expected, Adler32.Compute(kWikipediaBytes, 0, kWikipediaBytes.Length));
		}

		[TestMethod]
		public void Compute_Stream_ReturnsAdler32AndRestoresPosition()
		{
			using var stream = new MemoryStream(kWikipediaBytes);
			stream.Position = 1;
			uint expected = Adler32.Compute(kWikipediaBytes, 1, kWikipediaBytes.Length - 1);

			uint actual = Adler32.Compute(stream, kWikipediaBytes.Length - 1, restorePosition: true);

			Assert.AreEqual(expected, actual);
			Assert.AreEqual(1, stream.Position);
		}

		[TestMethod]
		public void Compute_NullBuffer_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("buffer", () => Adler32.Compute(null!));
			AssertThrowsArgumentNull("buffer", () => Adler32.Compute(null!, 0, 0));
		}

		[TestMethod]
		public void Compute_InvalidRange_ThrowsArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange("offset", () => Adler32.Compute(kWikipediaBytes, -1, 0));
			AssertThrowsArgumentOutOfRange("length", () => Adler32.Compute(kWikipediaBytes, 0, -1));
			AssertThrowsArgumentOutOfRange("offset", () =>
				Adler32.Compute(kWikipediaBytes, kWikipediaBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("length", () => Adler32.Compute(kWikipediaBytes, 1, kWikipediaBytes.Length));
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

		[TestMethod]
		public void BitComputerCompute_InvalidArguments_Throw()
		{
			var computer = Adler32.BitComputer.New;

			AssertThrowsArgumentNull("buffer", () => computer.Compute(null!, 0, 0));
			AssertThrowsArgumentOutOfRange("offset", () => computer.Compute(kWikipediaBytes, -1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kWikipediaBytes, 0, -1));
			AssertThrowsArgumentOutOfRange("offset", () =>
				computer.Compute(kWikipediaBytes, kWikipediaBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kWikipediaBytes, 1, kWikipediaBytes.Length));
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
