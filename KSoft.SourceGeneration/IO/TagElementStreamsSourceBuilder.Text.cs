using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	private static void WriteTagElementTextStream(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class TagElementTextStream<TDoc, TCursor>"))
		{
			WriteTextReadElementImplRegion(writer);
			writer.WriteLine();
			WriteTextReadAttributeRegion(writer);
			writer.WriteLine();
			WriteTextReadElementOptRegion(writer);
			writer.WriteLine();
			WriteTextReadAttributeOptRegion(writer);
			writer.WriteLine();
			WriteTextWriteElementImplRegion(writer);
			writer.WriteLine();
			WriteTextWriteAttributeRegion(writer);
		}
	}

	private static void WriteTextReadElementImplRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElement impl"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				string keyword = typeSpec.Keyword;
				writer.WriteLine($"protected override void ReadElement(TCursor n, ref {keyword} value)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					if (IsString(typeSpec))
					{
						writer.WriteLine("value = GetInnerText(n) ?? string.Empty;");
					}
					else
					{
						writer.WriteLine("TagElementTextStreamUtils.ParseString(GetInnerText(n), ref value, kThrowExcept,");
						writer.WriteLine("\tmReadErrorState);");
					}
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"protected override void ReadElement(TCursor n, ref {typeSpec.Keyword} value, NumeralBase fromBase)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						"Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, mReadErrorState, " +
						"fromBase);");
				}
			}
		}
	}

	private static void WriteTextReadAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				string keyword = typeSpec.Keyword;
				writer.WriteLine($"public override void ReadAttribute(string name, ref {keyword} value)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					if (IsString(typeSpec))
					{
						writer.WriteLine("value = ReadAttribute(name);");
					}
					else
					{
						writer.WriteLine("TagElementTextStreamUtils.ParseString(ReadAttribute(name), ref value, kThrowExcept,");
						writer.WriteLine("\tmReadErrorState);");
					}
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"public override void ReadAttribute(string name, ref {typeSpec.Keyword} value, " +
					"NumeralBase fromBase = kDefaultRadix)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						"Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, mReadErrorState, " +
						"fromBase);");
				}
			}
		}
	}

	private static void WriteTextReadElementOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElementOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				string keyword = OptionalReadKeyword(typeSpec);
				string valueParameter = IsString(typeSpec)
					? "[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] ref string? value"
					: $"ref {keyword} value";
				writer.WriteLine($"public override bool ReadElementOpt(string name, {valueParameter})");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					if (IsString(typeSpec))
					{
						writer.WriteLine("return (value = ReadElementOpt(name)) != null;");
					}
					else
					{
						writer.WriteLine("return TagElementTextStreamUtils.ParseString(ReadElementOpt(name), ref value, kNoExcept,");
						writer.WriteLine("\tmReadErrorState);");
					}
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"public override bool ReadElementOpt(string name, ref {typeSpec.Keyword} value, " +
					"NumeralBase fromBase = kDefaultRadix)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						"return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, mReadErrorState, " +
						"fromBase);");
				}
			}
		}
	}

	private static void WriteTextReadAttributeOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttributeOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				string keyword = OptionalReadKeyword(typeSpec);
				string valueParameter = IsString(typeSpec)
					? "[System.Diagnostics.CodeAnalysis.NotNullWhen(true)] ref string? value"
					: $"ref {keyword} value";
				writer.WriteLine($"public override bool ReadAttributeOpt(string name, {valueParameter})");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					if (IsString(typeSpec))
					{
						writer.WriteLine("return (value = ReadAttributeOpt(name)) != null;");
					}
					else
					{
						writer.WriteLine("return TagElementTextStreamUtils.ParseString(ReadAttributeOpt(name), ref value, kNoExcept,");
						writer.WriteLine("\tmReadErrorState);");
					}
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"public override bool ReadAttributeOpt(string name, ref {typeSpec.Keyword} value, " +
					"NumeralBase fromBase = kDefaultRadix)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						"return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, mReadErrorState, " +
						"fromBase);");
				}
			}
		}
	}

	private static void WriteTextWriteElementImplRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteElement impl"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				if (IsString(typeSpec))
				{
					continue;
				}

				writer.WriteLine($"protected override void WriteElement(TCursor n, {typeSpec.Keyword} value)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					WriteTextWriteElementBody(writer, typeSpec);
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"protected override void WriteElement(TCursor n, {typeSpec.Keyword} value, NumeralBase toBase)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("WriteElement(n, Numbers.ToString(value, toBase));");
				}
			}
		}
	}

	private static void WriteTextWriteAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				writer.WriteLine($"public override void WriteAttribute(string name, {typeSpec.Keyword} value)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					WriteTextWriteAttributeBody(writer, typeSpec);
				}
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				writer.WriteLine(
					$"public override void WriteAttribute(string name, {typeSpec.Keyword} value, NumeralBase toBase)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("CursorWriteAttribute(name, Numbers.ToString(value, toBase));");
				}
			}
		}
	}

	private static void WriteTextWriteElementBody(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		switch (typeSpec.TypeCode)
		{
			case TypeCode.Char:
				writer.WriteLine("WriteElement(n, new string(value, 1));");
				break;
			case TypeCode.Single:
				writer.WriteLine("WriteElement(n, value.ToStringInvariant(this.SingleFormatSpecifier));");
				break;
			case TypeCode.Double:
				writer.WriteLine("WriteElement(n, value.ToStringInvariant(this.DoubleFormatSpecifier));");
				break;
			case TypeCode.Boolean:
				writer.WriteLine("WriteElement(n, value ? \"true\" : \"false\");");
				break;
			default:
				writer.WriteLine("WriteElement(n, value.ToString());");
				break;
		}
	}

	private static void WriteTextWriteAttributeBody(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		switch (typeSpec.TypeCode)
		{
			case TypeCode.String:
				writer.WriteLine("CursorWriteAttribute(name, value);");
				break;
			case TypeCode.Char:
				writer.WriteLine("CursorWriteAttribute(name, new string(value, 1));");
				break;
			case TypeCode.Single:
				writer.WriteLine("CursorWriteAttribute(name, value.ToStringInvariant(this.SingleFormatSpecifier));");
				break;
			case TypeCode.Double:
				writer.WriteLine("CursorWriteAttribute(name, value.ToStringInvariant(this.DoubleFormatSpecifier));");
				break;
			case TypeCode.Boolean:
				writer.WriteLine("CursorWriteAttribute(name, value ? \"true\" : \"false\");");
				break;
			default:
				writer.WriteLine("CursorWriteAttribute(name, value.ToString());");
				break;
		}
	}
};
