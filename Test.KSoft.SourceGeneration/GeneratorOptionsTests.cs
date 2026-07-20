using System.Collections.Generic;
using KSoft.SourceGeneration.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class GeneratorOptionsTests
{
	[TestMethod]
	public void UseSourceGenerationPropertyNameMatchesExpectedMSBuildNameTest()
	{
		Assert.AreEqual("KSoftUseSourceGeneration", GeneratorOptions.UseSourceGenerationProperty);
		Assert.AreEqual("build_property.KSoftUseSourceGeneration", GeneratorOptions.UseSourceGenerationBuildProperty);
	}

	[TestMethod]
	public void MissingPropertyDefaultsToDisabledTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>()));

		Assert.IsFalse(options.UseSourceGeneration);
		Assert.IsFalse(options.HasInvalidBooleanProperties);
	}

	[TestMethod]
	public void CompilerVisibleBooleanPropertyIsParsedTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "True",
		}));

		Assert.IsTrue(options.UseSourceGeneration);
		Assert.IsFalse(options.HasInvalidBooleanProperties);
	}

	[TestMethod]
	public void FalseCompilerVisibleBooleanPropertyIsParsedTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "false",
		}));

		Assert.IsFalse(options.UseSourceGeneration);
		Assert.IsFalse(options.HasInvalidBooleanProperties);
	}

	[TestMethod]
	public void InvalidBooleanPropertyIsRecordedTest()
	{
		var options = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "yes",
		}));

		Assert.IsFalse(options.UseSourceGeneration);
		Assert.IsTrue(options.HasInvalidBooleanProperties);
		Assert.AreEqual(GeneratorOptions.UseSourceGenerationProperty, options.InvalidBooleanProperties[0]);
	}

	[TestMethod]
	public void EquivalentOptionsAreEqualTest()
	{
		var left = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "true",
		}));
		var right = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "true",
		}));

		Assert.AreEqual(left, right);
		Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
	}

	[TestMethod]
	public void DifferentOptionsAreNotEqualTest()
	{
		var left = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "true",
		}));
		var right = GeneratorOptions.From(new AnalyzerConfigOptionsStub(new Dictionary<string, string>
		{
			[GeneratorOptions.UseSourceGenerationBuildProperty] = "false",
		}));

		Assert.AreNotEqual(left, right);
	}
};
