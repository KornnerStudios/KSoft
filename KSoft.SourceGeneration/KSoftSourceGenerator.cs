using System;
using System.Text;
using KSoft.SourceGeneration.Diagnostics;
using KSoft.SourceGeneration.Options;
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

			// The registry lets one feature emit multiple .g.cs files without adding more incremental pipeline branches.
			foreach (GeneratorFeatureRegistration feature in GeneratorRegistry.Features)
			{
				if (!generationInput.Options.IsEnabled(feature.Feature))
				{
					continue;
				}

				foreach (GeneratedSourceRegistration source in feature.Sources)
				{
					AddSource(sourceContext, source);
				}
			}
		});
	}

	private static void AddSource(SourceProductionContext context, GeneratedSourceRegistration source)
	{
		context.AddSource(source.HintName, SourceText.From(source.BuildSource(), Encoding.UTF8));
	}
};
