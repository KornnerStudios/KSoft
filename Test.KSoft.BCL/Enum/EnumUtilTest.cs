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
}
