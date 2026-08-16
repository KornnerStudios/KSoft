using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test;

[TestClass]
public class VerifyGroupTagsTest : BaseTestClass
{
	static void AssertThrowsArgument(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
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
	public void ExactLength_ThrowsExpectedExceptions()
	{
		char[] tag = null!;
		string tagString = null!;

		AssertThrowsArgumentNull(nameof(tag), () => Verify.GroupTags.ExactLength(tag, 4, nameof(tag)));
		AssertThrowsArgumentNull(nameof(tagString), () => Verify.GroupTags.ExactLength(tagString, 4, nameof(tagString)));
		AssertThrowsArgument(nameof(tagString), () => Verify.GroupTags.ExactLength(string.Empty, 4, nameof(tagString)));
		AssertThrowsArgumentOutOfRange(nameof(tag), () => Verify.GroupTags.ExactLength(Array.Empty<char>(), 4, nameof(tag)));
		AssertThrowsArgumentOutOfRange(nameof(tag), () => Verify.GroupTags.ExactLength("abc".ToCharArray(), 4, nameof(tag)));
		AssertThrowsArgumentOutOfRange(nameof(tagString), () => Verify.GroupTags.ExactLength("abc", 4, nameof(tagString)));

		Verify.GroupTags.ExactLength("test".ToCharArray(), 4, nameof(tag));
		Verify.GroupTags.ExactLength("test", 4, nameof(tagString));
	}
}
