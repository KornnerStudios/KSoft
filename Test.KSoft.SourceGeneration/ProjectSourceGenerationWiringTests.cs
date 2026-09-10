using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class ProjectSourceGenerationWiringTests
{
	private const string kAnalyzerProjectReference =
		"$(VitaRootDir)KSoft\\KSoft.SourceGeneration\\KSoft.SourceGeneration.csproj";
	private const string kPropertyChangedAnalyzerProjectReference =
		"$(VitaRootDir)KSoft\\KSoft.PropertyChanged.SourceGeneration\\KSoft.PropertyChanged.SourceGeneration.csproj";

	[TestMethod]
	public void ProjectsDoNotExposeSourceGenerationBuildSwitchTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);

			Assert.IsEmpty(ElementsNamed(document, "CompilerVisibleProperty"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "KSoftUseSourceGeneration"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "KSoftSourceGenerationEnabled"), project.DisplayName);
			Assert.IsEmpty(ElementsNamed(document, "KSoftSourceGenerationProperty"), project.DisplayName);
			Assert.IsFalse(
				document.Descendants().Any(static x => IsLegacyFeatureFlagName(x.Name.LocalName)),
				project.DisplayName);
		}
	}

	[TestMethod]
	public void ProjectsDoNotCarryRollbackCompileMetadataTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var generatedSources = ElementsNamed(document, "KSoftGeneratedSource");
			var rollbackCompileItems = ElementsNamed(document, "RollbackCompile");

			Assert.IsEmpty(generatedSources, project.DisplayName);
			Assert.IsEmpty(rollbackCompileItems, project.DisplayName);
		}
	}

	[TestMethod]
	public void AnalyzerReferencesAreUnconditionalTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var analyzerReference = ElementsNamed(document, "ProjectReference")
				.Single(x => string.Equals(
					(string?)x.Attribute("Include"),
					kAnalyzerProjectReference,
					StringComparison.Ordinal));

			Assert.IsNull(analyzerReference.Attribute("Condition"), project.DisplayName);
			Assert.AreEqual("Analyzer", analyzerReference.Attribute("OutputItemType")?.Value, project.DisplayName);
			Assert.AreEqual("False", analyzerReference.Attribute("ReferenceOutputAssembly")?.Value, project.DisplayName);
			Assert.AreEqual("all", analyzerReference.Attribute("PrivateAssets")?.Value, project.DisplayName);
		}
	}

	[TestMethod]
	public void PropertyChangedAnalyzerReferencesAreScopedAndUnconditionalTest()
	{
		foreach (ProjectFile project in PropertyChangedConsumerProjectFiles())
		{
			var document = LoadProject(project);
			var analyzerReference = ElementsNamed(document, "ProjectReference")
				.Single(x => string.Equals(
					(string?)x.Attribute("Include"),
					kPropertyChangedAnalyzerProjectReference,
					StringComparison.Ordinal));

			Assert.IsNull(analyzerReference.Attribute("Condition"), project.DisplayName);
			Assert.AreEqual("Analyzer", analyzerReference.Attribute("OutputItemType")?.Value, project.DisplayName);
			Assert.AreEqual("False", analyzerReference.Attribute("ReferenceOutputAssembly")?.Value, project.DisplayName);
			Assert.AreEqual("all", analyzerReference.Attribute("PrivateAssets")?.Value, project.DisplayName);
		}

		foreach (ProjectFile project in PropertyChangedTestProjectFiles())
		{
			var document = LoadProject(project);
			Assert.IsFalse(
				ElementsNamed(document, "ProjectReference").Any(
					x => string.Equals(
							(string?)x.Attribute("OutputItemType"),
							"Analyzer",
							StringComparison.Ordinal)
						&& ((string?)x.Attribute("Include"))?.Contains(
							"KSoft.PropertyChanged.SourceGeneration",
							StringComparison.Ordinal) == true),
				project.DisplayName);
		}
	}

	[TestMethod]
	public void ProjectsDoNotUseLegacyT4BuildMetadataTest()
	{
		foreach (ProjectFile project in ProjectFiles())
		{
			var document = LoadProject(project);
			var projectReferenceIncludes = ElementsNamed(document, "ProjectReference")
				.Select(static x => (string?)x.Attribute("Include"))
				.OfType<string>()
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

	private static IReadOnlyList<ProjectFile> PropertyChangedConsumerProjectFiles()
	{
		return new[] {
			new ProjectFile("KSoft.WPF", Path.Combine("KSoft.WPF", "KSoft.WPF.csproj")),
			new ProjectFile(
				"KSoft.Phoenix",
				Path.Combine("..", "Games", "Phoenix", "KSoft.Phoenix", "KSoft.Phoenix.csproj")),
			new ProjectFile(
				"MegHalomaniac",
				Path.Combine("..", "Games", "Blam", "MegHalomaniac", "MegHalomaniac.csproj")),
			new ProjectFile(
				"PhxGui",
				Path.Combine("..", "Games", "Phoenix", "PhxGui", "PhxGui.csproj")),
			new ProjectFile(
				"PhxStudio",
				Path.Combine("..", "Games", "PhxStudio", "PhxStudio", "PhxStudio.csproj")),
			new ProjectFile(
				"KSoft.Blam",
				Path.Combine("..", "Games", "Blam", "KSoft.Blam", "KSoft.Blam.csproj")),
			new ProjectFile(
				"Test.KSoft.BCL",
				Path.Combine("Test.KSoft.BCL", "Test.KSoft.BCL.csproj")),
		};
	}

	private static IReadOnlyList<ProjectFile> PropertyChangedTestProjectFiles()
	{
		return new[] {
			new ProjectFile(
				"Test.KSoft.SourceGeneration",
				Path.Combine("Test.KSoft.SourceGeneration", "Test.KSoft.SourceGeneration.csproj")),
			new ProjectFile("Test.KSoft.WPF", Path.Combine("Test.KSoft.WPF", "Test.KSoft.WPF.csproj")),
			new ProjectFile(
				"Test.KSoft.Phoenix",
				Path.Combine("..", "Games", "Phoenix", "Test.KSoft.Phoenix", "Test.KSoft.Phoenix.csproj")),
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
		DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
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
