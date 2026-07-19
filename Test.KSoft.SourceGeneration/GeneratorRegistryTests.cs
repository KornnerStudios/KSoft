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

	[TestMethod]
	public void BitsEncodingRegistersEveryEncodingOutputTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.BitsEncoding);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Bits.Decode.g.cs",
				"KSoft.Bits.Encode.g.cs",
				"KSoft.Bits.NoneableEncoding.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void BitsCoreRegistersEveryCoreOutputTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.BitsCore);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Bits.BitReverse.g.cs",
				"KSoft.Bits.BitSwap.g.cs",
				"KSoft.Bits.Constants.g.cs",
				"KSoft.Bits.Core.g.cs",
				"KSoft.Bits.Vectors.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void ByteSwapRegistersExpectedOutputTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.ByteSwap);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Bitwise.ByteSwap.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void FlagsRegistersExpectedOutputTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.Flags);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Bitwise.Flags.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void HandleBitEncoderRegistersExpectedOutputTest()
	{
		var registration = GeneratorRegistry.Features.Single(
			static x => x.Feature == GeneratorFeature.HandleBitEncoder);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Bitwise.HandleBitEncoder.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void BitSetRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.BitSet);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Collections.BitSet.g.cs",
				"KSoft.Collections.IReadOnlyBitSet.Enumerators.g.cs",
			},
			hints);
	}
};
