using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static class EndianStreamsNumbersSourceBuilder
{
	public const string HintName = "KSoft.IO.EndianStreams.Numbers.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Buffers.Binary;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
		WriteEndianReader(writer);
		writer.WriteLine();
		WriteEndianWriter(writer);
		writer.WriteLine();
		WriteEndianStream(writer);

		return writer.ToString();
	}

	private static void WriteEndianReader(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class EndianReader"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				if (typeSpec.IsByte)
				{
					continue;
				}

				WriteReadScalarOverride(writer, typeSpec);
				writer.WriteLine();
			}

			writer.WriteLine();
			WriteReadFixedArrayRegion(writer);
		}
	}

	private static void WriteEndianWriter(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class EndianWriter"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				if (typeSpec.IsByte)
				{
					continue;
				}

				WriteWriteScalarOverride(writer, typeSpec);
				writer.WriteLine();
			}

			writer.WriteLine();
			WriteWriteFixedArrayRegion(writer);
		}
	}

	private static void WriteEndianStream(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class EndianStream"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				WriteStreamValueMethod(writer, typeSpec);
				writer.WriteLine();
			}

			writer.WriteLine();
			WriteStreamFixedArrayRegion(writer);
		}
	}

	private static void WriteReadScalarOverride(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();

		writer.WriteXmlDocSummary($"Reads a {typeSpec.SimpleDescription}");
		writer.WriteXmlDocReturns();
		writer.WriteLine($"/// <seealso cref=\"System.IO.BinaryReader.Read{code}()\"/>");
		writer.WriteLine($"public override {typeSpec.Keyword} Read{code}()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("// #VITA_SHIM: BinaryPrimitives reads the stream byte order directly without byte-swap wrappers.");
			writer.WriteLine($"Span<byte> bytes = stackalloc byte[sizeof({typeSpec.Keyword})];");
			writer.WriteLine("BaseStream.ReadExactly(bytes);");
			writer.WriteLine("return ByteOrder == Shell.EndianFormat.Little");
			writer.WriteLine($"\t? BinaryPrimitives.Read{code}LittleEndian(bytes)");
			writer.WriteLine($"\t: BinaryPrimitives.Read{code}BigEndian(bytes);");
		}
	}

	private static void WriteWriteScalarOverride(SourceWriter writer, NumberSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();

		writer.WriteXmlDocSummary($"Writes a {typeSpec.SimpleDescription}");
		writer.WriteXmlDocParam("value", "");
		writer.WriteLine($"/// <seealso cref=\"System.IO.BinaryWriter.Write({typeSpec.Keyword})\"/>");
		writer.WriteLine($"public override void Write({typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine(
				"// #VITA_SHIM: BinaryPrimitives writes the stream byte order directly without byte-swap wrappers.");
			writer.WriteLine($"Span<byte> bytes = stackalloc byte[sizeof({typeSpec.Keyword})];");
			writer.WriteLine("if (ByteOrder == Shell.EndianFormat.Little)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"BinaryPrimitives.Write{code}LittleEndian(bytes, value);");
			}

			writer.WriteLine("else");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"BinaryPrimitives.Write{code}BigEndian(bytes, value);");
			}

			writer.WriteLine("base.Write(bytes);");
		}
	}

	private static void WriteReadFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadFixedArray"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				WriteReadFixedArrayMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteWriteFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteFixedArray"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				WriteWriteFixedArrayMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteStreamFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamFixedArray"))
		{
			foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
			{
				WriteStreamFixedArrayMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteReadFixedArrayMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		string readMethodName = ReadMethodName(typeSpec);

		writer.WriteLine(
			$"public {typeSpec.Keyword}[] ReadFixedArray({typeSpec.Keyword}[] array, int startIndex, int length)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteFixedArrayContracts(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine("for (int x = startIndex, end = startIndex+length; x < end; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"array[x] = {readMethodName}();");
			}

			writer.WriteLine();
			writer.WriteLine("return array;");
		}

		writer.WriteLine($"public {typeSpec.Keyword}[] ReadFixedArray({typeSpec.Keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(array);");
			writer.WriteLine();
			writer.WriteLine("return ReadFixedArray(array, 0, array.Length);");
		}
	}

	private static void WriteWriteFixedArrayMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteLine(
			$"public {typeSpec.Keyword}[] WriteFixedArray({typeSpec.Keyword}[] array, int startIndex, int length)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteFixedArrayContracts(writer, typeSpec);
			writer.WriteLine();
			writer.WriteLine("for (int x = startIndex, end = startIndex+length; x < end; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("Write(array[x]);");
			}

			writer.WriteLine();
			writer.WriteLine("return array;");
		}

		writer.WriteLine($"public {typeSpec.Keyword}[] WriteFixedArray({typeSpec.Keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(array);");
			writer.WriteLine();
			writer.WriteLine("return WriteFixedArray(array, 0, array.Length);");
		}
	}

	private static void WriteStreamValueMethod(SourceWriter writer, NumberSpec typeSpec)
	{
		string readMethodName = ReadMethodName(typeSpec);

		writer.WriteLine($"public EndianStream Stream(ref {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"if (IsReading) {{ value = Reader.{readMethodName}(); }}");
			writer.WriteLine("else if (IsWriting) { Writer.Write(value); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}
	}

	private static void WriteStreamFixedArrayMethods(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteLine(
			$"public EndianStream StreamFixedArray({typeSpec.Keyword}[] array, int startIndex, int length)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(array);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startIndex);");
			writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(length);");
			writer.WriteLine();
			writer.WriteLine("if (IsReading) { Reader.ReadFixedArray(array, startIndex, length); }");
			writer.WriteLine("else if (IsWriting) { Writer.WriteFixedArray(array, startIndex, length); }");
			writer.WriteLine();
			writer.WriteLine("return this;");
		}

		writer.WriteLine($"public EndianStream StreamFixedArray({typeSpec.Keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ArgumentNullException.ThrowIfNull(array);");
			writer.WriteLine();
			writer.WriteLine("return StreamFixedArray(array, 0, array.Length);");
		}
	}

	private static void WriteFixedArrayContracts(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteLine("ArgumentNullException.ThrowIfNull(array);");
		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(startIndex);");
		writer.WriteLine("ArgumentOutOfRangeException.ThrowIfNegative(length);");
	}

	private static string ReadMethodName(NumberSpec typeSpec)
	{
		return typeSpec.TypeCode switch
		{
			TypeCode.Byte	=> "ReadByte",
			TypeCode.SByte	=> "ReadSByte",
			_				=> $"Read{typeSpec.TypeCode}",
		};
	}
};
