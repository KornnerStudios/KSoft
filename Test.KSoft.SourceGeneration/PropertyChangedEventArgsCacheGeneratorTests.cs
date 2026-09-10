using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

public sealed partial class PropertyChangedGeneratorTests
{
	[TestMethod]
	public void CacheOnlyContractEmitsLegacyNamedFieldsWithoutPropertiesTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class CacheModel
			{
				private int mType;
				private string mValue = "";

				[GeneratedPropertyChangedEventArgs]
				public int Type
				{
					get => mType;
					set
					{
						mType = value;
						NotifyPropertyChanged(kTypeChangedEventArgs);
					}
				}

				[GeneratedPropertyChangedEventArgs]
				public string Value
				{
					get => mValue;
					set
					{
						mValue = value;
						NotifyPropertyChanged(kValueChangedEventArgs);
					}
				}

				private static void NotifyPropertyChanged(
					global::System.ComponentModel.PropertyChangedEventArgs args)
				{
				}
			}
			""");
		GeneratedSourceResult[] sources = PropertySources(run);
		Assert.HasCount(2, sources);
		string generated = string.Join(
			Environment.NewLine,
			sources.Select(static source => source.SourceText.ToString()));
		StringAssert.Contains(
			generated,
			"PropertyChangedEventArgs kTypeChangedEventArgs",
			StringComparison.Ordinal);
		StringAssert.Contains(
			generated,
			"PropertyChangedEventArgs kValueChangedEventArgs",
			StringComparison.Ordinal);
		StringAssert.Contains(generated, "new(nameof(Type));", StringComparison.Ordinal);
		StringAssert.Contains(generated, "new(nameof(Value));", StringComparison.Ordinal);
		Assert.IsFalse(generated.Contains("public int Type", StringComparison.Ordinal));
		AssertValid(run);
	}

	[TestMethod]
	public void CacheOnlyContractInvalidHostAndFieldCollisionFailClosedTest()
	{
		TestRun invalidHost = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public class InvalidCacheHost
			{
				[GeneratedPropertyChangedEventArgs]
				public int Value { get; set; }
			}
			""");
		AssertDiagnosticIds(invalidHost, "KSPC0007");

		TestRun fieldCollision = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public partial class CacheFieldCollision
			{
				private static readonly global::System.ComponentModel.PropertyChangedEventArgs kValueChangedEventArgs =
					new(nameof(Value));
				[GeneratedPropertyChangedEventArgs]
				public int Value { get; set; }
			}
			""");
		AssertDiagnosticIds(fieldCollision, "KSPC0006");
	}

	[TestMethod]
	public void CacheOnlyContractRejectsExplicitInterfacePropertyTest()
	{
		TestRun run = Run("""
			using KSoft.PropertyChanged.SourceGeneration;
			public interface IValue
			{
				int Value { get; set; }
			}

			public partial class ExplicitCacheProperty : IValue
			{
				[GeneratedPropertyChangedEventArgs]
					int IValue.Value { get; set; }
			}
			""");
		AssertDiagnosticIds(run, "KSPC0007");
	}
}
