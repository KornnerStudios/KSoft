using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void BasicViewModelDependentsAreOrderedAndUseVirtualDispatchTest()
	{
		const string assemblyName = "BasicDependentRuntimeFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class DependentBasicViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public string First => "";
				public string Second => "";
				public int VirtualCalls { get; private set; }

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(First), nameof(Second) })]
					public partial int Value { get; set; }

				[GeneratedPropertyChanged(
					AlwaysNotify = true,
					DependentProperties = new[] { nameof(First) })]
					public partial int Forced { get; set; }

				protected override void OnPropertyChanged(string propertyName = "")
				{
					VirtualCalls++;
					base.OnPropertyChanged(propertyName);
				}
			}
			""",
			assemblyName: assemblyName);

		AssertSetterEquivalent(
			"""
			{
				if (base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value))
				{
					OnPropertyChanged("First");
					OnPropertyChanged("Second");
				}
			}
			""",
			run,
			"Value");
		AssertSetterEquivalent(
			"""
			{
				if (base.SetField<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Forced, true))
				{
					OnPropertyChanged("First");
				}
			}
			""",
			run,
			"Forced");
		AssertValid(run);

		using var assemblyStream = new MemoryStream();
		var emitResult = run.OutputCompilation.Emit(assemblyStream);
		Assert.IsTrue(
			emitResult.Success,
			string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
		assemblyStream.Position = 0;

		System.Reflection.Assembly assembly =
			System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
		Type type = assembly.GetType("DependentBasicViewModel", throwOnError: true)!;
		object model = Activator.CreateInstance(type)!;
		var propertyNames = new List<string?>();
		var propertyChanged = type.GetEvent("PropertyChanged")!;
		propertyChanged.AddEventHandler(
			model,
			new global::System.ComponentModel.PropertyChangedEventHandler(
				(_, args) => propertyNames.Add(args.PropertyName)));

		System.Reflection.PropertyInfo value = type.GetProperty("Value")!;
		value.SetValue(model, 1);
		value.SetValue(model, 1);
		System.Reflection.PropertyInfo forced = type.GetProperty("Forced")!;
		forced.SetValue(model, 0);
		forced.SetValue(model, 0);

		CollectionAssert.AreEqual(
			new[] { "Value", "First", "Second", "Forced", "First", "Forced", "First" },
			propertyNames);
		Assert.AreEqual(4, type.GetProperty("VirtualCalls")!.GetValue(model));
	}

	[TestMethod]
	public void BasicViewModelDependentsRequireOnPropertyChangedContractTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			namespace KSoft.ObjectModel
			{
				public abstract class BasicViewModel
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

			public partial class MissingNotificationViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int Dependent => 0;

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(Dependent) })]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"OnPropertyChanged",
			StringComparison.Ordinal);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	[DataRow(
		"protected int OnPropertyChanged;",
		"declares accessible member",
		DisplayName = "Field")]
	[DataRow(
		"protected new void OnPropertyChanged(int value) { }",
		"declares accessible member",
		DisplayName = "WrongOverload")]
	[DataRow(
		"protected new virtual void OnPropertyChanged(string propertyName = \"\") { }",
		"genuine override",
		DisplayName = "NewVirtual")]
	public void BasicViewModelDependentsRejectHidingOnPropertyChangedTest(
		string hidingMember,
		string expectedReason)
	{
		TestRun run = Run($$"""
			using KSoft.PropertyChanged.SourceGeneration;

			public abstract class HidingNotificationBase : KSoft.ObjectModel.BasicViewModel
			{
				{{hidingMember}}
			}

			public partial class HiddenNotificationViewModel : HidingNotificationBase
			{
				public int Dependent => 0;

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(Dependent) })]
					public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			expectedReason,
			StringComparison.Ordinal);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	public void BasicViewModelDependentsAllowIntermediateOverrideChainTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public abstract class FirstNotificationBase : KSoft.ObjectModel.BasicViewModel
			{
				protected override void OnPropertyChanged(string propertyName = "")
				{
					base.OnPropertyChanged(propertyName);
				}
			}

			public abstract class SecondNotificationBase : FirstNotificationBase
			{
				protected override void OnPropertyChanged(string propertyName = "")
				{
					base.OnPropertyChanged(propertyName);
				}
			}

			public partial class OverriddenNotificationViewModel : SecondNotificationBase
			{
				public int Dependent => 0;

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(Dependent) })]
					public partial int Value { get; set; }
			}
			""");

		StringAssert.Contains(
			PropertySource(run),
			"OnPropertyChanged(\"Dependent\");",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void CaliburnDependentsPreserveReceiverOrderSuppressionAndIsNotifyingTest()
	{
		const string assemblyName = "CaliburnDependentRuntimeFixture";
		TestRun run = Run("""
			#nullable disable
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class DependentCaliburnViewModel : Caliburn.Micro.PropertyChangedBase
			{
				public string First => "";
				public string Second => "";
				public int NotificationCalls { get; private set; }

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(First), nameof(Second) })]
					public partial int Value { get; set; }

				[GeneratedPropertyChanged(
					AlwaysNotify = true,
					DependentProperties = new[] { nameof(First) })]
					public partial int Forced { get; set; }

				public override void NotifyOfPropertyChange(string propertyName = null)
				{
					NotificationCalls++;
					base.NotifyOfPropertyChange(propertyName);
				}
			}
			""",
			includeHost: false,
			assemblyName: assemblyName);

		AssertSetterEquivalent(
			"""
			{
				if (__PropertyChangedValuesEqual(ref field, value))
				{
					return;
				}

				field = value;
				global::Caliburn.Micro.PropertyChangedBase propertyChangedNotifier = this;
				if (propertyChangedNotifier.IsNotifying)
				{
					propertyChangedNotifier.NotifyOfPropertyChange(nameof(Value));
					propertyChangedNotifier.NotifyOfPropertyChange("First");
					propertyChangedNotifier.NotifyOfPropertyChange("Second");
				}
			}
			""",
			run,
			"Value");
		AssertValid(run);

		using var assemblyStream = new MemoryStream();
		var emitResult = run.OutputCompilation.Emit(assemblyStream);
		Assert.IsTrue(
			emitResult.Success,
			string.Join(Environment.NewLine, emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
		assemblyStream.Position = 0;

		System.Reflection.Assembly assembly =
			System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
		Type type = assembly.GetType("DependentCaliburnViewModel", throwOnError: true)!;
		var model = (Caliburn.Micro.PropertyChangedBase)Activator.CreateInstance(type)!;
		var propertyNames = new List<string?>();
		model.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);

		System.Reflection.PropertyInfo value = type.GetProperty("Value")!;
		value.SetValue(model, 1);
		value.SetValue(model, 1);
		model.IsNotifying = false;
		value.SetValue(model, 2);
		System.Reflection.PropertyInfo forced = type.GetProperty("Forced")!;
		forced.SetValue(model, 0);
		model.IsNotifying = true;
		forced.SetValue(model, 0);
		forced.SetValue(model, 0);

		CollectionAssert.AreEqual(
			new[] { "Value", "First", "Second", "Forced", "First", "Forced", "First" },
			propertyNames);
		Assert.AreEqual(7, type.GetProperty("NotificationCalls")!.GetValue(model));
	}

	[TestMethod]
	public void CaliburnDependentsUseExactReceiverDespiteHiddenMembersTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class HiddenDependentCaliburnViewModel : Caliburn.Micro.PropertyChangedBase
			{
				public int Dependent => 0;
				public new bool IsNotifying => false;
				public new void NotifyOfPropertyChange(string propertyName = null)
				{
					throw new global::System.InvalidOperationException();
				}

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(Dependent) })]
					public partial int Value { get; set; }
			}
			""");

		AccessorDeclarationSyntax setter = GeneratedProperty(run, "Value")
			.AccessorList!.Accessors.Single(
				static accessor => accessor.IsKind(
					Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration));
		string setterText = setter.NormalizeWhitespace().ToFullString();
		StringAssert.Contains(
			setterText,
			"propertyChangedNotifier.NotifyOfPropertyChange(nameof(Value));",
			StringComparison.Ordinal);
		StringAssert.Contains(
			setterText,
			"propertyChangedNotifier.NotifyOfPropertyChange(\"Dependent\");",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void DependentPropertiesResolveAcrossLocalBaseAndInterfacesTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public interface IListAutoIdObject
			{
				object Data { get; }
			}

			public interface IInheritedValue
			{
				int InterfaceValue { get; }
			}

			public abstract class DependentBase : KSoft.ObjectModel.BasicViewModel, IInheritedValue
			{
				public int BaseValue => 0;
				public int InterfaceValue => 0;
			}

			public partial class RichDependentViewModel : DependentBase, IListAutoIdObject
			{
				public int LocalValue => 0;
				object IListAutoIdObject.Data => this;

				[GeneratedPropertyChanged(
					DependentProperties = new[]
					{
						nameof(LocalValue),
						nameof(BaseValue),
						nameof(IInheritedValue.InterfaceValue),
						nameof(IListAutoIdObject.Data),
					})]
					public partial int Value { get; set; }
			}
			""");

		string source = PropertySource(run);
		int localIndex = source.IndexOf("OnPropertyChanged(\"LocalValue\")", StringComparison.Ordinal);
		int baseIndex = source.IndexOf("OnPropertyChanged(\"BaseValue\")", StringComparison.Ordinal);
		int interfaceIndex = source.IndexOf("OnPropertyChanged(\"InterfaceValue\")", StringComparison.Ordinal);
		int explicitIndex = source.IndexOf("OnPropertyChanged(\"Data\")", StringComparison.Ordinal);
		Assert.IsGreaterThanOrEqualTo(0, localIndex);
		Assert.IsGreaterThan(localIndex, baseIndex);
		Assert.IsGreaterThan(baseIndex, interfaceIndex);
		Assert.IsGreaterThan(interfaceIndex, explicitIndex);
		AssertValid(run);
	}

	[TestMethod]
	[DataRow("new[] { \"\" }", "non-empty", DisplayName = "Empty")]
	[DataRow("new[] { \"   \" }", "non-empty", DisplayName = "Whitespace")]
	[DataRow("new[] { nameof(Value) }", "itself", DisplayName = "Self")]
	[DataRow("new[] { nameof(Other), nameof(Other) }", "more than once", DisplayName = "Duplicate")]
	[DataRow("new[] { \"Missing\" }", "instance property", DisplayName = "Missing")]
	[DataRow("new[] { nameof(StaticValue) }", "static", DisplayName = "Static")]
	public void InvalidDependentPropertiesFailClosedTest(string declaration, string expectedReason)
	{
		TestRun run = Run($$"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class InvalidDependentViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int Other => 0;
				public static int StaticValue => 0;

				[GeneratedPropertyChanged(DependentProperties = {{declaration}})]
					public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0009");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			expectedReason,
			StringComparison.OrdinalIgnoreCase);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	public void NullDependentPropertyArrayFailsClosedTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class NullDependentViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged(DependentProperties = null)]
					public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0009");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"cannot be null",
			StringComparison.Ordinal);
	}

	[TestMethod]
	public void CachedEventArgsProviderRejectsDependentNotificationsTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class CachedDependentHost
			{
				public int Dependent => 0;

				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged(DependentProperties = new[] { nameof(Dependent) })]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0010");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"CachedEventArgs",
			StringComparison.Ordinal);
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	public void DependentNotificationOutputIsDeterministicTest()
	{
		const string source = """
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class DeterministicDependentViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int First => 0;
				public int Second => 0;

				[GeneratedPropertyChanged(
					DependentProperties = new[] { nameof(Second), nameof(First) })]
					public partial int Value { get; set; }
			}
			""";

		TestRun first = Run(source);
		TestRun second = Run(source);
		CollectionAssert.AreEqual(GeneratedOutputs(first), GeneratedOutputs(second));
		AssertValid(first);
		AssertValid(second);
	}
}
