using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Bitwise;

internal static class BitsRotateSourceBuilder
{
	public const string HintName = "KSoft.Bits.Rotate.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class Bits"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.BittableTypesUnsigned)
			{
				if (UsesBitOperations(typeSpec))
				{
					continue;
				}

				WriteRotateMethod(writer, typeSpec, "RotateLeft", "<<", ">>");
				WriteRotateMethod(writer, typeSpec, "RotateRight", ">>", "<<");
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	private static void WriteRotateMethod(
		SourceWriter writer,
		NumberSpec typeSpec,
		string methodName,
		string firstShiftOperator,
		string secondShiftOperator)
	{
		string bitCountConstantName = $"k{typeSpec.ConstantKeyword}BitCount";

		writer.WritePurityAnnotation();
		writer.WriteLine($"public static {typeSpec.Keyword} {methodName}({typeSpec.Keyword} x, int shift)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(shift);");
			writer.WriteLine($"ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(shift, {bitCountConstantName});");
			writer.WriteLine();

			// BitOperations only exposes 32/64-bit rotates; byte and ushort must wrap inside their own width.
			writer.WriteLine("// #VITA_KEEP: byte/ushort rotates are width-specific; BitOperations exposes 32/64-bit rotates.");
			string rotateExpression =
				$"return ({typeSpec.Keyword})( (x {firstShiftOperator} shift) " +
				$"| (x {secondShiftOperator} ({bitCountConstantName} - shift)) );";
			writer.WriteLine(rotateExpression);
		}
	}

	private static bool UsesBitOperations(NumberSpec typeSpec)
		=> typeSpec.TypeCode is TypeCode.UInt32 or TypeCode.UInt64;
};
