namespace KSoft.SourceGeneration.Text;

internal enum SourceWriterBlockType
{
	NoBraces,
	Braces,
	/// <summary>Mainly for type definition statements that keep Vita's trailing semicolon style.</summary>
	BracesStatement,
};
