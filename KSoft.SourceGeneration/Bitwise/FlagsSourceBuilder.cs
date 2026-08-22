using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static class FlagsSourceBuilder
{
	public const string HintName = "KSoft.Bitwise.Flags.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Bitwise");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Flags"))
		{
			// FlagsT4 is small, but still generator-backed so unsigned and major-word overload families stay uniform.
			WriteTestRegion(writer);
			writer.WriteLine();
			WriteBinaryWordOperationRegion(
				writer,
				new WordOperationSpec(
					"Add",
					"Adds <paramref name=\"rhs\"/> to <paramref name=\"lhs\"/>",
					"Existing bit-vector reference",
					"Other bit-vector whose bits we wish to add to <paramref name=\"lhs\"/>",
					"<paramref name=\"lhs\"/> != <paramref name=\"rhs\"/>",
					"lhs | rhs",
					"lhs |= rhs;"));
			writer.WriteLine();
			WriteBinaryWordOperationRegion(
				writer,
				new WordOperationSpec(
					"Remove",
					"Removes <paramref name=\"rhs\"/> from <paramref name=\"lhs\"/>",
					"Existing bit-vector",
					"Other bit-vector whose bits we wish to remove from <paramref name=\"lhs\"/>",
					"<paramref name=\"lhs\"/> AND-EQUALS ~<paramref name=\"rhs\"/>",
					"lhs & ~rhs",
					"lhs &= ~rhs;"));
			writer.WriteLine();
			WriteBinaryWordOperationRegion(
				writer,
				new WordOperationSpec(
					"Toggle",
					"Complements <paramref name=\"rhs\"/> bits in <paramref name=\"lhs\"/>",
					"Existing bit-vector",
					"Other bit-vector whose bits we wish to complement in <paramref name=\"lhs\"/>",
					"<paramref name=\"lhs\"/> XOR-EQUALS <paramref name=\"rhs\"/>",
					"lhs ^ rhs",
					"lhs ^= rhs;"));
			writer.WriteLine();
			WriteModifyRegion(writer);
		}

		return writer.ToString();
	}

	private static void WriteTestRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Test"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				using (writer.EnterRegion($"{spec.SizeOfInBits}-bit"))
				{
					WriteTestMethod(writer, spec.Keyword);
					WriteTestMethod(writer, spec.SignedKeyword);
					WriteTestAnyMethod(writer, spec.Keyword);
					writer.WriteLine();
					WriteTestParamsMethod(writer, spec.Keyword);
					writer.WriteLine();
					WriteTestAnyParamsMethod(writer, spec.Keyword);
				}
			}
		}
	}

	private static void WriteTestMethod(SourceWriter writer, string keyword)
	{
		WriteTestFlagDocs(
			writer,
			"Returns true if <paramref name=\"flag\"/> is active in <paramref name=\"value\"/>",
			"(<paramref name=\"value\"/> &amp; <paramref name=\"flag\"/>) == <paramref name=\"flag\"/>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static bool Test({keyword} value, {keyword} flag)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return (value & flag) == flag;");
		}
	}

	private static void WriteTestAnyMethod(SourceWriter writer, string keyword)
	{
		WriteTestFlagDocs(
			writer,
			"Returns true if any bits in <paramref name=\"flag\"/> are active in <paramref name=\"value\"/>",
			"(<paramref name=\"value\"/> &amp; <paramref name=\"flag\"/>) != 0");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static bool TestAny({keyword} value, {keyword} flag)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return (value & flag) != 0;");
		}
	}

	private static void WriteTestParamsMethod(SourceWriter writer, string keyword)
	{
		WriteTestParamsDocs(
			writer,
			"Returns true if all the flags in <paramref name=\"flags\"/> are active in <paramref name=\"value\"/>",
			"Returns true if ALL the flag values in <paramref name=\"flags\"/> are set in <paramref name=\"value\"/>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static bool Test({keyword} value, params {keyword}[] flags)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(flags);");
			writer.WriteLine();
			WriteForeachFlagLoop(writer, "if (!Test(value, i))", "return false;");
			writer.WriteLine();
			writer.WriteLine("return true;");
		}
	}

	private static void WriteTestAnyParamsMethod(SourceWriter writer, string keyword)
	{
		writer.WriteXmlDocSummary(
			"Returns true if any one of the flags in <paramref name=\"flags\"/> are active in " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "");
		writer.WriteXmlDocParam("flags", "");
		writer.WriteXmlDocReturns(
			"Returns true if any (one, some, or all) flag values in <paramref name=\"flags\"/> are set in " +
			"<paramref name=\"value\"/>");
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static bool TestAny({keyword} value, params {keyword}[] flags)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(flags);");
			writer.WriteLine();
			WriteForeachFlagLoop(writer, "if (Test(value, i))", "return true;");
			writer.WriteLine();
			writer.WriteLine("return false;");
		}
	}

	private static void WriteBinaryWordOperationRegion(SourceWriter writer, WordOperationSpec operation)
	{
		using (writer.EnterRegion(operation.MethodName))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesMajorWords)
			{
				string keyword = spec.Keyword;
				writer.WriteXmlDocSummary(operation.Summary);
				writer.WriteXmlDocParam("lhs", "Existing bit-vector");
				writer.WriteXmlDocParam("rhs", operation.RhsDescription);
				writer.WriteXmlDocReturns(operation.Returns);
				writer.WriteLine($"public static {keyword} {operation.MethodName}({keyword} lhs, {keyword} rhs)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine($"return {operation.ReturnExpression};");
				}
				writer.WriteXmlDocSummary(operation.Summary);
				writer.WriteXmlDocParam("lhs", operation.RefLhsDescription);
				writer.WriteXmlDocParam("rhs", operation.RhsDescription);
				writer.WriteLine($"public static void {operation.MethodName}(ref {keyword} lhs, {keyword} rhs)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(operation.RefStatement);
				}
				writer.WriteLine();
			}
		}
	}

	private static void WriteModifyRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Modify"))
		{
			foreach (NumberSpec spec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				string keyword = spec.Keyword;
				using (writer.EnterRegion($"{spec.SizeOfInBits}-bit"))
				{
					WriteModifyReturnValueMethod(writer, keyword);
					WriteModifyRefMethod(writer, keyword);
				}
			}
		}
	}

	private static void WriteModifyReturnValueMethod(SourceWriter writer, string keyword)
	{
		WriteModifyDocs(writer, includeReturnBlock: true);
		writer.WriteLine($"public static {keyword} Modify(bool addOrRemove, {keyword} lhs, {keyword} rhs)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return (addOrRemove == true ?");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("lhs |= rhs :");
				writer.WriteLine($"lhs &= ({keyword})~rhs);");
			}
		}
	}

	private static void WriteModifyRefMethod(SourceWriter writer, string keyword)
	{
		WriteModifyDocs(writer, includeReturnBlock: false);
		writer.WriteLine($"public static bool Modify(bool addOrRemove, ref {keyword} lhs, {keyword} rhs)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (addOrRemove == true)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("lhs |= rhs;");
			}
			writer.WriteLine("else");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"lhs &= ({keyword})~rhs;");
			}
			writer.WriteLine();
			writer.WriteLine("return addOrRemove;");
		}
	}

	private static void WriteTestFlagDocs(SourceWriter writer, string summary, string returns)
	{
		writer.WriteXmlDocSummary(summary);
		writer.WriteXmlDocParam("value", "Value to test in");
		writer.WriteXmlDocParam("flag", "Value to test for");
		writer.WriteXmlDocReturns(returns);
	}

	private static void WriteTestParamsDocs(SourceWriter writer, string summary, string returns)
	{
		writer.WriteXmlDocSummary(summary);
		writer.WriteXmlDocParam("value", "Value to test in");
		writer.WriteXmlDocParam("flags", "Values to test for");
		writer.WriteXmlDocReturns(returns);
	}

	private static void WriteModifyDocs(SourceWriter writer, bool includeReturnBlock)
	{
		writer.WriteXmlDocSummary("Modify <paramref name=\"lhs\"/> with <paramref name=\"rhs\"/>");
		writer.WriteXmlDocParam("addOrRemove", "True to add <paramref name=\"rhs\"/>, false to remove");
		writer.WriteXmlDocParam("lhs", "Existing bit-vector");
		writer.WriteXmlDocParam("rhs", "Other bit-vector whose bits we wish to modify on <paramref name=\"lhs\"/>");
		if (includeReturnBlock)
		{
			writer.WriteLine("/// <returns>");
			writer.WriteLine("/// If <paramref name=\"addOrRemove\"/> is True:");
			writer.WriteLine("/// <paramref name=\"lhs\"/> |= <paramref name=\"rhs\"/>");
			writer.WriteLine("///");
			writer.WriteLine("/// Else:");
			writer.WriteLine("/// <paramref name=\"lhs\"/> &amp;= <paramref name=\"rhs\"/>");
			writer.WriteLine("/// </returns>");
		}
		else
		{
			writer.WriteXmlDocReturns("<paramref name=\"addOrRemove\"/>");
		}
	}

	private static void WriteForeachFlagLoop(SourceWriter writer, string condition, string statement)
	{
		writer.WriteLine("foreach (var i in flags)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine(condition);
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(statement);
			}
		}
	}

	private readonly struct WordOperationSpec
	{
		public WordOperationSpec(
			string methodName,
			string summary,
			string refLhsDescription,
			string rhsDescription,
			string returns,
			string returnExpression,
			string refStatement)
		{
			MethodName = methodName;
			Summary = summary;
			RefLhsDescription = refLhsDescription;
			RhsDescription = rhsDescription;
			Returns = returns;
			ReturnExpression = returnExpression;
			RefStatement = refStatement;
		}

		public string MethodName { get; }

		public string Summary { get; }

		public string RefLhsDescription { get; }

		public string RhsDescription { get; }

		public string Returns { get; }

		public string ReturnExpression { get; }

		public string RefStatement { get; }
	}
}
