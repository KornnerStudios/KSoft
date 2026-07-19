namespace KSoft.SourceGeneration;

internal static class SourceGenerationConstants
{
	// Shared generic enum constraint emitted by multiple legacy T4 surfaces.
	public const string EnumConstraint = "struct, Enum, IComparable, IFormattable, IConvertible";
}
