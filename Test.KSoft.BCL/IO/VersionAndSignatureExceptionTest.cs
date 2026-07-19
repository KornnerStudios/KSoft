using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test;

[TestClass]
public class VersionAndSignatureExceptionTest : BaseTestClass
{
	[TestMethod]
	public void VersionMismatchAssertFormatsUnsignedStreamValuesTest()
	{
		using var reader = CreateReader(0x56, 0x78);

		var exception = Assert.Throws<VersionMismatchException>(
			() => VersionMismatchException.Assert(reader, (ushort)0x1234));

		Assert.Contains("@00000000", exception.Message);
		Assert.Contains("Expected '1234', got '5678' (newer data)", exception.Message);
	}

	[TestMethod]
	public void VersionOutOfRangeAssertReturnsValidValueAndFormatsInvalidStreamValueTest()
	{
		using var validReader = CreateReader(0x05);

		Assert.AreEqual((byte)0x05, VersionOutOfRangeException.Assert(validReader, (byte)0x01, (byte)0x0A));

		using var invalidReader = CreateReader(0x11);

		var exception = Assert.Throws<VersionOutOfRangeException>(
			() => VersionOutOfRangeException.Assert(invalidReader, (byte)0x01, (byte)0x0A));

		Assert.Contains("@00000000", exception.Message);
		Assert.Contains("Expected value between 01 and 0A, got '11' (newer data)", exception.Message);
	}

	[TestMethod]
	public void SignatureMismatchAssertFormatsUnsignedStreamValuesTest()
	{
		using var reader = CreateReader(0xDE, 0xAD, 0xBE, 0xEF);

		var exception = Assert.Throws<SignatureMismatchException>(
			() => SignatureMismatchException.Assert(reader, 0x01234567U));

		Assert.Contains("@00000000", exception.Message);
		Assert.Contains("Expected '01234567', got 'DEADBEEF'", exception.Message);
	}

	[TestMethod]
	public void VersionDescriptionConstructorsFormatSignedAndUnsignedValuesTest()
	{
		var unsignedException = new VersionMismatchException("cache", 2U, 1U);
		var signedException = new VersionOutOfRangeException("cache", 1, 3, 4);

		Assert.Contains("Invalid 'cache' version! Expected '2', got '1' (older data)", unsignedException.Message);
		Assert.Contains(
			"Invalid 'cache' version! Expected value between 1 and 3, got '4' (newer data)",
			signedException.Message);
	}

	private static EndianReader CreateReader(params byte[] bytes)
	{
		return new EndianReader(new MemoryStream(bytes), Shell.EndianFormat.Big);
	}
}
