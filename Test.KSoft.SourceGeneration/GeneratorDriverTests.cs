using System;
using System.Linq;
using KSoft.SourceGeneration;
using KSoft.SourceGeneration.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class GeneratorDriverTests
{
	[TestMethod]
	public void GeneratorReportsUnsupportedTargetAssemblyTest()
	{
		CSharpCompilation compilation = CreateCompilation("GeneratorSmoke");
		var driver = CSharpGeneratorDriver.Create(new KSoftSourceGenerator());

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out var diagnostics,
			TestContext.CancellationToken);

		Assert.AreEqual(1, diagnostics.Length);
		Assert.AreEqual(DiagnosticDescriptors.UnsupportedTargetAssembly.Id, diagnostics[0].Id);
		Assert.AreEqual(1, outputCompilation.SyntaxTrees.Count());
	}

	[TestMethod]
	public void GeneratorEmitsKSoftTargetSourcesTest()
	{
		AssertGeneratorEmitsTargetSources(
			GeneratorTargetAssembly.KSoft,
			GeneratorTargetAssemblyFacts.KSoftAssemblyName);
	}

	[TestMethod]
	public void GeneratorEmitsTagElementStreamsTargetSourcesTest()
	{
		AssertGeneratorEmitsTargetSources(
			GeneratorTargetAssembly.KSoftIOTagElementStreams,
			GeneratorTargetAssemblyFacts.KSoftIOTagElementStreamsAssemblyName);
	}

	[TestMethod]
	public void GeneratorReportsUnsupportedTargetAssemblyWithDriverTest()
	{
		CSharpCompilation compilation = CreateCompilation("Unexpected.Assembly");
		var driver = CreateDriver();

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out var diagnostics,
			TestContext.CancellationToken);

		Assert.AreEqual(1, diagnostics.Length);
		Assert.AreEqual(DiagnosticDescriptors.UnsupportedTargetAssembly.Id, diagnostics[0].Id);
		Assert.AreEqual(1, outputCompilation.SyntaxTrees.Count());
	}

	public TestContext TestContext { get; set; }

	private static CSharpCompilation CreateCompilation(string assemblyName)
	{
		return CSharpCompilation.Create(
			assemblyName,
			[CSharpSyntaxTree.ParseText("internal static class Input { }")],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
	}

	private static GeneratorDriver CreateDriver() =>
		CSharpGeneratorDriver.Create([new KSoftSourceGenerator().AsSourceGenerator()]);

	private void AssertGeneratorEmitsTargetSources(GeneratorTargetAssembly targetAssembly, string assemblyName)
	{
		CSharpCompilation compilation = CreateCompilation(assemblyName);
		var driver = CreateDriver();

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out var diagnostics,
			TestContext.CancellationToken);

		Assert.IsEmpty(diagnostics);
		var outputPaths = outputCompilation.SyntaxTrees
			.Select(static x => x.FilePath)
			.ToArray();
		var targetSources = GeneratorRegistry.FeaturesForTarget(targetAssembly)
			.SelectMany(static x => x.Sources)
			.ToArray();
		var otherSources = GeneratorRegistry.Features
			.Where(x => x.TargetAssembly != targetAssembly)
			.SelectMany(static x => x.Sources)
			.ToArray();

		foreach (GeneratedSourceRegistration source in targetSources)
		{
			Assert.IsTrue(outputPaths.Any(x => x.EndsWith(source.HintName, StringComparison.Ordinal)));
		}

		foreach (GeneratedSourceRegistration source in otherSources)
		{
			Assert.IsFalse(outputPaths.Any(x => x.EndsWith(source.HintName, StringComparison.Ordinal)));
		}
	}
};
