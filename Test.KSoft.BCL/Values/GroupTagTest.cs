using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Values.Test;

[TestClass]
public sealed class GroupTagTest : BaseTestClass
{
	const string TestUuid = "00000000-0000-0000-0000-000000000000";

	sealed class TestGroupTagContainerHost32
	{
		public static GroupTag32Collection Groups { get; } =
			new(new GroupTagData32("test", "Test"));
		public static GroupTag32Collection OtherGroups { get; } =
			new(new GroupTagData32("othr", "Other"));
	}

	[GroupTagContainer32(typeof(TestGroupTagContainerHost32))]
	sealed class TestGroupTagContainerTarget32
	{
	}

	sealed class MissingGroupsHost
	{
	}

	sealed class ExposedGroupTagContainer32Attribute : GroupTagContainer32Attribute
	{
		public ExposedGroupTagContainer32Attribute(Type container, string collectionName)
			: base(container, collectionName)
		{
		}
	}

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
	public void GroupTagData_Constructors_ThrowExpectedExceptions()
	{
		var major = new GroupTagData32("majo", "Major");
		var minor = new GroupTagData32("mino", "Minor");

		AssertThrowsArgumentNull("groupTag", () => _ = new GroupTagData32(null!, "Name"));
		AssertThrowsArgument("groupTag", () => _ = new GroupTagData32(string.Empty, "Name"));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData32("abc", "Name"));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData32("abcde", "Name"));
		AssertThrowsArgumentNull("name", () => _ = new GroupTagData32("test", null!));
		AssertThrowsArgument("name", () => _ = new GroupTagData32("test", string.Empty));

		AssertThrowsArgumentNull("groupTag", () => _ = new GroupTagData64(null!, "Name"));
		AssertThrowsArgument("groupTag", () => _ = new GroupTagData64(string.Empty, "Name"));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData64("tag7chr", "Name"));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData64("tag9chars", "Name"));
		AssertThrowsArgumentNull("name", () => _ = new GroupTagData64("testtag8", null!));
		AssertThrowsArgument("name", () => _ = new GroupTagData64("testtag8", string.Empty));

		AssertThrowsArgumentNull("maj", () => _ = new GroupTagData64(null!, minor, "Name"));
		AssertThrowsArgumentNull("min", () => _ = new GroupTagData64(major, null!, "Name"));
		AssertThrowsArgument("maj", () => _ = new GroupTagData64(GroupTagData32.Null, minor, "Name"));
		AssertThrowsArgument("min", () => _ = new GroupTagData64(major, GroupTagData32.Null, "Name"));
		AssertThrowsArgumentNull("name", () => _ = new GroupTagData64(major, minor, null!));
		AssertThrowsArgument("name", () => _ = new GroupTagData64(major, minor, string.Empty));
	}

	[TestMethod]
	public void GroupTagData_OperatorAndCompareGuards_ThrowArgumentNullException()
	{
		var groupTag = new GroupTagData32("test", "Test");

		AssertThrowsArgumentNull("value", () => _ = (string)(GroupTagData)null!);
		AssertThrowsArgumentNull("value", () => _ = (char[])(GroupTagData)null!);
		AssertThrowsArgumentNull("other", () => _ = groupTag.CompareId(null!));
		AssertThrowsArgumentNull("value", () => _ = (uint)(GroupTagData32)null!);
		AssertThrowsArgumentNull("value", () => _ = (ulong)(GroupTagData64)null!);
	}

	[TestMethod]
	public void GroupTagData32_ShortTags_ThrowArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.Swap(new char[3]));
		AssertThrowsArgumentOutOfRange("tag1", () => _ = GroupTagData32.Test(new char[3], new char[4]));
		AssertThrowsArgumentOutOfRange("tag2", () => _ = GroupTagData32.Test(new char[4], new char[3]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.ToUInt(new char[3]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.ToUInt("abc"));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData32.FromUInt(0, new char[3]));
	}

	[TestMethod]
	public void GroupTagData32_NullTags_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData32.Swap(null!));
		AssertThrowsArgumentNull("tag1", () => _ = GroupTagData32.Test(null!, new char[4]));
		AssertThrowsArgumentNull("tag2", () => _ = GroupTagData32.Test(new char[4], null!));
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData32.ToUInt((char[])null!));
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData32.ToUInt((string)null!));
		AssertThrowsArgument("tag", () => _ = GroupTagData32.ToUInt(string.Empty));
	}

	[TestMethod]
	public void GroupTagData64_ShortTags_ThrowArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.Swap(new char[7]));
		AssertThrowsArgumentOutOfRange("tag1", () => _ = GroupTagData64.Test(new char[7], new char[8]));
		AssertThrowsArgumentOutOfRange("tag2", () => _ = GroupTagData64.Test(new char[8], new char[7]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.ToULong(new char[7]));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.ToULong("tag7chr"));
		AssertThrowsArgumentOutOfRange("tag", () => _ = GroupTagData64.FromULong(0, new char[7]));
	}

	[TestMethod]
	public void GroupTagData64_NullTags_ThrowArgumentNullException()
	{
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData64.Swap(null!));
		AssertThrowsArgumentNull("tag1", () => _ = GroupTagData64.Test(null!, new char[8]));
		AssertThrowsArgumentNull("tag2", () => _ = GroupTagData64.Test(new char[8], null!));
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData64.ToULong((char[])null!));
		AssertThrowsArgumentNull("tag", () => _ = GroupTagData64.ToULong((string)null!));
		AssertThrowsArgument("tag", () => _ = GroupTagData64.ToULong(string.Empty));
	}

	[TestMethod]
	public void GroupTagData_TestGuards_ThrowExpectedExceptions()
	{
		var groupTag32 = new GroupTagData32("test", "Test");
		var groupTag64 = new GroupTagData64("testtag8", "Test");

		AssertThrowsArgumentNull("other", () => _ = groupTag32.Test(null!));
		AssertThrowsArgumentOutOfRange("other", () => _ = groupTag32.Test(new char[3]));
		AssertThrowsArgumentOutOfRange("other", () => _ = groupTag32.Test(new char[5]));
		Assert.IsTrue(groupTag32.Test("test".ToCharArray()));

		AssertThrowsArgumentNull("other", () => _ = groupTag64.Test(null!));
		AssertThrowsArgumentOutOfRange("other", () => _ = groupTag64.Test(new char[7]));
		AssertThrowsArgumentOutOfRange("other", () => _ = groupTag64.Test(new char[9]));
		Assert.IsTrue(groupTag64.Test("testtag8".ToCharArray()));
	}

	[TestMethod]
	public void GroupTagData_EqualsNull_ReturnsFalse()
	{
		var groupTag32 = new GroupTagData32("test", "Test");
		var groupTag64 = new GroupTagData64("testtag8", "Test");

		Assert.IsFalse(groupTag32.Equals((GroupTagData)null!));
		Assert.IsFalse(groupTag64.Equals((GroupTagData)null!));
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

	[TestMethod]
	public void GroupTagCollections_NullElements_ThrowArgumentException()
	{
		AssertThrowsArgument("groupTags", () => _ = new GroupTag32Collection(new GroupTagData32("test", "Test"), null!));
		AssertThrowsArgument("groupTags", () => _ = new GroupTag64Collection(new GroupTagData64("testtag8", "Test"), null!));
	}

	[TestMethod]
	public void GroupTagCollection_SearchGuards_ThrowExpectedExceptions()
	{
		var groupTag = new GroupTagData32("test", "Test");
		var collection = new GroupTag32Collection(groupTag);

		AssertThrowsArgumentNull("tag", () => _ = collection[(char[])null!]);
		AssertThrowsArgumentOutOfRange("tag", () => _ = collection["abc".ToCharArray()]);
		AssertThrowsArgumentNull("groupTag", () => _ = collection.FindGroupIndexByTag((char[])null!));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = collection.FindGroupIndexByTag("abc".ToCharArray()));
		AssertThrowsArgumentNull("tagString", () => _ = collection.FindGroupIndexByTag((string)null!));
		AssertThrowsArgument("tagString", () => _ = collection.FindGroupIndexByTag(string.Empty));
		AssertThrowsArgumentOutOfRange("tagString", () => _ = collection.FindGroupIndexByTag("abc"));
		AssertThrowsArgumentNull("groupName", () => _ = collection.FindGroupIndex((string)null!));
		AssertThrowsArgument("groupName", () => _ = collection.FindGroupIndex(string.Empty));
		AssertThrowsArgumentNull("group", () => _ = collection.FindGroupIndex((GroupTagData)null!));
		AssertThrowsArgumentNull("groupTag", () => _ = collection.FindGroup((char[])null!));
		AssertThrowsArgumentNull("tagString", () => _ = collection.FindGroupByTag((string)null!));
		AssertThrowsArgumentNull("groupName", () => _ = collection.FindGroup((string)null!));

		Assert.AreEqual("Test", collection["test".ToCharArray()]);
		Assert.AreSame(groupTag, collection.FindGroupByTag("test"));
		Assert.IsNull(collection.FindGroup("Missing"));
	}

	[TestMethod]
	public void GroupTagDataAttributes_InvalidArguments_ThrowBeforeUuidParsing()
	{
		AssertThrowsArgumentNull("groupTag", () => _ = new GroupTagData32Attribute(null!, "Name", null!));
		AssertThrowsArgument("groupTag", () => _ = new GroupTagData32Attribute(string.Empty, "Name", TestUuid));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData32Attribute("abc", "Name", TestUuid));
		AssertThrowsArgumentNull("name", () => _ = new GroupTagData32Attribute("test", null!, TestUuid));
		AssertThrowsArgument("name", () => _ = new GroupTagData32Attribute("test", string.Empty, TestUuid));

		AssertThrowsArgumentNull("groupTag", () => _ = new GroupTagData64Attribute(null!, "Name", null!));
		AssertThrowsArgument("groupTag", () => _ = new GroupTagData64Attribute(string.Empty, "Name", TestUuid));
		AssertThrowsArgumentOutOfRange("groupTag", () => _ = new GroupTagData64Attribute("tag7chr", "Name", TestUuid));
		AssertThrowsArgumentNull("name", () => _ = new GroupTagData64Attribute("testtag8", null!, TestUuid));
		AssertThrowsArgument("name", () => _ = new GroupTagData64Attribute("testtag8", string.Empty, TestUuid));
	}

	[TestMethod]
	public void GroupTagContainerAttributes_GuardsAndLookup_PreserveContainerBehavior()
	{
		AssertThrowsArgumentNull("container", () => _ = new GroupTagContainer32Attribute(null!));
		AssertThrowsArgumentNull("container", () => _ = new ExposedGroupTagContainer32Attribute(null!, "Groups"));
		AssertThrowsArgumentNull("collectionName", () =>
			_ = new ExposedGroupTagContainer32Attribute(typeof(TestGroupTagContainerHost32), null!));
		AssertThrowsArgument("collectionName", () =>
			_ = new ExposedGroupTagContainer32Attribute(typeof(TestGroupTagContainerHost32), string.Empty));
		AssertThrowsArgument("collectionName", () =>
			_ = new ExposedGroupTagContainer32Attribute(typeof(MissingGroupsHost), "Groups"));
		AssertThrowsArgumentNull("container", () => _ = GroupTagContainerAttribute.GetCollection(null!));
		AssertThrowsArgumentNull("container", () => _ = GroupTagContainerAttribute.GetAllCollections(null!));

		var collection = GroupTagContainerAttribute.GetCollection(typeof(TestGroupTagContainerTarget32));
		Assert.AreSame(TestGroupTagContainerHost32.Groups, collection);

		int collectionCount = 0;
		foreach (var pair in GroupTagContainerAttribute.GetAllCollections(typeof(TestGroupTagContainerTarget32)))
		{
			Assert.IsNotNull(pair.Key);
			Assert.IsNotNull(pair.Value);
			collectionCount++;
		}
		Assert.AreEqual(2, collectionCount);
	}
}
