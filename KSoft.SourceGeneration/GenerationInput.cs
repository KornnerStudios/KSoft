#nullable enable

using System;

namespace KSoft.SourceGeneration;

internal readonly struct GenerationInput : IEquatable<GenerationInput>
{
	public GenerationInput(string? assemblyName)
	{
		AssemblyName = assemblyName ?? string.Empty;
		TargetAssembly = GeneratorTargetAssemblyFacts.FromAssemblyName(AssemblyName);
	}

	public string AssemblyName { get; }

	public GeneratorTargetAssembly TargetAssembly { get; }

	public bool Equals(GenerationInput other)
		=> string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal)
			&& TargetAssembly == other.TargetAssembly;

	public override bool Equals(object? obj)
		=> obj is GenerationInput other && Equals(other);

	public override int GetHashCode()
	{
		int hashCode = 17;
		hashCode = HashCodeBuilder.AddOrdinalString(hashCode, AssemblyName);
		hashCode = HashCodeBuilder.Add(hashCode, TargetAssembly);
		return hashCode;
	}
};
