using System;
using System.Collections.Generic;
using KSoft.SourceGeneration.Options;

namespace KSoft.SourceGeneration;

/// <summary>
/// Registers one source-generation domain and the generated files it owns.
/// </summary>
/// <remarks>
/// The MSBuild property controls the whole domain; individual source outputs let a domain builder mirror old T4 file
/// boundaries when that improves reviewability.
/// </remarks>
internal readonly struct GeneratorFeatureRegistration
{
	public GeneratorFeatureRegistration(
		GeneratorFeature feature,
		string propertyName,
		params GeneratedSourceRegistration[] sources)
	{
		ExceptionHelpers.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));
		if (sources == null)
		{
			throw new ArgumentNullException(nameof(sources));
		}
		if (sources.Length == 0)
		{
			throw new ArgumentException("At least one generated source is required.", nameof(sources));
		}

		Feature = feature;
		PropertyName = propertyName;
		Sources = sources;
	}

	public GeneratorFeature Feature { get; }

	/// <summary>
	/// Gets the MSBuild property exposed through <c>CompilerVisibleProperty</c>.
	/// </summary>
	public string PropertyName { get; }

	/// <summary>
	/// Gets all Roslyn source files emitted when <see cref="Feature" /> is enabled.
	/// </summary>
	public IReadOnlyList<GeneratedSourceRegistration> Sources { get; }

	public GeneratorOptionDefinition ToOptionDefinition()
		=> new(Feature, PropertyName);
};
