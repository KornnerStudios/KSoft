using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.IO;

internal static class EndianStreamsCoreSourceBuilder
{
	public const string BaseHintName = "KSoft.IO.EndianStreams.Base.g.cs";
	public const string TypeExtensionsHintName = "KSoft.TypeExtensions.EndianStreams.g.cs";
	public const string VirtualAddressTranslationHintName = "KSoft.IO.EndianStreams.VirtualAddressTranslation.g.cs";

	// Maps to KSoft.T4.EndianStreamsT4.ClassNames; EndianStreams owns the reader/writer pair ordering.
	private static readonly string[] kEndianStreamClassNames =
		[
			"EndianReader",
			"EndianWriter",
		];

	public static string BuildBase()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.IO;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
		bool needsSeparator = false;
		foreach (string typeName in kEndianStreamClassNames)
		{
			if (needsSeparator)
			{
				writer.WriteLine();
			}
			WriteEndianStreamBaseType(writer, typeName);
			needsSeparator = true;
		}

		return writer.ToString();
	}

	public static string BuildTypeExtensions()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteFileScopedNamespace("KSoft");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class TypeExtensions"))
		{
			foreach (PrimitiveSpec typeSpec in PrimitiveCatalog.Primitives)
			{
				WriteTypeExtensionMethods(writer, typeSpec);
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	public static string BuildVirtualAddressTranslation()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.IO");
		writer.WriteLine();
		bool needsSeparator = false;
		foreach (string typeName in kEndianStreamClassNames)
		{
			if (needsSeparator)
			{
				writer.WriteLine();
			}
			WriteVirtualAddressTranslationType(writer, typeName);
			needsSeparator = true;
		}

		return writer.ToString();
	}

	private static void WriteEndianStreamBaseType(SourceWriter writer, string typeName)
	{
		using (writer.EnterTypeDeclaration($"partial class {typeName}"))
		{
			using (writer.EnterRegion("EndianStream"))
			{
				WriteCommonStateProperties(writer);
				WriteEndianSwitching(writer, typeName);
				writer.WriteLine();
				WriteStringEncodingRegion(writer);
				writer.WriteLine();
				WriteSeekRegion(writer);
				writer.WriteLine();
				WritePositionPtrRegion(writer);
			}
		}
	}

	private static void WriteCommonStateProperties(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Owner of this stream");
		writer.WriteLine("public object? Owner { get; set; }");
		writer.WriteLine();
		writer.WriteLine("public object? UserData { get; set; }");
		writer.WriteLine();
		writer.WriteXmlDocSummary("Do we own the base stream?");
		writer.WriteLine(
			"/// <remarks>If we don't own the stream, when this object is disposed, " +
			"the <see cref=\"BaseStream\"/> won't be closed\\disposed</remarks>");
		writer.WriteLine("public bool BaseStreamOwner { get; set; }");
		writer.WriteLine();
		writer.WriteXmlDocSummary("Name of the underlying stream this object is interfacing with");
		writer.WriteLine(
			"/// <remarks>So if this endian stream is interfacing with a file, this will be it's name</remarks>");
		writer.WriteLine("public string? StreamName { get; private set; }");
		writer.WriteLine();
		writer.WriteXmlDocSummary("Base address used for simulating pointers in the stream");
		writer.WriteLine("/// <remarks>Default value is <see cref=\"Data.PtrHandle.Null32\"/></remarks>");
		writer.WriteLine("public Values.PtrHandle BaseAddress { get; set; }");
		writer.WriteLine();
	}

	private static void WriteEndianSwitching(SourceWriter writer, string typeName)
	{
		using (writer.EnterRegion("IKSoftEndianStream"))
		{
			writer.WriteXmlDocSummary("The assumed byte order of the stream");
			writer.WriteLine("/// <remarks>Use <see cref=\"ChangeByteOrder\"/> to properly change this property</remarks>");
			writer.WriteLine("public Shell.EndianFormat ByteOrder { get; private set; }");
			writer.WriteLine();
			writer.WriteXmlDocSummary("Change the order in which bytes are ordered to/from the stream");
			writer.WriteXmlDocParam("newOrder", "The new byte order to switch to");
			writer.WriteLine(
				"/// <remarks>If <paramref name=\"newOrder\"/> is the same as <see cref=\"ByteOrder\"/> " +
				"nothing will happen</remarks>");
			writer.WriteLine("public void ChangeByteOrder(Shell.EndianFormat newOrder)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (newOrder != ByteOrder)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ByteOrder = newOrder;");
					writer.WriteLine("mRequiresByteSwap = !mRequiresByteSwap;");
				}
			}
			writer.WriteLine();
			writer.WriteLine(
				"/// <summary>This will be true when the stream's byte order is not the same as " +
				"the <see cref=\"Shell.Platform.Environment\"/>'s byte order</summary>");
			writer.WriteLine("/*readonly*/ bool mRequiresByteSwap;");
			writer.WriteLine();
			writer.WriteLine(
				"/// <summary>Convenience class for C# \"using\" statements where we want to temporarily inverse " +
				"the current byte order</summary>");
			WriteEndianFormatSwitchBlock(writer, typeName);
			writer.WriteLine();
			WriteBeginEndianSwitchMethods(writer);
		}
	}

	private static void WriteEndianFormatSwitchBlock(SourceWriter writer, string typeName)
	{
		using (writer.EnterTypeDeclaration("class EndianFormatSwitchBlock : IDisposable"))
		{
			writer.WriteLine($"readonly {typeName}? mStream;");
			writer.WriteLine("readonly Shell.EndianFormat mOldByteOrder;");
			writer.WriteLine("readonly bool mOldRequiresByteSwap;");
			writer.WriteXmlDocSummary("");
			writer.WriteXmlDocParam("s", "");
			writer.WriteXmlDocParam("requiresSwitch", "Is there an actual order switch even occurring?");
			writer.WriteLine($"public EndianFormatSwitchBlock({typeName} s, bool requiresSwitch)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("mStream = requiresSwitch");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("? s");
					writer.WriteLine(": null;");
				}
				writer.WriteLine();
				writer.WriteLine("if (requiresSwitch) // if not, don't do anything but keep the IDisposable wheel turning");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("mOldByteOrder = s.ByteOrder;");
					writer.WriteLine("mOldRequiresByteSwap = s.mRequiresByteSwap;");
					writer.WriteLine();
					writer.WriteLine("s.ChangeByteOrder(mOldByteOrder.Invert());");
				}
			}
			writer.WriteLine();
			using (writer.EnterRegion("IDisposable Members"))
			{
				writer.WriteLine("public void Dispose()");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("mStream?.ChangeByteOrder(mOldByteOrder);");
				}
			}
		}
	}

	private static void WriteBeginEndianSwitchMethods(SourceWriter writer)
	{
		writer.WriteLine(
			"/// <summary>Convenience method for C# \"using\" statements. Temporarily inverts " +
			"the current byte order which is used for read/writes.</summary>");
		writer.WriteLine(
			"/// <returns>Object which when Disposed will return this stream to its original " +
			"<see cref=\"Shell.EndianFormat\"/> state</returns>");
		writer.WriteLine("public IDisposable BeginEndianSwitch()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return new EndianFormatSwitchBlock(this, true);");
		}
		writer.WriteLine(
			"/// <summary>Convenience method for C# \"using\" statements. Temporarily inverts " +
			"the current byte order which is used for read/writes.</summary>");
		writer.WriteXmlDocParam("switchTo", "Byte order to switch to");
		writer.WriteLine(
			"/// <returns>Object which when Disposed will return this stream to its original " +
			"<see cref=\"Shell.EndianFormat\"/> state</returns>");
		writer.WriteLine("/// <remarks>");
		writer.WriteLine("/// If <paramref name=\"switchTo\"/> is the same as <see cref=\"EndianStream.State\"/>");
		writer.WriteLine("/// then no actual object state changes will happen. However, this construct");
		writer.WriteLine("/// will continue to be usable and will Dispose of properly with no error");
		writer.WriteLine("/// </remarks>");
		writer.WriteLine("public IDisposable BeginEndianSwitch(Shell.EndianFormat switchTo)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (switchTo == this.ByteOrder)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return Util.NullDisposable;");
			}
			writer.WriteLine();
			writer.WriteLine("return new EndianFormatSwitchBlock(this, true);");
		}
	}

	private static void WriteStringEncodingRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("StringEncoding"))
		{
			writer.WriteLine("System.Text.Encoding? mStringEncoding;");
		}
	}

	private static void WriteSeekRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("Seek"))
		{
			WriteSeek32Method(writer, "uint", "Begin");
			WriteSeek32Method(writer, "uint", "Origin");
			WriteSeek32Method(writer, "int", "Begin");
			WriteSeek32Method(writer, "int", "Origin");
			writer.WriteLine();
			WriteSeekMethod(writer, "long", "Begin");
			WriteSeekMethod(writer, "long", "Origin");
		}
	}

	private static void WriteSeek32Method(SourceWriter writer, string typeKeyword, string overload)
	{
		if (overload == "Origin")
		{
			writer.WriteLine(
				"/// <summary>Moves the stream cursor to <paramref name=\"offset\"/> relative to " +
				"<paramref name=\"origin\"/></summary>");
		}
		else
		{
			writer.WriteLine(
				"/// <summary>Moves the stream cursor to <paramref name=\"offset\"/> relative to " +
				"the beginning of the stream</summary>");
		}
		writer.WriteXmlDocParam("offset", "Offset to seek to");
		if (overload == "Origin")
		{
			writer.WriteXmlDocParam("origin", "Origin to base seek operation");
			writer.WriteLine($"public void Seek32({typeKeyword} offset, SeekOrigin origin)\t=> Seek(offset, origin);");
		}
		else
		{
			writer.WriteLine($"public void Seek32({typeKeyword} offset)\t\t\t\t\t\t=> Seek(offset, SeekOrigin.Begin);");
		}
	}

	private static void WriteSeekMethod(SourceWriter writer, string typeKeyword, string overload)
	{
		if (overload == "Origin")
		{
			writer.WriteLine(
				"/// <summary>Moves the stream cursor to <paramref name=\"offset\"/> relative to " +
				"<paramref name=\"origin\"/></summary>");
		}
		else
		{
			writer.WriteLine(
				"/// <summary>Moves the stream cursor to <paramref name=\"offset\"/> relative to " +
				"the beginning of the stream</summary>");
		}
		writer.WriteXmlDocParam("offset", "Offset to seek to");
		if (overload == "Origin")
		{
			writer.WriteXmlDocParam("origin", "Origin to base seek operation");
			writer.WriteLine(
				$"public void Seek({typeKeyword} offset, SeekOrigin origin)\t=> base.BaseStream.Seek(offset, origin);");
		}
		else
		{
			writer.WriteLine($"public void Seek({typeKeyword} offset)\t\t\t\t\t\t=> Seek(offset, SeekOrigin.Begin);");
		}
	}

	private static void WritePositionPtrRegion(SourceWriter writer)
	{
		using (writer.EnterRegion("PositionPtr"))
		{
			writer.WriteXmlDocSummary("Get the current position as a <see cref=\"Data.PtrHandle\"/>");
			writer.WriteXmlDocParam("ptrSize", "Pointer size to use for the result handle");
			writer.WriteXmlDocReturns();
			writer.WriteLine("public Values.PtrHandle GetPositionPtrWithExplicitWidth(Shell.ProcessorSize ptrSize) =>");
			writer.WriteLine("\tnew(ptrSize, (ulong)BaseStream.Position);");
			writer.WriteLine();
			writer.WriteXmlDocSummary("Current position as a <see cref=\"Data.PtrHandle\"/>");
			writer.WriteLine("/// <remarks>Pointer traits\\info is inherited from <see cref=\"BaseAddress\"/></remarks>");
			writer.WriteLine("public Values.PtrHandle PositionPtr =>");
			writer.WriteLine("\tnew(BaseAddress, (ulong)BaseStream.Position);");
		}
	}

	private static void WriteTypeExtensionMethods(SourceWriter writer, PrimitiveSpec typeSpec)
	{
		string code = typeSpec.TypeCode.ToString();

		writer.WriteLine(
			$"public static void Read(this IO.EndianReader s, out {typeSpec.Keyword} value)\t=> value = s.Read{code}();");
		writer.WriteLine($"public static void Write(this {typeSpec.Keyword} value, IO.EndianWriter s)\t\t=> s.Write(value);");
	}

	private static void WriteVirtualAddressTranslationType(SourceWriter writer, string typeName)
	{
		using (writer.EnterTypeDeclaration($"partial class {typeName}"))
		{
			using (writer.EnterRegion("VirtualAddressTranslation"))
			{
				writer.WriteLine("Memory.VirtualAddressTranslationStack? mVAT;");
				writer.WriteLine();
				WriteVerifyVATMethod(writer);
				WriteVirtualAddressTranslationInitializeMethod(writer);
				WriteVirtualAddressTranslationPushMethod(writer);
				WriteVirtualAddressTranslationPushPositionMethod(writer);
				WriteVirtualAddressTranslationIncreaseMethod(writer);
				WriteVirtualAddressTranslationPopMethod(writer);
			}
		}
	}

	private static void WriteVerifyVATMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Verify the state of the VAT (is it initialized?)");
		writer.WriteLine("[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(mVAT))]");
		writer.WriteLine("void VerifyVAT()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (mVAT == null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("throw new InvalidOperationException(\"VAT uninitialized\");");
			}
		}
	}

	private static void WriteVirtualAddressTranslationInitializeMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Initialize the VAT with a specific handle size and initial table capacity");
		writer.WriteXmlDocParam("vaSize", "Handle size");
		writer.WriteXmlDocParam("translationCapacity", "The initial table capacity");
		writer.WriteLine(
			"public void VirtualAddressTranslationInitialize(Shell.ProcessorSize vaSize, int translationCapacity = 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (mVAT == null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("mVAT = new Memory.VirtualAddressTranslationStack(vaSize, translationCapacity);");
				writer.WriteLine("mVAT.PushNull(); // implicitly use null as our initial VA translator");
			}
		}
	}

	private static void WriteVirtualAddressTranslationPushMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Push a PA into to the VAT table, setting the current PA in the process");
		writer.WriteXmlDocParam("physicalAddress", "PA to push and to use as the VAT's current address");
		writer.WriteLine("public void VirtualAddressTranslationPush(Values.PtrHandle physicalAddress)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("VerifyVAT();");
			writer.WriteLine();
			writer.WriteLine("mVAT.PushPhysicalAddress(physicalAddress);");
		}
	}

	private static void WriteVirtualAddressTranslationPushPositionMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Push the stream's position (as a physical address) into the VAT table");
		writer.WriteLine("public void VirtualAddressTranslationPushPosition()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("VirtualAddressTranslationPush(PositionPtr);");
		}
	}

	private static void WriteVirtualAddressTranslationIncreaseMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Increase the current address (PA) by a relative offset");
		writer.WriteXmlDocParam("relativeOffset", "Offset, relative to the current address");
		writer.WriteLine("public void VirtualAddressTranslationIncrease(Values.PtrHandle relativeOffset)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("VerifyVAT();");
			writer.WriteLine();
			writer.WriteLine("mVAT.PushPhysicalAddressOffset(relativeOffset);");
		}
	}

	private static void WriteVirtualAddressTranslationPopMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Pop and return the current address (PA) in the VAT table");
		writer.WriteXmlDocReturns("The VAT's current address value before this call");
		writer.WriteLine("public Values.PtrHandle VirtualAddressTranslationPop()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("VerifyVAT();");
			writer.WriteLine();
			writer.WriteLine("if (mVAT.Count == 1)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("throw new InvalidOperationException(\"Pop underflow\");");
			}
			writer.WriteLine();
			writer.WriteLine("return mVAT.PopPhysicalAddress();");
		}
	}
};
