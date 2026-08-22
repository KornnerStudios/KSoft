using Microsoft.CodeAnalysis;

namespace KSoft.SourceGeneration.Diagnostics;

internal static class DiagnosticReporter
{
	public static void ReportUnsupportedTargetAssembly(SourceProductionContext context, GenerationInput input)
	{
		context.ReportDiagnostic(Diagnostic.Create(
			DiagnosticDescriptors.UnsupportedTargetAssembly,
			Location.None,
			input.AssemblyName,
			GeneratorTargetAssemblyFacts.ExpectedAssemblyNames));
	}
};
