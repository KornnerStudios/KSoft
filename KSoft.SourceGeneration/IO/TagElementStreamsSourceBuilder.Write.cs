using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	private static void WriteWriteElementImplRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteElement impl"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteElementImpl(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteElementImpl(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteElementImpl(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary("");
		writer.WriteXmlDocParam("n", "Node element to write");
		writer.WriteXmlDocParam("value", "Data to set the element's <see cref=\"TCursor.InnerText\"/> to");
		writer.WriteLine($"protected abstract void WriteElement(TCursor n, {typeSpec.Keyword} value);");
	}

	private static void WriteWriteElementImpl(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("");
		writer.WriteXmlDocParam("n", "Node element to write");
		writer.WriteXmlDocParam("value", "Data to set the element's <see cref=\"TCursor.InnerText\"/> to");
		writer.WriteXmlDocParam("toBase", "");
		writer.WriteLine($"protected abstract void WriteElement(TCursor n, {typeSpec.Keyword} value, NumeralBase toBase);");
	}

	private static void WriteWriteCursorRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteCursor"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteCursor(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteCursor(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteCursor(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Set <see cref=\"Cursor\"/>'s value to <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "Data to set the <see cref=\"Cursor\"/> to");
		writer.WriteLine($"public void WriteCursor({typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (IsString(typeSpec))
			{
				writer.WriteLine("ArgumentNullException.ThrowIfNull(value);");
			}

			writer.WriteLine("WriteElement(Cursor, value);");
		}
	}

	private static void WriteWriteCursor(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Set <see cref=\"Cursor\"/>'s value to <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "Data to set the <see cref=\"Cursor\"/> to");
		writer.WriteXmlDocParam("toBase", "Numerical base to use");
		writer.WriteLine($"public void WriteCursor({typeSpec.Keyword} value, NumeralBase toBase = kDefaultRadix)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("WriteElement(Cursor, value, toBase);");
		}
	}

	private static void WriteWriteElementRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteElement"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteElement(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteElement(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteElement(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Create a new element in the underlying <see cref=\"XmlDocument\"/>, " +
			"relative to <see cref=\"Cursor\"/>");
		writer.WriteXmlDocParam("name", "The <see cref=\"XmlElement\"/>'s name");
		writer.WriteXmlDocParam("value", "Data to set the element's <see cref=\"XmlElement.InnerText\"/> to");
		writer.WriteLine("/// <remarks>Does not change <see cref=\"Cursor\"/></remarks>");
		writer.WriteLine($"public void WriteElement(TName name, {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			if (IsString(typeSpec))
			{
				writer.WriteLine("ArgumentNullException.ThrowIfNull(value);");
			}

			writer.WriteLine();
			writer.WriteLine("WriteElement(WriteElementAppend(name), value);");
		}
	}

	private static void WriteWriteElement(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary(
			"Create a new element in the underlying <see cref=\"XmlDocument\"/>, " +
			"relative to <see cref=\"Cursor\"/>");
		writer.WriteXmlDocParam("name", "The <see cref=\"XmlElement\"/>'s name");
		writer.WriteXmlDocParam("toBase", "Numerical base to use");
		writer.WriteXmlDocParam("value", "Data to set the element's <see cref=\"XmlElement.InnerText\"/> to");
		writer.WriteLine("/// <remarks>Does not change <see cref=\"Cursor\"/></remarks>");
		writer.WriteLine(
			$"public void WriteElement(TName name, {typeSpec.Keyword} value, " +
			"NumeralBase toBase = kDefaultRadix)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine();
			writer.WriteLine("WriteElement(WriteElementAppend(name), value, toBase);");
		}
	}

	private static void WriteWriteAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteAttribute(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteAttribute(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteAttribute(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Create a new attribute for <see cref=\"Cursor\"/>");
		writer.WriteXmlDocParam("name", "Name of the <see cref=\"XmlAttribute\"/>");
		writer.WriteXmlDocParam("value", "Data to set the attribute text to");
		writer.WriteLine($"public abstract void WriteAttribute(TName name, {typeSpec.Keyword} value);");
	}

	private static void WriteWriteAttribute(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Create a new attribute for <see cref=\"Cursor\"/>");
		writer.WriteXmlDocParam("name", "Name of the <see cref=\"XmlAttribute\"/>");
		writer.WriteXmlDocParam("toBase", "Numerical base to use");
		writer.WriteXmlDocParam("value", "Data to set the attribute text to");
		writer.WriteLine(
			$"public abstract void WriteAttribute(TName name, {typeSpec.Keyword} value, NumeralBase toBase = kDefaultRadix);");
	}

	private static void WriteWriteElementOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteElementOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteElementOpt(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteElementOpt(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteElementOpt(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Element", onTrue: true, includeCursorContract: false, includeBase: false);
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Element", onTrue: false, includeCursorContract: false, includeBase: false);
	}

	private static void WriteWriteElementOpt(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Element", onTrue: true, includeCursorContract: true, includeBase: true);
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Element", onTrue: false, includeCursorContract: true, includeBase: true);
	}

	private static void WriteWriteAttributeOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteAttributeOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteAttributeOpt(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteAttributeOpt(writer, typeSpec);
			}
		}
	}

	private static void WriteWriteAttributeOpt(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Attribute", onTrue: true, includeCursorContract: true, includeBase: false);
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Attribute", onTrue: false, includeCursorContract: true, includeBase: false);
	}

	private static void WriteWriteAttributeOpt(SourceWriter writer, NumberSpec typeSpec)
	{
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Attribute", onTrue: true, includeCursorContract: true, includeBase: true);
		WriteWriteOptMethod(
			writer, typeSpec.Keyword, "Attribute", onTrue: false, includeCursorContract: true, includeBase: true);
	}

	private static void WriteWriteOptMethod(
		SourceWriter writer,
		string keyword,
		string subject,
		bool onTrue,
		bool includeCursorContract,
		bool includeBase)
	{
		string predicateDoc = onTrue
			? "Predicate that defines the conditions for when <paramref name=\"value\"/> <b>is</b> written"
			: "Predicate that defines the conditions for when <paramref name=\"value\"/> <b>isn't</b> written";
		string methodName = $"Write{subject}OptOn{(onTrue ? "True" : "False")}";
		string baseParameter = includeBase
			? ", NumeralBase toBase = NumeralBase.Decimal"
			: "";
		string writeBaseArgument = includeBase
			? ", toBase"
			: "";

		writer.WriteXmlDocSummary(subject == "Element"
			? "Create a new element in the underlying <see cref=\"XmlDocument\"/>, relative to <see cref=\"Cursor\"/>"
			: "Create a new attribute for <see cref=\"Cursor\"/>");
		writer.WriteXmlDocParam("name", subject == "Element"
			? "The <see cref=\"XmlElement\"/>'s name"
			: "Name of the <see cref=\"XmlAttribute\"/>");
		writer.WriteXmlDocParam("value", subject == "Element"
			? "Data to set the element's <see cref=\"XmlElement.InnerText\"/> to"
			: "Data to set the attribute text to");
		writer.WriteXmlDocParam("predicate", predicateDoc);
		if (includeBase)
		{
			writer.WriteXmlDocParam("toBase", "Numerical base to use");
		}

		if (subject == "Element")
		{
			writer.WriteLine("/// <remarks>Does not change <see cref=\"Cursor\"/></remarks>");
		}

		writer.WriteXmlDocReturns("True if <paramref name=\"value\"/> was written");
		writer.WriteLine(
			$"public bool {methodName}(TName name, {keyword} value, Predicate<{keyword}> predicate{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(predicate != null);");
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			if (includeCursorContract)
			{
				writer.WriteLine("Contract.Requires(Cursor != null, kCursorNullMsg);");
			}

			if (!onTrue && keyword == "string")
			{
				writer.WriteLine("if (predicate != string.IsNullOrEmpty && value == null)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("throw new ArgumentNullException(nameof(value));");
				}
			}

			writer.WriteLine();
			writer.WriteLine($"bool result = IgnoreWritePredicates || {(onTrue ? "" : "!")}predicate(value);");
			writer.WriteLine();
			writer.WriteLine("if (result)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"Write{subject}(name, value{writeBaseArgument});");
			}

			writer.WriteLine();
			writer.WriteLine("return result;");
		}
	}

	private static void WriteWriteElementsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteElements"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteWriteElements(writer, typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteWriteElements(writer, typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteWriteElements(SourceWriter writer, string keyword, bool includeBase)
	{
		string baseParameter = includeBase
			? ", NumeralBase toBase = kDefaultRadix"
			: "";
		string baseArgument = includeBase
			? ", toBase"
			: "";

		writer.WriteLine($"public void WriteElements(TName elementName, ICollection<{keyword}> coll{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(elementName));");
			writer.WriteLine("ArgumentNullException.ThrowIfNull(coll);");
			writer.WriteLine();
			writer.WriteLine("foreach (var value in coll)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"WriteElement(elementName, value{baseArgument});");
			}
		}
	}
};
