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
				"KSoftGenerateBitsCore",
				"KSoftGenerateBitsEncoding",
				"KSoftGenerateBitsRotate",
				"KSoftGenerateByteSwap",
				"KSoftGenerateFlags",
				"KSoftGenerateBitVectors",
				"KSoftGenerateBitStream",
				"KSoftGenerateEndianStreamsNumbers",
				"KSoftGenerateTagElementStreams",
				"KSoftGenerateIntegerMath",
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
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsCore)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsEncoding)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsRotate)] = "True",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.ByteSwap)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.Flags)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitVectors)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitStream)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.EndianStreamsNumbers)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.TagElementStreams)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.IntegerMath)] = "false",
		}));

		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsBitCount));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsCore));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsEncoding));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitsRotate));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.ByteSwap));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.Flags));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitVectors));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.BitStream));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.EndianStreamsNumbers));
		Assert.IsTrue(options.IsEnabled(GeneratorFeature.TagElementStreams));
		Assert.IsFalse(options.IsEnabled(GeneratorFeature.IntegerMath));
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

	[TestMethod]
	public void EquivalentFeatureSetsAreEqualTest()
	{
		var left = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsRotate)] = "false",
		}));
		var right = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "true",
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsRotate)] = "false",
		}));

		Assert.AreEqual(left, right);
		Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
	}

	[TestMethod]
	public void DifferentFeatureSetsAreNotEqualTest()
	{
		var left = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "true",
		}));
		var right = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsRotate)] = "true",
		}));

		Assert.AreNotEqual(left, right);
	}
};
