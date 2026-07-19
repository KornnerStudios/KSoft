using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Bitwise;
using KSoft.SourceGeneration.Collections;
using KSoft.SourceGeneration.IO;
using KSoft.SourceGeneration.Math;
using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

/// <summary>
/// Central registry for feature flags, MSBuild property names, and generated source outputs.
/// </summary>
/// <remarks>
/// Keep new generator domains here instead of adding parallel option lists or <c>KSoftSourceGenerator</c> branches.
/// Registry tests enforce feature coverage and uniqueness as this list grows.
/// </remarks>
internal static class GeneratorRegistry
{
	private static readonly IReadOnlyList<GeneratorFeatureRegistration> kFeatures =
		[
			// Bitwise domains.
			new(
				GeneratorFeature.BitsBitCount,
				"KSoftGenerateBitsBitCount",
				new GeneratedSourceRegistration(BitsBitCountSourceBuilder.HintName, BitsBitCountSourceBuilder.Build)),
			new(
				GeneratorFeature.BitsCore,
				"KSoftGenerateBitsCore",
				new GeneratedSourceRegistration(
					BitsCoreSourceBuilder.BitReverseHintName,
					BitsCoreSourceBuilder.BuildBitReverse),
				new GeneratedSourceRegistration(
					BitsCoreSourceBuilder.BitSwapHintName,
					BitsCoreSourceBuilder.BuildBitSwap),
				new GeneratedSourceRegistration(
					BitsCoreSourceBuilder.ConstantsHintName,
					BitsCoreSourceBuilder.BuildConstants),
				new GeneratedSourceRegistration(
					BitsCoreSourceBuilder.CoreHintName,
					BitsCoreSourceBuilder.BuildCore),
				new GeneratedSourceRegistration(
					BitsCoreSourceBuilder.VectorsHintName,
					BitsCoreSourceBuilder.BuildVectors)),
			new(
				GeneratorFeature.BitsEncoding,
				"KSoftGenerateBitsEncoding",
				new GeneratedSourceRegistration(
					BitsEncodingSourceBuilder.DecodeHintName,
					BitsEncodingSourceBuilder.BuildDecode),
				new GeneratedSourceRegistration(
					BitsEncodingSourceBuilder.EncodeHintName,
					BitsEncodingSourceBuilder.BuildEncode),
				new GeneratedSourceRegistration(
					BitsEncodingSourceBuilder.NoneableEncodingHintName,
					BitsEncodingSourceBuilder.BuildNoneableEncoding)),
			new(
				GeneratorFeature.BitsRotate,
				"KSoftGenerateBitsRotate",
				new GeneratedSourceRegistration(BitsRotateSourceBuilder.HintName, BitsRotateSourceBuilder.Build)),
			new(
				GeneratorFeature.ByteSwap,
				"KSoftGenerateByteSwap",
				new GeneratedSourceRegistration(ByteSwapSourceBuilder.HintName, ByteSwapSourceBuilder.Build)),
			new(
				GeneratorFeature.Flags,
				"KSoftGenerateFlags",
				new GeneratedSourceRegistration(FlagsSourceBuilder.HintName, FlagsSourceBuilder.Build)),

			// Collections domains.
			new(
				GeneratorFeature.BitVectors,
				"KSoftGenerateBitVectors",
				new GeneratedSourceRegistration(BitVectorsSourceBuilder.HintName, BitVectorsSourceBuilder.Build)),

			// IO domains.
			new(
				GeneratorFeature.BitStream,
				"KSoftGenerateBitStream",
				new GeneratedSourceRegistration(BitStreamSourceBuilder.HintName, BitStreamSourceBuilder.Build)),
			new(
				GeneratorFeature.EndianStreamsNumbers,
				"KSoftGenerateEndianStreamsNumbers",
				new GeneratedSourceRegistration(
					EndianStreamsNumbersSourceBuilder.HintName,
					EndianStreamsNumbersSourceBuilder.Build)),
			new(
				GeneratorFeature.TagElementStreams,
				"KSoftGenerateTagElementStreams",
				new GeneratedSourceRegistration(
					TagElementStreamsSourceBuilder.HintName,
					TagElementStreamsSourceBuilder.Build)),

			// Math domains.
			new(
				GeneratorFeature.IntegerMath,
				"KSoftGenerateIntegerMath",
				new GeneratedSourceRegistration(IntegerMathSourceBuilder.HintName, IntegerMathSourceBuilder.Build)),
		];

	// GeneratorOptions consumes option definitions from the registry so there is only one feature/property list.
	private static readonly IReadOnlyList<GeneratorOptionDefinition> kFeatureDefinitions =
		kFeatures.Select(static x => x.ToOptionDefinition()).ToArray();

	public static IReadOnlyList<GeneratorFeatureRegistration> Features => kFeatures;

	public static IReadOnlyList<GeneratorOptionDefinition> FeatureDefinitions => kFeatureDefinitions;
};
