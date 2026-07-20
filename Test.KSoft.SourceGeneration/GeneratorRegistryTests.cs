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
	public void RegistryUsesUniqueFeatureAndHintNamesTest()
	{
		var featureNames = GeneratorRegistry.Features.Select(static x => x.Feature.ToString()).ToArray();
		var hintNames = GeneratorRegistry.Features
			.SelectMany(static x => x.Sources)
			.Select(static x => x.HintName)
			.ToArray();

		CollectionAssert.AllItemsAreUnique(featureNames);
		CollectionAssert.AllItemsAreUnique(hintNames);
	}

	[TestMethod]
	public void KSoftTargetOwnsBclFeatureGroupTest()
	{
		var features = GeneratorRegistry.FeaturesForTarget(GeneratorTargetAssembly.KSoft)
			.Select(static x => x.Feature)
			.ToArray();

		CollectionAssert.AreEqual(
			new[] {
				GeneratorFeature.BitsBitCount,
				GeneratorFeature.BitsCore,
				GeneratorFeature.BitsEncoding,
				GeneratorFeature.BitsRotate,
				GeneratorFeature.ByteSwap,
				GeneratorFeature.Flags,
				GeneratorFeature.HandleBitEncoder,
				GeneratorFeature.BitSet,
				GeneratorFeature.BitVectors,
				GeneratorFeature.Enums,
				GeneratorFeature.BitStream,
				GeneratorFeature.EndianStreamsCore,
				GeneratorFeature.EndianStreamsNumbers,
				GeneratorFeature.IOExceptions,
				GeneratorFeature.IntegerMath,
				GeneratorFeature.TextNumbers,
			},
			features);
	}

	[TestMethod]
	public void TagElementStreamsTargetOwnsOnlyTagElementFeatureTest()
	{
		var features = GeneratorRegistry.FeaturesForTarget(GeneratorTargetAssembly.KSoftIOTagElementStreams)
			.Select(static x => x.Feature)
			.ToArray();

		CollectionAssert.AreEqual(
			new[] {
				GeneratorFeature.TagElementStreams,
			},
			features);
	}

	[TestMethod]
	public void UnsupportedTargetOwnsNoFeaturesTest()
	{
		Assert.IsEmpty(GeneratorRegistry.FeaturesForTarget(GeneratorTargetAssembly.Unsupported));
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

	[TestMethod]
	public void EnumsRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.Enums);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.EnumBitEncoder.g.cs",
				"KSoft.Reflection.EnumValue.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void IOExceptionsRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.IOExceptions);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.IO.VersionMismatchException.g.cs",
				"KSoft.IO.SignatureMismatchException.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void EndianStreamsCoreRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.EndianStreamsCore);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.IO.EndianStreams.Base.g.cs",
				"KSoft.TypeExtensions.EndianStreams.g.cs",
				"KSoft.IO.EndianStreams.VirtualAddressTranslation.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void BitStreamRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.BitStream);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.IO.BitStream.g.cs",
				"KSoft.IO.BitStream.Cache.g.cs",
			},
			hints);
	}

	[TestMethod]
	public void TextNumbersRegistersExpectedOutputsTest()
	{
		var registration = GeneratorRegistry.Features.Single(static x => x.Feature == GeneratorFeature.TextNumbers);
		var hints = registration.Sources.Select(static x => x.HintName).ToArray();

		CollectionAssert.AreEqual(
			new[] {
				"KSoft.Numbers.ToString.g.cs",
				"KSoft.Numbers.Parse.g.cs",
				"KSoft.Text.CharLookupTables.g.cs",
			},
			hints);
	}
};
