using System;
using System.Linq;
using KSoft.SourceGeneration;
using KSoft.SourceGeneration.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class GeneratorRegistryTests
{
	[TestMethod]
	public void RegistryIncludesEveryFeatureEnumValueTest()
	{
		var enumFeatures = Enum.GetValues(typeof(GeneratorFeature))
			.Cast<GeneratorFeature>()
			.OrderBy(static x => x)
			.ToArray();
		var registeredFeatures = GeneratorRegistry.Features
			.Select(static x => x.Feature)
			.OrderBy(static x => x)
			.ToArray();

		CollectionAssert.AreEqual(enumFeatures, registeredFeatures);
	}

	[TestMethod]
	public void RegistryUsesUniqueFeaturePropertyAndHintNamesTest()
	{
		var featureNames = GeneratorRegistry.Features.Select(static x => x.Feature.ToString()).ToArray();
		var propertyNames = GeneratorRegistry.Features.Select(static x => x.PropertyName).ToArray();
		var hintNames = GeneratorRegistry.Features
			.SelectMany(static x => x.Sources)
			.Select(static x => x.HintName)
			.ToArray();

		CollectionAssert.AllItemsAreUnique(featureNames);
		CollectionAssert.AllItemsAreUnique(propertyNames);
		CollectionAssert.AllItemsAreUnique(hintNames);
	}

	[TestMethod]
	public void FeatureDefinitionsAreDerivedFromRegistryTest()
	{
		var registryProperties = GeneratorRegistry.Features.Select(static x => x.PropertyName).ToArray();
		var optionProperties = GeneratorOptions.FeatureDefinitions.Select(static x => x.PropertyName).ToArray();

		CollectionAssert.AreEqual(registryProperties, optionProperties);
	}
};
