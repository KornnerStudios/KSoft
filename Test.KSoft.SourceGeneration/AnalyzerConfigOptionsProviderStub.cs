using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Test.KSoft.SourceGeneration;

internal sealed class AnalyzerConfigOptionsProviderStub : AnalyzerConfigOptionsProvider
{
	public AnalyzerConfigOptionsProviderStub(AnalyzerConfigOptions globalOptions)
	{
		ArgumentNullException.ThrowIfNull(globalOptions);

		GlobalOptions = globalOptions;
	}

	public override AnalyzerConfigOptions GlobalOptions { get; }

	public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		=> GlobalOptions;

	public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		=> GlobalOptions;
};
