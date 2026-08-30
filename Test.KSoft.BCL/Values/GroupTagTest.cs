using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Values.Test;

[TestClass]
public sealed class GroupTagTest : BaseTestClass
{
	const string TestUuid = "00000000-0000-0000-0000-000000000000";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Provides static tags discovered by GroupTagContainer reflection.")]
	sealed class TestGroupTagContainerHost32
	{
		public static GroupTag32Collection Groups { get; } =
			new(new GroupTagData32("test", "Test"));
		public static GroupTag32Collection OtherGroups { get; } =
			new(new GroupTagData32("othr", "Other"));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Provides static tags discovered by GroupTagContainer reflection.")]
	sealed class TestGroupTagContainerHost64
	{
		public static GroupTag64Collection Groups { get; } =
			new(new GroupTagData64("testtag8", "Test"));
	}

	[GroupTagContainer32(typeof(TestGroupTagContainerHost32))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Supplies metadata consumed by GroupTagContainer reflection.")]
	sealed class TestGroupTagContainerTarget32
	{
	}

	[GroupTagContainer32(typeof(NullGroupsHost))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Supplies intentional invalid metadata for reflection validation.")]
	sealed class NullGroupsTarget
	{
	}

	[GroupTagContainer32(typeof(NonCollectionGroupsHost))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Supplies intentional invalid metadata for reflection validation.")]
	sealed class NonCollectionGroupsTarget
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Intentional missing-member fixture for reflection validation.")]
	sealed class MissingGroupsHost
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Intentional null-member fixture for reflection validation.")]
	sealed class NullGroupsHost
	{
		public static GroupTag32Collection Groups { get; } = null!;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Intentional non-collection fixture for reflection validation.")]
	sealed class NonCollectionGroupsHost
	{
		public static string Groups { get; } = "not a group collection";
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

	static void GroupTagData32_TestWithUndersizedFirstSpan()
	{
		ReadOnlySpan<char> tag1 = stackalloc char[3];
		ReadOnlySpan<char> tag2 = "ABCD";
		_ = GroupTagData32.Test(tag1, tag2);
	}

	static void GroupTagData32_TestWithUndersizedSecondSpan()
	{
		ReadOnlySpan<char> tag1 = "ABCD";
		ReadOnlySpan<char> tag2 = stackalloc char[3];
		_ = GroupTagData32.Test(tag1, tag2);
	}

	static void GroupTagData32_ToUIntWithUndersizedSpan()
	{
		ReadOnlySpan<char> tag = stackalloc char[3];
		_ = GroupTagData32.ToUInt(tag);
	}

	static void GroupTagData32_FromUIntWithUndersizedSpan()
	{
		Span<char> tag = stackalloc char[3];
		GroupTagData32.FromUInt(0, tag);
	}

	static void GroupTagData64_TestWithUndersizedFirstSpan()
	{
		ReadOnlySpan<char> tag1 = stackalloc char[7];
		ReadOnlySpan<char> tag2 = "ABCDEFGH";
		_ = GroupTagData64.Test(tag1, tag2);
	}

	static void GroupTagData64_TestWithUndersizedSecondSpan()
	{
		ReadOnlySpan<char> tag1 = "ABCDEFGH";
		ReadOnlySpan<char> tag2 = stackalloc char[7];
		_ = GroupTagData64.Test(tag1, tag2);
	}

	static void GroupTagData64_ToULongWithUndersizedSpan()
	{
		ReadOnlySpan<char> tag = stackalloc char[7];
		_ = GroupTagData64.ToULong(tag);
	}

	static void GroupTagData64_FromULongWithUndersizedSpan()
	{
		Span<char> tag = stackalloc char[7];
		GroupTagData64.FromULong(0, tag);
	}

	static void GroupTagData_TestWithSizedSpan(GroupTagData groupTag, int length)
	{
		Span<char> other = stackalloc char[length];
		_ = groupTag.Test(other);
	}

	static void GroupTagCollection_IndexWithSizedSpan(GroupTagCollection collection, int length)
	{
		Span<char> tag = stackalloc char[length];
		_ = collection[tag];
	}

	static void GroupTagCollection_FindIndexWithSizedSpan(GroupTagCollection collection, int length)
	{
		Span<char> groupTag = stackalloc char[length];
		_ = collection.FindGroupIndexByTag(groupTag);
	}

	static void GroupTagCollection_FindGroupWithSizedSpan(GroupTagCollection collection, int length)
	{
		Span<char> groupTag = stackalloc char[length];
		_ = collection.FindGroup(groupTag);
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
		AssertThrowsArgumentNull("other", () => _ = groupTag.CompareId(null!));
		AssertThrowsArgumentNull("value", () => _ = (uint)(GroupTagData32)null!);
		AssertThrowsArgumentNull("value", () => _ = (ulong)(GroupTagData64)null!);
	}

	[TestMethod]
	public void GroupTagData_SpanOverloads_UseStackDestinationsAndPreserveEndianLayouts()
	{
		const uint tag32Value = 0x41424344;
		const ulong tag64Value = 0x4142434445464748;
		const uint expectedTag32Value = 0x41424344;
		const ulong expectedTag64Value = 0x4142434445464748;

		Span<char> tag32 = stackalloc char[5];
		tag32.Fill('_');
		GroupTagData32.FromUInt(tag32Value, tag32);
		Assert.AreEqual("ABCD", new string(tag32.Slice(0, 4)));
		Assert.AreEqual('_', tag32[4]);
		Assert.AreEqual(System.BitConverter.IsLittleEndian ? expectedTag32Value : 0x44434241u, GroupTagData32.ToUInt(tag32));
		Assert.IsTrue(GroupTagData32.Test(tag32, "ABCD".AsSpan()));
		GroupTagData32.FromUInt(tag32Value, tag32, isBigEndian: false);
		Assert.AreEqual("DCBA", new string(tag32.Slice(0, 4)));

		Span<char> tag64 = stackalloc char[9];
		tag64.Fill('_');
		GroupTagData64.FromULong(tag64Value, tag64);
		Assert.AreEqual("EFGHABCD", new string(tag64.Slice(0, 8)));
		Assert.AreEqual('_', tag64[8]);
		Assert.AreEqual(System.BitConverter.IsLittleEndian ? expectedTag64Value : 0x4847464544434241ul,
			GroupTagData64.ToULong("ABCDEFGH".AsSpan()));
		Assert.IsTrue(GroupTagData64.Test(tag64, "EFGHABCD".AsSpan()));
		GroupTagData64.FromULong(tag64Value, tag64, isBigEndian: false);
		Assert.AreEqual("HGFEDCBA", new string(tag64.Slice(0, 8)));
	}

	[TestMethod]
	public void GroupTagData_SpanOverloads_RejectUndersizedBuffersWithExpectedParameterNames()
	{
		AssertThrowsArgumentOutOfRange("tag1", GroupTagData32_TestWithUndersizedFirstSpan);
		AssertThrowsArgumentOutOfRange("tag2", GroupTagData32_TestWithUndersizedSecondSpan);
		AssertThrowsArgumentOutOfRange("tag", GroupTagData32_ToUIntWithUndersizedSpan);
		AssertThrowsArgumentOutOfRange("tag", GroupTagData32_FromUIntWithUndersizedSpan);
		AssertThrowsArgumentOutOfRange("tag1", GroupTagData64_TestWithUndersizedFirstSpan);
		AssertThrowsArgumentOutOfRange("tag2", GroupTagData64_TestWithUndersizedSecondSpan);
		AssertThrowsArgumentOutOfRange("tag", GroupTagData64_ToULongWithUndersizedSpan);
		AssertThrowsArgumentOutOfRange("tag", GroupTagData64_FromULongWithUndersizedSpan);
	}

	[TestMethod]
	public void GroupTagData_StaticSpanOperations_ProcessRequiredPrefixes()
	{
		Assert.IsTrue(GroupTagData32.Test("ABCD_".AsSpan(), "ABCD!".AsSpan()));
		Assert.IsTrue(GroupTagData64.Test("ABCDEFGH_".AsSpan(), "ABCDEFGH!".AsSpan()));
		Assert.AreEqual(GroupTagData32.ToUInt("ABCD".AsSpan()), GroupTagData32.ToUInt("ABCD_".AsSpan()));
		Assert.AreEqual(GroupTagData64.ToULong("ABCDEFGH".AsSpan()), GroupTagData64.ToULong("ABCDEFGH_".AsSpan()));
	}

	[TestMethod]
	public void GroupTagData32_SpanSeam_StreamTagBigEndianUsesFourCharacterScratchSpan()
	{
		const uint tagValue = 0x41424344;

		foreach (var (byteOrder, expectedBytes) in new[]
		{
			(KSoft.Shell.EndianFormat.Big, new byte[] { 0x41, 0x42, 0x43, 0x44 }),
			(KSoft.Shell.EndianFormat.Little, new byte[] { 0x44, 0x43, 0x42, 0x41 }),
		})
		{
			using var bytes = new System.IO.MemoryStream();
			using (var writer = new KSoft.IO.EndianWriter(bytes, byteOrder) { BaseStreamOwner = false })
			using (var stream = KSoft.IO.EndianStream.UsingWriter(writer))
			{
				uint value = tagValue;
				stream.StreamTagBigEndian(ref value);
			}

			CollectionAssert.AreEqual(expectedBytes, bytes.ToArray());

			using var reader = new KSoft.IO.EndianReader(new System.IO.MemoryStream(expectedBytes), byteOrder);
			using var streamReader = KSoft.IO.EndianStream.UsingReader(reader);
			uint readValue = 0;
			streamReader.StreamTagBigEndian(ref readValue);
			Assert.AreEqual(tagValue, readValue);
		}
	}

	[TestMethod]
	public void GroupTagData_InstanceTests_RequireExactWidth()
	{
		var groupTag32 = new GroupTagData32("test", "Test");
		var groupTag64 = new GroupTagData64("testtag8", "Test");

		AssertThrowsArgumentOutOfRange("other", () => GroupTagData_TestWithSizedSpan(groupTag32, 3));
		AssertThrowsArgumentOutOfRange("other", () => GroupTagData_TestWithSizedSpan(groupTag32, 5));
		Assert.IsTrue(groupTag32.Test("test".AsSpan()));

		AssertThrowsArgumentOutOfRange("other", () => GroupTagData_TestWithSizedSpan(groupTag64, 7));
		AssertThrowsArgumentOutOfRange("other", () => GroupTagData_TestWithSizedSpan(groupTag64, 9));
		Assert.IsTrue(groupTag64.Test("testtag8".AsSpan()));
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

		AssertThrowsArgumentOutOfRange("tag", () => GroupTagCollection_IndexWithSizedSpan(collection, 3));
		AssertThrowsArgumentOutOfRange("tag", () => GroupTagCollection_IndexWithSizedSpan(collection, 5));
		AssertThrowsArgumentOutOfRange("groupTag", () => GroupTagCollection_FindIndexWithSizedSpan(collection, 3));
		AssertThrowsArgumentOutOfRange("groupTag", () => GroupTagCollection_FindIndexWithSizedSpan(collection, 5));
		AssertThrowsArgumentNull("tagString", () => _ = collection.FindGroupIndexByTag((string)null!));
		AssertThrowsArgument("tagString", () => _ = collection.FindGroupIndexByTag(string.Empty));
		AssertThrowsArgumentOutOfRange("tagString", () => _ = collection.FindGroupIndexByTag("abc"));
		AssertThrowsArgumentOutOfRange("tagString", () => _ = collection.FindGroupIndexByTag("abcde"));
		AssertThrowsArgumentNull("groupName", () => _ = collection.FindGroupIndex((string)null!));
		AssertThrowsArgument("groupName", () => _ = collection.FindGroupIndex(string.Empty));
		AssertThrowsArgumentNull("group", () => _ = collection.FindGroupIndex((GroupTagData)null!));
		AssertThrowsArgumentOutOfRange("groupTag", () => GroupTagCollection_FindGroupWithSizedSpan(collection, 3));
		AssertThrowsArgumentOutOfRange("groupTag", () => GroupTagCollection_FindGroupWithSizedSpan(collection, 5));
		AssertThrowsArgumentNull("tagString", () => _ = collection.FindGroupByTag((string)null!));
		AssertThrowsArgumentNull("groupName", () => _ = collection.FindGroup((string)null!));

		Assert.AreEqual("Test", collection["test".AsSpan()]);
		Assert.AreEqual(collection.NullGroupTag.Name, collection["miss".AsSpan()]);
		Assert.AreEqual(0, collection.FindGroupIndexByTag("test".AsSpan()));
		Assert.AreSame(groupTag, collection.FindGroup("test".AsSpan()));
		Assert.AreSame(groupTag, collection.FindGroupByTag("test"));
		Assert.IsNull(collection.FindGroup("Missing"));
	}

	[TestMethod]
	public void GroupTagCollections_SearchAndTypedContainerLookup_PreserveNullSentinels()
	{
		var collection32 = new GroupTag32Collection(
			new GroupTagData32("zeta", "Zeta"),
			new GroupTagData32("alph", "Alpha"));
		var collection64 = new GroupTag64Collection(
			new GroupTagData64("zetatag8", "Zeta"),
			new GroupTagData64("alpha000", "Alpha"));

		Assert.IsNull(collection32.FindGroup("Missing"));
		Assert.IsNull(collection32.FindGroupByTag("miss"));
		Assert.IsNull(collection32.FindGroup("miss".AsSpan()));
		Assert.IsNull(collection32.FindGroupByTag(0));
		collection32.Sort();
		Assert.AreEqual("Alpha", collection32.GroupTags[0].Name);

		Assert.IsNull(collection64.FindGroup("Missing"));
		Assert.IsNull(collection64.FindGroupByTag("missing!"));
		Assert.IsNull(collection64.FindGroup("missing!".AsSpan()));
		Assert.IsNull(collection64.FindGroupByTag(0));
		collection64.Sort();
		Assert.AreEqual("Alpha", collection64.GroupTags[0].Name);

		Assert.IsNull(new GroupTagContainer32Attribute(typeof(TestGroupTagContainerHost64)).Collection);
		Assert.IsNull(new GroupTagContainer64Attribute(typeof(TestGroupTagContainerHost32)).Collection);
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

		Assert.IsNull(GroupTagContainerAttribute.GetCollection(typeof(NullGroupsTarget)));
		Assert.IsNull(GroupTagContainerAttribute.GetCollection(typeof(NonCollectionGroupsTarget)));

		int collectionCount = 0;
		foreach (var pair in GroupTagContainerAttribute.GetAllCollections(typeof(TestGroupTagContainerTarget32)))
		{
			Assert.IsNotNull(pair.Key);
			Assert.IsNotNull(pair.Value);
			collectionCount++;
		}
		Assert.AreEqual(2, collectionCount);

		int nullCollectionCount = 0;
		foreach (var pair in GroupTagContainerAttribute.GetAllCollections(typeof(NullGroupsTarget)))
		{
			Assert.AreEqual("Groups", pair.Key);
			Assert.IsNull(pair.Value);
			nullCollectionCount++;
		}
		Assert.AreEqual(1, nullCollectionCount);
		Assert.IsFalse(GroupTagContainerAttribute.GetAllCollections(typeof(NonCollectionGroupsTarget)).GetEnumerator().MoveNext());
	}
}
