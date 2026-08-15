using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KSoft.SourceGeneration.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class ProjectSourceGenerationWiringTests
{
	private const string kSingleSwitchCondition = " '$(KSoftUseSourceGeneration)' == 'true' ";
	private const string kAnalyzerProjectReference =
		"$(VitaRootDir)KSoft\\KSoft.SourceGeneration\\KSoft.SourceGeneration.csproj";

	[TestMethod]
	public void ProjectsExposeOnlySingleSourceGenerationBuildPropertyTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var compilerVisibleProperties = ElementsNamed(document, "CompilerVisibleProperty")
				.Select(static x => (string)x.Attribute("Include"))
				.ToArray();

			CollectionAssert.AreEqual(
				new[] {
					GeneratorOptions.UseSourceGenerationProperty,
				},
				compilerVisibleProperties,
				project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "KSoftSourceGenerationProperty"), project.DisplayName);
			Assert.IsFalse(
				document.Descendants().Any(static x => IsLegacyFeatureFlagName(x.Name.LocalName)),
				project.DisplayName);
		}
	}

	[TestMethod]
	public void GeneratedSourceItemsUseSingleSourceGenerationSwitchTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var generatedSources = ElementsNamed(document, "KSoftGeneratedSource").ToArray();
			var rollbackCompileItems = generatedSources
				.Select(static x => x.Element("RollbackCompile")?.Value)
				.ToArray();

			CollectionAssert.AllItemsAreUnique(rollbackCompileItems, project.DisplayName);
			foreach (XElement generatedSource in generatedSources)
			{
				Assert.AreEqual(kSingleSwitchCondition, (string)generatedSource.Attribute("Condition"), project.DisplayName);
				XElement rollbackCompile = generatedSource.Element("RollbackCompile");
				if (rollbackCompile != null)
				{
					Assert.IsFalse(string.IsNullOrWhiteSpace(rollbackCompile.Value));
				}
			}
		}
	}

	[TestMethod]
	public void AnalyzerReferencesStayConditionedOnSingleSourceGenerationSwitchTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var analyzerReference = ElementsNamed(document, "ProjectReference")
				.Single(x => string.Equals((string)x.Attribute("Include"), kAnalyzerProjectReference, StringComparison.Ordinal));

			Assert.AreEqual(kSingleSwitchCondition, (string)analyzerReference.Attribute("Condition"), project.DisplayName);
			Assert.AreEqual("Analyzer", analyzerReference.Attribute("OutputItemType")?.Value, project.DisplayName);
			Assert.AreEqual("False", analyzerReference.Attribute("ReferenceOutputAssembly")?.Value, project.DisplayName);
			Assert.AreEqual("all", analyzerReference.Attribute("PrivateAssets")?.Value, project.DisplayName);
		}
	}

	[TestMethod]
	public void ProjectsDoNotUseLegacyT4BuildMetadataTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var projectReferenceIncludes = ElementsNamed(document, "ProjectReference")
				.Select(static x => (string)x.Attribute("Include"))
				.ToArray();

			Assert.IsFalse(
				projectReferenceIncludes.Any(static x => x.Contains("KSoft.T4", StringComparison.Ordinal)),
				project.DisplayName);
			Assert.IsFalse(
				ElementsNamed(document, "Generator")
					.Any(static x => string.Equals(x.Value, "TextTemplatingFileGenerator", StringComparison.Ordinal)),
				project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "LastGenOutput"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "AutoGen"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "DesignTime"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "DependentUpon"), project.DisplayName);
		}
	}

	private static IReadOnlyList<ProjectFile> ProjectFiles()
	{
		return new[] {
			new ProjectFile("KSoft", Path.Combine("KSoft", "KSoft.csproj")),
			new ProjectFile(
				"KSoft.IO.TagElementStreams",
				Path.Combine("KSoft.IO.TagElementStreams", "KSoft.IO.TagElementStreams.csproj")),
		};
	}

	private static XDocument LoadProject(ProjectFile project)
		=> XDocument.Load(Path.Combine(FindKSoftRoot(), project.RelativePath));

	private static IEnumerable<XElement> ElementsNamed(XDocument document, string localName)
		=> document.Descendants().Where(x => string.Equals(x.Name.LocalName, localName, StringComparison.Ordinal));

	private static bool IsLegacyFeatureFlagName(string name)
		=> name.StartsWith("KSoftGenerate", StringComparison.Ordinal)
			&& !string.Equals(name, "KSoftGeneratedSource", StringComparison.Ordinal)
			&& !string.Equals(name, "KSoftGeneratedSourceItems", StringComparison.Ordinal);

	private static string FindKSoftRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory != null)
		{
			string directProject = Path.Combine(
				directory.FullName,
				"KSoft.SourceGeneration",
				"KSoft.SourceGeneration.csproj");
			if (File.Exists(directProject))
			{
				return directory.FullName;
			}

			string nestedProject = Path.Combine(
				directory.FullName,
				"KSoft",
				"KSoft.SourceGeneration",
				"KSoft.SourceGeneration.csproj");
			if (File.Exists(nestedProject))
			{
				return Path.Combine(directory.FullName, "KSoft");
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException("Could not locate the KSoft source-generation project.");
	}

	private readonly struct ProjectFile
	{
		public ProjectFile(string displayName, string relativePath)
		{
			DisplayName = displayName;
			RelativePath = relativePath;
		}

		public string DisplayName { get; }

		public string RelativePath { get; }
	}
}
