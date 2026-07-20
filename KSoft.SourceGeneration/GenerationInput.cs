#nullable enable

using System;
using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

internal readonly struct GenerationInput : IEquatable<GenerationInput>
{
	public GenerationInput(GeneratorOptions options, string? assemblyName)
	{
		ExceptionHelpers.ThrowIfNull(options, nameof(options));

		Options = options;
		AssemblyName = assemblyName ?? string.Empty;
		TargetAssembly = GeneratorTargetAssemblyFacts.FromAssemblyName(AssemblyName);
	}

	public GeneratorOptions Options { get; }

	public string AssemblyName { get; }

	public GeneratorTargetAssembly TargetAssembly { get; }

	public bool Equals(GenerationInput other)
		=> Options.Equals(other.Options)
			&& string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
			&& TargetAssembly == other.TargetAssembly;

	public override bool Equals(object? obj)
		=> obj is GenerationInput other && Equals(other);

	public override int GetHashCode()
	{
		int hashCode = 17;
		hashCode = HashCodeBuilder.Add(hashCode, Options.GetHashCode());
		hashCode = HashCodeBuilder.AddOrdinalString(hashCode, AssemblyName);
		hashCode = HashCodeBuilder.Add(hashCode, TargetAssembly);
		return hashCode;
	}
};
