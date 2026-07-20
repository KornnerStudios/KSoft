namespace KSoft.SourceGeneration;

internal enum GeneratorTargetAssembly
{
	Unsupported,
	KSoft,
	KSoftIOTagElementStreams,
};

internal static class GeneratorTargetAssemblyFacts
{
	public const string KSoftAssemblyName = "KSoft";
	public const string KSoftIOTagElementStreamsAssemblyName = "KSoft.IO.TagElementStreams";
	public const string ExpectedAssemblyNames = KSoftAssemblyName + ", " + KSoftIOTagElementStreamsAssemblyName;

	public static GeneratorTargetAssembly FromAssemblyName(string assemblyName)
	{
		return assemblyName switch
		{
			KSoftAssemblyName => GeneratorTargetAssembly.KSoft,
			KSoftIOTagElementStreamsAssemblyName => GeneratorTargetAssembly.KSoftIOTagElementStreams,
			_ => GeneratorTargetAssembly.Unsupported,
		};
	}
};
