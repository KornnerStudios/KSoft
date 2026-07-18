#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Test.KSoft.SourceGeneration;

internal sealed class AnalyzerConfigOptionsStub : AnalyzerConfigOptions
{
	private readonly Dictionary<string, string> mValues;

	public AnalyzerConfigOptionsStub(Dictionary<string, string> values)
	{
		ArgumentNullException.ThrowIfNull(values);

		mValues = values;
	}

	public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
		=> mValues.TryGetValue(key, out value);
};
