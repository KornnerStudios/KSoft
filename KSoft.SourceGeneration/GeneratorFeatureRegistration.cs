using System;
using System.Collections.Generic;
using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

/// <summary>
/// Registers one source-generation domain and the generated files it owns.
/// </summary>
/// <remarks>
/// The target assembly owns the domain; individual source outputs let a domain builder mirror old T4 file boundaries
/// when that improves reviewability.
/// </remarks>
internal readonly struct GeneratorFeatureRegistration
{
	public GeneratorFeatureRegistration(
		GeneratorTargetAssembly targetAssembly,
		GeneratorFeature feature,
		params GeneratedSourceRegistration[] sources)
	{
		if (targetAssembly == GeneratorTargetAssembly.Unsupported)
		{
			throw new ArgumentOutOfRangeException(nameof(targetAssembly), targetAssembly, "Target assembly is required.");
		}
		if (sources == null)
		{
			throw new ArgumentNullException(nameof(sources));
		}
		if (sources.Length == 0)
		{
			throw new ArgumentException("At least one generated source is required.", nameof(sources));
		}

		TargetAssembly = targetAssembly;
		Feature = feature;
		Sources = sources;
	}

	public GeneratorTargetAssembly TargetAssembly { get; }

	public GeneratorFeature Feature { get; }

	/// <summary>
	/// Gets all Roslyn source files emitted when <see cref="Feature" /> is enabled.
	/// </summary>
	public IReadOnlyList<GeneratedSourceRegistration> Sources { get; }
};
