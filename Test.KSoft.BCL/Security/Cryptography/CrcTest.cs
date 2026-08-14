using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class CrcTest : BaseTestClass
	{
		static readonly byte[] kStandardBytes = Encoding.ASCII.GetBytes("123456789");

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

		static void AssertThrowsArgumentException(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void Crc16_StandardVector_ReturnsExpectedValue()
		{
			const ushort expected = 0x29B1;
			var definition = new Crc16.Definition();
			ushort crc = 0;

			Assert.AreEqual(expected, definition.Crc(ref crc, kStandardBytes, kStandardBytes.Length));

			var computer = new Crc16.BitComputer(definition);
			computer.ComputeBegin();
			computer.Compute(kStandardBytes, 0, kStandardBytes.Length);
			Assert.AreEqual(expected, computer.ComputeFinish());

			using var hash = new CrcHash16();
			Assert.AreEqual("B129", Convert.ToHexString(hash.ComputeHash(kStandardBytes)));
		}

		[TestMethod]
		public void Crc32_StandardVector_ReturnsExpectedValue()
		{
			const uint expected = 0x340BC6D9;
			var definition = new Crc32.Definition();
			uint crc = 0;

			Assert.AreEqual(expected, definition.Crc(ref crc, kStandardBytes, kStandardBytes.Length));

			var computer = new Crc32.BitComputer(definition);
			computer.ComputeBegin();
			computer.Compute(kStandardBytes, 0, kStandardBytes.Length);
			Assert.AreEqual(expected, computer.ComputeFinish());

			using var hash = new CrcHash32();
			Assert.AreEqual("D9C60B34", Convert.ToHexString(hash.ComputeHash(kStandardBytes)));
		}

		[TestMethod]
		public void CrcDefinitions_InvalidTableLength_ThrowsArgumentException()
		{
			Assert.AreEqual(Crc16.kCrcTableSize, new Crc16.Definition(crcTable: null).CrcTable.Length);
			Assert.AreEqual(Crc32.kCrcTableSize, new Crc32.Definition(crcTable: null).CrcTable.Length);

			AssertThrowsArgumentException("crcTable", () =>
				new Crc16.Definition(crcTable: new ushort[] { 1 }));
			AssertThrowsArgumentException("crcTable", () =>
				new Crc32.Definition(crcTable: new uint[] { 1 }));
		}

		[TestMethod]
		public void CrcConstructors_NullDefinition_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull("definition", () => new CrcHash16(null));
			AssertThrowsArgumentNull("definition", () => new CrcHash32(null));
			AssertThrowsArgumentNull("definition", () => new Crc16.BitComputer(null));
			AssertThrowsArgumentNull("definition", () => new Crc32.BitComputer(null));
		}

		[TestMethod]
		public void Crc16BitComputerCompute_InvalidArguments_Throw()
		{
			var computer = new Crc16.BitComputer(new Crc16.Definition());

			AssertThrowsArgumentNull("buffer", () => computer.Compute(null, 0, 0));
			AssertThrowsArgumentOutOfRange("offset", () => computer.Compute(kStandardBytes, -1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kStandardBytes, 0, -1));
			AssertThrowsArgumentOutOfRange("offset", () =>
				computer.Compute(kStandardBytes, kStandardBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kStandardBytes, 1, kStandardBytes.Length));
		}

		[TestMethod]
		public void Crc32BitComputerCompute_InvalidArguments_Throw()
		{
			var computer = new Crc32.BitComputer(new Crc32.Definition());

			AssertThrowsArgumentNull("buffer", () => computer.Compute(null, 0, 0));
			AssertThrowsArgumentOutOfRange("offset", () => computer.Compute(kStandardBytes, -1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kStandardBytes, 0, -1));
			AssertThrowsArgumentOutOfRange("offset", () =>
				computer.Compute(kStandardBytes, kStandardBytes.Length + 1, 0));
			AssertThrowsArgumentOutOfRange("length", () => computer.Compute(kStandardBytes, 1, kStandardBytes.Length));
		}
	}
}
