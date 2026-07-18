using Microsoft.CodeAnalysis;

namespace KSoft.SourceGeneration.Diagnostics;

internal static class DiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor InvalidBooleanProperty = new(
		"KSG0001",
		"Invalid source-generation MSBuild property",
		"MSBuild property '{0}' must be 'true' or 'false'",
		"Configuration",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);
};
