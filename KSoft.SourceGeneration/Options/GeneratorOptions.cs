#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KSoft.SourceGeneration.Options;

internal sealed class GeneratorOptions : IEquatable<GeneratorOptions>
{
	private const string BuildPropertyPrefix = "build_property.";
	private const string UseSourceGenerationPropertyName = "KSoftUseSourceGeneration";

	private readonly bool mUseSourceGeneration;
	private readonly string[] mInvalidBooleanProperties;

	private GeneratorOptions(
		bool useSourceGeneration,
		string[] invalidBooleanProperties)
	{
		mUseSourceGeneration = useSourceGeneration;
		mInvalidBooleanProperties = invalidBooleanProperties;
	}

	/// <summary>
	/// Gets the MSBuild property exposed through <c>CompilerVisibleProperty</c>.
	/// </summary>
	public static string UseSourceGenerationProperty => UseSourceGenerationPropertyName;

	public static string UseSourceGenerationBuildProperty => BuildPropertyPrefix + UseSourceGenerationPropertyName;

	public IReadOnlyList<string> InvalidBooleanProperties => mInvalidBooleanProperties;

	public bool HasInvalidBooleanProperties => InvalidBooleanProperties.Count != 0;

	public bool UseSourceGeneration => mUseSourceGeneration;

	public static GeneratorOptions From(AnalyzerConfigOptions globalOptions)
	{
		ExceptionHelpers.ThrowIfNull(globalOptions, nameof(globalOptions));

		var invalidBooleanProperties = new List<string>();
		bool useSourceGeneration = ReadBooleanProperty(globalOptions, invalidBooleanProperties);

		return new GeneratorOptions(useSourceGeneration, invalidBooleanProperties.ToArray());
	}

	public bool Equals(GeneratorOptions? other)
		=> other is not null
			&& mUseSourceGeneration == other.mUseSourceGeneration
			&& mInvalidBooleanProperties.SequenceEqual(other.mInvalidBooleanProperties);

	public override bool Equals(object? obj)
		=> Equals(obj as GeneratorOptions);

	public override int GetHashCode()
	{
		int hashCode = 17;
		hashCode = HashCodeBuilder.Add(hashCode, mUseSourceGeneration);

		foreach (string invalidBooleanProperty in mInvalidBooleanProperties)
		{
			hashCode = HashCodeBuilder.AddOrdinalString(hashCode, invalidBooleanProperty);
		}

		return hashCode;
	}

	private static bool ReadBooleanProperty(
		AnalyzerConfigOptions options,
		List<string> invalidBooleanProperties)
	{
		if (!options.TryGetValue(UseSourceGenerationBuildProperty, out string? value) || string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		if (bool.TryParse(value, out bool result))
		{
			return result;
		}

		invalidBooleanProperties.Add(UseSourceGenerationPropertyName);
		return false;
	}
};
