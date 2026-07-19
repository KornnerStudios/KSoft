#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KSoft.SourceGeneration.Options;

internal sealed class GeneratorOptions : IEquatable<GeneratorOptions>
{
	private const string BuildPropertyPrefix = "build_property.";

	private readonly GeneratorFeature[] mEnabledFeatures;
	private readonly string[] mInvalidBooleanProperties;

	private GeneratorOptions(
		GeneratorFeature[] enabledFeatures,
		string[] invalidBooleanProperties)
	{
		mEnabledFeatures = enabledFeatures;
		mInvalidBooleanProperties = invalidBooleanProperties;
	}

	/// <summary>
	/// Gets the registry-backed option definitions parsed from analyzer config.
	/// </summary>
	public static IReadOnlyList<GeneratorOptionDefinition> FeatureDefinitions => GeneratorRegistry.FeatureDefinitions;

	public IReadOnlyList<string> InvalidBooleanProperties => mInvalidBooleanProperties;

	public bool HasInvalidBooleanProperties => InvalidBooleanProperties.Count != 0;

	public bool IsEnabled(GeneratorFeature feature)
		=> Array.IndexOf(mEnabledFeatures, feature) >= 0;

	public static string BuildPropertyNameFor(GeneratorFeature feature)
		=> BuildPropertyPrefix + PropertyNameFor(feature);

	public static GeneratorOptions From(AnalyzerConfigOptions globalOptions)
	{
		ExceptionHelpers.ThrowIfNull(globalOptions, nameof(globalOptions));

		// Keep option parsing data-driven so adding a generator feature only updates GeneratorRegistry.
		var enabledFeatures = new List<GeneratorFeature>();
		var invalidBooleanProperties = new List<string>();
		foreach (GeneratorOptionDefinition definition in FeatureDefinitions)
		{
			ReadBooleanProperty(globalOptions, definition, enabledFeatures, invalidBooleanProperties);
		}

		return new GeneratorOptions(enabledFeatures.ToArray(), invalidBooleanProperties.ToArray());
	}

	public static string PropertyNameFor(GeneratorFeature feature)
	{
		foreach (GeneratorOptionDefinition definition in FeatureDefinitions)
		{
			if (definition.Feature == feature)
			{
				return definition.PropertyName;
			}
		}

		throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown generator feature.");
	}

	public bool Equals(GeneratorOptions? other)
		=> other is not null
			&& mEnabledFeatures.SequenceEqual(other.mEnabledFeatures)
			&& mInvalidBooleanProperties.SequenceEqual(other.mInvalidBooleanProperties);

	public override bool Equals(object? obj)
		=> Equals(obj as GeneratorOptions);

	public override int GetHashCode()
	{
		int hashCode = 17;
		// Feature order is defined by GeneratorRegistry, keeping equality and incremental-generator cache keys stable.
		foreach (GeneratorFeature feature in mEnabledFeatures)
		{
			hashCode = HashCodeBuilder.Add(hashCode, feature);
		}

		foreach (string invalidBooleanProperty in mInvalidBooleanProperties)
		{
			hashCode = HashCodeBuilder.AddOrdinalString(hashCode, invalidBooleanProperty);
		}

		return hashCode;
	}

	private static void ReadBooleanProperty(
		AnalyzerConfigOptions options,
		GeneratorOptionDefinition definition,
		List<GeneratorFeature> enabledFeatures,
		List<string> invalidBooleanProperties)
	{
		string fullPropertyName = BuildPropertyPrefix + definition.PropertyName;
		if (!options.TryGetValue(fullPropertyName, out string? value) || string.IsNullOrWhiteSpace(value))
		{
			return;
		}

		if (bool.TryParse(value, out bool result))
		{
			if (result)
			{
				enabledFeatures.Add(definition.Feature);
			}

			return;
		}

		invalidBooleanProperties.Add(definition.PropertyName);
	}
};
