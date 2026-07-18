using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

internal readonly struct GenerationInput
{
	public GenerationInput(GeneratorOptions options)
	{
		Options = options;
	}

	public GeneratorOptions Options { get; }
};
