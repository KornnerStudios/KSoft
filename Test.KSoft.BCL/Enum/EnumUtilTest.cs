using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Reflection.Test;

[TestClass]
public sealed class EnumUtilTest : BaseTestClass
{
	enum MetadataEnum : long
	{
		Zero = 0,
		Positive = 1,
		PositiveAlias = Positive,
		BigPositive = long.MaxValue,
		Negative = -1,
	};

	[Flags]
	enum MetadataFlagsEnum
	{
		None = 0,
		One = 1,
		Two = 2,
	};

	static void AssertThrowsArgumentNull(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	static void AssertThrowsArgument(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	[TestMethod]
	public void Enum_UtilMetadataMatchesGenericBclApis()
	{
		string[] expectedNames =
		{
			nameof(MetadataEnum.Zero),
			nameof(MetadataEnum.Positive),
			nameof(MetadataEnum.PositiveAlias),
			nameof(MetadataEnum.BigPositive),
			nameof(MetadataEnum.Negative),
		};
		MetadataEnum[] expectedValues =
		{
			MetadataEnum.Zero,
			MetadataEnum.Positive,
			MetadataEnum.PositiveAlias,
			MetadataEnum.BigPositive,
			MetadataEnum.Negative,
		};

		CollectionAssert.AreEqual(expectedNames, EnumUtil<MetadataEnum>.Names);
		CollectionAssert.AreEqual(expectedValues, EnumUtil<MetadataEnum>.Values);

		// Enum.GetNames/GetValues define the sorted value order, including duplicate underlying values.
		CollectionAssert.AreEqual(Enum.GetNames<MetadataEnum>(), EnumUtil<MetadataEnum>.Names);
		CollectionAssert.AreEqual(Enum.GetValues<MetadataEnum>(), EnumUtil<MetadataEnum>.Values);
		Assert.AreEqual(typeof(long), EnumUtil<MetadataEnum>.UnderlyingType);
		Assert.AreEqual(TypeCode.Int64, EnumUtil<MetadataEnum>.UnderlyingTypeCode);
		Assert.IsFalse(EnumUtil<MetadataEnum>.IsFlags);
	}

	[TestMethod]
	public void Enum_UtilMetadataPropertiesKeepCachedArrayInstances()
	{
		// Existing callers can observe the arrays directly, so the shim must keep returning cached instances.
		Assert.AreSame(EnumUtil<MetadataEnum>.Names, EnumUtil<MetadataEnum>.Names);
		Assert.AreSame(EnumUtil<MetadataEnum>.Values, EnumUtil<MetadataEnum>.Values);
	}

	[TestMethod]
	public void EnumUtils_AssertHelpers_InvalidArgumentsThrow()
	{
		AssertThrowsArgumentNull("kEnumType", () =>
			KSoft.Reflection.EnumUtils.AssertUnderlyingTypeIsSupported(null!, null!));
		AssertThrowsArgumentNull("theType", () =>
			KSoft.Reflection.EnumUtils.AssertTypeIsEnum(null!));
		AssertThrowsArgumentNull("theType", () =>
			KSoft.Reflection.EnumUtils.AssertTypeIsFlagsEnum(null!));

		Assert.ThrowsExactly<NotSupportedException>(() =>
			KSoft.Reflection.EnumUtils.AssertTypeIsEnum(typeof(string)));
		Assert.ThrowsExactly<NotSupportedException>(() =>
			KSoft.Reflection.EnumUtils.AssertTypeIsFlagsEnum(typeof(MetadataEnum)));

		KSoft.Reflection.EnumUtils.AssertUnderlyingTypeIsSupported(typeof(MetadataEnum), null);
		KSoft.Reflection.EnumUtils.AssertTypeIsEnum(typeof(MetadataEnum));
		KSoft.Reflection.EnumUtils.AssertTypeIsFlagsEnum(typeof(MetadataFlagsEnum));
	}

	[TestMethod]
	public void EnumUtils_GetEnumFields_ReturnsEnumFields()
	{
		AssertThrowsArgumentNull("enumType", () =>
			KSoft.Reflection.EnumUtils.GetEnumFields(null!));
		AssertThrowsArgument("enumType", () =>
			KSoft.Reflection.EnumUtils.GetEnumFields(typeof(string)));

		var fields = KSoft.Reflection.EnumUtils.GetEnumFields(typeof(MetadataEnum));
		var fieldNames = fields.ConvertAll(field => field.Name);

		CollectionAssert.AreEqual(new[]
		{
			nameof(MetadataEnum.Zero),
			nameof(MetadataEnum.Positive),
			nameof(MetadataEnum.PositiveAlias),
			nameof(MetadataEnum.BigPositive),
			nameof(MetadataEnum.Negative),
		}, fieldNames);
	}
}
