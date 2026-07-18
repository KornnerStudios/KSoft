using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

#pragma warning disable CA1861 // Avoid constant arrays as arguments

[TestClass]
public sealed class GeneratorOptionsTests
{
	[TestMethod]
	public void FeatureDefinitionsMatchExpectedPropertyNamesTest()
	{
		CollectionAssert.AreEqual(
			new[] {
				"KSoftGenerateBitsBitCount",
				"KSoftGenerateBitsRotate",
				"KSoftGenerateIntegerMath",
				"KSoftGenerateSourceGenerationSmokeTest",
			},
			GeneratorOptions.FeatureDefinitions.Select(static x => x.PropertyName).ToArray());
	}

	[TestMethod]
	public void MissingPropertiesDefaultToDisabledTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>()));

		foreach (GeneratorOptionDefinition definition in GeneratorOptions.FeatureDefinitions)
		{
			Assert.IsFalse(options.IsEnabled(definition.Feature));
		}

		Assert.IsFalse(options.HasInvalidBooleanProperties);
	}

	[TestMethod]
	public void CompilerVisibleBooleanPropertiesAreParsedTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsRotate)] = "True",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.IntegerMath)] = "false",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.SourceGenerationSmokeTest)] = "true",
		}));

		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsBitCount));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsRotate));
		Assert.IsFalse(options.IsEnabled(GeneratorFeature.IntegerMath));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.SourceGenerationSmokeTest));
		Assert.IsFalse(options.HasInvalidBooleanProperties);
	}

	[TestMethod]
	public void InvalidBooleanPropertiesAreRecordedTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "yes",
		}));

		Assert.IsFalse(options.IsEnabled(GeneratorFeature.BitsBitCount));
		Assert.IsTrue(options.HasInvalidBooleanProperties);
		Assert.AreEqual(
			GeneratorOptions.PropertyNameFor(GeneratorFeature.BitsBitCount),
			options.InvalidBooleanProperties[0]);
	}
};
