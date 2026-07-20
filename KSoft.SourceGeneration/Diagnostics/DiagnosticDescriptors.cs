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

	public static readonly DiagnosticDescriptor UnsupportedTargetAssembly = new(
		"KSG0002",
		"Unsupported source-generation target assembly",
		"KSoft source generation is enabled for unsupported target assembly '{0}'; expected one of: {1}",
		"Configuration",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);
};
