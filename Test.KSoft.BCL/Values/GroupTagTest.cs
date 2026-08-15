using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Values.Test;

[TestClass]
public sealed class GroupTagTest : BaseTestClass
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
	public void GroupTagData_ReservedNullGroupName_ThrowsArgumentException()
	{
		var major = new GroupTagData32("majo", "Major");
		var minor = new GroupTagData32("mino", "Minor");

		AssertThrowsArgument("name", () => _ = new GroupTagData32("test", GroupTagData.kNullGroupName));
		AssertThrowsArgument("name", () => _ = new GroupTagData64("testtag8", GroupTagData.kNullGroupName));
		AssertThrowsArgument("name", () => _ = new GroupTagData64(major, minor, GroupTagData.kNullGroupName));
	}

	[TestMethod]
	public void GroupTagData32_ShortTags_ThrowArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.Swap(new char[3]));
		AssertThrowsArgumentOutOfRange("tag1", () => _ = GroupTagData32.Test(new char[3], new char[4]));
		AssertThrowsArgumentOutOfRange("tag2", () => _ = GroupTagData32.Test(new char[4], new char[3]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.ToUInt(new char[3]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.ToUInt("abc"));
	}

	[TestMethod]
	public void GroupTagData64_ShortTags_ThrowArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.Swap(new char[7]));
		AssertThrowsArgumentOutOfRange("tag1", () => _ = GroupTagData64.Test(new char[7], new char[8]));
		AssertThrowsArgumentOutOfRange("tag2", () => _ = GroupTagData64.Test(new char[8], new char[7]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.ToULong(new char[7]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.ToULong("tag7chr"));
	}

	[TestMethod]
	public void GroupTagCollections_NullGroupTags_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag32Collection((GroupTagData32[])null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag32Collection(KGuid.Empty, null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag32Collection(sort: false, null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag32Collection(KGuid.Empty, sort: false, null!));

		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag64Collection((GroupTagData64[])null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag64Collection(KGuid.Empty, null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag64Collection(sort: false, null!));
		AssertThrowsArgumentNull("groupTags", () => _ = new GroupTag64Collection(KGuid.Empty, sort: false, null!));
	}
}
