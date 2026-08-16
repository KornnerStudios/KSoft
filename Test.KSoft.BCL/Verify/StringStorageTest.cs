using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class VerifyStringStorageTest : BaseTestClass
{
	[TestMethod]
	public void ForStreaming_ThrowsForUnfixedCharArrayWithoutLength()
	{
		Assert.ThrowsExactly<InvalidDataException>(() =>
			Verify.StringStorage.ForStreaming(Memory.Strings.StringStorage.AsciiString, TypeExtensions.kNone));
		Assert.ThrowsExactly<InvalidDataException>(() =>
			Verify.StringStorage.ForStreaming(Memory.Strings.StringStorage.AsciiString, 0));

		Verify.StringStorage.ForStreaming(Memory.Strings.StringStorage.AsciiString, 1);
		Verify.StringStorage.ForStreaming(Memory.Strings.StringStorage.CStringAscii, TypeExtensions.kNone);
		Verify.StringStorage.ForStreaming(
			new Memory.Strings.StringStorage(
				Memory.Strings.StringStorageWidthType.Ascii,
				Memory.Strings.StringStorageType.CharArray,
				1),
			TypeExtensions.kNone);
	}
}
