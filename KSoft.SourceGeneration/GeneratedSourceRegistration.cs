using System;

namespace KSoft.SourceGeneration;

/// <summary>
/// Describes one Roslyn <c>AddSource</c> output produced by a generator feature.
/// </summary>
/// <remarks>
/// A feature can register multiple outputs so future domains can split generated files without adding more MSBuild
/// feature flags or source-generator routing code.
/// </remarks>
internal readonly struct GeneratedSourceRegistration
{
	public GeneratedSourceRegistration(string hintName, Func<string> buildSource)
	{
		ExceptionHelpers.ThrowIfNullOrEmpty(hintName, nameof(hintName));
		ExceptionHelpers.ThrowIfNull(buildSource, nameof(buildSource));

		HintName = hintName;
		BuildSource = buildSource;
	}

	public string HintName { get; }

	/// <summary>
	/// Builds the source text lazily after the owning feature has been enabled.
	/// </summary>
	public Func<string> BuildSource { get; }
};
