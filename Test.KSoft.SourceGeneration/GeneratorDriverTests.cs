using System.Linq;
using KSoft.SourceGeneration;
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

	public TestContext TestContext { get; set; }
};
