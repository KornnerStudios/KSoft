using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class ValueCheckTest : BaseTestClass
{

	[TestMethod]
	public void GuardMethods_NullOrEmptyArgumentsThrowExpectedExceptions()
	{
		AssertThrowsArgumentNull("description", () => ValueCheck.AreEqual(null!, 1, 1));
		AssertThrowsArgument("description", () => ValueCheck.AreEqual(string.Empty, 1, 1));
		AssertThrowsArgumentNull("description", () => ValueCheck.IsLessThanEqualTo(null!, 1, 1));
		AssertThrowsArgumentNull("description", () => ValueCheck.IsGreaterThanEqualTo(null!, 1, 1));
		AssertThrowsArgumentNull("description", () => ValueCheck.IsDistinct(null!, "value", Array.Empty<int>()));
		AssertThrowsArgumentNull("valueName", () => ValueCheck.IsDistinct("description", null!, Array.Empty<int>()));
		AssertThrowsArgument("valueName", () => ValueCheck.IsDistinct("description", string.Empty, Array.Empty<int>()));
		AssertThrowsArgumentNull("seq", () => ValueCheck.IsDistinct("description", "value", (int[])null!));
	}

	[TestMethod]
	public void AreEqual_NullDisplayValuesStillReportMismatch()
	{
		var exception = Assert.ThrowsExactly<InvalidDataException>(
			() => ValueCheck.AreEqual<string>("field", null!, "actual"));

		Assert.Contains("field. Expected '' but got 'actual'", exception.Message);
	}
}
