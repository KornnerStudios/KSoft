using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class ValueCheckTest : BaseTestClass
{
	static void AssertThrowsArgument(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void GuardMethods_NullOrEmptyArgumentsThrowExpectedExceptions()
	{
		AssertThrowsArgumentNull(() => ValueCheck.AreEqual(null!, 1, 1), "description");
		AssertThrowsArgument(() => ValueCheck.AreEqual(string.Empty, 1, 1), "description");
		AssertThrowsArgumentNull(() => ValueCheck.IsLessThanEqualTo(null!, 1, 1), "description");
		AssertThrowsArgumentNull(() => ValueCheck.IsGreaterThanEqualTo(null!, 1, 1), "description");
		AssertThrowsArgumentNull(() => ValueCheck.IsDistinct(null!, "value", Array.Empty<int>()), "description");
		AssertThrowsArgumentNull(() => ValueCheck.IsDistinct("description", null!, Array.Empty<int>()), "valueName");
		AssertThrowsArgument(() => ValueCheck.IsDistinct("description", string.Empty, Array.Empty<int>()), "valueName");
		AssertThrowsArgumentNull(() => ValueCheck.IsDistinct("description", "value", (int[])null!), "seq");
	}
}
