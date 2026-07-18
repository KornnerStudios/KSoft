using System;
using System.Linq;
using KSoft.SourceGeneration.Descriptors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class PrimitiveCatalogTests
{
	[TestMethod]
	public void NumbersMatchT4OrderTest()
	{
		CollectionAssert.AreEqual(
			new[] { "byte", "sbyte", "ushort", "short", "uint", "int", "ulong", "long", "float", "double" },
			PrimitiveCatalog.Numbers.Select(static x => x.Keyword).ToArray());
	}

	[TestMethod]
	public void PrimitivesMatchT4OrderWithoutStringTest()
	{
		CollectionAssert.AreEqual(
			new[] {
				"bool", "char", "byte", "sbyte", "ushort", "short",
				"uint", "int", "ulong", "long", "float", "double",
			},
			PrimitiveCatalog.Primitives.Select(static x => x.Keyword).ToArray());
	}

	[TestMethod]
	public void BittableListsMatchT4OrderTest()
	{
		CollectionAssert.AreEqual(
			new[] { "byte", "ushort", "uint", "ulong" },
			PrimitiveCatalog.BittableTypesUnsigned.Select(static x => x.Keyword).ToArray());
		CollectionAssert.AreEqual(
			new[] { "uint", "ulong" },
			PrimitiveCatalog.BittableTypesMajorWords.Select(static x => x.Keyword).ToArray());
		CollectionAssert.AreEqual(
			new[] { "uint", "int", "ulong", "long" },
			PrimitiveCatalog.BittableTypesInt32And64.Select(static x => x.Keyword).ToArray());
	}

	[TestMethod]
	public void NumberPropertiesMatchT4BehaviorTest()
	{
		NumberSpec byteSpec = PrimitiveCatalog.Numbers.Single(static x => x.TypeCode == TypeCode.Byte);
		NumberSpec ushortSpec = PrimitiveCatalog.Numbers.Single(static x => x.TypeCode == TypeCode.UInt16);
		NumberSpec uintSpec = PrimitiveCatalog.Numbers.Single(static x => x.TypeCode == TypeCode.UInt32);
		NumberSpec ulongSpec = PrimitiveCatalog.Numbers.Single(static x => x.TypeCode == TypeCode.UInt64);

		Assert.AreEqual("uint", byteSpec.OperationWord);
		Assert.AreEqual("uint", ushortSpec.OperationWord);
		Assert.AreEqual("uint", uintSpec.OperationWord);
		Assert.AreEqual("ulong", ulongSpec.OperationWord);
		Assert.AreEqual("Byte", byteSpec.ConstantKeyword);
		Assert.AreEqual("Int16", ushortSpec.ConstantKeyword);
		Assert.AreEqual("Int32", uintSpec.ConstantKeyword);
		Assert.AreEqual("Int64", ulongSpec.ConstantKeyword);
		Assert.AreEqual("X2", byteSpec.ToStringHexFormat);
		Assert.AreEqual("X4", ushortSpec.ToStringHexFormat);
		Assert.AreEqual("X8", uintSpec.ToStringHexFormat);
		Assert.AreEqual("X16", ulongSpec.ToStringHexFormat);
		Assert.IsTrue(byteSpec.BitOperatorsImplicitlyUpCast);
		Assert.IsTrue(ushortSpec.BitOperatorsImplicitlyUpCast);
		Assert.IsFalse(uintSpec.BitOperatorsImplicitlyUpCast);
		Assert.IsFalse(ulongSpec.BitOperatorsImplicitlyUpCast);
	}

	[TestMethod]
	public void NumericPrimitivesExposePrimitiveViewTest()
	{
		Assert.AreEqual(PrimitiveCatalog.Numbers[0].Primitive, PrimitiveCatalog.Primitives[2]);
		Assert.IsFalse(PrimitiveCatalog.Bool.IsInteger);
		Assert.IsFalse(PrimitiveCatalog.Char.IsInteger);
		Assert.AreEqual(TypeCode.Object, PrimitiveCatalog.KGuid.TypeCode);
	}

	[TestMethod]
	public void SimpleDescriptionsMatchT4BehaviorTest()
	{
		CollectionAssert.AreEqual(
			new[] {
				"unsigned 8-bit integer", "signed 8-bit integer",
				"unsigned 16-bit integer", "signed 16-bit integer",
				"unsigned 32-bit integer", "signed 32-bit integer",
				"unsigned 64-bit integer", "signed 64-bit integer",
				"single-precision number", "double-precision number",
			},
			PrimitiveCatalog.Numbers.Select(static x => x.SimpleDescription).ToArray());
		Assert.AreEqual("NO DESC", PrimitiveCatalog.Bool.SimpleDescription);
	}
};
