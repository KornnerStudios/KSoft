#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KSoft.SourceGeneration.Options;

internal sealed class GeneratorOptions
{
	private const string BuildPropertyPrefix = "build_property.";

	private static readonly IReadOnlyList<GeneratorOptionDefinition> kFeatureDefinitions =
		[
			new(GeneratorFeature.BitsBitCount, "KSoftGenerateBitsBitCount"),
			new(GeneratorFeature.BitsRotate, "KSoftGenerateBitsRotate"),
			new(GeneratorFeature.IntegerMath, "KSoftGenerateIntegerMath"),
			new(GeneratorFeature.SourceGenerationSmokeTest, "KSoftGenerateSourceGenerationSmokeTest"),
		];

	private readonly HashSet<GeneratorFeature> mEnabledFeatures;

	private GeneratorOptions(
		HashSet<GeneratorFeature> enabledFeatures,
		IReadOnlyList<string> invalidBooleanProperties)
	{
		mEnabledFeatures = enabledFeatures;
		InvalidBooleanProperties = invalidBooleanProperties;
	}

	public static IReadOnlyList<GeneratorOptionDefinition> FeatureDefinitions => kFeatureDefinitions;

	public IReadOnlyList<string> InvalidBooleanProperties { get; }

	public bool HasInvalidBooleanProperties => InvalidBooleanProperties.Count != 0;

	public bool IsEnabled(GeneratorFeature feature)
		=> mEnabledFeatures.Contains(feature);

	public static string BuildPropertyNameFor(GeneratorFeature feature)
		=> BuildPropertyPrefix + PropertyNameFor(feature);

	public static GeneratorOptions From(AnalyzerConfigOptions globalOptions)
	{
		ExceptionHelpers.ThrowIfNull(globalOptions, nameof(globalOptions));

		var enabledFeatures = new HashSet<GeneratorFeature>();
		var invalidBooleanProperties = new List<string>();
		foreach (GeneratorOptionDefinition definition in kFeatureDefinitions)
		{
			ReadBooleanProperty(globalOptions, definition, enabledFeatures, invalidBooleanProperties);
		}

		return new GeneratorOptions(enabledFeatures, invalidBooleanProperties.AsReadOnly());
	}

	public static string PropertyNameFor(GeneratorFeature feature)
	{
		foreach (GeneratorOptionDefinition definition in kFeatureDefinitions)
		{
			if (definition.Feature == feature)
			{
				return definition.PropertyName;
			}
		}

		throw new ArgumentOutOfRangeException(nameof(feature), feature, "Unknown generator feature.");
	}

	private static void ReadBooleanProperty(
		AnalyzerConfigOptions options,
		GeneratorOptionDefinition definition,
		HashSet<GeneratorFeature> enabledFeatures,
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
