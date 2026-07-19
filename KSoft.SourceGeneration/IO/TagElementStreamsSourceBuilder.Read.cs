using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	private static void WriteReadElementImplRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElement impl"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadElementImpl(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadElementImpl(writer, typeSpec);
			}
		}
	}

	private static void WriteReadElementImpl(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the InnerText of element <paramref name=\"name\"/> into " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("n", "Node element to read");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine($"protected abstract void ReadElement(TCursor n, ref {typeSpec.Keyword} value);");
	}

	private static void WriteReadElementImpl(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteReadNumeralXmlDocs(writer, "n");
		writer.WriteLine(
			$"protected abstract void ReadElement(TCursor n, ref {typeSpec.Keyword} value, " +
			"NumeralBase fromBase);");
	}

	private static void WriteReadCursorRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadCursor"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadCursor(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadCursor(writer, typeSpec);
			}
		}
	}

	private static void WriteReadCursor(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Stream out the Value of <see cref=\"Cursor\"/> into <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine($"public void ReadCursor(ref {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ReadElement(Cursor, ref value);");
		}
	}

	private static void WriteReadCursor(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the Value of <see cref=\"Cursor\"/>",
			"using numerical base of <paramref name=\"base\"/> into <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("fromBase", "numerical base to use");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine($"public void ReadCursor(ref {typeSpec.Keyword} value, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("ReadElement(Cursor, ref value, fromBase);");
		}
	}

	private static void WriteReadElementRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElement"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadElement(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadElement(writer, typeSpec);
			}
		}
	}

	private static void WriteReadElement(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the InnerText of element <paramref name=\"name\"/> into " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Element name");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine($"public void ReadElement(TName name, ref {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine();
			writer.WriteLine("ReadElement(GetElement(name), ref value);");
		}
	}

	private static void WriteReadElement(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteReadNumeralXmlDocs(writer, "name");
		writer.WriteLine(
			$"public void ReadElement(TName name, ref {typeSpec.Keyword} value, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine();
			writer.WriteLine("ReadElement(GetElement(name), ref value, fromBase);");
		}
	}

	private static void WriteReadAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadAttribute(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadAttribute(writer, typeSpec);
			}
		}
	}

	private static void WriteReadAttribute(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the attribute data of <paramref name=\"name\"/> into " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Attribute name");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine($"public abstract void ReadAttribute(TName name, ref {typeSpec.Keyword} value);");
	}

	private static void WriteReadAttribute(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the attribute data of <paramref name=\"name\"/>",
			"using numerical base of <paramref name=\"base\"/> into",
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Attribute name");
		writer.WriteXmlDocParam("fromBase", "numerical base to use");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine(
			$"public abstract void ReadAttribute(TName name, ref {typeSpec.Keyword} value, " +
			"NumeralBase fromBase = NumeralBase.Decimal);");
	}

	private static void WriteReadElementOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElementOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadElementOpt(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadElementOpt(writer, typeSpec);
			}
		}
	}

	private static void WriteReadElementOpt(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the InnerText of element <paramref name=\"name\"/> into " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Element name");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteLine("/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>");
		writer.WriteXmlDocReturns("true if the value exists");
		writer.WriteLine($"public abstract bool ReadElementOpt(TName name, ref {typeSpec.Keyword} value);");
	}

	private static void WriteReadElementOpt(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteReadNumeralXmlDocs(writer, "name");
		writer.WriteLine("/// <remarks>If inner text is just an empty string, the stream ignores its existence</remarks>");
		writer.WriteXmlDocReturns("true if the value exists");
		writer.WriteLine(
			$"public abstract bool ReadElementOpt(TName name, ref {typeSpec.Keyword} value, " +
			"NumeralBase fromBase = NumeralBase.Decimal);");
	}

	private static void WriteReadAttributeOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttributeOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadAttributeOpt(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadAttributeOpt(writer, typeSpec);
			}
		}
	}

	private static void WriteReadAttributeOpt(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the attribute data of <paramref name=\"name\"/> into " +
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Attribute name");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteXmlDocReturns("true if the value exists");
		writer.WriteLine($"public abstract bool ReadAttributeOpt(TName name, ref {typeSpec.Keyword} value);");
	}

	private static void WriteReadAttributeOpt(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Stream out the attribute data of <paramref name=\"name\"/>",
			"using numerical base of <paramref name=\"base\"/> into",
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", "Attribute name");
		writer.WriteXmlDocParam("fromBase", "numerical base to use");
		writer.WriteXmlDocParam("value", "value to receive the data");
		writer.WriteXmlDocReturns("true if the value exists");
		writer.WriteLine(
			$"public abstract bool ReadAttributeOpt(TName name, ref {typeSpec.Keyword} value, " +
			"NumeralBase fromBase = NumeralBase.Decimal);");
	}

	private static void WriteReadElementsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElements"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadElements(writer, typeSpec);
				writer.WriteLine();
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadElements(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteReadElements(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine($"void ReadElements(IEnumerable<TCursor> elements, ICollection<{keyword}> coll)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadCollectionLoop(writer, keyword, "ReadCursor(ref value);");
		}

		writer.WriteLine($"public void ReadElements(ICollection<{keyword}> coll)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(coll != null);");
			writer.WriteLine();
			writer.WriteLine("ReadElements(this.Elements, coll);");
		}

		writer.WriteLine($"public void ReadElements(TName name, ICollection<{keyword}> coll)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(coll != null);");
			writer.WriteLine();
			writer.WriteLine("ReadElements(this.ElementsByName(name), coll);");
		}
	}

	private static void WriteReadElements(SourceWriter writer, NumberSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine(
			$"void ReadElements(IEnumerable<TCursor> elements, ICollection<{keyword}> coll, " +
			"NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadCollectionLoop(writer, keyword, "ReadCursor(ref value, fromBase);", "0");
		}

		writer.WriteLine(
			$"public void ReadElements(ICollection<{keyword}> coll, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(coll != null);");
			writer.WriteLine();
			writer.WriteLine("ReadElements(this.Elements, coll, fromBase);");
		}

		writer.WriteLine(
			$"public void ReadElements(TName name, ICollection<{keyword}> coll, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(coll != null);");
			writer.WriteLine();
			writer.WriteLine("ReadElements(this.ElementsByName(name), coll, fromBase);");
		}
	}

	private static void WriteReadCollectionLoop(
		SourceWriter writer,
		string keyword,
		string readStatement,
		string initializer = null)
	{
		writer.WriteLine("foreach (var node in elements)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("using (EnterCursorBookmark(node))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				if (initializer == null)
				{
					writer.WriteLine($"var value = default({keyword});");
				}
				else
				{
					writer.WriteLine($"{keyword} value = {initializer};");
				}

				writer.WriteLine(readStatement);
				writer.WriteLine();
				writer.WriteLine("coll.Add(value);");
			}
		}
	}

	private static void WriteReadFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadFixedArray"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteReadFixedArray(writer, typeSpec);
				writer.WriteLine();
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteReadFixedArray(writer, typeSpec);
				writer.WriteLine();
			}
		}
	}

	private static void WriteReadFixedArray(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine($"int ReadFixedArray(IEnumerable<TCursor> elements, {keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadFixedArrayLoop(writer, "ReadCursor(ref array[count++]);");
		}

		writer.WriteLine($"public int ReadFixedArray({keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(array != null);");
			writer.WriteLine();
			writer.WriteLine("return ReadFixedArray(this.Elements, array);");
		}

		writer.WriteLine($"public int ReadFixedArray(TName name, {keyword}[] array)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(array != null);");
			writer.WriteLine();
			writer.WriteLine("return ReadFixedArray(this.ElementsByName(name), array);");
		}
	}

	private static void WriteReadFixedArray(SourceWriter writer, NumberSpec typeSpec)
	{
		string keyword = typeSpec.Keyword;

		writer.WriteLine(
			$"int ReadFixedArray(IEnumerable<TCursor> elements, {keyword}[] array, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadFixedArrayLoop(writer, "ReadCursor(ref array[count++], fromBase);");
		}

		writer.WriteLine($"public int ReadFixedArray({keyword}[] array, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentNullException>(array != null);");
			writer.WriteLine();
			writer.WriteLine("return ReadFixedArray(this.Elements, array, fromBase);");
		}

		writer.WriteLine(
			$"public int ReadFixedArray(TName name, {keyword}[] array, NumeralBase fromBase = NumeralBase.Decimal)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(array != null);");
			writer.WriteLine();
			writer.WriteLine("return ReadFixedArray(this.ElementsByName(name), array, fromBase);");
		}
	}

	private static void WriteReadFixedArrayLoop(SourceWriter writer, string readStatement)
	{
		writer.WriteLine("int count = 0;");
		writer.WriteLine("foreach (var node in elements)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("using (EnterCursorBookmark(node))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(readStatement);
			}

			writer.WriteLine();
			writer.WriteLine("if (count == array.Length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("break;");
			}
		}

		writer.WriteLine();
		writer.WriteLine("return count;");
	}

	private static void WriteReadNumeralXmlDocs(SourceWriter writer, string nodeParamName)
	{
		writer.WriteXmlDocSummary(
			"Stream out the InnerText of element <paramref name=\"name\"/>",
			"using numerical base of <paramref name=\"base\"/> into",
			"<paramref name=\"value\"/>");
		writer.WriteXmlDocParam(nodeParamName, nodeParamName == "n" ? "Node element to read" : "Element name");
		writer.WriteXmlDocParam("fromBase", "numerical base to use");
		writer.WriteXmlDocParam("value", "value to receive the data");
	}
};
