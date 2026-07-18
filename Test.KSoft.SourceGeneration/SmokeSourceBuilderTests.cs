using KSoft.SourceGeneration.SmokeTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class SmokeSourceBuilderTests
{
	[TestMethod]
	public void SmokeSourceIsDeterministicTest()
	{
		string source = SmokeSourceBuilder.Build();

		Assert.Contains("public const int NumberCount = 10;", source);
		Assert.Contains("public const int PrimitiveCount = 12;", source);
		Assert.Contains("public const int BittableUnsignedCount = 4;", source);
		Assert.Contains("public const int BittableMajorWordCount = 2;", source);
		Assert.Contains("public const string BittableUnsignedKeywords = \"byte,ushort,uint,ulong\";", source);
	}
};
