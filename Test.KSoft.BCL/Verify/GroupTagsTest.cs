using System;
using KSoft.Values;
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

	[TestMethod]
	public void NameAndExactLength_ThrowsExpectedExceptions()
	{
		string groupTag = null!;
		string name = null!;
		int expectedLength = 0;

		AssertThrowsArgumentNull(nameof(groupTag), () =>
			Verify.GroupTags.NameAndExactLength(groupTag, "Name", 4));
		groupTag = string.Empty;
		AssertThrowsArgument(nameof(groupTag), () =>
			Verify.GroupTags.NameAndExactLength(groupTag, "Name", 4));
		AssertThrowsArgumentNull(nameof(name), () =>
			Verify.GroupTags.NameAndExactLength("test", name, 4));
		name = string.Empty;
		AssertThrowsArgument(nameof(name), () =>
			Verify.GroupTags.NameAndExactLength("test", name, 4));
		AssertThrowsArgumentOutOfRange(nameof(expectedLength), () =>
			Verify.GroupTags.NameAndExactLength("test", "Name", expectedLength));
		groupTag = "abc";
		AssertThrowsArgumentOutOfRange(nameof(groupTag), () =>
			Verify.GroupTags.NameAndExactLength(groupTag, "Name", 4));

		Verify.GroupTags.NameAndExactLength("test", "Name", 4);
	}

	[TestMethod]
	public void CompositePair_ThrowsExpectedExceptions()
	{
		GroupTagData32 maj = null!;
		GroupTagData32 min = null!;
		string name = null!;
		var validMajor = new GroupTagData32("majo", "Major");
		var validMinor = new GroupTagData32("mino", "Minor");

		AssertThrowsArgumentNull(nameof(maj), () =>
			Verify.GroupTags.CompositePair(maj, validMinor, "Name"));
		AssertThrowsArgumentNull(nameof(min), () =>
			Verify.GroupTags.CompositePair(validMajor, min, "Name"));
		AssertThrowsArgumentNull(nameof(name), () =>
			Verify.GroupTags.CompositePair(validMajor, validMinor, name));
		name = string.Empty;
		AssertThrowsArgument(nameof(name), () =>
			Verify.GroupTags.CompositePair(validMajor, validMinor, name));
		maj = GroupTagData32.Null;
		AssertThrowsArgument(nameof(maj), () =>
			Verify.GroupTags.CompositePair(maj, validMinor, "Name"));
		min = GroupTagData32.Null;
		AssertThrowsArgument(nameof(min), () =>
			Verify.GroupTags.CompositePair(validMajor, min, "Name"));

		Verify.GroupTags.CompositePair(validMajor, validMinor, "Name");
	}
}
