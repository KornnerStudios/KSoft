#nullable enable

using System;
using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

internal readonly struct GenerationInput : IEquatable<GenerationInput>
{
	public GenerationInput(GeneratorOptions options)
	{
		ExceptionHelpers.ThrowIfNull(options, nameof(options));

		Options = options;
	}

	public GeneratorOptions Options { get; }

	public bool Equals(GenerationInput other)
		=> Options.Equals(other.Options);

	public override bool Equals(object? obj)
		=> obj is GenerationInput other && Equals(other);

	public override int GetHashCode()
		=> Options.GetHashCode();
};
