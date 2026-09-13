using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void ChangedHookNoneMatchesOmittedOutputForEveryProviderTest()
	{
		AssertNoneMatchesOmitted(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class BasicNoneViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""",
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class BasicNoneViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.None)]
				public partial int Value { get; set; }
			}
			""",
			includeHost: true);
		AssertNoneMatchesOmitted(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class CachedNoneViewModel
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""",
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class CachedNoneViewModel
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.None)]
				public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertNoneMatchesOmitted(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class CaliburnNoneViewModel : Caliburn.Micro.PropertyChangedBase
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""",
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class CaliburnNoneViewModel : Caliburn.Micro.PropertyChangedBase
			{
				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.None)]
				public partial int Value { get; set; }
			}
			""",
			includeHost: false);
	}

	[TestMethod]
	public void ParameterlessChangedHooksSupportEscapedExplicitAndCompilerFieldsTest()
	{
		const string source = """
			#nullable enable
			using KSoft.PropertyChanged.SourceGeneration;

			public class IntermediateViewModel : KSoft.ObjectModel.BasicViewModel
			{
			}

			public partial class EscapedHookViewModel : IntermediateViewModel
			{
				private int mClass;

				[GeneratedPropertyChanged(
					BackingField = nameof(mClass),
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int @class { get; private set; }

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial string? Absent { get; set; }

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial string? Expression { get; private set; }

				partial void OnclassChanged()
				{
				}

				partial void OnExpressionChanged() => _ = @class;
			}
			""";

		TestRun first = Run(source);
		TestRun second = Run(source);
		string generated = PropertySource(first);
		CompilationUnitSyntax compilationUnit = GeneratedCompilationUnit(first);
		MethodDeclarationSyntax[] hooks = compilationUnit.DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.Where(static method => method.Identifier.ValueText.StartsWith("On", StringComparison.Ordinal))
			.ToArray();

		Assert.HasCount(3, hooks);
		foreach (MethodDeclarationSyntax hook in hooks)
		{
			Assert.IsEmpty(hook.AttributeLists);
			CollectionAssert.AreEqual(
				new[] { SyntaxKind.PartialKeyword },
				hook.Modifiers.Select(static modifier => modifier.Kind()).ToArray());
			Assert.IsNull(hook.Body);
			Assert.IsNull(hook.ExpressionBody);
		}
		StringAssert.Contains(generated, "partial void OnclassChanged();", StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"private set",
			StringComparison.Ordinal);
		CollectionAssert.AreEqual(GeneratedOutputs(first), GeneratedOutputs(second));
		AssertValid(first);
		AssertValid(second);
	}

	[TestMethod]
	public void ParameterlessChangedHooksPreserveHelpersAndAcceptedChangeOrderingTest()
	{
		const string assemblyName = "ChangedHookRuntimeFixture";
		TestRun run = Run(
			"""
			#nullable enable
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class ChangedHookRuntimeViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int Dependent => 0;
				public int HookCalls { get; private set; }

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(Dependent) })]
				public partial int Value { get; set; }

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial global::System.DayOfWeek Mode { get; set; }

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial string? Text { get; set; }

				[GeneratedPropertyChanged(
					AlwaysNotify = true,
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(Dependent) })]
				public partial int Forced { get; set; }

				private void RecordHook()
				{
					HookCalls++;
				}

				partial void OnValueChanged() => RecordHook();
				partial void OnModeChanged() => RecordHook();
				partial void OnTextChanged() => RecordHook();
				partial void OnForcedChanged() => RecordHook();
			}
			""",
			assemblyName: assemblyName);

		string generated = PropertySource(run);
		StringAssert.Contains(
			generated,
			"base.SetFieldVal<global::System.Int32>(ref field, value, __PropertyChangedEventArgs.s_Value)",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"base.SetFieldEnum<global::System.DayOfWeek>(ref field, value, __PropertyChangedEventArgs.s_Mode)",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"base.SetField<global::System.String>(ref field, value, __PropertyChangedEventArgs.s_Text)",
			StringComparison.Ordinal);
		StringAssert.Contains(generated, "this.OnValueChanged();", StringComparison.Ordinal);
		Assert.IsLessThan(
			generated.IndexOf("this.OnValueChanged();", StringComparison.Ordinal),
			generated.IndexOf("OnPropertyChanged(\"Dependent\");", StringComparison.Ordinal));
		AssertValid(run);

		Type type = Emit(run).GetType("ChangedHookRuntimeViewModel", throwOnError: true)!;
		object model = Activator.CreateInstance(type)!;
		PropertyInfo value = type.GetProperty("Value")!;
		PropertyInfo mode = type.GetProperty("Mode")!;
		PropertyInfo text = type.GetProperty("Text")!;
		PropertyInfo forced = type.GetProperty("Forced")!;

		value.SetValue(model, 1);
		mode.SetValue(model, DayOfWeek.Monday);
		text.SetValue(model, "text");
		Assert.AreEqual(3, type.GetProperty("HookCalls")!.GetValue(model));

		var propertyNames = new List<string?>();
		type.GetEvent("PropertyChanged")!.AddEventHandler(
			model,
			new System.ComponentModel.PropertyChangedEventHandler(
				(_, args) => propertyNames.Add(args.PropertyName)));

		value.SetValue(model, 2);
		value.SetValue(model, 2);
		forced.SetValue(model, 0);
		forced.SetValue(model, 0);

		CollectionAssert.AreEqual(
			new[] { "Value", "Dependent", "Forced", "Dependent", "Forced", "Dependent" },
			propertyNames);
		Assert.AreEqual(6, type.GetProperty("HookCalls")!.GetValue(model));
	}

	[TestMethod]
	public void UndefinedChangedHookValueRefusesOnlyThatCandidateTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class InvalidChangedHookViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged(
					ChangedHook = (GeneratedPropertyChangedHook)42)]
				public partial int Invalid { get; set; }
			}

			public partial class ValidChangedHookViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0007");
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"ChangedHook",
			StringComparison.Ordinal);
		StringAssert.Contains(
			run.GeneratorDiagnostics.Single().GetMessage(),
			"None",
			StringComparison.Ordinal);
		Assert.HasCount(1, PropertySources(run));
		StringAssert.Contains(PropertySource(run), "ValidChangedHookViewModel", StringComparison.Ordinal);
	}

	[TestMethod]
	public void ParameterlessChangedHooksRejectCachedEventArgsProvidersTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class CachedHookViewModel
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class ExplicitCachedBasicHookViewModel : KSoft.ObjectModel.BasicViewModel
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}

			""");

		AssertDiagnosticIds(run, "KSPC0007", "KSPC0007");
		Assert.HasCount(0, PropertySources(run));
		Assert.IsTrue(run.GeneratorDiagnostics.All(
			static diagnostic => diagnostic.GetMessage().Contains(
				"Parameterless",
				StringComparison.Ordinal)));
	}

	[TestMethod]
	public void ParameterlessChangedHooksSupportCaliburnWithVirtualNotificationSemanticsTest()
	{
		const string assemblyName = "CaliburnChangedHookRuntimeFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class CaliburnChangedHookViewModel : Caliburn.Micro.PropertyChangedBase
			{
				public int Dependent => 0;
				public int HookCalls { get; private set; }
				public bool ThrowInHook { get; set; }
				public bool ThrowInNotification { get; set; }

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(Dependent) })]
				public partial int Value { get; set; }

				[GeneratedPropertyChanged(
					AlwaysNotify = true,
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Forced { get; set; }

				public override void NotifyOfPropertyChange(string? propertyName = null)
				{
					if (ThrowInNotification)
					{
						throw new global::System.InvalidOperationException("primary");
					}

					base.NotifyOfPropertyChange(propertyName);
				}

				partial void OnValueChanged()
				{
					if (ThrowInHook)
					{
						throw new global::System.InvalidOperationException("hook");
					}

					HookCalls++;
				}

				partial void OnForcedChanged() => HookCalls++;
			}
			""",
			includeHost: false,
			assemblyName: assemblyName);

		CompilationUnitSyntax compilationUnit = GeneratedCompilationUnit(run);
		MethodDeclarationSyntax[] hooks = compilationUnit.DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.Where(static method =>
				method.Identifier.ValueText is "OnValueChanged" or "OnForcedChanged")
			.ToArray();
		Assert.HasCount(2, hooks);
		foreach (MethodDeclarationSyntax hook in hooks)
		{
			Assert.IsEmpty(hook.AttributeLists);
			CollectionAssert.AreEqual(
				new[] { SyntaxKind.PartialKeyword },
				hook.Modifiers.Select(static modifier => modifier.Kind()).ToArray());
		}
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
				}

				this.OnValueChanged();
				if (propertyChangedNotifier.IsNotifying)
				{
					propertyChangedNotifier.NotifyOfPropertyChange("Dependent");
				}
			}
			""",
			run,
			"Value");
		AssertValid(run);

		Type type = Emit(run).GetType("CaliburnChangedHookViewModel", throwOnError: true)!;
		var model = (Caliburn.Micro.PropertyChangedBase)Activator.CreateInstance(type)!;
		PropertyInfo value = type.GetProperty("Value")!;
		PropertyInfo forced = type.GetProperty("Forced")!;
		var propertyNames = new List<string?>();
		var hookCountsAtNotification = new List<int>();
		model.PropertyChanged += (_, args) =>
		{
			propertyNames.Add(args.PropertyName);
			hookCountsAtNotification.Add((int)type.GetProperty("HookCalls")!.GetValue(model)!);
		};

		value.SetValue(model, 1);
		value.SetValue(model, 1);
		CollectionAssert.AreEqual(new[] { "Value", "Dependent" }, propertyNames);
		CollectionAssert.AreEqual(new[] { 0, 1 }, hookCountsAtNotification);
		Assert.AreEqual(1, type.GetProperty("HookCalls")!.GetValue(model));

		model.IsNotifying = false;
		value.SetValue(model, 2);
		forced.SetValue(model, 0);
		forced.SetValue(model, 0);
		CollectionAssert.AreEqual(new[] { "Value", "Dependent" }, propertyNames);
		Assert.AreEqual(4, type.GetProperty("HookCalls")!.GetValue(model));

		model.IsNotifying = true;
		type.GetProperty("ThrowInNotification")!.SetValue(model, true);
		AssertInvocationException(() => value.SetValue(model, 3), "primary");
		CollectionAssert.AreEqual(new[] { "Value", "Dependent" }, propertyNames);
		Assert.AreEqual(4, type.GetProperty("HookCalls")!.GetValue(model));

		type.GetProperty("ThrowInNotification")!.SetValue(model, false);
		type.GetProperty("ThrowInHook")!.SetValue(model, true);
		AssertInvocationException(() => value.SetValue(model, 4), "hook");
		CollectionAssert.AreEqual(new[] { "Value", "Dependent", "Value" }, propertyNames);
		Assert.AreEqual(4, type.GetProperty("HookCalls")!.GetValue(model));
	}

	[TestMethod]
	public void ChangedCallbackSupportsGenericCaliburnHostsWithoutDeclaringAPartialHookTest()
	{
		const string assemblyName = "GenericCaliburnChangedCallbackFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public sealed class ComparableValue : global::System.IComparable<ComparableValue>
			{
				public int CompareTo(ComparableValue? other) => 0;
			}

			public abstract partial class GenericCallbackViewModel<TProto, TExplorer>
				: Caliburn.Micro.PropertyChangedBase
				where TProto : class, global::System.IComparable<TProto>
				where TExplorer : class, new()
			{
				private TProto? mValue;
				public int CallbackCalls { get; private set; }

				[GeneratedPropertyChanged(
					BackingField = nameof(mValue),
					ChangedCallback = nameof(OnValueChanged))]
				public partial TProto? Value { get; set; }

				protected virtual void OnValueChanged() => CallbackCalls++;
			}

			public sealed class ConcreteCallbackViewModel
				: GenericCallbackViewModel<ComparableValue, object>
			{
			}
			""",
			includeHost: false,
			assemblyName: assemblyName);

		string generated = PropertySource(run);
		StringAssert.Contains(generated, "#nullable enable annotations", StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"public abstract partial class GenericCallbackViewModel<TProto, TExplorer>",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"where TProto : class, global::System.IComparable<TProto>",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"where TExplorer : class, new()",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"partial TProto? Value",
			StringComparison.Ordinal);
		StringAssert.Contains(generated, "this.OnValueChanged();", StringComparison.Ordinal);
		Assert.IsFalse(generated.Contains("partial void OnValueChanged();", StringComparison.Ordinal));
		AssertValid(run);

		Assembly assembly = Emit(run);
		Type type = assembly.GetType("ConcreteCallbackViewModel", throwOnError: true)!;
		object comparableValue = Activator.CreateInstance(
			assembly.GetType("ComparableValue", throwOnError: true)!)!;
		var model = (Caliburn.Micro.PropertyChangedBase)Activator.CreateInstance(type)!;
		PropertyInfo value = type.GetProperty("Value")!;
		var propertyNames = new List<string?>();
		model.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);

		value.SetValue(model, comparableValue);
		value.SetValue(model, value.GetValue(model));
		Assert.AreEqual(1, type.GetProperty("CallbackCalls")!.GetValue(model));
		CollectionAssert.AreEqual(new[] { "Value" }, propertyNames);

		model.IsNotifying = false;
		value.SetValue(model, null);
		Assert.AreEqual(2, type.GetProperty("CallbackCalls")!.GetValue(model));
		CollectionAssert.AreEqual(new[] { "Value" }, propertyNames);
	}

	[TestMethod]
	public void ChangedCallbackRequiresOneCompatibleCaliburnMethodAndCannotCombineWithChangedHookTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class MissingCallbackViewModel : Caliburn.Micro.PropertyChangedBase
			{
				[GeneratedPropertyChanged(ChangedCallback = "Missing")]
				public partial int Value { get; set; }
			}

			public partial class ConflictingCallbackViewModel : Caliburn.Micro.PropertyChangedBase
			{
				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					ChangedCallback = nameof(OnValueChanged))]
				public partial int Value { get; set; }

				private void OnValueChanged()
				{
				}
			}
			""",
			includeHost: false);

		AssertDiagnosticIds(run, "KSPC0007", "KSPC0007");
		Assert.HasCount(0, PropertySources(run));
		Assert.IsTrue(run.GeneratorDiagnostics.All(
			static diagnostic => diagnostic.GetMessage().Contains(
				"ChangedCallback",
				StringComparison.Ordinal)));
	}

	[TestMethod]
	public void GenericHostConstraintClausesArePreservedTest()
	{
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class GenericConstraintViewModel<
				TReference,
				TNullableReference,
				TValue,
				TUnmanaged,
				TNotNull,
				TConstructor,
				TAllowsRefStruct>
				: Caliburn.Micro.PropertyChangedBase
				where TReference : class, global::System.IDisposable
				where TNullableReference : class?
				where TValue : struct, global::System.IEquatable<TValue>
				where TUnmanaged : unmanaged
				where TNotNull : notnull
				where TConstructor : new()
				where TAllowsRefStruct : allows ref struct
			{
				[GeneratedPropertyChanged]
				public partial int Value { get; set; }
			}
			""",
			includeHost: false);

		string generated = PropertySource(run);
		StringAssert.Contains(
			generated,
			"GenericConstraintViewModel<TReference, TNullableReference, TValue, TUnmanaged, TNotNull, TConstructor, TAllowsRefStruct>",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"where TReference : class, global::System.IDisposable",
			StringComparison.Ordinal);
		StringAssert.Contains(generated, "where TNullableReference : class?", StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"where TValue : struct, global::System.IEquatable<TValue>",
			StringComparison.Ordinal);
		StringAssert.Contains(generated, "where TUnmanaged : unmanaged", StringComparison.Ordinal);
		StringAssert.Contains(generated, "where TNotNull : notnull", StringComparison.Ordinal);
		StringAssert.Contains(generated, "where TConstructor : new()", StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"where TAllowsRefStruct : allows ref struct",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	[DataRow("partial void OnValueChanged();", DisplayName = "DefiningPartial")]
	[DataRow("void OnValueChanged() { }", DisplayName = "OrdinaryMethod")]
	[DataRow("int OnValueChanged() => 0;", DisplayName = "WrongReturn")]
	[DataRow("partial void OnValueChanged(int value) { }", DisplayName = "Parameter")]
	[DataRow("partial void OnValueChanged<T>() { }", DisplayName = "Generic")]
	[DataRow("static partial void OnValueChanged() { }", DisplayName = "Static")]
	[DataRow("async partial void OnValueChanged() { await global::System.Threading.Tasks.Task.Yield(); }", DisplayName = "Async")]
	[DataRow("private partial void OnValueChanged() { }", DisplayName = "Accessibility")]
	[DataRow("int OnValueChanged;", DisplayName = "NonMethod")]
	public void ParameterlessChangedHookLocalConflictsRefuseTheHostTest(string member)
	{
		TestRun run = Run($$"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class LocalHookConflictViewModel : KSoft.ObjectModel.BasicViewModel
			{
				{{member}}

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}
			""");

		AssertDiagnosticIds(run, "KSPC0006");
		Assert.HasCount(0, PropertySources(run));
	}

	[TestMethod]
	public void ParameterlessChangedHookInheritedConflictsAndLaterCollisionsRefuseOnlyTheirHostsTest()
	{
		TestRun accessibleInherited = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public class AccessibleHookBase : KSoft.ObjectModel.BasicViewModel
			{
				protected void OnValueChanged()
				{
				}
			}

			public partial class AccessibleHookViewModel : AccessibleHookBase
			{
				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}
			""");
		AssertDiagnosticIds(accessibleInherited, "KSPC0006");
		Assert.HasCount(0, PropertySources(accessibleInherited));

		TestRun privateInherited = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public class PrivateHookBase : KSoft.ObjectModel.BasicViewModel
			{
				private void OnValueChanged()
				{
				}
			}

			public partial class PrivateHookViewModel : PrivateHookBase
			{
				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}
			""");
		AssertValid(privateInherited);

		TestRun laterCollision = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class LaterCollisionViewModel : KSoft.ObjectModel.BasicViewModel
			{
				private int OnZetaChanged;

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Alpha { get; set; }

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Zeta { get; set; }
			}

			public partial class SeparateValidViewModel : KSoft.ObjectModel.BasicViewModel
			{
				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int Value { get; set; }
			}
			""");
		AssertDiagnosticIds(laterCollision, "KSPC0006");
		GeneratedSourceResult[] outputs = PropertySources(laterCollision);
		Assert.HasCount(1, outputs);
		StringAssert.Contains(
			outputs[0].SourceText.ToString(),
			"SeparateValidViewModel",
			StringComparison.Ordinal);
	}

	[TestMethod]
	public void ParameterlessChangedHooksPropagateExceptionsAndRemainSynchronousUnderReentryTest()
	{
		const string assemblyName = "ChangedHookExceptionAndReentryFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class ExceptionAndReentryViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public string First => "";
				public string Second => "";
				public bool ThrowInHook { get; set; }
				public bool ReenterFromHook { get; set; }
				public int HookCalls { get; private set; }
				public global::System.Collections.Generic.List<int> HookValues { get; } = new();

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(First), nameof(Second) })]
				public partial int Value { get; set; }

				partial void OnValueChanged()
				{
					if (ThrowInHook)
					{
						throw new global::System.InvalidOperationException("hook");
					}

					if (ReenterFromHook && Value == 1)
					{
						Value = 2;
					}

					HookCalls++;
					HookValues.Add(Value);
				}
			}
			""",
			assemblyName: assemblyName);
		AssertValid(run);

		Type type = Emit(run).GetType("ExceptionAndReentryViewModel", throwOnError: true)!;
		PropertyInfo value = type.GetProperty("Value")!;
		EventInfo propertyChanged = type.GetEvent("PropertyChanged")!;

		object primaryExceptionModel = Activator.CreateInstance(type)!;
		var primaryExceptionNames = new List<string?>();
		propertyChanged.AddEventHandler(
			primaryExceptionModel,
			new System.ComponentModel.PropertyChangedEventHandler((_, args) =>
			{
				primaryExceptionNames.Add(args.PropertyName);
				if (args.PropertyName == "Value")
				{
					throw new InvalidOperationException("primary");
				}
			}));
		AssertInvocationException(
			() => value.SetValue(primaryExceptionModel, 1),
			"primary");
		CollectionAssert.AreEqual(new[] { "Value" }, primaryExceptionNames);
		Assert.AreEqual(1, value.GetValue(primaryExceptionModel));
		Assert.AreEqual(0, type.GetProperty("HookCalls")!.GetValue(primaryExceptionModel));

		object hookExceptionModel = Activator.CreateInstance(type)!;
		var hookExceptionNames = TrackPropertyChanges(propertyChanged, hookExceptionModel);
		type.GetProperty("ThrowInHook")!.SetValue(hookExceptionModel, true);
		AssertInvocationException(() => value.SetValue(hookExceptionModel, 1), "hook");
		CollectionAssert.AreEqual(new[] { "Value" }, hookExceptionNames);
		Assert.AreEqual(1, value.GetValue(hookExceptionModel));
		Assert.AreEqual(0, type.GetProperty("HookCalls")!.GetValue(hookExceptionModel));

		object dependentExceptionModel = Activator.CreateInstance(type)!;
		var dependentExceptionNames = new List<string?>();
		propertyChanged.AddEventHandler(
			dependentExceptionModel,
			new System.ComponentModel.PropertyChangedEventHandler((_, args) =>
			{
				dependentExceptionNames.Add(args.PropertyName);
				if (args.PropertyName == "First")
				{
					throw new InvalidOperationException("dependent");
				}
			}));
		AssertInvocationException(
			() => value.SetValue(dependentExceptionModel, 1),
			"dependent");
		CollectionAssert.AreEqual(new[] { "Value", "First" }, dependentExceptionNames);
		Assert.AreEqual(1, value.GetValue(dependentExceptionModel));
		Assert.AreEqual(1, type.GetProperty("HookCalls")!.GetValue(dependentExceptionModel));

		object primaryReentryModel = Activator.CreateInstance(type)!;
		var primaryReentryNames = new List<string?>();
		bool primaryReentered = false;
		propertyChanged.AddEventHandler(
			primaryReentryModel,
			new System.ComponentModel.PropertyChangedEventHandler((_, args) =>
			{
				primaryReentryNames.Add(args.PropertyName);
				if (!primaryReentered && args.PropertyName == "Value")
				{
					primaryReentered = true;
					value.SetValue(primaryReentryModel, 2);
				}
			}));
		value.SetValue(primaryReentryModel, 1);
		CollectionAssert.AreEqual(
			new[] { "Value", "Value", "First", "Second", "First", "Second" },
			primaryReentryNames);
		CollectionAssert.AreEqual(
			new[] { 2, 2 },
			(List<int>)type.GetProperty("HookValues")!.GetValue(primaryReentryModel)!);

		object hookReentryModel = Activator.CreateInstance(type)!;
		var hookReentryNames = TrackPropertyChanges(propertyChanged, hookReentryModel);
		type.GetProperty("ReenterFromHook")!.SetValue(hookReentryModel, true);
		value.SetValue(hookReentryModel, 1);
		CollectionAssert.AreEqual(
			new[] { "Value", "Value", "First", "Second", "First", "Second" },
			hookReentryNames);
		CollectionAssert.AreEqual(
			new[] { 2, 2 },
			(List<int>)type.GetProperty("HookValues")!.GetValue(hookReentryModel)!);
	}

	[TestMethod]
	public void DependentNotificationsRemainNotificationOnlyAndEqualFeedbackTerminatesTest()
	{
		const string assemblyName = "ChangedHookDependentEdgesFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class DependentEdgesViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int AHookCalls { get; private set; }
				public int BHookCalls { get; private set; }
				public int CHookCalls { get; private set; }

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(B) })]
				public partial int A { get; set; }

				[GeneratedPropertyChanged(
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(C) })]
				public partial int B { get; set; }

				[GeneratedPropertyChanged(ChangedHook = GeneratedPropertyChangedHook.Parameterless)]
				public partial int C { get; set; }

				partial void OnAChanged() => AHookCalls++;
				partial void OnBChanged() => BHookCalls++;
				partial void OnCChanged() => CHookCalls++;
			}
			""",
			assemblyName: assemblyName);
		AssertValid(run);

		Type type = Emit(run).GetType("DependentEdgesViewModel", throwOnError: true)!;
		object model = Activator.CreateInstance(type)!;
		PropertyInfo a = type.GetProperty("A")!;
		var propertyNames = new List<string?>();
		type.GetEvent("PropertyChanged")!.AddEventHandler(
			model,
			new System.ComponentModel.PropertyChangedEventHandler((_, args) =>
			{
				propertyNames.Add(args.PropertyName);
				if (args.PropertyName == "B")
				{
					a.SetValue(model, 1);
				}
			}));

		a.SetValue(model, 1);

		CollectionAssert.AreEqual(new[] { "A", "B" }, propertyNames);
		Assert.AreEqual(1, type.GetProperty("AHookCalls")!.GetValue(model));
		Assert.AreEqual(0, type.GetProperty("BHookCalls")!.GetValue(model));
		Assert.AreEqual(0, type.GetProperty("CHookCalls")!.GetValue(model));
	}

	[TestMethod]
	public void ForcedChangedHookFeedbackDoesNotDeduplicateOrQueueNotificationsTest()
	{
		const string assemblyName = "ChangedHookForcedFeedbackFixture";
		TestRun run = Run(
			"""
			using KSoft.PropertyChanged.SourceGeneration;

			public partial class ForcedFeedbackViewModel : KSoft.ObjectModel.BasicViewModel
			{
				public int HookCalls { get; private set; }
				public int B => 0;

				[GeneratedPropertyChanged(
					AlwaysNotify = true,
					ChangedHook = GeneratedPropertyChangedHook.Parameterless,
					DependentProperties = new[] { nameof(B) })]
				public partial int A { get; set; }

				partial void OnAChanged() => HookCalls++;
			}
			""",
			assemblyName: assemblyName);
		AssertValid(run);

		Type type = Emit(run).GetType("ForcedFeedbackViewModel", throwOnError: true)!;
		object model = Activator.CreateInstance(type)!;
		PropertyInfo a = type.GetProperty("A")!;
		var propertyNames = new List<string?>();
		int bNotifications = 0;
		type.GetEvent("PropertyChanged")!.AddEventHandler(
			model,
			new System.ComponentModel.PropertyChangedEventHandler((_, args) =>
			{
				propertyNames.Add(args.PropertyName);
				if (args.PropertyName != "B")
				{
					return;
				}

				bNotifications++;
				if (bNotifications == 3)
				{
					throw new InvalidOperationException("sentinel");
				}

				a.SetValue(model, 0);
			}));

		AssertInvocationException(() => a.SetValue(model, 0), "sentinel");

		CollectionAssert.AreEqual(new[] { "A", "B", "A", "B", "A", "B" }, propertyNames);
		Assert.AreEqual(3, bNotifications);
		Assert.AreEqual(3, type.GetProperty("HookCalls")!.GetValue(model));
	}

	private static void AssertNoneMatchesOmitted(
		string omittedSource,
		string noneSource,
		bool includeHost)
	{
		TestRun omitted = Run(omittedSource, includeHost: includeHost);
		TestRun none = Run(noneSource, includeHost: includeHost);

		AssertSourceEquivalent(PropertySource(omitted), PropertySource(none));
		Assert.IsFalse(PropertySource(none).Contains("OnValueChanged", StringComparison.Ordinal));
		AssertValid(omitted);
		AssertValid(none);
	}

	private static Assembly Emit(TestRun run)
	{
		using var stream = new MemoryStream();
		var result = run.OutputCompilation.Emit(stream);
		Assert.IsTrue(
			result.Success,
			string.Join(
				Environment.NewLine,
				result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
		stream.Position = 0;
		return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
	}

	private static List<string?> TrackPropertyChanges(EventInfo propertyChanged, object model)
	{
		var propertyNames = new List<string?>();
		propertyChanged.AddEventHandler(
			model,
			new System.ComponentModel.PropertyChangedEventHandler(
				(_, args) => propertyNames.Add(args.PropertyName)));
		return propertyNames;
	}

	private static void AssertInvocationException(Action action, string message)
	{
		var exception = Assert.ThrowsExactly<TargetInvocationException>(action);
		Exception? innerException = exception.InnerException;
		while (innerException is TargetInvocationException { InnerException: Exception nestedException })
		{
			innerException = nestedException;
		}
		Assert.IsNotNull(innerException);
		Assert.IsInstanceOfType<InvalidOperationException>(innerException);
		Assert.AreEqual(message, innerException.Message);
	}
}
