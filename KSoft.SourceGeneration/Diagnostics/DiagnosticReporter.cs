using KSoft.SourceGeneration.Options;
using Microsoft.CodeAnalysis;

namespace KSoft.SourceGeneration.Diagnostics;

internal static class DiagnosticReporter
{
	public static void ReportInvalidOptions(SourceProductionContext context, GeneratorOptions options)
	{
		foreach (string property in options.InvalidBooleanProperties)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.InvalidBooleanProperty,
				Location.None,
				property));
		}
	}

	public static void ReportUnsupportedTargetAssembly(SourceProductionContext context, GenerationInput input)
	{
		context.ReportDiagnostic(Diagnostic.Create(
			DiagnosticDescriptors.UnsupportedTargetAssembly,
			Location.None,
			input.AssemblyName,
			GeneratorTargetAssemblyFacts.ExpectedAssemblyNames));
	}
};
