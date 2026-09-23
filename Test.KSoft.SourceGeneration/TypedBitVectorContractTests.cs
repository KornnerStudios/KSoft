using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KSoft.Collections;
using KSoft.SourceGeneration.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class TypedBitVectorContractTests
{
	[TestMethod]
	public void Consumer_AssociatedEnum_CompilesWithNonGenericTestParameter()
	{
		var compilation = Compile("a.Set(A.First); _ = a.Test(A.First); a = a.With(A.First, false);");
		var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
		var tree = compilation.SyntaxTrees.Single();
		var invocation = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>()
			.Single(node => node.Expression.ToString() == "a.Test");
		var method = Assert.IsInstanceOfType<IMethodSymbol>(compilation.GetSemanticModel(tree).GetSymbolInfo(invocation).Symbol);
		Assert.IsFalse(method.IsGenericMethod);
		Assert.AreEqual("A", method.Parameters[0].Type.Name);
	}

	[TestMethod]
	[DataRow("a.Test(B.First);", "CS1503")]
	[DataRow("a.Set(B.First);", "CS1503")]
	[DataRow("a[B.First] = true;", "CS1503")]
	[DataRow("BitVector32<B> other = a;", "CS0029")]
	[DataRow("_ = a | default(BitVector32<B>);", "CS0019")]
	[DataRow("BitVector32 raw = a;", "CS0029")]
	[DataRow("a.Test<B>(B.First);", "CS0308")]
	public void Consumer_WrongDomainOrImplicitEscape_DoesNotCompile(string statement, string errorId)
	{
		var errors = Compile(statement).GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.HasCount(1, errors, string.Join(Environment.NewLine, errors.Select(e => e.ToString())));
		Assert.AreEqual(errorId, errors[0].Id);
	}

	[TestMethod]
	public void Generator_RepeatedBuild_ProducesIdenticalTypedDeclarations()
	{
		string first = TypedBitVectorsSourceBuilder.Build();
		Assert.AreEqual(first, TypedBitVectorsSourceBuilder.Build());
		Assert.Contains("public struct BitVector32<TBits>", first);
		Assert.Contains("public struct BitVector64<TBits>", first);
		Assert.DoesNotContain("Test<TEnum>", first);
	}

	[TestMethod]
	public void Generator_Documentation_IsWellFormedAndAttachedToBothTypedApis()
	{
		var tree = CSharpSyntaxTree.ParseText(TypedBitVectorsSourceBuilder.Build(),
			CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose));
		var diagnostics = tree.GetDiagnostics().ToArray();
		Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString())));
		var types = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>().ToArray();
		Assert.HasCount(2, types);
		foreach (var type in types)
		{
			var typeDocs = GetDocumentation(type);
			Assert.AreEqual("TBits", typeDocs.Element("typeparam")?.Attribute("name")?.Value);
			Assert.IsNotNull(typeDocs.Element("remarks"));
			var methods = type.Members.OfType<MethodDeclarationSyntax>().ToArray();
			foreach (string name in new[] { "FromRaw", "ToRaw", "Test", "Set", "Toggle", "With", "Clear", "SetAll", "Not", "ToFlagsString", "ToString", "GetEnumerator" })
			{
				Assert.IsNotNull(GetDocumentation(methods.Single(m => m.Identifier.ValueText == name)).Element("summary"));
			}
			var setDocs = GetDocumentation(methods.Single(m => m.Identifier.ValueText == "Set"));
			CollectionAssert.AreEqual(new[] { "bit", "value" }, setDocs.Elements("param").Select(p => p.Attribute("name")!.Value).ToArray());
			Assert.Contains("copy", setDocs.Element("returns")!.Value);
			Assert.Contains("chaining", setDocs.Element("remarks")!.Value);
			var formatDocs = GetDocumentation(methods.Single(m => m.Identifier.ValueText == "ToFlagsString"));
			CollectionAssert.AreEqual(new[] { "separator", "stateFilter" }, formatDocs.Elements("param").Select(p => p.Attribute("name")!.Value).ToArray());
			Assert.Contains("ArgumentNullException", formatDocs.Elements("exception").Select(e => e.Attribute("cref")!.Value));
			Assert.IsNotNull(GetDocumentation(type.Members.OfType<IndexerDeclarationSyntax>().Single()).Element("param"));
			foreach (var property in type.Members.OfType<PropertyDeclarationSyntax>().Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword)))
			{
				Assert.IsNotNull(GetDocumentation(property).Element("summary"));
			}
		}
	}

	static XElement GetDocumentation(MemberDeclarationSyntax declaration)
	{
		var docs = Assert.ContainsSingle(declaration.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>());
		string xml = string.Join("\n", docs.ToFullString().Split('\n').Select(line =>
		{
			string text = line.TrimStart();
			return text.StartsWith("///", StringComparison.Ordinal) ? text[3..] : text;
		}));
		return XElement.Parse("<member>" + xml + "</member>");
	}

	static CSharpCompilation Compile(string statement)
	{
		string source = """
			using KSoft.Collections;
			enum A { First }
			enum B { First }
			static class Consumer
			{
				static void Run()
				{
					BitVector32<A> a = default;
			""" + statement + "\n}\n}";
		var paths = Assert.IsInstanceOfType<string>(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
			.Split(Path.PathSeparator).Append(typeof(BitVector32).Assembly.Location)
			.Distinct(StringComparer.OrdinalIgnoreCase);
		return CSharpCompilation.Create("TypedBitVectorConsumer",
			[CSharpSyntaxTree.ParseText(source)],
			paths.Select(path => MetadataReference.CreateFromFile(path)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}
}
