using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using KSoft.PropertyChanged.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	private const string HostSource = """
		#nullable enable
		namespace KSoft.ObjectModel
		{
			public abstract class BasicViewModel
			{
				protected bool SetField<T>(ref T field, T value, global::System.ComponentModel.PropertyChangedEventArgs args)
					{ field = value; return true; }
				protected bool SetField<T>(ref T field, T value, global::System.ComponentModel.PropertyChangedEventArgs args, bool overrideChecks)
					{ field = value; return overrideChecks; }
				protected bool SetField(ref int field, int value, global::System.ComponentModel.PropertyChangedEventArgs args)
					{ throw new global::System.InvalidOperationException("The non-generic overload must not be selected."); }
				protected bool SetFieldVal<T>(ref T field, T value, global::System.ComponentModel.PropertyChangedEventArgs args) where T : struct, global::System.IEquatable<T>
					{ field = value; return true; }
				protected bool SetFieldVal(ref int field, int value, global::System.ComponentModel.PropertyChangedEventArgs args)
					{ throw new global::System.InvalidOperationException("The non-generic overload must not be selected."); }
				protected bool SetFieldEnum<T>(ref T field, T value, global::System.ComponentModel.PropertyChangedEventArgs args) where T : struct, global::System.Enum
					{ field = value; return true; }
			}
		}
		""";

	private static readonly ImmutableArray<MetadataReference> sReferences = CreateReferences();

	private static TestRun Run(
		string source,
		LanguageVersion languageVersion = LanguageVersion.CSharp14,
		bool includeHost = true,
		bool includeCaliburnReference = true,
		string assemblyName = "PropertyChangedGeneratorFixture")
	{
		var parseOptions = new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose, SourceCodeKind.Regular);
		var trees = new List<SyntaxTree>();
		if (includeHost) trees.Add(CSharpSyntaxTree.ParseText(HostSource, parseOptions, path: "BasicViewModel.cs"));
		trees.Add(CSharpSyntaxTree.ParseText(source, parseOptions, path: "Input.cs"));

		string caliburnAssemblyPath = typeof(Caliburn.Micro.PropertyChangedBase).Assembly.Location;
		IEnumerable<MetadataReference> references = includeCaliburnReference
			? sReferences
			: sReferences.Where(reference =>
				!string.Equals(
					(reference as PortableExecutableReference)?.FilePath,
					caliburnAssemblyPath,
					StringComparison.OrdinalIgnoreCase));
		CSharpCompilation compilation = CSharpCompilation.Create(
			assemblyName,
			trees,
			references,
			new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				nullableContextOptions: NullableContextOptions.Enable));
		GeneratorDriver driver = CSharpGeneratorDriver.Create(
			new[] { new PropertyChangedGenerator().AsSourceGenerator() },
			parseOptions: parseOptions);
		driver = driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out ImmutableArray<Diagnostic> diagnostics);
		return new TestRun(driver, outputCompilation, diagnostics);
	}

	private static ImmutableArray<MetadataReference> CreateReferences()
	{
		string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
		Assert.IsNotNull(trustedPlatformAssemblies);
		string componentModelAssembly = typeof(PropertyChangedEventArgs).Assembly.Location;
		string caliburnAssembly = typeof(Caliburn.Micro.PropertyChangedBase).Assembly.Location;
		string[] paths = trustedPlatformAssemblies.Split(Path.PathSeparator);
		Assert.IsTrue(paths.Contains(componentModelAssembly, StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(paths.Contains(caliburnAssembly, StringComparer.OrdinalIgnoreCase));
		return paths
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Select(static path => MetadataReference.CreateFromFile(path))
			.ToImmutableArray<MetadataReference>();
	}

	private static string PropertySource(TestRun run)
	{
		GeneratedSourceResult[] sources = PropertySources(run);
		Assert.AreEqual(1, sources.Length);
		return sources[0].SourceText.ToString();
	}

	private static PropertyDeclarationSyntax GeneratedProperty(TestRun run, string propertyName) =>
		GeneratedCompilationUnit(run)
			.DescendantNodes()
			.OfType<PropertyDeclarationSyntax>()
			.Single(property => string.Equals(
				property.Identifier.ValueText,
				propertyName,
				StringComparison.Ordinal));

	private static CompilationUnitSyntax GeneratedCompilationUnit(TestRun run) =>
		CSharpSyntaxTree.ParseText(PropertySource(run)).GetCompilationUnitRoot();

	private static void AssertSetterEquivalent(
		string expectedBlock,
		TestRun run,
		string propertyName)
	{
		AccessorDeclarationSyntax setter = GeneratedProperty(run, propertyName)
			.AccessorList!.Accessors.Single(
				static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
		BlockSyntax expected = (BlockSyntax)SyntaxFactory.ParseStatement(
			NormalizeLineEndings(expectedBlock));
		Assert.AreEqual(
			expected.NormalizeWhitespace().ToFullString(),
			setter.Body!.NormalizeWhitespace().ToFullString());
	}

	private static GeneratedSourceResult[] PropertySources(TestRun run) =>
		run.Driver.GetRunResult().Results.Single().GeneratedSources
			.Where(static source => !string.Equals(
				source.HintName,
				"KSoft.PropertyChanged.Contracts.g.cs",
				StringComparison.Ordinal))
			.ToArray();

	private static string GeneratedSource(TestRun run, string hintName) =>
		run.Driver.GetRunResult().Results.Single().GeneratedSources.Single(
			source => string.Equals(source.HintName, hintName, StringComparison.Ordinal)).SourceText.ToString();

	private static string[] GeneratedOutputs(TestRun run) =>
		run.Driver.GetRunResult().Results.Single().GeneratedSources
			.OrderBy(static source => source.HintName, StringComparer.Ordinal)
			.Select(static source => source.HintName + "\n" + source.SourceText)
			.ToArray();

	private static void AssertSourceEquivalent(string expected, string actual)
	{
		string normalizedExpected = CSharpSyntaxTree.ParseText(NormalizeLineEndings(expected))
			.GetCompilationUnitRoot()
			.NormalizeWhitespace()
			.ToFullString();
		string normalizedActual = CSharpSyntaxTree.ParseText(NormalizeLineEndings(actual))
			.GetCompilationUnitRoot()
			.NormalizeWhitespace()
			.ToFullString();
		Assert.AreEqual(normalizedExpected, normalizedActual);
	}

	private static string NormalizeLineEndings(string value) =>
		value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

	private static void AssertValid(TestRun run)
	{
		Assert.IsEmpty(run.GeneratorDiagnostics);
		Diagnostic[] diagnostics = run.OutputCompilation.GetDiagnostics().ToArray();
		Diagnostic[] errors = diagnostics
			.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		Assert.IsEmpty(
			errors,
			string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
		Diagnostic[] generatedWarnings = diagnostics
			.Where(static diagnostic =>
				diagnostic.Severity == DiagnosticSeverity.Warning
				&& diagnostic.Location.SourceTree is { FilePath: string path }
				&& !path.EndsWith("Input.cs", StringComparison.OrdinalIgnoreCase)
				&& !path.EndsWith("BasicViewModel.cs", StringComparison.OrdinalIgnoreCase))
			.ToArray();
		Assert.IsEmpty(
			generatedWarnings,
			string.Join(
				Environment.NewLine,
				generatedWarnings.Select(static diagnostic => diagnostic.ToString())));
	}

	private static void AssertDiagnosticIds(TestRun run, params string[] expected)
	{
		string[] actual = run.GeneratorDiagnostics
			.Select(static diagnostic => diagnostic.Id)
			.OrderBy(static id => id, StringComparer.Ordinal)
			.ToArray();
		string[] sortedExpected = expected.OrderBy(static id => id, StringComparer.Ordinal).ToArray();
		CollectionAssert.AreEqual(sortedExpected, actual);
	}

	private sealed class TestRun
	{
		public TestRun(
			GeneratorDriver driver,
			Compilation outputCompilation,
			ImmutableArray<Diagnostic> generatorDiagnostics)
		{
			Driver = driver;
			OutputCompilation = outputCompilation;
			GeneratorDiagnostics = generatorDiagnostics;
		}

		public GeneratorDriver Driver { get; }
		public Compilation OutputCompilation { get; }
		public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }
	}
}
