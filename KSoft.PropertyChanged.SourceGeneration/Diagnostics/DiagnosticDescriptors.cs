using Microsoft.CodeAnalysis;

namespace KSoft.PropertyChanged.SourceGeneration.Diagnostics;

internal static class DiagnosticDescriptors
{
	private const string Category = "KSoft.PropertyChanged.SourceGeneration";

	public static readonly DiagnosticDescriptor NonPartialType = Create("KSPC0001", "Property-change host must be partial", "Type '{0}' must be partial to contain generated property-change properties");
	public static readonly DiagnosticDescriptor InvalidPartialProperty = Create("KSPC0002", "Invalid generated property-change target", "Property '{0}' must be a defining partial, non-static, non-indexer property with get and set accessors without bodies");
	public static readonly DiagnosticDescriptor InvalidBackingField = Create("KSPC0003", "Invalid property-change backing field", "Backing field '{0}' for property '{1}' must name one writable instance field of the same type");
	public static readonly DiagnosticDescriptor InvalidBasicViewModelHost = Create("KSPC0004", "Invalid property-change host", "Type '{0}' must directly derive from KSoft.ObjectModel.BasicViewModel");
	public static readonly DiagnosticDescriptor GeneratedMemberCollision = Create("KSPC0006", "Generated property-change member collision", "Property-change generation for type '{0}' collides with generated member or hint name '{1}'");
	public static readonly DiagnosticDescriptor UnsupportedTarget = Create("KSPC0007", "Unsupported generated property-change target", "Property '{0}' uses an unsupported language, host, accessor, or interface shape: {1}");

	private static DiagnosticDescriptor Create(string id, string title, string messageFormat) => new(
		id, title, messageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
