using KSoft.SourceGeneration.Descriptors;
using KSoft.SourceGeneration.Text;

namespace KSoft.SourceGeneration.Collections;

internal static class TypedBitVectorsSourceBuilder
{
	public const string HintName = "KSoft.Collections.TypedBitVectors.g.cs";

	public static string Build()
	{
		var writer = new SourceWriter();
		writer.WriteGeneratedFileHeader();
		writer.WriteLine("using System;");
		writer.WriteFileScopedNamespace("KSoft.Collections");
		foreach (var word in PrimitiveCatalog.BittableTypesMajorWords)
		{
			WriteVector(writer, word);
		}
		return writer.ToString();
	}

	static void WriteVector(SourceWriter writer, NumberSpec word)
	{
		string width = word.SizeOfInBits.ToString(PrimitiveCatalog.InvariantCulture);
		string raw = "BitVector" + width;
		string type = raw + "<TBits>";
		string bits = word.SizeOfInBits == 32 ? "unchecked((ulong)(uint)mBits.Data)" : "unchecked((ulong)mBits.Data)";
		writer.WriteLine();
		writer.WriteXmlDocSummary("A mutable, fixed-width value whose named bit operations use the associated enum.");
		writer.WriteLine("/// <typeparam name=\"TBits\">The bit-index enum governed by <see cref=\"EnumBitTraits{TBits}\"/>.</typeparam>");
		writer.WriteLine($"/// <remarks>Contains one {width}-bit word; assignment copies the bits. Named access, raw import, whole-word mutations/combinations, formatting, and creation of named enumerators validate the enum domain and width. Zero-initialization, raw state queries/export, equality, comparison, and hashing do not. Use <see cref=\"With\"/> and assign the result for ordinary value-returning properties; copy/assign explicitly for other mutators. <see cref=\"IEnumBitVector\"/> has method-specific adapter contracts.</remarks>");
		writer.WriteLine($"public struct {type} : IEquatable<{type}>, IComparable<{type}>, IEnumBitVector");
		writer.WriteLine("where TBits : struct, Enum");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine($"{raw} mBits;");
			writer.WriteLine($"private {raw}({raw} bits)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"EnumBitTraits<TBits>.ValidateWidth({width});");
				writer.WriteLine("mBits = bits;");
			}
			writer.WriteXmlDocSummary("Creates a typed value without altering the supplied raw bits.");
			writer.WriteXmlDocParam("bits", "The raw word to import.");
			writer.WriteXmlDocReturns("A typed copy of the supplied word.");
			writer.WriteLine("/// <remarks>This preserves set positions that have no declared enum member.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or its logical extent exceeds the vector width.</exception>");
			writer.WriteLine($"public static {type} FromRaw({raw} bits) => new(bits);");
			writer.WriteXmlDocSummary("Returns the complete stored word as an untyped vector.");
			writer.WriteXmlDocReturns("A copy of the raw vector without validating the enum domain.");
			writer.WriteLine($"public readonly {raw} ToRaw() => mBits;");
			writer.WriteXmlDocSummary($"Gets the physical width of {width} bits, not a named-member count.");
			writer.WriteLine("public readonly int Length => mBits.Length;");
			writer.WriteXmlDocSummary("Gets the number of set bits in the entire stored word, including unnamed positions.");
			writer.WriteLine("public readonly int Cardinality => mBits.Cardinality;");
			writer.WriteXmlDocSummary("Gets the number of clear bits in the entire stored word, including unnamed positions.");
			writer.WriteLine("public readonly int CardinalityZeros => mBits.CardinalityZeros;");
			writer.WriteXmlDocSummary("Reports whether the entire stored word is clear, without enum validation.");
			writer.WriteLine("public readonly bool IsAllClear => mBits.IsAllClear;");
			writer.WriteXmlDocSummary("Reports whether the entire stored word is set, without enum validation.");
			writer.WriteLine("public readonly bool IsAllSet => mBits.IsAllSet;");
			writer.WriteLine("private readonly int GetIndex(TBits bit)");
			writer.WriteLine($"\t=> EnumBitTraits<TBits>.ToIndex(bit, {width}, nameof(bit));");
			writer.WriteXmlDocSummary("Gets or changes a usable declared bit in this value.");
			writer.WriteXmlDocParam("bit", "The declared bit to address.");
			writer.WriteXmlDocReturns("The stored bit state.");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"bit\"/> is not a usable declared member.</exception>");
			writer.WriteLine("public bool this[TBits bit] { readonly get => Test(bit); set => Set(bit, value); }");
			writer.WriteXmlDocSummary("Tests the stored state of a usable declared bit.");
			writer.WriteXmlDocParam("bit", "The declared bit to test.");
			writer.WriteXmlDocReturns("The stored bit state.");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"bit\"/> is not a usable declared member.</exception>");
			writer.WriteLine("public readonly bool Test(TBits bit) => mBits[GetIndex(bit)];");
			writer.WriteXmlDocSummary("Changes a usable declared bit in this receiver.");
			writer.WriteXmlDocParam("bit", "The declared bit to change.");
			writer.WriteXmlDocParam("value", "The new bit state.");
			writer.WriteXmlDocReturns("A copy of the resulting value.");
			writer.WriteLine("/// <remarks>A value-returning property supplies a temporary receiver; chaining mutators on this returned copy does not retain every change in the original variable. Assign the final result when chaining.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"bit\"/> is not a usable declared member.</exception>");
			writer.WriteLine($"public {type} Set(TBits bit, bool value = true)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("mBits[GetIndex(bit)] = value;");
				writer.WriteLine("return this;");
			}
			writer.WriteXmlDocSummary("Flips a usable declared bit in this receiver.");
			writer.WriteXmlDocParam("bit", "The declared bit to change.");
			writer.WriteXmlDocReturns("A copy of the resulting value.");
			writer.WriteLine("/// <remarks>The property-copy and chaining rules are the same as for <see cref=\"Set\"/>.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"bit\"/> is not a usable declared member.</exception>");
			writer.WriteLine($"public {type} Toggle(TBits bit)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("int index = GetIndex(bit);");
				writer.WriteLine("mBits[index] = !mBits[index];");
				writer.WriteLine("return this;");
			}
			writer.WriteXmlDocSummary("Returns an updated copy without changing this receiver.");
			writer.WriteXmlDocParam("bit", "The declared bit to change in the copy.");
			writer.WriteXmlDocParam("value", "The new bit state.");
			writer.WriteXmlDocReturns("The updated value; assign it back to a property to invoke its setter.");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"bit\"/> is not a usable declared member.</exception>");
			writer.WriteLine($"public readonly {type} With(TBits bit, bool value = true)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("var copy = this;");
				writer.WriteLine("return copy.Set(bit, value);");
			}
			writer.WriteXmlDocSummary("Clears the entire stored word in this receiver, including unnamed positions.");
			writer.WriteLine("/// <remarks>For a value-returning property, modify a local copy and assign it back; this mutator returns no replacement value.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine($"public void Clear() {{ EnumBitTraits<TBits>.ValidateWidth({width}); mBits.Clear(); }}");
			writer.WriteXmlDocSummary("Changes every position in this receiver's stored word, including unnamed positions.");
			writer.WriteXmlDocParam("value", "The state to assign to every physical bit.");
			writer.WriteLine("/// <remarks>For a value-returning property, modify a local copy and assign it back; this mutator returns no replacement value.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine($"public void SetAll(bool value) {{ EnumBitTraits<TBits>.ValidateWidth({width}); mBits.SetAll(value); }}");
			foreach (string operation in new[] { "And", "AndNot", "Or", "Xor" })
			{
				writer.WriteXmlDocSummary($"Returns the whole-word <c>{operation}</c> result without changing either value.");
				writer.WriteXmlDocParam("other", "The value of the same closed vector type to combine with.");
				writer.WriteXmlDocReturns("A new value containing the result, including unnamed positions.");
				writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
				writer.WriteLine($"public readonly {type} {operation}({type} other) => new(mBits.{operation}(other.mBits));");
			}
			writer.WriteXmlDocSummary("Returns the complement of the entire stored word, including unnamed positions.");
			writer.WriteXmlDocReturns("A new complemented value; this receiver is unchanged.");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine($"public readonly {type} Not() => new(mBits.Not());");
			foreach (string op in new[] { "&", "|", "^" })
			{
				writer.WriteXmlDocSummary("Returns a whole-word bitwise result for two values of the same closed vector type.");
				writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
				writer.WriteLine($"public static {type} operator {op}({type} left, {type} right) => new(left.mBits {op} right.mBits);");
			}
			writer.WriteXmlDocSummary("Returns a whole-word complement with the same validation as <see cref=\"Not\"/>.");
			writer.WriteLine($"public static {type} operator ~({type} value) => value.Not();");
			writer.WriteXmlDocSummary("Tests equality of the complete stored words without enum validation.");
			writer.WriteLine($"public readonly bool Equals({type} other) => mBits.Equals(other.mBits);");
			writer.WriteXmlDocSummary("Tests same-closed-type stored-word equality without enum validation.");
			writer.WriteLine($"public override readonly bool Equals(object? other) => other is {type} value && Equals(value);");
			writer.WriteXmlDocSummary("Gets the stored word's hash code without enum validation.");
			writer.WriteLine("public override readonly int GetHashCode() => mBits.GetHashCode();");
			writer.WriteXmlDocSummary("Compares the stored words without enum validation.");
			writer.WriteLine($"public readonly int CompareTo({type} other) => mBits.CompareTo(other.mBits);");
			writer.WriteXmlDocSummary("Tests stored-word equality without enum validation.");
			writer.WriteLine($"public static bool operator ==({type} left, {type} right) => left.Equals(right);");
			writer.WriteXmlDocSummary("Tests stored-word inequality without enum validation.");
			writer.WriteLine($"public static bool operator !=({type} left, {type} right) => !left.Equals(right);");
			foreach (string op in new[] { "<", "<=", ">", ">=" })
			{
				writer.WriteXmlDocSummary("Compares stored words without enum validation.");
				writer.WriteLine($"public static bool operator {op}({type} left, {type} right) => left.CompareTo(right) {op} 0;");
			}
			writer.WriteXmlDocSummary("Formats matching declared bits using their canonical C# names.");
			writer.WriteXmlDocParam("separator", "Text between names; defaults to a comma.");
			writer.WriteXmlDocParam("stateFilter", "True selects set named bits; false selects clear named bits.");
			writer.WriteXmlDocReturns("Names in ascending index order, aliases once, or an empty string when none match.");
			writer.WriteLine("/// <remarks>Uses the first ordinary declared name for each index, not display labels or UI visibility rules.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentNullException\"><paramref name=\"separator\"/> is null.</exception>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("public readonly string ToFlagsString(string separator = \",\", bool stateFilter = true)");
			writer.WriteLine($"\t=> EnumBitTraits<TBits>.FormatVector({bits}, {width}, separator, stateFilter);");
			writer.WriteXmlDocSummary("Formats named set bits with the default <see cref=\"ToFlagsString\"/> options.");
			writer.WriteLine("/// <remarks>This validates the enum domain and width; it is not a raw binary diagnostic string for an invalid closed type.</remarks>");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine("public override readonly string ToString() => ToFlagsString();");
			writer.WriteXmlDocSummary("Gets a value-snapshot enumerator over declared set indices in ascending order.");
			writer.WriteXmlDocReturns("An enumerator unaffected by later changes to this vector.");
			writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
			writer.WriteLine($"public readonly EnumBitVectorEnumerator<TBits> GetEnumerator() => new({bits}, {width}, true);");
			foreach (string state in new[] { "Set", "Clear" })
			{
				string filter = state == "Set" ? "true" : "false";
				writer.WriteXmlDocSummary($"Gets a snapshot enumeration of declared {state.ToLowerInvariant()} bits, once per index in ascending order.");
				writer.WriteLine("/// <remarks>Returns enum values, not display names; alias text from the enum's own ToString is not controlled here.</remarks>");
				writer.WriteLine("/// <exception cref=\"ArgumentException\">The enum domain is invalid or exceeds the vector width.</exception>");
				writer.WriteLine($"public readonly EnumeratorWrapper<TBits, EnumBitVectorEnumerator<TBits>> {state}BitIndices");
				writer.WriteLine($"\t=> new(new EnumBitVectorEnumerator<TBits>({bits}, {width}, {filter}));");
			}
			writer.WriteLine($"readonly Type IEnumBitVector.BitsEnumType => EnumBitTraits<TBits>.GetEnumType({width});");
			writer.WriteLine("readonly bool IEnumBitVector.IsDefinedIndex(int bitIndex) => EnumBitTraits<TBits>.IsDefinedIndex(bitIndex);");
			writer.WriteLine("readonly string IEnumBitVector.GetBitName(int bitIndex) => EnumBitTraits<TBits>.GetMemberName(bitIndex);");
			writer.WriteLine("readonly bool IEnumBitVector.GetBit(int bitIndex) => mBits[bitIndex];");
			writer.WriteLine("readonly IEnumBitVector IEnumBitVector.WithBit(int bitIndex, bool value)");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"EnumBitTraits<TBits>.ValidateWidth({width});");
				writer.WriteLine("EnumBitTraits<TBits>.ValidateIndex(bitIndex, nameof(bitIndex));");
				writer.WriteLine("var copy = this;");
				writer.WriteLine("copy.mBits[bitIndex] = value;");
				writer.WriteLine("return copy;");
			}
		}
	}
}
