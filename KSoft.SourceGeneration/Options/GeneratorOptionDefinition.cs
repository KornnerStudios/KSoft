using System;

namespace KSoft.SourceGeneration.Options;

internal readonly struct GeneratorOptionDefinition : IEquatable<GeneratorOptionDefinition>
{
	public GeneratorOptionDefinition(GeneratorFeature feature, string propertyName)
	{
		ExceptionHelpers.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));

		Feature = feature;
		PropertyName = propertyName;
	}

	public GeneratorFeature Feature { get; }

	public string PropertyName { get; }

	public override bool Equals(object obj)
		=> obj is GeneratorOptionDefinition other && Equals(other);

	public bool Equals(GeneratorOptionDefinition other)
	{
		return Feature == other.Feature
			&& PropertyName == other.PropertyName;
	}

	public override int GetHashCode()
		=> HashCodeBuilder.Combine(Feature, PropertyName);
};
