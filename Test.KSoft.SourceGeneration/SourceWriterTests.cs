using KSoft.SourceGeneration.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

[TestClass]
public sealed class SourceWriterTests
{
	[TestMethod]
	public void BraceBlocksManageIndentationTest()
	{
		var writer = new SourceWriter();

		writer.WriteLine("namespace Example");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("internal static class Type");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("public const int Value = 1;");
			}
		}

		Assert.AreEqual(
			string.Join(SourceWriter.NewLine, [
				"namespace Example",
				"{",
				"\tinternal static class Type",
				"\t{",
				"\t\tpublic const int Value = 1;",
				"\t}",
				"}",
				"",
			]),
			writer.ToString());
	}

	[TestMethod]
	public void BracesStatementWritesTrailingSemicolonTest()
	{
		var writer = new SourceWriter();

		writer.WriteLine("internal sealed class Type");
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine("public const int Value = 1;");
		}

		Assert.AreEqual(
			string.Join(SourceWriter.NewLine, [
				"internal sealed class Type",
				"{",
				"\tpublic const int Value = 1;",
				"};",
				"",
			]),
			writer.ToString());
	}
};
