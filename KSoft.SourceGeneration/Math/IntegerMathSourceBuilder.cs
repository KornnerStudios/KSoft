using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Math;

internal static class IntegerMathSourceBuilder
{
	public const string HintName = "KSoft.IntegerMath.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class IntegerMath"))
		{
			WriteAlignRegion(writer);
			WritePaddingRequiredRegion(writer);
			WriteIsSignedRegion(writer);
			WriteSetSignBitRegion(writer);
		}

		return writer.ToString();
	}

	private static void WriteAlignRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Align"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesInt32And64)
			{
				WriteAlignMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}

		writer.WriteLine();
	}

	private static void WritePaddingRequiredRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("PaddingRequired"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesInt32And64)
			{
				WritePaddingRequiredMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}

		writer.WriteLine();
	}

	private static void WriteIsSignedRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("IsSigned"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteIsSignedMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}

		writer.WriteLine();
	}

	private static void WriteSetSignBitRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("SetSignBit"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				WriteSetSignBitMethod(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteAlignMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteAlignXmlDocs(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} Align(int alignmentBit, {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteCommonPreconditions(writer, typeSpec);
			writer.WriteLine($"{typeSpec.Keyword} align_size = 1{typeSpec.LiteralSuffix} << alignmentBit;");
			writer.WriteLine();
			writer.WriteLine("return (value + (align_size-1)) & ~(align_size-1);");
		}
	}

	private static void WritePaddingRequiredMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		WritePaddingRequiredXmlDocs(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static int PaddingRequired(int alignmentBit, {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteCommonPreconditions(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine("return (int)(Align(alignmentBit, value) - value);");
		}
	}

	private static void WriteIsSignedMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteIsSignedXmlDocs(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static bool IsSigned({typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"return ((value >> {typeSpec.MostSignificantByteBitShift}) & 0x80) != 0;");
		}
	}

	private static void WriteSetSignBitMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteSetSignBitXmlDocs(writer);
		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} SetSignBit({typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			string expression =
				$"value | (0x80{typeSpec.LiteralSuffix} << {typeSpec.MostSignificantByteBitShift})";
			if (typeSpec.BitOperatorsImplicitlyUpCast)
			{
				writer.WriteLine($"return ({typeSpec.Keyword})({expression});");
			}
			else
			{
				writer.WriteLine($"return {expression};");
			}
		}
	}

	private static void WriteCommonPreconditions(SourceWriter writer, NumberSpec typeSpec)
	{
		// The old T4 emitted typed contract preconditions. Generated KSoft code can use modern throw helpers directly
		// while preserving the same exception type for invalid inputs.
		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfGreaterThan(alignmentBit, kMaxAlignmentBit);");
		if (typeSpec.IsSigned)
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(value);");
		}
	}

	private static void WriteAlignXmlDocs(SourceWriter writer)
	{
		writer.WriteXmlDocSummary(
			"Takes <paramref name=\"value\"/> and returns what it would be if it were aligned to",
			"<paramref name=\"align_size\"/> bytes");
		writer.WriteXmlDocParam("alignmentBit", "Alignment size in log2 form");
		writer.WriteXmlDocParam("value", "Value to align");
		writer.WriteXmlDocReturns(
			"<paramref name=\"value\"/> aligned to the next <paramref name=\"alignmentBit\"/> boundary,",
			"if it isn't already");
	}

	private static void WritePaddingRequiredXmlDocs(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Calculate the number of padding bytes, if any, needed to align a value");
		writer.WriteXmlDocParam("alignmentBit", "Alignment size in log2 form");
		writer.WriteXmlDocParam("value", "Value to align");
		writer.WriteXmlDocReturns(
			"Bytes needed to align <paramref name=\"value\"/> to the next <paramref name=\"alignmentBit\"/> boundary,",
			"or zero if it is already aligned");
	}

	private static void WriteIsSignedXmlDocs(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Tests to see if the given value's sign-bit is on");
		writer.WriteXmlDocParam("value", "Value to test");
		writer.WriteXmlDocReturns("True if the sign-bit is set");
	}

	private static void WriteSetSignBitXmlDocs(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Set the sign-bit in the value given");
		writer.WriteXmlDocParam("value", "Value to return with its sign-bit set");
		writer.WriteXmlDocReturns("<paramref name=\"value\"/> with its sign-bit set");
	}
};
