using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static partial class TagElementStreamsSourceBuilder
{
	private static void WriteTagElementStreamContract(SourceWriter writer)
	{
		using (writer.EnterTypeDeclaration("partial class TagElementStreamContract<TDoc, TCursor, TName>"))
		{
			WriteContractReadAttributeRegion(writer);
			writer.WriteLine();
			WriteContractReadElementOptRegion(writer);
			writer.WriteLine();
			WriteContractReadAttributeOptRegion(writer);
			writer.WriteLine();
			WriteContractWriteAttributeRegion(writer);
		}
	}

	private static void WriteContractReadAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteContractThrowingRead(writer, "ReadAttribute", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteContractThrowingRead(writer, "ReadAttribute", typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteContractReadElementOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadElementOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteContractThrowingRead(writer, "ReadElementOpt", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteContractThrowingRead(writer, "ReadElementOpt", typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteContractReadAttributeOptRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("ReadAttributeOpt"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteContractThrowingRead(writer, "ReadAttributeOpt", typeSpec.Keyword, includeBase: false);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteContractThrowingRead(writer, "ReadAttributeOpt", typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteContractThrowingRead(SourceWriter writer, string methodName, string keyword, bool includeBase)
	{
		string returnType = methodName.EndsWith("Opt", StringComparison.Ordinal)
			? "bool"
			: "void";
		string baseParameter = includeBase
			? ", NumeralBase fromBase"
			: "";

		writer.WriteLine($"public override {returnType} {methodName}(TName name, ref {keyword} value{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			writer.WriteLine("throw new NotImplementedException();");
		}
	}

	private static void WriteContractWriteAttributeRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("WriteAttribute"))
		{
			foreach (PrimitiveSpec typeSpec in MiscTypes())
			{
				WriteContractWriteAttribute(writer, typeSpec.Keyword);
			}

			writer.WriteLine();
			foreach (NumberSpec typeSpec in IntegerTypes())
			{
				WriteContractWriteAttribute(writer, typeSpec.Keyword, includeBase: true);
			}
		}
	}

	private static void WriteContractWriteAttribute(SourceWriter writer, string keyword, bool includeBase = false)
	{
		string baseParameter = includeBase
			? ", NumeralBase toBase"
			: "";

		writer.WriteLine($"public override void WriteAttribute(TName name, {keyword} value{baseParameter})");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires(ValidateNameArg(name));");
			if (keyword == "string")
			{
				writer.WriteLine("ArgumentNullException.ThrowIfNull(value);");
			}

			writer.WriteLine("Contract.Requires(Cursor != null, kCursorNullMsg);");
			writer.WriteLine();
			writer.WriteLine("throw new NotImplementedException();");
		}
	}
};
