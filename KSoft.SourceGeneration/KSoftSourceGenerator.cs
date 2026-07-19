using System;
using System.Text;
using KSoft.SourceGeneration.Bitwise;
using KSoft.SourceGeneration.Collections;
using KSoft.SourceGeneration.Diagnostics;
using KSoft.SourceGeneration.IO;
using KSoft.SourceGeneration.Math;
using KSoft.SourceGeneration.Options;
using KSoft.SourceGeneration.SmokeTests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace KSoft.SourceGeneration;

/// <summary>
/// Entry point for KSoft descriptor-driven Roslyn source generation.
/// </summary>
[Generator]
public sealed class KSoftSourceGenerator : IIncrementalGenerator
{
	/// <summary>
	/// Initializes the generator's incremental pipeline.
	/// </summary>
	/// <param name="context">Generator initialization context.</param>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var options = context.AnalyzerConfigOptionsProvider
			.Select(static (provider, _) => GeneratorOptions.From(provider.GlobalOptions));

		var input = options
			.Select(static (generatorOptions, _) => new GenerationInput(generatorOptions));

		context.RegisterSourceOutput(input, static (sourceContext, generationInput) =>
		{
			DiagnosticReporter.ReportInvalidOptions(sourceContext, generationInput.Options);
			if (generationInput.Options.HasInvalidBooleanProperties)
			{
				return;
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.SourceGenerationSmokeTest))
			{
				AddSource(sourceContext, SmokeSourceBuilder.HintName, SmokeSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.BitsBitCount))
			{
				AddSource(sourceContext, BitsBitCountSourceBuilder.HintName, BitsBitCountSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.BitsRotate))
			{
				AddSource(sourceContext, BitsRotateSourceBuilder.HintName, BitsRotateSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.BitVectors))
			{
				AddSource(sourceContext, BitVectorsSourceBuilder.HintName, BitVectorsSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.IntegerMath))
			{
				AddSource(sourceContext, IntegerMathSourceBuilder.HintName, IntegerMathSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.EndianStreamsNumbers))
			{
				AddSource(
					sourceContext,
					EndianStreamsNumbersSourceBuilder.HintName,
					EndianStreamsNumbersSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.BitStream))
			{
				AddSource(sourceContext, BitStreamSourceBuilder.HintName, BitStreamSourceBuilder.Build);
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.TagElementStreams))
			{
				AddSource(sourceContext, TagElementStreamsSourceBuilder.HintName, TagElementStreamsSourceBuilder.Build);
			}
		});
	}

	private static void AddSource(SourceProductionContext context, string hintName, Func<string> buildSource)
	{
		ExceptionHelpers.ThrowIfNullOrEmpty(hintName, nameof(hintName));
		ExceptionHelpers.ThrowIfNull(buildSource, nameof(buildSource));

		context.AddSource(hintName, SourceText.From(buildSource(), Encoding.UTF8));
	}
};
