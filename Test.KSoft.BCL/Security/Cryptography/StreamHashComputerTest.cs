using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class StreamHashComputerTest : BaseTestClass
	{
		static readonly byte[] kInputBytes = Encoding.ASCII.GetBytes("0123456789abcdef");

		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void StreamHashComputer_ExplicitRange_HashesBytesAndRestoresPosition()
		{
			using var stream = new MemoryStream(kInputBytes);
			stream.Position = 7;
			using var expectedAlgorithm = SHA256.Create();
			byte[] expected = expectedAlgorithm.ComputeHash(kInputBytes, 2, 5);
			using var actualAlgorithm = SHA256.Create();
			var computer = new StreamHashComputer<SHA256>(actualAlgorithm,
				stream,
				restorePosition: true,
				preallocatedBuffer: new byte[3]);

			computer.SetRangeAndOffset(2, 5);
			SHA256 result = computer.Compute();

			CollectionAssert.AreEqual(expected, result.Hash);
			Assert.AreEqual(7, stream.Position);
		}

		[TestMethod]
		public void StreamBlockHashComputer_ExplicitRange_HashesBytesAndRestoresPosition()
		{
			using var stream = new MemoryStream(kInputBytes);
			stream.Position = 8;
			using var expectedAlgorithm = new TigerHash();
			byte[] expected = expectedAlgorithm.ComputeHash(kInputBytes, 3, 6);
			var computer = new StreamBlockHashComputer<TigerHash>(
				new TigerHash(),
				stream,
				restorePosition: true);

			computer.SetRangeAndOffset(3, 6);
			TigerHash result = computer.Compute();

			CollectionAssert.AreEqual(expected, result.Hash);
			Assert.AreEqual(8, stream.Position);
		}

		[TestMethod]
		public void StreamHashComputer_InvalidConstructorArguments_Throw()
		{
			AssertThrowsArgumentNull("algo", () =>
				new StreamHashComputer<SHA256>(null, new MemoryStream(kInputBytes)));
			AssertThrowsArgumentNull("inputStream", () =>
				new StreamHashComputer<SHA256>(SHA256.Create(), null));
			AssertThrowsArgument("inputStream", () =>
				new StreamHashComputer<SHA256>(SHA256.Create(), new NonSeekableStream(kInputBytes)));
			AssertThrowsArgument("preallocatedBuffer", () =>
				new StreamHashComputer<SHA256>(SHA256.Create(), new MemoryStream(kInputBytes), preallocatedBuffer: []));
		}

		[TestMethod]
		public void StreamBlockHashComputer_InvalidConstructorArguments_Throw()
		{
			AssertThrowsArgumentNull("algo", () =>
				new StreamBlockHashComputer<TigerHash>(null, new MemoryStream(kInputBytes)));
			AssertThrowsArgumentNull("inputStream", () =>
				new StreamBlockHashComputer<TigerHash>(new TigerHash(), null));
			AssertThrowsArgument("inputStream", () =>
				new StreamBlockHashComputer<TigerHash>(new TigerHash(), new NonSeekableStream(kInputBytes)));
		}

		[TestMethod]
		public void StreamHashComputer_InvalidRangeArguments_Throw()
		{
			using var algorithm = SHA256.Create();
			var computer = new StreamHashComputer<SHA256>(algorithm, new MemoryStream(kInputBytes));

			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAtCurrentOffset(-1));
			AssertThrowsArgumentOutOfRange("offset", () => computer.SetRangeAndOffset(-2, 0));
			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAndOffset(0, -1));
			AssertThrowsArgumentOutOfRange("offset", () => computer.SetRangeAndOffset(kInputBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAndOffset(1, kInputBytes.Length));
		}

		[TestMethod]
		public void StreamBlockHashComputer_InvalidRangeArguments_Throw()
		{
			var computer = new StreamBlockHashComputer<TigerHash>(new TigerHash(), new MemoryStream(kInputBytes));

			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAtCurrentOffset(-1));
			AssertThrowsArgumentOutOfRange("offset", () => computer.SetRangeAndOffset(-2, 0));
			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAndOffset(0, -1));
			AssertThrowsArgumentOutOfRange("offset", () => computer.SetRangeAndOffset(kInputBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("count", () => computer.SetRangeAndOffset(1, kInputBytes.Length));
		}

		[TestMethod]
		public void StreamComputers_ComputeBeforeRange_ThrowsInvalidOperationException()
		{
			using var algorithm = SHA256.Create();
			var streamComputer = new StreamHashComputer<SHA256>(algorithm, new MemoryStream(kInputBytes));
			var blockComputer = new StreamBlockHashComputer<TigerHash>(new TigerHash(), new MemoryStream(kInputBytes));

			Assert.ThrowsExactly<InvalidOperationException>(() => streamComputer.Compute());
			Assert.ThrowsExactly<InvalidOperationException>(() => blockComputer.Compute());
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
