using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void ExplicitCachedEventArgsHostTakesPrecedenceOverInferredProvidersTest()
	{
		TestRun basicRun = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class ExplicitBasicViewModel : KSoft.ObjectModel.BasicViewModel
			{
				protected void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""");
		string basicSource = PropertySource(basicRun);
		StringAssert.Contains(basicSource, "this.NotifyPropertyChanged(kValueChangedEventArgs);", StringComparison.Ordinal);
		Assert.IsFalse(basicSource.Contains("base.SetField", StringComparison.Ordinal));
		Assert.IsFalse(basicSource.Contains("class __PropertyChangedEventArgs", StringComparison.Ordinal));
		AssertValid(basicRun);

		TestRun caliburnRun = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(PublishChange))]
			public partial class ExplicitCaliburnViewModel : Caliburn.Micro.PropertyChangedBase
			{
				protected void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial string Value { get; set; }
			}
			""");
		string caliburnSource = PropertySource(caliburnRun);
		StringAssert.Contains(caliburnSource, "this.PublishChange(kValueChangedEventArgs);", StringComparison.Ordinal);
		Assert.IsFalse(caliburnSource.Contains("NotifyOfPropertyChange", StringComparison.Ordinal));
		AssertValid(caliburnRun);
	}

	[TestMethod]
	public void NearestCachedEventArgsHostMarkerWinsTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyBase))]
			public partial class MarkedBase
			{
				protected void NotifyBase(global::System.ComponentModel.PropertyChangedEventArgs args) { }
			}

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyIntermediate))]
			public partial class MarkedIntermediate : MarkedBase
			{
				protected void NotifyIntermediate(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			public partial class NearestMarkerHost : MarkedIntermediate
			{
				[GeneratedPropertyChanged]
					public partial string Value { get; set; }
			}
			""",
			includeHost: false);
		string generated = PropertySource(run);
		StringAssert.Contains(generated, "this.NotifyIntermediate(kValueChangedEventArgs);", StringComparison.Ordinal);
		Assert.IsFalse(generated.Contains("this.NotifyBase(", StringComparison.Ordinal));
		AssertValid(run);
	}

	[TestMethod]
	public void CachedEventArgsHostUsesExactProtectedInheritedNotifierTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(PublishChange))]
			public partial class MarkedNotifierBase
			{
				protected void PublishChange(string propertyName) { }
				protected void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			public partial class ProtectedNotifierHost : MarkedNotifierBase
			{
				[GeneratedPropertyChanged]
					public partial string Value { get; set; }
			}
			""",
			includeHost: false);
		StringAssert.Contains(
			PropertySource(run),
			"this.PublishChange(kValueChangedEventArgs);",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void CachedEventArgsHostRejectsInaccessibleAndAmbiguousNotifiersTest()
	{
		TestRun inaccessible = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public class PrivateNotifierBase
			{
				private void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = "PublishChange")]
			public partial class InaccessibleNotifierHost : PrivateNotifierBase
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertDiagnosticIds(inaccessible, "KSPC0007");

		TestRun ambiguous = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			public class FirstNotifier
			{
				protected void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(PublishChange))]
			public partial class AmbiguousNotifierHost : FirstNotifier
			{
				protected new void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertDiagnosticIds(ambiguous, "KSPC0007");
	}

	[TestMethod]
	public void CachedEventArgsHostIgnoresInaccessibleIntermediateMemberTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(PublishChange))]
			public class MarkedNotifierBase
			{
				protected void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			public class PrivateIntermediate : MarkedNotifierBase
			{
				private void PublishChange(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			public partial class DerivedNotifierHost : PrivateIntermediate
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		StringAssert.Contains(
			PropertySource(run),
			"this.PublishChange(kValueChangedEventArgs);",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void CachedEventArgsHostRejectsWrongNotifierSignaturesTest()
	{
		string[] invalidMethods = {
			"public static void PublishChange(global::System.ComponentModel.PropertyChangedEventArgs args) { }",
			"public void PublishChange<T>(global::System.ComponentModel.PropertyChangedEventArgs args) { }",
			"public void PublishChange(ref global::System.ComponentModel.PropertyChangedEventArgs args) { }",
			"public bool PublishChange(global::System.ComponentModel.PropertyChangedEventArgs args) => true;",
			"public void PublishChange(string propertyName) { }",
		};

		foreach (string invalidMethod in invalidMethods)
		{
			TestRun run = Run($$"""
				using KSoft.PropertyChanged.SourceGeneration;

				[GeneratedPropertyChangedHost(
					GeneratedPropertyChangedHostProvider.CachedEventArgs,
					NotificationMethod = "PublishChange")]
				public partial class InvalidNotifierHost
				{
					{{invalidMethod}}

					[GeneratedPropertyChanged]
						public partial int Value { get; set; }
				}
				""",
				includeHost: false);
			AssertDiagnosticIds(run, "KSPC0007");
		}
	}

	[TestMethod]
	public void CachedEventArgsHostEmitsReusableLegacyFieldsTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class LegacyFieldHost
			{
				public global::System.ComponentModel.PropertyChangedEventArgs ValueEventArgs =>
					kValueChangedEventArgs;

				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial string Value { get; set; }
			}
			""",
			includeHost: false);
		StringAssert.Contains(
			PropertySource(run),
			"private static readonly global::System.ComponentModel.PropertyChangedEventArgs kValueChangedEventArgs =",
			StringComparison.Ordinal);
		AssertValid(run);
	}

	[TestMethod]
	public void CachedEventArgsHostChecksEveryLegacyFieldCollisionTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class LegacyFieldCollisionHost
			{
				private static readonly global::System.ComponentModel.PropertyChangedEventArgs kZetaChangedEventArgs =
					new(nameof(Zeta));

				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial int Alpha { get; set; }

				[GeneratedPropertyChanged]
					public partial int Zeta { get; set; }
			}
			""",
			includeHost: false);
		AssertDiagnosticIds(run, "KSPC0006");
	}

	[TestMethod]
	public void CachedEventArgsHostChecksAccessibleInheritedFieldCollisionTest()
	{
		TestRun collision = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public class CollisionBase
			{
				protected static readonly global::System.ComponentModel.PropertyChangedEventArgs
					kValueChangedEventArgs = new(nameof(Value));

				protected void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				protected int Value => 0;
			}

			public partial class CollisionHost : CollisionBase
			{
				[GeneratedPropertyChanged]
					public new partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertDiagnosticIds(collision, "KSPC0006");

		TestRun privateBaseMember = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public class PrivateMemberBase
			{
				private static readonly global::System.ComponentModel.PropertyChangedEventArgs
					kValueChangedEventArgs = new("PrivateValue");

				protected void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}

			public partial class PrivateMemberHost : PrivateMemberBase
			{
				[GeneratedPropertyChanged]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertValid(privateBaseMember);
	}

	[TestMethod]
	public void ConflictingPropertyAttributesReportKspc0008WithoutDuplicateMembersTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class ConflictingAttributesHost
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
				[GeneratedPropertyChangedEventArgs]
					public partial int Value { get; set; }
			}
			""",
			includeHost: false);
		AssertDiagnosticIds(run, "KSPC0008");
		Assert.HasCount(1, PropertySources(run));
		Assert.IsFalse(run.OutputCompilation.GetDiagnostics().Any(
			static diagnostic => diagnostic.Id == "CS0102"));
	}

	[TestMethod]
	public void CachedEventArgsHostPreservesEqualityAndAlwaysNotifyOrderingTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class AssignmentOrderHost
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial int Checked { get; set; }

				[GeneratedPropertyChanged(AlwaysNotify = true)]
					public partial int Forced { get; set; }
			}
			""",
			includeHost: false);
		AssertSetterEquivalent(
			"""
			{
				if (__PropertyChangedValuesEqual(ref field, value))
				{
					return;
				}

				field = value;
				this.NotifyPropertyChanged(kCheckedChangedEventArgs);
			}
			""",
			run,
			"Checked");
		AssertSetterEquivalent(
			"""
			{
				field = value;
				this.NotifyPropertyChanged(kForcedChangedEventArgs);
			}
			""",
			run,
			"Forced");
		AssertValid(run);
	}

	[TestMethod]
	public void CachedEventArgsHostHasNoBlamDependencyTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;

			[GeneratedPropertyChangedHost(
				GeneratedPropertyChangedHostProvider.CachedEventArgs,
				NotificationMethod = nameof(NotifyPropertyChanged))]
			public partial class StandaloneHost
			{
				private void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}

				[GeneratedPropertyChanged]
					public partial string Value { get; set; }
			}
			""",
			includeHost: false,
			includeCaliburnReference: false);
		Assert.IsFalse(GeneratedOutputs(run).Any(
			static source => source.Contains("Blam", StringComparison.Ordinal)));
		Assert.IsFalse(typeof(global::KSoft.PropertyChanged.SourceGeneration.PropertyChangedGenerator).Assembly
			.GetReferencedAssemblies()
			.Any(static assembly => assembly.Name?.Contains("Blam", StringComparison.Ordinal) == true));
		AssertValid(run);
	}
}
