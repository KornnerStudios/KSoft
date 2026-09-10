using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
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
	public void UnsupportedHostReportsOneDiagnosticAtFirstSortedPropertyTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class UnknownHost
			{
				[GeneratedPropertyChanged] public partial int Zeta { get; set; }
				[GeneratedPropertyChanged] public partial int Alpha { get; set; }
				[GeneratedPropertyChanged] public partial int Middle { get; set; }
			}
			""",
			includeHost: false);

		Assert.HasCount(1, run.GeneratorDiagnostics);
		Diagnostic diagnostic = run.GeneratorDiagnostics.Single();
		Assert.AreEqual("KSPC0004", diagnostic.Id);
		StringAssert.Contains(
			diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan),
			"Alpha",
			StringComparison.Ordinal);
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
		string[] ids = run.GeneratorDiagnostics
			.Select(static diagnostic => diagnostic.Id)
			.OrderBy(static id => id, StringComparer.Ordinal)
			.ToArray();
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
		CollectionAssert.AreEqual(
			new[] { "KSPC0003", "KSPC0003", "KSPC0003" },
			run.GeneratorDiagnostics
				.Select(static diagnostic => diagnostic.Id)
				.OrderBy(static id => id, StringComparer.Ordinal)
				.ToArray());
	}
}
