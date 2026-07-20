using System;
using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Bitwise;
using KSoft.SourceGeneration.Collections;
using KSoft.SourceGeneration.Enum;
using KSoft.SourceGeneration.IO;
using KSoft.SourceGeneration.Math;
using KSoft.SourceGeneration.Options;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration;

/// <summary>
/// Central registry for target assemblies, feature domains, and generated source outputs.
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
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitsBitCount,
				new GeneratedSourceRegistration(BitsBitCountSourceBuilder.HintName, BitsBitCountSourceBuilder.Build)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitsCore,
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
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitsEncoding,
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
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitsRotate,
				new GeneratedSourceRegistration(BitsRotateSourceBuilder.HintName, BitsRotateSourceBuilder.Build)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.ByteSwap,
				new GeneratedSourceRegistration(ByteSwapSourceBuilder.HintName, ByteSwapSourceBuilder.Build)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.Flags,
				new GeneratedSourceRegistration(FlagsSourceBuilder.HintName, FlagsSourceBuilder.Build)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.HandleBitEncoder,
				new GeneratedSourceRegistration(
					HandleBitEncoderSourceBuilder.HintName,
					HandleBitEncoderSourceBuilder.Build)),

			// Collections domains.
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitSet,
				new GeneratedSourceRegistration(
					BitSetSourceBuilder.BitSetHintName,
					BitSetSourceBuilder.BuildBitSet),
				new GeneratedSourceRegistration(
					BitSetSourceBuilder.EnumeratorsHintName,
					BitSetSourceBuilder.BuildEnumerators)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitVectors,
				new GeneratedSourceRegistration(BitVectorsSourceBuilder.HintName, BitVectorsSourceBuilder.Build)),

			// Enum domains.
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.Enums,
				new GeneratedSourceRegistration(
					EnumSourceBuilder.EnumBitEncoderHintName,
					EnumSourceBuilder.BuildEnumBitEncoder),
				new GeneratedSourceRegistration(
					EnumSourceBuilder.EnumValueHintName,
					EnumSourceBuilder.BuildEnumValue)),

			// IO domains.
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.BitStream,
				new GeneratedSourceRegistration(BitStreamSourceBuilder.HintName, BitStreamSourceBuilder.Build),
				new GeneratedSourceRegistration(BitStreamSourceBuilder.CacheHintName, BitStreamSourceBuilder.BuildCache)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.EndianStreamsCore,
				new GeneratedSourceRegistration(
					EndianStreamsCoreSourceBuilder.BaseHintName,
					EndianStreamsCoreSourceBuilder.BuildBase),
				new GeneratedSourceRegistration(
					EndianStreamsCoreSourceBuilder.TypeExtensionsHintName,
					EndianStreamsCoreSourceBuilder.BuildTypeExtensions),
				new GeneratedSourceRegistration(
					EndianStreamsCoreSourceBuilder.VirtualAddressTranslationHintName,
					EndianStreamsCoreSourceBuilder.BuildVirtualAddressTranslation)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.EndianStreamsNumbers,
				new GeneratedSourceRegistration(
					EndianStreamsNumbersSourceBuilder.HintName,
					EndianStreamsNumbersSourceBuilder.Build)),
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.IOExceptions,
				new GeneratedSourceRegistration(
					IOExceptionsSourceBuilder.VersionMismatchHintName,
					IOExceptionsSourceBuilder.BuildVersionMismatch),
				new GeneratedSourceRegistration(
					IOExceptionsSourceBuilder.SignatureMismatchHintName,
					IOExceptionsSourceBuilder.BuildSignatureMismatch)),
			new(
				GeneratorTargetAssembly.KSoftIOTagElementStreams,
				GeneratorFeature.TagElementStreams,
				new GeneratedSourceRegistration(
					TagElementStreamsSourceBuilder.HintName,
					TagElementStreamsSourceBuilder.Build)),

			// Math domains.
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.IntegerMath,
				new GeneratedSourceRegistration(IntegerMathSourceBuilder.HintName, IntegerMathSourceBuilder.Build)),

			// Text domains.
			new(
				GeneratorTargetAssembly.KSoft,
				GeneratorFeature.TextNumbers,
				new GeneratedSourceRegistration(
					TextSourceBuilder.NumbersToStringHintName,
					TextSourceBuilder.BuildNumbersToString),
				new GeneratedSourceRegistration(
					TextSourceBuilder.NumbersParseHintName,
					TextSourceBuilder.BuildNumbersParse),
				new GeneratedSourceRegistration(
					TextSourceBuilder.CharLookupTablesHintName,
					TextSourceBuilder.BuildCharLookupTables)),
		];

	private static readonly IReadOnlyList<GeneratorFeatureRegistration> kKSoftFeatures =
		kFeatures.Where(static x => x.TargetAssembly == GeneratorTargetAssembly.KSoft).ToArray();
	private static readonly IReadOnlyList<GeneratorFeatureRegistration> kKSoftIOTagElementStreamsFeatures =
		kFeatures.Where(static x => x.TargetAssembly == GeneratorTargetAssembly.KSoftIOTagElementStreams).ToArray();

	public static IReadOnlyList<GeneratorFeatureRegistration> Features => kFeatures;

	public static IReadOnlyList<GeneratorFeatureRegistration> FeaturesForTarget(GeneratorTargetAssembly targetAssembly)
	{
		return targetAssembly switch
		{
			GeneratorTargetAssembly.KSoft => kKSoftFeatures,
			GeneratorTargetAssembly.KSoftIOTagElementStreams => kKSoftIOTagElementStreamsFeatures,
			_ => Array.Empty<GeneratorFeatureRegistration>(),
		};
	}
};
