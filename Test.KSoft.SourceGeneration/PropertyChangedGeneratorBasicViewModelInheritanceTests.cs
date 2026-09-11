using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void BasicViewModelProviderSupportsOneMultipleAndGenericIntermediateBasesTest()
	{
		string[] baseDeclarations = {
			"public abstract class OneBase : KSoft.ObjectModel.BasicViewModel { }",
			"public abstract class FirstBase : KSoft.ObjectModel.BasicViewModel { } public abstract class OneBase : FirstBase { }",
			"public abstract class GenericBase<T> : KSoft.ObjectModel.BasicViewModel { } public abstract class OneBase : GenericBase<int> { }",
		};

		foreach (string baseDeclaration in baseDeclarations)
		{
			TestRun run = Run($$"""
				using KSoft.PropertyChanged.SourceGeneration;

				{{baseDeclaration}}

				public partial class IndirectViewModel : OneBase
				{
					[GeneratedPropertyChanged]
						public partial int Value { get; set; }
				}
				""");

			StringAssert.Contains(
				PropertySource(run),
				"set => base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value);",
				StringComparison.Ordinal);
			AssertValid(run);
		}
	}

	[TestMethod]
	[DataRow(
		"SetField",
		"string",
		"protected int SetField;",
		DisplayName = "SetField")]
	[DataRow(
		"SetFieldVal",
		"int",
		"protected int SetFieldVal;",
		DisplayName = "SetFieldVal")]
	[DataRow(
		"SetFieldEnum",
		"global::System.DayOfWeek",
		"protected int SetFieldEnum;",
		DisplayName = "SetFieldEnum")]
	public void BasicViewModelProviderRejectsAccessibleIntermediateHelperHidingTest(
		string helperName,
		string propertyType,
		string hiddenMember)
	{
		TestRun run = Run($$"""
			using KSoft.PropertyChanged.SourceGeneration;

			public abstract class HidingBase : KSoft.ObjectModel.BasicViewModel
			{
				{{hiddenMember}}
			}

			public partial class HiddenHelperViewModel : HidingBase
			{
				[GeneratedPropertyChanged]
					public partial {{propertyType}} Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			helperName,
			StringComparison.Ordinal);
	}

	[TestMethod]
	public void BasicViewModelProviderIgnoresInaccessibleIntermediateHelperTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public abstract class PrivateHidingBase : KSoft.ObjectModel.BasicViewModel
			{
				private int SetFieldVal;
			}

			public partial class PrivateHiddenHelperViewModel : PrivateHidingBase
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""");

		AssertValid(run);
	}

	[TestMethod]
	public void BasicViewModelProviderInvalidHelperContractFailsClosedTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace KSoft.ObjectModel
			{
				public abstract class BasicViewModel
				{
					protected bool SetField<T>(
						ref T field,
						T value,
						global::System.ComponentModel.PropertyChangedEventArgs args)
					{
						field = value;
						return true;
					}
				}
			}

			public partial class InvalidBasicViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"SetFieldVal",
			StringComparison.Ordinal);
	}

	[TestMethod]
	[DataRow(
		"SetField",
		"int",
		"AlwaysNotify = true",
		"where T : notnull",
		"no generic constraints",
		DisplayName = "SetFieldNotNull")]
	[DataRow(
		"SetFieldVal",
		"int",
		"",
		"where T : class",
		"where T : struct, IEquatable<T>",
		DisplayName = "SetFieldValClass")]
	[DataRow(
		"SetFieldVal",
		"int",
		"",
		"where T : struct",
		"where T : struct, IEquatable<T>",
		DisplayName = "SetFieldValMissingEquatable")]
	[DataRow(
		"SetFieldVal",
		"int",
		"",
		"where T : struct, global::System.IEquatable<T>, global::System.IComparable<T>",
		"where T : struct, IEquatable<T>",
		DisplayName = "SetFieldValExtraConstraint")]
	[DataRow(
		"SetFieldEnum",
		"global::System.DayOfWeek",
		"",
		"where T : global::System.Enum",
		"where T : struct, System.Enum",
		DisplayName = "SetFieldEnumMissingStruct")]
	[DataRow(
		"SetFieldEnum",
		"global::System.DayOfWeek",
		"",
		"where T : struct, global::System.Enum, global::System.IComparable",
		"where T : struct, System.Enum",
		DisplayName = "SetFieldEnumExtraConstraint")]
	public void BasicViewModelProviderRejectsIncompatibleHelperConstraintsTest(
		string helperName,
		string propertyType,
		string attributeArguments,
		string constraints,
		string expectedConstraints)
	{
		string attribute = attributeArguments.Length == 0
			? "GeneratedPropertyChanged"
			: $"GeneratedPropertyChanged({attributeArguments})";
		int parameterCount = attributeArguments.Length == 0 ? 3 : 4;
		string fourthParameter = parameterCount == 4 ? ", bool overrideChecks" : "";
		TestRun run = Run(
			$$"""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace KSoft.ObjectModel
			{
				public abstract class BasicViewModel
				{
					protected bool {{helperName}}<T>(
						ref T field,
						T value,
						global::System.ComponentModel.PropertyChangedEventArgs args{{fourthParameter}})
						{{constraints}}
					{
						field = value;
						return true;
					}
				}
			}

			public partial class InvalidConstraintViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[{{attribute}}]
					public partial {{propertyType}} Value { get; set; }
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0007");
		string message = run.GeneratorDiagnostics.Single().GetMessage();
		StringAssert.Contains(message, helperName, StringComparison.Ordinal);
		StringAssert.Contains(message, expectedConstraints, StringComparison.Ordinal);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	[DataRow(
		"SetField",
		"string",
		"",
		"",
		"",
		DisplayName = "SetField")]
	[DataRow(
		"SetField",
		"int",
		"AlwaysNotify = true",
		", bool overrideChecks",
		"",
		DisplayName = "SetFieldAlwaysNotify")]
	[DataRow(
		"SetFieldVal",
		"int",
		"",
		"",
		"where T : struct, global::System.IEquatable<T>",
		DisplayName = "SetFieldVal")]
	[DataRow(
		"SetFieldEnum",
		"global::System.DayOfWeek",
		"",
		"",
		"where T : struct, global::System.Enum",
		DisplayName = "SetFieldEnum")]
	public void BasicViewModelProviderRejectsAbstractHelpersTest(
		string helperName,
		string propertyType,
		string attributeArguments,
		string fourthParameter,
		string constraints)
	{
		string attribute = attributeArguments.Length == 0
			? "GeneratedPropertyChanged"
			: $"GeneratedPropertyChanged({attributeArguments})";
		TestRun run = Run(
			$$"""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace KSoft.ObjectModel
			{
				public abstract class BasicViewModel
				{
					protected abstract bool {{helperName}}<T>(
						ref T field,
						T value,
						global::System.ComponentModel.PropertyChangedEventArgs args{{fourthParameter}})
						{{constraints}};
				}
			}

			public abstract partial class AbstractHelperViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[{{attribute}}]
					public partial {{propertyType}} Value { get; set; }
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			helperName,
			StringComparison.Ordinal);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	public void BasicViewModelProviderPrecedesCaliburnProviderTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace KSoft.ObjectModel
			{
				public abstract class BasicViewModel : Caliburn.Micro.PropertyChangedBase
				{
					protected bool SetFieldVal<T>(
						ref T field,
						T value,
						global::System.ComponentModel.PropertyChangedEventArgs args)
						where T : struct, global::System.IEquatable<T>
					{
						field = value;
						return true;
					}
				}
			}

			public abstract class IntermediateBase : KSoft.ObjectModel.BasicViewModel { }

			public partial class ProviderOrderViewModel : IntermediateBase
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);

		string source = PropertySource(run);
		StringAssert.Contains(source, "base.SetFieldVal<global::System.Int32>", StringComparison.Ordinal);
		Assert.IsFalse(source.Contains("NotifyOfPropertyChange", StringComparison.Ordinal));
		AssertValid(run);
	}

	[TestMethod]
	public void UnrelatedBasicViewModelNameIsNotAcceptedTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace Other
			{
				public abstract class BasicViewModel { }
			}

			public partial class UnsupportedViewModel : Other.BasicViewModel
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0004");
	}
}
