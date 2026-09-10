using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void GeneratorInjectsInternalCompileTimeContractTest()
	{
		TestRun run = Run("internal static class Input { }");
		string contract = GeneratedSource(run, "KSoft.PropertyChanged.Contracts.g.cs");
		StringAssert.Contains(contract, "namespace KSoft.PropertyChanged.SourceGeneration", StringComparison.Ordinal);
		Assert.IsFalse(contract.Contains("#nullable", StringComparison.Ordinal));

		INamedTypeSymbol generatedPropertyAttribute = run.OutputCompilation.GetTypeByMetadataName(
			"KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedAttribute")!;
		Assert.IsNotNull(generatedPropertyAttribute);
		Assert.AreEqual(Accessibility.Internal, generatedPropertyAttribute.DeclaredAccessibility);
		Assert.IsTrue(generatedPropertyAttribute.IsSealed);
		Assert.AreEqual(
			SpecialType.System_String,
			generatedPropertyAttribute.GetMembers("BackingField").OfType<IPropertySymbol>().Single().Type.SpecialType);
		Assert.AreEqual(
			SpecialType.System_Boolean,
			generatedPropertyAttribute.GetMembers("AlwaysNotify").OfType<IPropertySymbol>().Single().Type.SpecialType);
		string? propertyDocumentation = generatedPropertyAttribute.GetDocumentationCommentXml();
		Assert.IsNotNull(propertyDocumentation);
		StringAssert.Contains(propertyDocumentation, "Caliburn.Micro.PropertyChangedBase", StringComparison.Ordinal);
		StringAssert.Contains(propertyDocumentation, "virtual string-based notification pipeline", StringComparison.Ordinal);

		INamedTypeSymbol cacheAttribute = run.OutputCompilation.GetTypeByMetadataName(
			"KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChangedEventArgsAttribute")!;
		Assert.IsNotNull(cacheAttribute);
		Assert.AreEqual(Accessibility.Internal, cacheAttribute.DeclaredAccessibility);
		Assert.IsTrue(cacheAttribute.IsSealed);
		Assert.IsFalse(string.IsNullOrWhiteSpace(cacheAttribute.GetDocumentationCommentXml()));
		AssertValid(run);
	}
}
