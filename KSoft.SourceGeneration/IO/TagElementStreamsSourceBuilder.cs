using System;
using System.Collections.Generic;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	public const string HintName = "KSoft.IO.TagElementStreams.g.cs";

	private enum StreamSubject
	{
		Cursor,
		Element,
		ElementOpt,
		Attribute,
		AttributeOpt,
	};

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("using Exprs = System.Linq.Expressions;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
		WriteTagElementStream(writer);
		writer.WriteLine();
		WriteTagElementTextStream(writer);

		return writer.ToString();
	}

	private static IEnumerable<PrimitiveSpec> MiscTypes()
	{
		yield return PrimitiveCatalog.String;
		yield return PrimitiveCatalog.Char;
		yield return PrimitiveCatalog.Bool;

		foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
		{
			if (typeSpec.TypeCode is TypeCode.Single or TypeCode.Double)
			{
				yield return typeSpec.Primitive;
			}
		}
	}

	private static IEnumerable<NumberSpec> IntegerTypes()
	{
		foreach (NumberSpec typeSpec in PrimitiveCatalog.Numbers)
		{
			if (typeSpec.IsInteger)
			{
				yield return typeSpec;
			}
		}
	}

	private static IEnumerable<PrimitiveSpec> StreamPropertyTypes()
	{
		foreach (PrimitiveSpec typeSpec in MiscTypes())
		{
			yield return typeSpec;
		}

		foreach (NumberSpec typeSpec in IntegerTypes())
		{
			yield return typeSpec.Primitive;
		}

		yield return PrimitiveCatalog.KGuid;
	}

	private static void WriteTagElementStream(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class TagElementStream<TDoc, TCursor, TName>"))
		{
			WriteReadElementImplRegion(writer);
			writer.WriteLine();
			WriteReadCursorRegion(writer);
			writer.WriteLine();
			WriteReadElementRegion(writer);
			writer.WriteLine();
			WriteReadAttributeRegion(writer);
			writer.WriteLine();
			WriteReadElementOptRegion(writer);
			writer.WriteLine();
			WriteReadAttributeOptRegion(writer);
			writer.WriteLine();
			WriteReadElementsRegion(writer);
			writer.WriteLine();
			WriteReadFixedArrayRegion(writer);
			writer.WriteLine();
			WriteWriteElementImplRegion(writer);
			writer.WriteLine();
			WriteWriteCursorRegion(writer);
			writer.WriteLine();
			WriteWriteElementRegion(writer);
			writer.WriteLine();
			WriteWriteAttributeRegion(writer);
			writer.WriteLine();
			WriteWriteElementOptRegion(writer);
			writer.WriteLine();
			WriteWriteAttributeOptRegion(writer);
			writer.WriteLine();
			WriteWriteElementsRegion(writer);
			writer.WriteLine();
			WriteStreamCursorRegion(writer);
			writer.WriteLine();
			WriteStreamElementRegion(writer);
			writer.WriteLine();
			WriteStreamElementOptRegion(writer);
			writer.WriteLine();
			WriteStreamAttributeRegion(writer);
			writer.WriteLine();
			WriteStreamAttributeOptRegion(writer);
			writer.WriteLine();
			WriteStreamElementsRegion(writer);
			writer.WriteLine();
			WriteStreamFixedArrayRegion(writer);
		}
	}

	private static void WriteReadWriteBranch(SourceWriter writer, string readStatement, string writeStatement)
	{
		writer.WriteLine($"if (IsReading) {{ {readStatement} }}");
		writer.WriteLine($"else if (IsWriting) {{ {writeStatement} }}");
	}

	private static bool IsString(PrimitiveSpec typeSpec)
		=> typeSpec.TypeCode == TypeCode.String;
};
