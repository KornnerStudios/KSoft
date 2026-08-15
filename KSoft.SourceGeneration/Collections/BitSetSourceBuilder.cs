using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Collections;

internal static class BitSetSourceBuilder
{
	public const string BitSetHintName = "KSoft.Collections.BitSet.g.cs";
	public const string EnumeratorsHintName = "KSoft.Collections.IReadOnlyBitSet.Enumerators.g.cs";

	public static string BuildBitSet()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteContractsAliasUsing();
		writer.WriteLine();
		writer.WriteLine(
			"using StateFilterEnumerator = KSoft.Collections.IReadOnlyBitSetEnumerators.StateFilterEnumerator;");
		writer.WriteLine(
			"using StateFilterEnumeratorWrapper = KSoft.EnumeratorWrapper<int, " +
			"KSoft.Collections.IReadOnlyBitSetEnumerators.StateFilterEnumerator>;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Collections");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("partial class BitSet"))
		{
			WriteBitStateMembers(writer);
			writer.WriteLine();
			foreach (BitOperationSpec operation in BitSetCatalog.BitOperations)
			{
				WriteBitOperation(writer, operation);
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	public static string BuildEnumerators()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Collections");
		writer.WriteLine();
		using (writer.EnterTypeDeclaration("static partial class IReadOnlyBitSetEnumerators"))
		{
			// The generated half owns the shared backing fields so the handwritten MoveNext halves stay field-order safe.
			foreach (BitSetEnumeratorSpec spec in BitSetCatalog.Enumerators)
			{
				WriteEnumerator(writer, spec);
				writer.WriteLine();
			}
		}

		return writer.ToString();
	}

	private static void WriteBitStateMembers(SourceWriter writer)
	{
		foreach (BitStateSpec state in BitSetCatalog.BitStates)
		{
			writer.WriteXmlDocSummary($"Get the bit index of the next bit which is {state.DocNameVerbose}");
			writer.WriteXmlDocParam("startBitIndex", "Bit index to start at");
			writer.WriteXmlDocReturns($"The next {state.DocName} bit index, or -1 if one isn't found");
			writer.WritePurityAnnotation();
			writer.WriteLine($"public int Next{state.ApiName}BitIndex(int startBitIndex = 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"return NextBitIndex(startBitIndex, {state.ValueKeyword});");
			}
			writer.WriteXmlDocSummary($"Enumeration of bit indexes in this BitSet which are {state.DocNameVerbose}");
			writer.WriteLine($"public StateFilterEnumeratorWrapper {state.ApiName}BitIndices");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine($"=> new(new StateFilterEnumerator(this, {state.ValueKeyword}));");
			}
			writer.WriteXmlDocSummary($"Enumeration of bit indexes in this BitSet which are {state.DocNameVerbose}");
			writer.WriteXmlDocParam("startBitIndex", "Bit index to start at");
			writer.WriteLine($"public StateFilterEnumeratorWrapper {state.ApiName}BitIndicesStartingAt(int startBitIndex)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("ThrowIfBitIndexOutOfRange(startBitIndex, nameof(startBitIndex));");
				writer.WriteLine();
				writer.WriteLine($"return new(new StateFilterEnumerator(this, {state.ValueKeyword}, startBitIndex));");
			}
			writer.WriteLine();
		}
	}

	private static void WriteBitOperation(SourceWriter writer, BitOperationSpec operation)
	{
		if (operation.IsPure)
		{
			writer.WritePurityAnnotation();
		}
		writer.WriteLine($"public {operation.ResultType} {operation.Name}Bits(int startBitIndex, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			if (!operation.IsPure)
			{
				writer.WriteLine("ThrowIfBitRangeOutOfRange(startBitIndex, bitCount);");
				writer.WriteLine();
			}
			writer.WriteLine("if (bitCount <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(operation.DefaultReturn);
			}
			writer.WriteLine();
			writer.WriteLine("var from_word_mask = kVectorElementSectionBitMask(startBitIndex);");
			writer.WriteLine("var last_word_mask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);");
			writer.WriteLine();
			writer.WriteLine("int last_bit_index = (startBitIndex+bitCount) - 1;");
			writer.WriteLine("var from_word_index = kVectorIndexInT(startBitIndex);");
			writer.WriteLine("var last_word_index = kVectorIndexInT(last_bit_index);");
			writer.WriteLine();
			writer.WriteLine("// target bits are only in one word...");
			writer.WriteLine("if (from_word_index == last_word_index)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("var mask = from_word_mask;// & last_word_mask;");
				WriteSingleWordOperation(writer, operation);
			}
			writer.WriteLine("// or the target bits are in multiple words...");
			writer.WriteLine();
			writer.WriteLine("// handle the first word");
			WriteFirstWordOperation(writer, operation);
			writer.WriteLine();
			writer.WriteLine("// handle any words in between");
			writer.WriteLine("for (int x = from_word_index+1; x < last_word_index; x++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteMiddleWordOperation(writer, operation);
			}
			writer.WriteLine();
			writer.WriteLine("// handle the last word");
			WriteLastWordOperation(writer, operation);
		}
	}

	private static void WriteSingleWordOperation(SourceWriter writer, BitOperationSpec operation)
	{
		if (!operation.IsPure)
		{
			writer.WriteLine("RecalculateCardinalityUndoRound(from_word_index);");
			writer.WriteLine($"{operation.FlagsMethod}(ref mArray[from_word_index], mask);");
			if (operation.RequiresCardinalityUpdate)
			{
				writer.WriteLine("RecalculateCardinalityRound(from_word_index);");
			}
			writer.WriteLine("return;");
		}
		else
		{
			writer.WriteLine($"return {operation.FlagsMethod}(mArray[from_word_index], mask);");
		}
	}

	private static void WriteFirstWordOperation(SourceWriter writer, BitOperationSpec operation)
	{
		if (!operation.IsPure)
		{
			writer.WriteLine("RecalculateCardinalityUndoRound(from_word_index);");
			writer.WriteLine($"{operation.FlagsMethod}(ref mArray[from_word_index], from_word_mask);");
			if (operation.RequiresCardinalityUpdate)
			{
				writer.WriteLine("RecalculateCardinalityRound(from_word_index);");
			}
		}
		else
		{
			writer.WriteLine($"if ({operation.FlagsMethod}(mArray[from_word_index], from_word_mask))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return true;");
			}
		}
	}

	private static void WriteMiddleWordOperation(SourceWriter writer, BitOperationSpec operation)
	{
		if (!operation.IsPure)
		{
			writer.WriteLine("RecalculateCardinalityUndoRound(x);");
			if (operation.Kind == BitOperationKind.Toggle)
			{
				writer.WriteLine($"{operation.FlagsMethod}(ref mArray[x], mArray[x]);");
			}
			else
			{
				string newValue = operation.Kind == BitOperationKind.Set
					? "kWordAllBitsSet"
					: "kWordAllBitsClear";
				writer.WriteLine($"mArray[x] = {newValue};");
			}
			if (operation.Kind == BitOperationKind.Set)
			{
				writer.WriteLine("Cardinality += kWordBitCount;");
			}
			else if (operation.Kind == BitOperationKind.Toggle)
			{
				writer.WriteLine("RecalculateCardinalityRound(x);");
			}
		}
		else
		{
			writer.WriteLine("if (mArray[x] > kWordAllBitsClear)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return true;");
			}
		}
	}

	private static void WriteLastWordOperation(SourceWriter writer, BitOperationSpec operation)
	{
		if (!operation.IsPure)
		{
			writer.WriteLine("RecalculateCardinalityUndoRound(last_word_index);");
			writer.WriteLine($"{operation.FlagsMethod}(ref mArray[last_word_index], last_word_mask);");
			if (operation.RequiresCardinalityUpdate)
			{
				writer.WriteLine("RecalculateCardinalityRound(last_word_index);");
			}
		}
		else
		{
			writer.WriteLine($"return {operation.FlagsMethod}(mArray[last_word_index], last_word_mask);");
		}
	}

	private static void WriteEnumerator(SourceWriter writer, BitSetEnumeratorSpec spec)
	{
		writer.WriteLine("[Serializable]");
		writer.WriteLine($"partial struct {spec.Name}Enumerator");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($": IEnumerator< {spec.ResultKeyword} >");
		}
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine("readonly IReadOnlyBitSet mSet;");
			writer.WriteLine("readonly int mLastIndex;");
			writer.WriteLine("readonly int mVersion;");
			writer.WriteLine("int mBitIndex;");
			writer.WriteLine($"{spec.ResultKeyword} mCurrent;");
			if (spec.HasStateFilterFields)
			{
				writer.WriteLine(
					"// defined here to avoid: CS0282: There is no defined ordering between fields in multiple " +
					"declarations of partial class or struct");
				writer.WriteLine("readonly bool mStateFilter;");
				writer.WriteLine("readonly int mStartBitIndex;");
			}
			writer.WriteLine();
			writer.WriteLine($"{spec.Name}Enumerator(IReadOnlyBitSet bitset,");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("[SuppressMessage(\"Microsoft.Design\", \"CA1801:ReviewUnusedParameters\")]");
				writer.WriteLine("[SuppressMessage(\"Microsoft.Design\", \"IDE0060:ReviewUnusedParameters\")]");
				writer.WriteLine("bool dummy)");
			}
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine(": this()");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("mSet = bitset;");
				writer.WriteLine("mLastIndex = bitset.Length - 1;");
				writer.WriteLine("mVersion = bitset.Version;");
				writer.WriteLine("mBitIndex = TypeExtensions.kNone;");
			}
			writer.WriteLine();
			writer.WriteLine("readonly void VerifyVersion()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (mVersion != mSet.Version)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						"throw new InvalidOperationException(\"Collection was modified; enumeration operation may not execute.\");");
				}
			}
			writer.WriteLine();
			writer.WriteLine($"public readonly {spec.ResultKeyword} Current {{ get {{");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine(
					"if (mBitIndex.IsNone())\t\t\t{ throw new InvalidOperationException(\"Enumeration has not started\"); }");
				writer.WriteLine(
					"if (mBitIndex > mLastIndex)\t\t{ throw new InvalidOperationException(\"Enumeration already finished\"); }");
				writer.WriteLine();
				writer.WriteLine("return mCurrent;");
			}
			writer.WriteLine("} }");
			writer.WriteLine("readonly object System.Collections.IEnumerator.Current { get => this.Current; }");
			writer.WriteLine();
			writer.WriteLine("public void Reset()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("VerifyVersion();");
				writer.WriteLine("mBitIndex = TypeExtensions.kNone;");
			}
			writer.WriteLine();
			writer.WriteLine("public readonly void Dispose()\t{ }");
		}
	}

}
