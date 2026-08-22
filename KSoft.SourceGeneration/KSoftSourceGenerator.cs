using System;
using System.Text;
using KSoft.SourceGeneration.Diagnostics;
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
		var input = context.CompilationProvider
			.Select(static (compilation, _) => compilation.AssemblyName);

		var generationInput = input.Select(static (assemblyName, _) => new GenerationInput(assemblyName));

		context.RegisterSourceOutput(generationInput, static (sourceContext, generationInput) =>
		{
			if (generationInput.TargetAssembly == GeneratorTargetAssembly.Unsupported)
			{
				DiagnosticReporter.ReportUnsupportedTargetAssembly(sourceContext, generationInput);
				return;
			}

			foreach (GeneratorFeatureRegistration feature in GeneratorRegistry.FeaturesForTarget(
				generationInput.TargetAssembly))
			{
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
