using System;
using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration;
using KSoft.SourceGeneration.Bitwise;
using KSoft.SourceGeneration.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class GeneratorDriverTests
{
	[TestMethod]
	public void GeneratorDoesNotEmitProductionSourcesByDefaultTest()
	{
		CSharpCompilation compilation = CSharpCompilation.Create(
			"GeneratorSmoke",
			[CSharpSyntaxTree.ParseText("internal static class Input { }", cancellationToken: TestContext.CancellationToken)],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
		var driver = CSharpGeneratorDriver.Create(new KSoftSourceGenerator());

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out var diagnostics,
			TestContext.CancellationToken);

		Assert.IsEmpty(diagnostics);
		Assert.AreEqual(1, outputCompilation.SyntaxTrees.Count());
	}

	[TestMethod]
	public void GeneratorEmitsBitCountWhenFeatureIsEnabledTest()
	{
		CSharpCompilation compilation = CSharpCompilation.Create(
			"GeneratorSmoke",
			[CSharpSyntaxTree.ParseText("internal static class Input { }", cancellationToken: TestContext.CancellationToken)],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
		var optionsProvider = new AnalyzerConfigOptionsProviderStub(new AnalyzerConfigOptionsStub(
			new Dictionary<string, string>
			{
				[GeneratorOptions.BuildPropertyNameFor(GeneratorFeature.BitsBitCount)] = "true",
			}));
		var driver = CSharpGeneratorDriver.Create(
			[new KSoftSourceGenerator().AsSourceGenerator()],
			optionsProvider: optionsProvider);

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out var diagnostics,
			TestContext.CancellationToken);

		Assert.IsEmpty(diagnostics);
		Assert.IsTrue(outputCompilation.SyntaxTrees.Any(
			x => x.FilePath.EndsWith(BitsBitCountSourceBuilder.HintName, StringComparison.Ordinal)));
	}

	public TestContext TestContext { get; set; }
};
