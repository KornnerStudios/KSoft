using System;
using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Collections;

internal static class BitVectorsSourceBuilder
{
	public const string HintName = "KSoft.Collections.BitVectors.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();

		writer.WriteGeneratedFileHeader();
		writer.WriteLine("#nullable disable");
		writer.WriteLine();
		writer.WriteLine("using System;");
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
		writer.WriteContractsAliasUsing();
		writer.WriteContractShimAliasUsing();
		writer.WriteLine();
		writer.WriteFileScopedNamespace("KSoft.Collections");
		writer.WriteLine();

		foreach (NumberSpec wordSpec in PrimitiveCatalog.BittableTypesMajorWords)
		{
			WriteBitVector(writer, new BitVectorSpec(wordSpec));
			writer.WriteLine();
		}

		return writer.ToString();
	}

	private static void WriteBitVector(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine("[System.Diagnostics.DebuggerDisplay(\"Data = {mWord}, Cardinality = {Cardinality}\")]");
		writer.WriteLine($"public struct {spec.TypeName}");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($": IComparable<{spec.TypeName}>");
			writer.WriteLine($", IEquatable<{spec.TypeName}>");
		}

		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			WriteFieldsAndConstructors(writer, spec);
			writer.WriteLine();
			WriteOverridesRegion(writer, spec);
			writer.WriteLine();
			WriteAccessRegion(writer, spec);
			writer.WriteLine();
			WriteRangedAccessRegion(writer, spec);
			writer.WriteLine();
			WriteBitOperationsRegion(writer, spec);
			writer.WriteLine();
			WriteClearAndSetAll(writer, spec);
			writer.WriteLine();
			writer.WriteLine($"public readonly int CompareTo({spec.TypeName} other)");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> mWord.CompareTo(other.mWord);");
			}
			writer.WriteLine();
			WriteMathOperatorsRegion(writer, spec);
			writer.WriteLine();
			WriteEnumeratorsRegion(writer, spec);
			writer.WriteLine();
			WriteEnumeratorImplsRegion(writer, spec);
			writer.WriteLine();
			WriteEnumInterfacesRegion(writer, spec);
		}
	}

	private static void WriteFieldsAndConstructors(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine($"const int kNumberOfBits = Bits.k{spec.ConstantKeyword}BitCount;");
		writer.WriteLine("// for Enumerators impl");
		writer.WriteLine("const int kLastIndex = kNumberOfBits - 1;");
		writer.WriteLine("const Shell.EndianFormat kVectorWordFormat = Shell.EndianFormat.Little;");
		writer.WriteLine();
		writer.WriteLine($"{spec.WordKeyword} mWord;");
		writer.WriteLine();
		writer.WriteLine($"public {spec.TypeName}({spec.WordKeyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("mWord = bits;");
		}
		writer.WriteLine($"public {spec.TypeName}({spec.SignedKeyword} bits)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"mWord = ({spec.WordKeyword})bits;");
		}
		writer.WriteLine();
		writer.WriteLine($"public readonly {spec.SignedKeyword} Data => ({spec.SignedKeyword})mWord;");
		writer.WriteLine();
		writer.WriteXmlDocSummary($"Length in bits. Always returns {spec.BitCount}");
		writer.WriteAttribute("SuppressMessage(\"Performance\", \"CA1822:Mark members as static\")");
		writer.WriteLine("public readonly int Length => kNumberOfBits;");
		writer.WriteXmlDocSummary("Number of bits set to true");
		writer.WriteLine("public readonly int Cardinality => Bits.BitCount(mWord);");
		writer.WriteXmlDocSummary("Number of bits set to false");
		writer.WriteLine("public readonly int CardinalityZeros => Length - Cardinality;");
		writer.WriteLine();
		writer.WriteXmlDocSummary("Are all the bits in this set currently false?");
		writer.WriteLine($"public readonly bool IsAllClear => mWord == {spec.WordKeyword}.MinValue;");
		writer.WriteXmlDocSummary("Are all the bits in this set currently true?");
		writer.WriteLine($"public readonly bool IsAllSet => mWord == {spec.WordKeyword}.MaxValue;");
		writer.WriteLine();
		writer.WriteLine("public readonly int TrailingZerosCount => Bits.TrailingZerosCount(mWord);");
		writer.WriteLine("public readonly int IndexOfHighestBitSet => Bits.IndexOfHighestBitSet(mWord);");
	}

	private static void WriteOverridesRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Overrides"))
		{
			writer.WriteLine($"public readonly bool Equals({spec.TypeName} other)");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> mWord == other.mWord;");
			}
			writer.WriteLine("public override readonly bool Equals(object o)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"if (o is not {spec.TypeName})");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("return false;");
				}
				writer.WriteLine();
				writer.WriteLine($"return Equals(({spec.TypeName})o);");
			}
			writer.WriteLine($"public static bool operator ==({spec.TypeName} x, {spec.TypeName} y)");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> x.Equals(y);");
			}
			writer.WriteLine($"public static bool operator !=({spec.TypeName} x, {spec.TypeName} y)");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> !x.Equals(y);");
			}
			writer.WriteLine();
			WriteComparisonOperator(writer, spec, "<");
			WriteComparisonOperator(writer, spec, "<=");
			WriteComparisonOperator(writer, spec, ">");
			WriteComparisonOperator(writer, spec, ">=");
			writer.WriteLine();
			writer.WriteLine("public override readonly int GetHashCode()");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> mWord.GetHashCode();");
			}
			writer.WriteLine();
			WriteToStringMethod(writer, spec);
		}
	}

	private static void WriteComparisonOperator(SourceWriter writer, BitVectorSpec spec, string operatorText)
	{
		writer.WriteLine(
			$"public static bool operator {operatorText}({spec.TypeName} left, {spec.TypeName} right)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> left.CompareTo(right) {operatorText} 0;");
		}
	}

	private static void WriteToStringMethod(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine($"public static string ToString({spec.TypeName} value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"const {spec.SignedKeyword} k_msb = 1 << (kNumberOfBits-1);");
			writer.WriteLine();
			writer.WriteLine(
				$"var sb = new System.Text.StringBuilder(/*\"{spec.TypeName}{{\".Length*/12 + " +
				"kNumberOfBits + /*\"}\".Length\"*/1);");
			writer.WriteLine($"sb.Append(\"{spec.TypeName}{{\");");
			writer.WriteLine("var word = value.Data;");
			writer.WriteLine("for (int i = 0; i < kNumberOfBits; i++)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("sb.Append((word & k_msb) != 0");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("? '1'");
					writer.WriteLine(": '0');");
				}
				writer.WriteLine();
				writer.WriteLine("word <<= 1;");
			}
			writer.WriteLine("sb.Append('}');");
			writer.WriteLine("return sb.ToString();");
		}
		writer.WriteLine("public override readonly string ToString()");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> {spec.TypeName}.ToString(this);");
		}
	}

	private static void WriteAccessRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Access"))
		{
			WriteBitIndexer(writer, spec);
			writer.WriteXmlDocSummary("Tests the states of a range of bits");
			writer.WriteXmlDocParam("frombitIndex", "bit index to start reading from (inclusive)");
			writer.WriteXmlDocParam("toBitIndex", "bit index to stop reading at (exclusive)");
			writer.WriteXmlDocReturns("True if any bits are set, false if they're all clear");
			writer.WriteLine(
				"/// <remarks>If <paramref name=\"toBitIndex\"/> == <paramref name=\"frombitIndex\"/> " +
				"this will always return false</remarks>");
			WriteRangeIndexer(writer);
			writer.WriteLine();
			writer.WritePurityAnnotation();
			writer.WriteLine("public readonly int NextBitIndex(");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("int prevBitIndex = TypeExtensions.kNone, bool stateFilter = true)");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(
					$"Contract.Requires(prevBitIndex.IsNoneOrPositive() && " +
					$"prevBitIndex < Bits.k{spec.ConstantKeyword}BitCount);");
				writer.WriteLine();
				writer.WriteLine("for (int bit_index = prevBitIndex+1; bit_index < kNumberOfBits; bit_index++)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("if (this[bit_index] == stateFilter)");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("return bit_index;");
					}
				}
				writer.WriteLine();
				writer.WriteLine("return TypeExtensions.kNone;");
			}
		}
	}

	private static void WriteBitIndexer(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine("public bool this[int bitIndex]");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("readonly get");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"Contract.Requires(bitIndex >= 0 && bitIndex < Bits.k{spec.ConstantKeyword}BitCount);");
				writer.WriteLine();
				writer.WriteLine($"return Bitwise.Flags.Test(mWord, (({spec.WordKeyword})1) << bitIndex);");
			}
			writer.WriteLine("set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"Contract.Requires(bitIndex >= 0 && bitIndex < Bits.k{spec.ConstantKeyword}BitCount);");
				writer.WriteLine();
				writer.WriteLine($"var flag = (({spec.WordKeyword})1) << bitIndex;");
				writer.WriteLine();
				writer.WriteLine("Bitwise.Flags.Modify(value, ref mWord, flag);");
			}
		}
	}

	private static void WriteRangeIndexer(SourceWriter writer)
	{
		writer.WriteLine("public bool this[int frombitIndex, int toBitIndex]");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("readonly get");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteRangeContracts(writer);
				writer.WriteLine();
				writer.WriteLine("int bitCount = toBitIndex - frombitIndex;");
				writer.WriteLine("return bitCount > 0 && TestBits(frombitIndex, bitCount);");
			}
			writer.WriteLine("set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteRangeContracts(writer);
				writer.WriteLine();
				writer.WriteLine("// handle the cases of the set already being all 1's or 0's");
				writer.WriteLine("if (value && Cardinality == Length)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("return;");
				}
				writer.WriteLine("if (!value && CardinalityZeros == Length)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("return;");
				}
				writer.WriteLine();
				writer.WriteLine("int bitCount = toBitIndex - frombitIndex;");
				writer.WriteLine("if (bitCount == 0)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("return;");
				}
				writer.WriteLine();
				writer.WriteLine("if (value)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("SetBits(frombitIndex, bitCount);");
				}
				writer.WriteLine("else");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("ClearBits(frombitIndex, bitCount);");
				}
			}
		}
	}

	private static void WriteRangeContracts(SourceWriter writer)
	{
		writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < Length);");
		writer.WriteLine(
			"Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= Length);");
	}

	private static void WriteRangedAccessRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Access (ranged)"))
		{
			WriteRangedOperation(writer, spec, "ClearBits", "void", "return;", "Remove", false);
			writer.WriteLine();
			WriteRangedOperation(writer, spec, "SetBits", "void", "return;", "Add", false);
			writer.WriteLine();
			WriteRangedOperation(writer, spec, "ToggleBits", "void", "return;", "Toggle", false);
			writer.WriteLine();
			WriteRangedOperation(writer, spec, "TestBits", "bool", "return false;", "TestAny", true);
		}
	}

	private static void WriteRangedOperation(
		SourceWriter writer,
		BitVectorSpec spec,
		string methodName,
		string returnKeyword,
		string defaultReturnStatement,
		string flagsMethod,
		bool isPure)
	{
		if (isPure)
		{
			writer.WritePurityAnnotation();
		}
		writer.WriteLine(
			$"public {(isPure ? "readonly " : "")}{returnKeyword} {methodName}" +
			"(int startBitIndex, int bitCount)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < Length);");
			writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= Length);");
			writer.WriteLine();
			writer.WriteLine("if (bitCount <= 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine(defaultReturnStatement);
			}
			writer.WriteLine();
			writer.WriteLine(
				$"var from_word_mask = Bits.VectorElementSectionBitMaskIn{spec.ConstantKeyword}" +
				"(startBitIndex, kVectorWordFormat);");
			writer.WriteLine(
				$"//			var last_word_mask = Bits.VectorElementBitMaskIn{spec.ConstantKeyword}" +
				"(startBitIndex+bitCount, kVectorWordFormat);");
			writer.WriteLine("// create a mask for all bits below the given length in a caboose word");
			writer.WriteLine("//			last_word_mask -= 1;");
			writer.WriteLine();
			writer.WriteLine("var mask = from_word_mask;// & last_word_mask;");
			if (isPure)
			{
				writer.WriteLine($"return Bitwise.Flags.{flagsMethod}(mWord, mask);");
			}
			else
			{
				writer.WriteLine($"Bitwise.Flags.{flagsMethod}(ref mWord, mask);");
			}
		}
	}

	private static void WriteBitOperationsRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Bit Operations"))
		{
			WriteVectorOperation(writer, spec, "Bit AND this vector with another", "And", "mWord & vector.mWord");
			WriteVectorOperation(writer, spec, "", "BitwiseAnd", "mWord & vector.mWord");
			WriteVectorOperation(
				writer,
				spec,
				"Clears all of the bits in this vector whose corresponding bit is set in the specified vector",
				"AndNot",
				"Bitwise.Flags.Remove(mWord, vector.mWord)",
				"vector with which to mask this vector");
			WriteVectorOperation(writer, spec, "Bit OR this set with another", "Or", "mWord | vector.mWord",
				"Vector with the bits to OR with");
			WriteVectorOperation(writer, spec, "", "BitwiseOr", "mWord | vector.mWord");
			WriteVectorOperation(writer, spec, "Bit XOR this vector with another", "Xor",
				"Bitwise.Flags.Toggle(mWord, vector.mWord)", "Vector with the bits to XOR with");
			writer.WriteLine();
			writer.WriteXmlDocSummary("Inverts all bits in this vector");
			writer.WriteXmlDocReturns("");
			writer.WritePurityAnnotation();
			writer.WriteLine($"public readonly {spec.TypeName} Not()");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> new(~mWord);");
			}
			writer.WritePurityAnnotation();
			writer.WriteLine($"public readonly {spec.TypeName} OnesComplement()");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> new(~mWord);");
			}
		}
	}

	private static void WriteVectorOperation(
		SourceWriter writer,
		BitVectorSpec spec,
		string summary,
		string methodName,
		string expression,
		string parameterText = "Vector with the bits to AND with")
	{
		if (summary.Length != 0)
		{
			writer.WriteXmlDocSummary(summary);
			writer.WriteXmlDocParam("vector", parameterText);
			writer.WriteXmlDocReturns("");
		}
		writer.WritePurityAnnotation();
		writer.WriteLine($"public readonly {spec.TypeName} {methodName}({spec.TypeName} vector)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> new({expression});");
		}
	}

	private static void WriteClearAndSetAll(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteXmlDocSummary("Set all the bits to zero");
		writer.WriteLine("public void Clear()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("mWord = 0;");
		}
		writer.WriteLine();
		writer.WriteLine("public void SetAll(bool value)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("var fill_value = value");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine($"? {spec.WordKeyword}.MaxValue");
				writer.WriteLine($": {spec.WordKeyword}.MinValue;");
			}
			writer.WriteLine();
			writer.WriteLine("mWord = fill_value;");
		}
	}

	private static void WriteMathOperatorsRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Math operators"))
		{
			WriteMathOperator(writer, spec, "&");
			WriteMathOperator(writer, spec, "|");
			WriteMathOperator(writer, spec, "^");
			writer.WriteLine();
			writer.WriteLine($"public static {spec.TypeName} operator ~({spec.TypeName} value)");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("=> new(~value.mWord);");
			}
		}
	}

	private static void WriteMathOperator(SourceWriter writer, BitVectorSpec spec, string operatorText)
	{
		writer.WriteLine($"public static {spec.TypeName} operator {operatorText}({spec.TypeName} lhs, {spec.TypeName} rhs)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> new(lhs.mWord {operatorText} rhs.mWord);");
		}
	}

	private static void WriteEnumeratorsRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Enumerators"))
		{
			WriteStateEnumeratorAccess(writer, spec, "Clear", "0 (clear)", "clear", "false");
			WriteStateEnumeratorAccess(writer, spec, "Set", "1 (set)", "set", "true");
		}
	}

	private static void WriteStateEnumeratorAccess(
		SourceWriter writer,
		BitVectorSpec spec,
		string apiName,
		string docNameVerbose,
		string docName,
		string valueKeyword)
	{
		writer.WriteXmlDocSummary($"Get the bit index of the next bit which is {docNameVerbose}");
		writer.WriteXmlDocParam("startBitIndex", "Bit index to start at");
		writer.WriteXmlDocReturns($"The next {docName} bit index, or -1 if one isn't found");
		writer.WriteLine($"public readonly int Next{apiName}BitIndex(int startBitIndex = -1)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> NextBitIndex(startBitIndex, {valueKeyword});");
		}
		writer.WriteXmlDocSummary($"Enumeration of bit indexes in this vector which are {docNameVerbose}");
		writer.WriteLine($"public readonly EnumeratorWrapper<int, StateFilterEnumerator> {apiName}BitIndices");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"=> new(new StateFilterEnumerator(this, {valueKeyword}));");
		}
	}

	private static void WriteEnumeratorImplsRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Enumerators impls"))
		{
			WriteStateEnumerator(writer, spec);
			writer.WriteLine();
			WriteStateFilterEnumerator(writer, spec);
		}
	}

	private static void WriteStateEnumerator(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine("public struct StateEnumerator");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(": IEnumerator< bool >");
		}
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine($"readonly {spec.TypeName} mVector;");
			writer.WriteLine("int mBitIndex;");
			writer.WriteLine("bool mCurrent;");
			writer.WriteLine();
			writer.WriteLine($"public StateEnumerator({spec.TypeName} vector");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine(")");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("mVector = vector;");
				writer.WriteLine("mBitIndex = TypeExtensions.kNone;");
				writer.WriteLine("mCurrent = default;");
			}
			writer.WriteLine();
			WriteEnumeratorCurrent(writer, "bool");
			writer.WriteLine();
			WriteEnumeratorResetAndDispose(writer);
			writer.WriteLine();
			writer.WriteLine("public bool MoveNext()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (mBitIndex < kLastIndex)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("mCurrent = mVector[++mBitIndex];");
					writer.WriteLine("return true;");
				}
				writer.WriteLine();
				writer.WriteLine("mBitIndex = kNumberOfBits;");
				writer.WriteLine("return false;");
			}
		}
	}

	private static void WriteStateFilterEnumerator(SourceWriter writer, BitVectorSpec spec)
	{
		writer.WriteLine("public struct StateFilterEnumerator");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(": IEnumerator< int >");
		}
		using (writer.EnterBlock(SourceWriterBlockType.BracesStatement))
		{
			writer.WriteLine($"readonly {spec.TypeName} mVector;");
			writer.WriteLine("int mBitIndex;");
			writer.WriteLine("int mCurrent;");
			writer.WriteLine("readonly bool mStateFilter;");
			writer.WriteLine("readonly int mStartBitIndex;");
			writer.WriteLine();
			writer.WriteLine($"public StateFilterEnumerator({spec.TypeName} vector");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine(", bool stateFilter, int startBitIndex = 0");
				writer.WriteLine(")");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);");
				writer.WriteLine("Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < vector.Length);");
				writer.WriteLine();
				writer.WriteLine("mStateFilter = stateFilter;");
				writer.WriteLine("mStartBitIndex = startBitIndex-1;");
				writer.WriteLine("mVector = vector;");
				writer.WriteLine("mBitIndex = TypeExtensions.kNone;");
				writer.WriteLine("mCurrent = default;");
			}
			writer.WriteLine();
			WriteEnumeratorCurrent(writer, "int");
			writer.WriteLine();
			WriteEnumeratorResetAndDispose(writer);
			writer.WriteLine();
			writer.WriteLine("public bool MoveNext()");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (mBitIndex.IsNone())");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("mBitIndex = mStartBitIndex;");
				}
				writer.WriteLine();
				writer.WriteLine("if (mBitIndex < kLastIndex)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("mCurrent = mVector.NextBitIndex(mBitIndex, mStateFilter);");
					writer.WriteLine();
					writer.WriteLine("if (mCurrent >= 0)");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("mBitIndex = mCurrent;");
						writer.WriteLine("return true;");
					}
				}
				writer.WriteLine();
				writer.WriteLine("mBitIndex = kNumberOfBits;");
				writer.WriteLine("return false;");
			}
		}
	}

	private static void WriteEnumeratorCurrent(SourceWriter writer, string resultKeyword)
	{
		writer.WriteLine($"public readonly {resultKeyword} Current");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("get");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (mBitIndex.IsNone())");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("throw new InvalidOperationException(\"Enumeration has not started\");");
				}
				writer.WriteLine("if (mBitIndex > kLastIndex)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("throw new InvalidOperationException(\"Enumeration already finished\");");
				}
				writer.WriteLine();
				writer.WriteLine("return mCurrent;");
			}
		}
		writer.WriteLine("readonly object System.Collections.IEnumerator.Current => this.Current;");
	}

	private static void WriteEnumeratorResetAndDispose(SourceWriter writer)
	{
		writer.WriteLine("public void Reset()");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("mBitIndex = TypeExtensions.kNone;");
		}
		writer.WriteLine();
		writer.WriteLine("public readonly void Dispose() { }");
	}

	private static void WriteEnumInterfacesRegion(SourceWriter writer, BitVectorSpec spec)
	{
		using (writer.EnterRegion("Enum interfaces"))
		{
			WriteValidateBitMethod(writer);
			writer.WriteLine();
			WriteEnumTestMethod(writer, spec);
			writer.WriteLine();
			WriteEnumSetMethod(writer, spec);
			writer.WriteLine();
			WriteToStringsMethod(writer, spec);
			writer.WriteLine();
			WriteEnumToStringMethod(writer, spec);
			writer.WriteLine();
			WriteTryParseFlagsStringMethod(writer);
			writer.WriteLine();
			WriteTryParseFlagsCollectionMethod(writer);
			writer.WriteLine();
			WriteTryParseFlagMethod(writer);
		}
	}

	private static void WriteValidateBitMethod(SourceWriter writer)
	{
		writer.WriteLine("private readonly void ValidateBit<TEnum>(TEnum bit, int bitIndex)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (bitIndex < 0 || bitIndex >= this.Length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("throw new ArgumentOutOfRangeException(nameof(bit), bit,");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("\"Enum member is out of range for indexing\");");
				}
			}
		}
	}

	private static void WriteEnumTestMethod(SourceWriter writer, BitVectorSpec spec)
	{
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine("public readonly bool Test<TEnum>(TEnum bit)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("int bitIndex = Reflection.EnumValue<TEnum>.ToInt32(bit);");
			writer.WriteLine("ValidateBit(bit, bitIndex);");
			writer.WriteLine();
			writer.WriteLine($"var flag = (({spec.WordKeyword})1) << bitIndex;");
			writer.WriteLine();
			writer.WriteLine("return Bitwise.Flags.Test(mWord, flag);");
		}
	}

	private static void WriteEnumSetMethod(SourceWriter writer, BitVectorSpec spec)
	{
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine($"public {spec.TypeName} Set<TEnum>(TEnum bit, bool value = true)");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("int bitIndex = Reflection.EnumValue<TEnum>.ToInt32(bit);");
			writer.WriteLine("ValidateBit(bit, bitIndex);");
			writer.WriteLine();
			writer.WriteLine($"var flag = (({spec.WordKeyword})1) << bitIndex;");
			writer.WriteLine();
			writer.WriteLine("Bitwise.Flags.Modify(value, ref mWord, flag);");
			writer.WriteLine("return this;");
		}
	}

	private static void WriteToStringsMethod(SourceWriter writer, BitVectorSpec spec)
	{
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine("public readonly List<string> ToStrings<TEnum>(TEnum maxCount");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", bool stateFilter = true");
			writer.WriteLine(", List<string> results = null)");
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			WriteResultsInitialization(writer);
			WriteMaxCountValidation(writer);
			WriteEnumMemberSetup(writer);
			writer.WriteLine("foreach (int bitIndex in bitsInDesiredState)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (bitIndex >= maxCountValue)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("break;");
				}
				writer.WriteLine();
				writer.WriteLine("results.Add(enumMembers[memberIndex+bitIndex].ToString());");
			}
			writer.WriteLine();
			writer.WriteLine("return results;");
		}
	}

	private static void WriteEnumToStringMethod(SourceWriter writer, BitVectorSpec spec)
	{
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine("public readonly string ToString<TEnum>(TEnum maxCount");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", string valueSeperator = \",\"");
			writer.WriteLine(", bool stateFilter = true)");
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (Cardinality == 0)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return \"\";");
			}
			writer.WriteLine();
			WriteMaxCountValidation(writer);
			writer.WriteLine("if (valueSeperator == null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("valueSeperator = \"\";");
			}
			writer.WriteLine();
			WriteEnumMemberSetup(writer, includeBitsInDesiredState: false);
			writer.WriteLine("var sb = new System.Text.StringBuilder();");
			WriteBitsInDesiredState(writer);
			writer.WriteLine("foreach (int bitIndex in bitsInDesiredState)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("if (bitIndex >= maxCountValue)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("break;");
				}
				writer.WriteLine();
				writer.WriteLine("if (sb.Length > 0)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("sb.Append(valueSeperator);");
				}
				writer.WriteLine();
				writer.WriteLine("sb.Append(enumMembers[memberIndex+bitIndex].ToString());");
			}
			writer.WriteLine();
			writer.WriteLine("return sb.ToString();");
		}
	}

	private static void WriteResultsInitialization(SourceWriter writer)
	{
		writer.WriteLine("if (results == null)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("results = new List<string>(Cardinality);");
		}
		writer.WriteLine();
		writer.WriteLine("if (Cardinality == 0)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("return results;");
		}
		writer.WriteLine();
	}

	private static void WriteMaxCountValidation(SourceWriter writer)
	{
		writer.WriteLine("int maxCountValue = Reflection.EnumValue<TEnum>.ToInt32(maxCount);");
		writer.WriteLine("if (maxCountValue < 0 || maxCountValue >= Length)");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("throw new ArgumentOutOfRangeException(nameof(maxCount), string.Format(Util.InvariantCultureInfo,");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("\"{0}/{1} is invalid\",");
				writer.WriteLine("maxCount, maxCountValue));");
			}
		}
		writer.WriteLine();
	}

	private static void WriteEnumMemberSetup(SourceWriter writer, bool includeBitsInDesiredState = true)
	{
		writer.WriteLine("var enumType = typeof(TEnum);");
		writer.WriteLine("var enumMembers = (TEnum[])Enum.GetValues(enumType);");
		writer.WriteLine();
		writer.WriteLine("// Find the member which represents bit-0");
		writer.WriteLine("int memberIndex = 0;");
		writer.WriteLine("while (memberIndex < enumMembers.Length");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("&& memberIndex < maxCountValue");
			writer.WriteLine("&& Reflection.EnumValue<TEnum>.ToInt32(enumMembers[memberIndex]) != 0)");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("memberIndex++;");
		}
		writer.WriteLine();
		if (includeBitsInDesiredState)
		{
			WriteBitsInDesiredState(writer);
		}
	}

	private static void WriteBitsInDesiredState(SourceWriter writer)
	{
		writer.WriteLine("var bitsInDesiredState = stateFilter");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine("? SetBitIndices");
			writer.WriteLine(": ClearBitIndices;");
		}
	}

	private static void WriteTryParseFlagsStringMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary(
			"Interprets the provided separated strings as Enum members and sets their corresponding bits");
		writer.WriteXmlDocReturns(
			"True if all strings were parsed successfully, false if there were some strings that failed to parse");
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine("public bool TryParseFlags<TEnum>(string line");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", string valueSeperator = \",\"");
			writer.WriteLine(", ICollection<string> errorsOutput = null)");
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("// LINQ stmt allows there to be whitespace around the commas");
			writer.WriteLine("return TryParseFlags<TEnum>(");
			using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
			{
				writer.WriteLine("KSoft.Util.Trim(System.Text.RegularExpressions.Regex.Split(line, valueSeperator)),");
				writer.WriteLine("errorsOutput);");
			}
		}
	}

	private static void WriteTryParseFlagsCollectionMethod(SourceWriter writer)
	{
		writer.WriteXmlDocSummary("Interprets the provided strings as Enum members and sets their corresponding bits");
		writer.WriteXmlDocReturns(
			"True if all strings were parsed successfully, false if there were some strings that failed to parse");
		WriteEnumBitIndexTypeParamDoc(writer);
		writer.WriteLine("public bool TryParseFlags<TEnum>(IEnumerable<string> collection");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", ICollection<string> errorsOutput = null)");
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("if (collection == null)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return false;");
			}
			writer.WriteLine();
			writer.WriteLine("bool success = true;");
			writer.WriteLine("foreach (string flagStr in collection)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("var parsed = TryParseFlag<TEnum>(flagStr, errorsOutput);");
				writer.WriteLine("if (parsed.HasValue==false)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("continue;");
				}
				writer.WriteLine("else if (parsed.Value==false)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine("success = false;");
				}
			}
			writer.WriteLine();
			writer.WriteLine("return success;");
		}
	}

	private static void WriteTryParseFlagMethod(SourceWriter writer)
	{
		writer.WriteLine("private bool? TryParseFlag<TEnum>(string flagStr");
		using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
		{
			writer.WriteLine(", ICollection<string> errorsOutput = null)");
			writer.WriteLine($"where TEnum : {SourceGenerationConstants.EnumConstraint}");
		}
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine("const bool ignore_case = true;");
			writer.WriteLine();
			writer.WriteLine(
				"// Enum.TryParse will call Trim on the value anyway, so don't add yet another allocation when we can " +
				"check for whitespace");
			writer.WriteLine("if (string.IsNullOrWhiteSpace(flagStr))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return null;");
			}
			writer.WriteLine();
			writer.WriteLine("if (!Enum.TryParse<TEnum>(flagStr, ignore_case, out TEnum flag))");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("errorsOutput?.AddFormat(\"Couldn't parse '{0}' as a {1} flag\",");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("flagStr, typeof(TEnum));");
				}
				writer.WriteLine();
				writer.WriteLine("return false;");
			}
			writer.WriteLine();
			writer.WriteLine("int bitIndex = Reflection.EnumValue<TEnum>.ToInt32(flag);");
			writer.WriteLine("if (bitIndex < 0 || bitIndex > Length)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("errorsOutput?.AddFormat(\"Member '{0}'={1} in enum {2} can't be used as a bit index\",");
				using (writer.EnterBlock(SourceWriterBlockType.NoBraces))
				{
					writer.WriteLine("flag, bitIndex, typeof(TEnum));");
				}
				writer.WriteLine();
				writer.WriteLine("return false;");
			}
			writer.WriteLine();
			writer.WriteLine("this[bitIndex] = true;");
			writer.WriteLine("return true;");
		}
	}

	private static void WriteEnumBitIndexTypeParamDoc(SourceWriter writer)
	{
		writer.WriteLine("/// <typeparam name=\"TEnum\">Members should be bit indices, not literal flag values</typeparam>");
	}

	private readonly struct BitVectorSpec
	{
		public BitVectorSpec(NumberSpec wordSpec)
		{
			WordSpec = wordSpec;
		}

		public NumberSpec WordSpec { get; }

		public string TypeName => $"BitVector{WordSpec.SizeOfInBits}";

		public string WordKeyword => WordSpec.Keyword;

		public string SignedKeyword => WordSpec.SignedKeyword;

		public string ConstantKeyword => WordSpec.ConstantKeyword;

		public int BitCount => WordSpec.SizeOfInBits;
	}
};
