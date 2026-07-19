using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	private static void WriteStreamCursorRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream Cursor"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamCursor(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamCursor(writer, typeSpec);
			}

			writer.WriteLine();
			foreach (PrimitiveSpec typeSpec in StreamPropertyTypes())
			{
				WriteObjectPropertyStreamMethod(writer, StreamSubject.Cursor, typeSpec);
			}
		}
	}

	private static void WriteStreamCursor(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Stream the Value of <see cref=\"Cursor\"/> to or from <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "Source or destination value");
		writer.WriteLine($"/// <seealso cref=\"ReadCursor(string, ref {typeSpec.Keyword})\"/>");
		writer.WriteLine($"/// <seealso cref=\"WriteCursor(string, {typeSpec.Keyword})\"/>");
		writer.WriteLine($"public void StreamCursor(ref {typeSpec.Keyword} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadWriteBranch(writer, "ReadCursor(ref value);", "WriteCursor(value);");
		}
	}

	private static void WriteStreamCursor(SourceWriter writer, NumberSpec typeSpec)
	{
		writer.WriteXmlDocSummary("Stream the Value of <see cref=\"Cursor\"/> to or from <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("value", "Source or destination value");
		writer.WriteXmlDocParam("numBase", "numerical base to use");
		writer.WriteLine($"/// <seealso cref=\"ReadCursor(string, ref {typeSpec.Keyword}, NumeralBase)\"/>");
		writer.WriteLine($"/// <seealso cref=\"WriteCursor(string, {typeSpec.Keyword}, NumeralBase)\"/>");
		writer.WriteLine($"public void StreamCursor(ref {typeSpec.Keyword} value, NumeralBase numBase = kDefaultRadix)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteReadWriteBranch(writer, "ReadCursor(ref value, numBase);", "WriteCursor(value, numBase);");
		}
	}

	private static void WriteStreamElementRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream Element"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamNamedValue(writer, "Element", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamNamedValue(writer, "Element", typeSpec.Keyword, includeBase: true);
			}

			writer.WriteLine();
			foreach (PrimitiveSpec typeSpec in StreamPropertyTypes())
			{
				WriteObjectPropertyStreamMethod(writer, StreamSubject.Element, typeSpec);
			}
		}
	}

	private static void WriteStreamElementOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamElementOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamNamedOptValue(writer, "Element", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamNamedOptValue(writer, "Element", typeSpec.Keyword, includeBase: true);
			}

			writer.WriteLine();
			foreach (PrimitiveSpec typeSpec in StreamPropertyTypes())
			{
				WriteObjectPropertyStreamMethod(writer, StreamSubject.ElementOpt, typeSpec);
			}
		}
	}

	private static void WriteStreamAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream Attribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamNamedValue(writer, "Attribute", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamNamedValue(writer, "Attribute", typeSpec.Keyword, includeBase: true);
			}

			writer.WriteLine();
			foreach (PrimitiveSpec typeSpec in StreamPropertyTypes())
			{
				WriteObjectPropertyStreamMethod(writer, StreamSubject.Attribute, typeSpec);
			}
		}
	}

	private static void WriteStreamAttributeOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StreamAttributeOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamNamedOptValue(writer, "Attribute", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamNamedOptValue(writer, "Attribute", typeSpec.Keyword, includeBase: true);
			}

			writer.WriteLine();
			foreach (PrimitiveSpec typeSpec in StreamPropertyTypes())
			{
				WriteObjectPropertyStreamMethod(writer, StreamSubject.AttributeOpt, typeSpec);
			}
		}
	}

	private static void WriteStreamNamedValue(SourceWriter writer, string subject, string keyword, bool includeBase)
	{
		string baseParameter = includeBase
			? ", NumeralBase numBase = kDefaultRadix"
			: "";
		string baseArgument = includeBase
			? ", numBase"
			: "";

		writer.WriteXmlDocSummary(
			$"Stream the Value of {subject.ToLowerInvariant()} <paramref name=\"name\"/> " +
			"to or from <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", $"{subject} name");
		writer.WriteXmlDocParam("value", "Source or destination value");
		if (includeBase)
		{
			writer.WriteXmlDocParam("numBase", "numerical base to use");
		}

		writer.WriteLine(
			$"/// <seealso cref=\"Read{subject}(TName, ref {keyword}" +
			$"{(includeBase ? ", NumeralBase" : "")})\"/>");
		writer.WriteLine(
			$"/// <seealso cref=\"Write{subject}(TName, {keyword}" +
			$"{(includeBase ? ", NumeralBase" : "")})\"/>");
		writer.WriteLine($"public void Stream{subject}(TName name, ref {keyword} value{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine();
			WriteReadWriteBranch(
				writer,
				$"Read{subject}(name, ref value{baseArgument});",
				$"Write{subject}(name, value{baseArgument});");
		}
	}

	private static void WriteStreamNamedOptValue(SourceWriter writer, string subject, string keyword, bool includeBase)
	{
		string baseParameter = includeBase
			? ", NumeralBase numBase = kDefaultRadix"
			: "";
		string baseArgument = includeBase
			? ", numBase"
			: "";

		writer.WriteXmlDocSummary(
			$"Stream the Value of {subject.ToLowerInvariant()} <paramref name=\"name\"/> " +
			"to or from <paramref name=\"value\"/>");
		writer.WriteXmlDocParam("name", $"{subject} name");
		writer.WriteXmlDocParam("value", "Source or destination value");
		writer.WriteXmlDocParam(
			"predicate",
			"Predicate that defines the conditions for when <paramref name=\"value\"/> <b>is</b> written");
		if (includeBase)
		{
			writer.WriteXmlDocParam("numBase", "numerical base to use");
		}

		writer.WriteXmlDocReturns("True if <paramref name=\"value\"/> was read/written from/to stream");
		writer.WriteLine(
			$"/// <seealso cref=\"Read{subject}Opt(TName, ref {keyword}" +
			$"{(includeBase ? ", NumeralBase" : "")})\"/>");
		writer.WriteLine(
			$"/// <seealso cref=\"Write{subject}OptOnTrue(TName, {keyword}, Predicate{{{keyword}}}" +
			$"{(includeBase ? ", NumeralBase" : "")})\"/>");
		writer.WriteLine(
			$"public bool Stream{subject}Opt(TName name, ref {keyword} value, " +
			$"Predicate<{keyword}> predicate = null{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine();
			writer.WriteLine("if (predicate == null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"predicate = Predicates.True<{keyword}>;");
			}

			writer.WriteLine();
			writer.WriteLine("bool executed = false;");
			WriteReadWriteBranch(
				writer,
				$"executed = Read{subject}Opt(name, ref value{baseArgument});",
				$"executed = Write{subject}OptOnTrue(name, value, predicate{baseArgument});");
			writer.WriteLine("return executed;");
		}
	}

	private static void WriteObjectPropertyStreamMethod(SourceWriter writer, StreamSubject subject, PrimitiveSpec typeSpec)
	{
		string subjectName = subject.ToString();
		bool hasName = subject != StreamSubject.Cursor;
		bool isOptional = subject is StreamSubject.ElementOpt or StreamSubject.AttributeOpt;
		bool isInteger = typeSpec.IsInteger;
		string keyword = typeSpec.Keyword;
		string returnType = isOptional
			? "bool"
			: "void";
		string nameParameter = hasName
			? "TName name, "
			: "";
		string predicateParameter = isOptional
			? $", Predicate<{keyword}> predicate = null"
			: "";
		string baseParameter = isInteger
			? ", NumeralBase numBase = kDefaultRadix"
			: "";
		string nameArgument = hasName
			? "name, "
			: "";
		string baseArgument = isInteger
			? ", numBase"
			: "";
		string assignmentPrefix = isOptional
			? "executed = "
			: "";
		string writeOptSuffix = isOptional
			? "OnTrue"
			: "";

		writer.WriteLine(
			$"public {returnType} Stream{subjectName}<T>({nameParameter}T theObj, " +
			$"Exprs.Expression<Func<T, {keyword}>> propExpr{predicateParameter}{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (hasName)
			{
				writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
				writer.WriteLine();
			}

			if (isOptional)
			{
				writer.WriteLine("if (predicate == null)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("predicate = x => true;");
				}

				writer.WriteLine();
				writer.WriteLine("bool executed = false;");
			}

			writer.WriteLine("var property = Reflection.Util.PropertyFromExpr(propExpr);");
			writer.WriteLine("if (IsReading)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"var value = default({keyword});");
				writer.WriteLine($"{assignmentPrefix}Read{subjectName}({nameArgument}ref value{baseArgument});");
				if (isOptional)
				{
					writer.WriteLine("if (executed)");
				}

				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("property.SetValue(theObj, value, null);");
				}
			}

			writer.WriteLine("else if (IsWriting)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				string predicateArgument = isOptional
					? ", predicate"
					: "";
				writer.WriteLine(
					$"{assignmentPrefix}Write{subjectName}{writeOptSuffix}({nameArgument}({keyword})" +
					$"property.GetValue(theObj, null){predicateArgument}{baseArgument});");
			}

			if (isOptional)
			{
				writer.WriteLine();
				writer.WriteLine("return executed;");
			}
		}
	}

	private static void WriteStreamElementsRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream Elements"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamElements(writer, typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamElements(writer, typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteStreamElements(SourceWriter writer, string keyword, bool includeBase)
	{
		string baseParameter = includeBase
			? ", NumeralBase numBase = kDefaultRadix"
			: "";
		string baseArgument = includeBase
			? ", numBase"
			: "";

		writer.WriteLine($"public void StreamElements(TName name, ICollection<{keyword}> coll{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(coll != null);");
			writer.WriteLine();
			WriteReadWriteBranch(
				writer,
				$"ReadElements(name, coll{baseArgument});",
				$"WriteElements(name, coll{baseArgument});");
		}
	}

	private static void WriteStreamFixedArrayRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Stream Fixed Array"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteStreamFixedArray(writer, typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteStreamFixedArray(writer, typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteStreamFixedArray(SourceWriter writer, string keyword, bool includeBase)
	{
		string baseParameter = includeBase
			? ", NumeralBase numBase = kDefaultRadix"
			: "";
		string baseArgument = includeBase
			? ", numBase"
			: "";

		writer.WriteLine($"public int StreamFixedArray(TName name, {keyword}[] array{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("Contract.Requires<ArgumentNullException>(array != null);");
			writer.WriteLine();
			writer.WriteLine($"if (IsReading) {{ return ReadFixedArray(name, array{baseArgument}); }}");
			writer.WriteLine($"else if (IsWriting) {{ WriteElements(name, array{baseArgument}); }}");
			writer.WriteLine();
			writer.WriteLine("return array.Length;");
		}
	}
};
