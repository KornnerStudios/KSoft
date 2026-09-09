using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using KSoft.PropertyChanged.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class PropertyChangedGeneratorTests
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

	[TestMethod]
	public void GeneratorInjectsInternalCompileTimeContractTest()
	{
		TestRun run = Run("internal static class Input { }");
		string contract = GeneratedSource(run, "KSoft.PropertyChanged.Contracts.g.cs");
		StringAssert.Contains(contract, "namespace KSoft.PropertyChanged.SourceGeneration", StringComparison.Ordinal);
		Assert.IsFalse(contract.Contains("#nullable", StringComparison.Ordinal));
		StringAssert.Contains(contract, "internal sealed class GeneratedPropertyChangedAttribute", StringComparison.Ordinal);
		StringAssert.Contains(contract, "public string BackingField { get; set; }", StringComparison.Ordinal);
		StringAssert.Contains(contract, "public bool AlwaysNotify { get; set; }", StringComparison.Ordinal);
		StringAssert.Contains(
			contract,
			"whether the generated setter assigns and raises a notification even when the old and new",
			StringComparison.Ordinal);
		Assert.IsFalse(contract.Contains("PropertyChangedEquality", StringComparison.Ordinal));
		Assert.IsEmpty(run.GeneratorDiagnostics);
	}

	[TestMethod]
	public void CompilerFieldModeGeneratesOneSortedCacheAndCompilesTest()
	{
		TestRun run = Run("""
			#nullable enable
			using KSoft.PropertyChanged.SourceGeneration;
			namespace Example;
			public partial class SampleViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public partial string? Zeta { get; set; }

				[GeneratedPropertyChanged]
				public partial int Alpha { get; set; }
			}
			""");
		string generated = PropertySource(run);
		Assert.IsFalse(generated.Contains("#nullable", StringComparison.Ordinal));
		StringAssert.Contains(generated, "namespace Example;", StringComparison.Ordinal);
		Assert.AreEqual(1, Count(generated, "private static class __PropertyChangedEventArgs"));
		Assert.IsTrue(generated.IndexOf("PropertyChangedEventArgs s_Alpha", StringComparison.Ordinal) < generated.IndexOf("PropertyChangedEventArgs s_Zeta", StringComparison.Ordinal));
		StringAssert.Contains(generated, "new(nameof(Alpha));", StringComparison.Ordinal);
		StringAssert.Contains(generated, "get;", StringComparison.Ordinal);
		StringAssert.Contains(generated, "set => base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Alpha);", StringComparison.Ordinal);
		StringAssert.Contains(generated, "global::System.String Zeta", StringComparison.Ordinal);
		StringAssert.Contains(generated, "global::System.CodeDom.Compiler.GeneratedCodeAttribute", StringComparison.Ordinal);
		StringAssert.Contains(generated, "global::System.Diagnostics.DebuggerNonUserCodeAttribute", StringComparison.Ordinal);
		StringAssert.Contains(generated, "global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute", StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void ExplicitFieldModePreservesDirectStorageAndCompilesTest()
	{
		TestRun run = Run("""
			#nullable enable
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class ExplicitViewModel : KSoft.ObjectModel.BasicViewModel
			{
				private string? mText;
				public ExplicitViewModel(string? text) { mText = text; }
				public string? ReadStorage() => mText;
				[GeneratedPropertyChanged(BackingField = nameof(mText))]
				public partial string? Text { get; set; }
			}
			""");
		string generated = PropertySource(run);
		StringAssert.Contains(generated, "get => mText;", StringComparison.Ordinal);
		StringAssert.Contains(generated, "set => base.SetField<global::System.String>(ref mText, value, __PropertyChangedEventArgs.s_Text);", StringComparison.Ordinal);
		Assert.IsFalse(generated.Contains("ref field", StringComparison.Ordinal));
		AssertValid(run);
	}

	[TestMethod]
	public void PropertyInitializerRemainsOnlyOnDefiningDeclarationTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class InitializedViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public partial bool Enabled { get; set; } = true;
			}
			""");
		string generated = PropertySource(run);
		Assert.IsFalse(generated.Contains("= true", StringComparison.Ordinal));
		StringAssert.Contains(generated, "get;", StringComparison.Ordinal);
		StringAssert.Contains(generated, "ref field", StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void PrivateSetterAccessibilityIsRepeatedTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class PrivateSetterViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; private set; }
			}
			""");
		StringAssert.Contains(PropertySource(run), "private set => base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value);", StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	[DataRow("string", "base.SetField<global::System.String>(ref field, value, __PropertyChangedEventArgs.s_Value);")]
	[DataRow("int", "base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value);")]
	[DataRow("global::System.DayOfWeek", "base.SetFieldEnum<global::System.DayOfWeek>(ref field, value, __PropertyChangedEventArgs.s_Value);")]
	public void PropertyTypeSelectsExactHelperTest(string typeName, string expectedCall)
	{
		string source = "using KSoft.PropertyChanged.SourceGeneration; public partial class ModeViewModel : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial " + typeName + " Value { get; set; } }";
		TestRun run = Run(source);
		StringAssert.Contains(PropertySource(run), expectedCall, StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void AlwaysNotifySelectsForcedGenericHelperTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class ForcedViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged(AlwaysNotify = true)]
				public partial int Value { get; set; }
			}
			""");
		StringAssert.Contains(
			PropertySource(run),
			"base.SetField<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value, true);",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void ExplicitTypeArgumentAvoidsCompetingNonGenericOverloadTest()
	{
		TestRun run = Run("using KSoft.PropertyChanged.SourceGeneration; public partial class OverloadViewModel : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } }");
		StringAssert.Contains(
			PropertySource(run),
			"base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value);",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void PropertyNamedLikeCacheTypeReportsKspc0006Test()
	{
		TestRun run = Run("using KSoft.PropertyChanged.SourceGeneration; public partial class CacheNameViewModel : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int __PropertyChangedEventArgs { get; set; } }");
		AssertDiagnosticIds(run, "KSPC0006");
	}

	[TestMethod]
	public void FileLocalHostReportsKspc0007Test()
	{
		TestRun run = Run("using KSoft.PropertyChanged.SourceGeneration; file partial class FileLocalViewModel : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } }");
		AssertDiagnosticIds(run, "KSPC0007");
	}

	[TestMethod]
	public void HostLocalHelperDoesNotHideExplicitBaseCallTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class HiddenHelperViewModel : KSoft.ObjectModel.BasicViewModel
			{
				private bool SetField<T>(
					ref T field,
					T value,
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
					field = value;
					return true;
				}

				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""");
		AssertValid(run);
	}

	[TestMethod]
	public void InvalidEventAttributeDoesNotCrashGeneratorTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class InvalidEventHost : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public event global::System.EventHandler? Changed;
			}
			""");

		Assert.IsFalse(run.GeneratorDiagnostics.Any(
			static diagnostic => string.Equals(diagnostic.Id, "AD0001", StringComparison.Ordinal)));
	}

	[TestMethod]
	public void OutputIsDeterministicAcrossDriverRunsTest()
	{
		const string source = "using KSoft.PropertyChanged.SourceGeneration; namespace Determinism; public partial class Model : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int B { get; set; } [GeneratedPropertyChanged] public partial int A { get; set; } }";
		TestRun first = Run(source);
		TestRun second = Run(source);
		CollectionAssert.AreEqual(GeneratedOutputs(first), GeneratedOutputs(second));
		AssertValid(first);
		AssertValid(second);
	}

	[TestMethod]
	public void RepresentativeInvalidOptInsReportEveryContractDiagnosticTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public class NonPartial : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } }
			public partial class InvalidProperty : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public static partial int Value { get; set; } }
			public partial class MissingField : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged(BackingField = "missing")] public partial int Value { get; set; } }
			public partial class WrongHost { [GeneratedPropertyChanged] public partial int Value { get; set; } }
			public partial class Collision : KSoft.ObjectModel.BasicViewModel
			{
				private static class __PropertyChangedEventArgs { }
				[GeneratedPropertyChanged] public partial int Value { get; set; }
			}
			public partial class Outer
			{
				public partial class Nested : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } }
			}
			""");
		AssertDiagnosticIds(run, "KSPC0001", "KSPC0002", "KSPC0003", "KSPC0004", "KSPC0006", "KSPC0007");
	}

	[TestMethod]
	public void ExistingPartialImplementationReportsKspc0002Test()
	{
		TestRun run = Run("using KSoft.PropertyChanged.SourceGeneration; public partial class Implemented : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } public partial int Value { get => 0; set { } } }");
		AssertDiagnosticIds(run, "KSPC0002");
	}
	[TestMethod]
	public void UnknownHostPathReportsKspc0004Test()
	{
		TestRun run = Run("using KSoft.PropertyChanged.SourceGeneration; public partial class UnknownHost { [GeneratedPropertyChanged] public partial int Value { get; set; } }", includeHost: false);
		AssertDiagnosticIds(run, "KSPC0004");
	}

	[TestMethod]
	public void IndirectBasicViewModelHostReportsKspc0004Test()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public class IntermediateViewModel : KSoft.ObjectModel.BasicViewModel { }
			public partial class IndirectViewModel : IntermediateViewModel
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""");
		AssertDiagnosticIds(run, "KSPC0004");
	}

	[TestMethod]
	public void UnsupportedGenericInitAndExplicitInterfacePropertiesReportKspc0007Test()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public interface IValue { int Value { get; set; } }
			public partial class GenericHost<T> : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; set; } }
			public partial class InitHost : KSoft.ObjectModel.BasicViewModel { [GeneratedPropertyChanged] public partial int Value { get; init; } }
			public partial class InterfaceHost : KSoft.ObjectModel.BasicViewModel, IValue { [GeneratedPropertyChanged] partial int IValue.Value { get; set; } }
			""");
		string[] ids = run.GeneratorDiagnostics.Select(static diagnostic => diagnostic.Id).OrderBy(static id => id, StringComparer.Ordinal).ToArray();
		CollectionAssert.AreEqual(new[] { "KSPC0007", "KSPC0007", "KSPC0007" }, ids);
	}

	[TestMethod]
	public void InvalidExplicitFieldsFailClosedTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class Fields : KSoft.ObjectModel.BasicViewModel
			{
				private readonly int readOnly;
				private static int staticField;
				private string wrongType = "";
				[GeneratedPropertyChanged(BackingField = nameof(readOnly))] public partial int A { get; set; }
				[GeneratedPropertyChanged(BackingField = nameof(staticField))] public partial int B { get; set; }
				[GeneratedPropertyChanged(BackingField = nameof(wrongType))] public partial int C { get; set; }
			}
			""");
		CollectionAssert.AreEqual(new[] { "KSPC0003", "KSPC0003", "KSPC0003" }, run.GeneratorDiagnostics.Select(static x => x.Id).OrderBy(static x => x, StringComparer.Ordinal).ToArray());
	}

	private static TestRun Run(
		string source,
		LanguageVersion languageVersion = LanguageVersion.CSharp14,
		bool includeHost = true)
	{
		var parseOptions = new CSharpParseOptions(languageVersion, DocumentationMode.Diagnose, SourceCodeKind.Regular);
		var trees = new List<SyntaxTree>();
		if (includeHost) trees.Add(CSharpSyntaxTree.ParseText(HostSource, parseOptions, path: "BasicViewModel.cs"));
		trees.Add(CSharpSyntaxTree.ParseText(source, parseOptions, path: "Input.cs"));
		CSharpCompilation compilation = CSharpCompilation.Create("PropertyChangedGeneratorFixture", trees, sReferences, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
		GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new PropertyChangedGenerator().AsSourceGenerator() }, parseOptions: parseOptions);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);
		return new TestRun(driver, outputCompilation, diagnostics);
	}

	private static ImmutableArray<MetadataReference> CreateReferences()
	{
		string? trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
		Assert.IsNotNull(trustedPlatformAssemblies);
		string componentModelAssembly = typeof(PropertyChangedEventArgs).Assembly.Location;
		string[] paths = trustedPlatformAssemblies.Split(Path.PathSeparator);
		Assert.IsTrue(paths.Contains(componentModelAssembly, StringComparer.OrdinalIgnoreCase));
		return paths.Distinct(StringComparer.OrdinalIgnoreCase).Select(static path => MetadataReference.CreateFromFile(path)).ToImmutableArray<MetadataReference>();
	}

	private static string PropertySource(TestRun run)
	{
		GeneratedSourceResult[] sources = run.Driver.GetRunResult().Results.Single().GeneratedSources.Where(static source => !string.Equals(source.HintName, "KSoft.PropertyChanged.Contracts.g.cs", StringComparison.Ordinal)).ToArray();
		Assert.AreEqual(1, sources.Length);
		return sources[0].SourceText.ToString();
	}

	private static string GeneratedSource(TestRun run, string hintName) => run.Driver.GetRunResult().Results.Single().GeneratedSources.Single(source => string.Equals(source.HintName, hintName, StringComparison.Ordinal)).SourceText.ToString();

	private static string[] GeneratedOutputs(TestRun run) => run.Driver.GetRunResult().Results.Single().GeneratedSources.OrderBy(static source => source.HintName, StringComparer.Ordinal).Select(static source => source.HintName + "\n" + source.SourceText).ToArray();

	private static void AssertValid(TestRun run)
	{
		Assert.IsEmpty(run.GeneratorDiagnostics);
		Diagnostic[] errors = run.OutputCompilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
		Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
	}

	private static void AssertDiagnosticIds(TestRun run, params string[] expected)
	{
		string[] actual = run.GeneratorDiagnostics.Select(static diagnostic => diagnostic.Id).OrderBy(static id => id, StringComparer.Ordinal).ToArray();
		string[] sortedExpected = expected.OrderBy(static id => id, StringComparer.Ordinal).ToArray();
		CollectionAssert.AreEqual(sortedExpected, actual);
	}

	private static int Count(string value, string text)
	{
		int count = 0;
		int index = 0;
		while ((index = value.IndexOf(text, index, StringComparison.Ordinal)) >= 0) { count++; index += text.Length; }
		return count;
	}

	private sealed class TestRun
	{
		public TestRun(GeneratorDriver driver, Compilation outputCompilation, ImmutableArray<Diagnostic> generatorDiagnostics) { Driver = driver; OutputCompilation = outputCompilation; GeneratorDiagnostics = generatorDiagnostics; }
		public GeneratorDriver Driver { get; }
		public Compilation OutputCompilation { get; }
		public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }
	}
}
