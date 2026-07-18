using System.Text;
using KSoft.SourceGeneration.Bitwise;
using KSoft.SourceGeneration.Diagnostics;
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
				sourceContext.AddSource(
					SmokeSourceBuilder.HintName,
					SourceText.From(SmokeSourceBuilder.Build(), Encoding.UTF8));
			}

			if (generationInput.Options.IsEnabled(GeneratorFeature.BitsBitCount))
			{
				sourceContext.AddSource(
					BitsBitCountSourceBuilder.HintName,
					SourceText.From(BitsBitCountSourceBuilder.Build(), Encoding.UTF8));
			}
		});
	}
};
