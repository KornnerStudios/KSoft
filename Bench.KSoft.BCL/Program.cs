using System;
using System.IO;
using BenchmarkDotNet.Running;

string vitaRootDir = FindVitaRootDir();

// BenchmarkDotNet builds generated projects under _bin. Without these process-scoped values, those generated projects
// import Vita's root Directory.Build.props without the KSoft submodule props that normally initialize VitaRootDir.
Environment.SetEnvironmentVariable("VitaRootDir", vitaRootDir);
Environment.SetEnvironmentVariable("VitaSolutionFamily", "KSoft.BCL");
Environment.SetEnvironmentVariable("VitaUseSolutionFamilyDirs", "true");
Environment.SetEnvironmentVariable("Configuration", "Release");

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

static string FindVitaRootDir()
{
	string currentRoot = TryFindVitaRootDir(Directory.GetCurrentDirectory());
	if (currentRoot != null)
	{
		return currentRoot;
	}

	string outputRoot = TryFindVitaRootDir(AppContext.BaseDirectory);
	if (outputRoot != null)
	{
		return outputRoot;
	}

	throw new InvalidOperationException("Unable to locate Vita.sln for benchmark MSBuild configuration.");
}

static string TryFindVitaRootDir(string startDirectory)
{
	DirectoryInfo directory = new DirectoryInfo(startDirectory);
	while (directory != null)
	{
		if (File.Exists(Path.Combine(directory.FullName, "Vita.sln")) &&
			Directory.Exists(Path.Combine(directory.FullName, "KSoft", "shared")))
		{
			return Path.EndsInDirectorySeparator(directory.FullName)
				? directory.FullName
				: directory.FullName + Path.DirectorySeparatorChar;
		}

		directory = directory.Parent;
	}

	return null;
}
