using System.Collections.Generic;

namespace KSoft.SourceGeneration.Descriptors;

internal static class BitSetCatalog
{
	/// <summary>
	/// Bit states used by the BitSet generator.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.CollectionsT4.BitStateDefs</c>.</remarks>
	public static IReadOnlyList<BitStateSpec> BitStates { get; } =
		[
			new(isSet: false),
			new(isSet: true),
		];

	/// <summary>
	/// BitSet operations used by the BitSet generator.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.BitOperation.kFirst..kLast</c>.</remarks>
	public static IReadOnlyList<BitOperationSpec> BitOperations { get; } =
		[
			new(BitOperationKind.Clear),
			new(BitOperationKind.Set),
			new(BitOperationKind.Toggle),
			new(BitOperationKind.Test),
		];

	/// <summary>
	/// Enumerator definitions used by the IReadOnlyBitSet enumerator generator.
	/// </summary>
	/// <remarks>Maps to <c>KSoft.T4.CollectionsT4.BitSetEnumeratorDefs</c>.</remarks>
	public static IReadOnlyList<BitSetEnumeratorSpec> Enumerators { get; } =
		[
			new("State", PrimitiveCatalog.Bool, hasStateFilterFields: false),
			new("StateFilter", PrimitiveCatalog.NumberFor(System.TypeCode.Int32).Primitive, hasStateFilterFields: true),
		];
}

internal enum BitOperationKind
{
	Clear,
	Set,
	Toggle,
	Test,
}

internal readonly struct BitStateSpec
{
	private readonly bool mIsSet;

	public BitStateSpec(bool isSet)
	{
		mIsSet = isSet;
	}

	public string ApiName => mIsSet ? "Set" : "Clear";
	public string DocName => mIsSet ? "set" : "clear";
	public string DocNameVerbose => mIsSet ? "1 (set)" : "0 (clear)";
	public string ValueKeyword => mIsSet ? "true" : "false";
}

internal readonly struct BitOperationSpec
{
	public BitOperationSpec(BitOperationKind kind)
	{
		Kind = kind;
	}

	public BitOperationKind Kind { get; }
	public string Name => Kind.ToString();
	public string ResultType => IsPure ? "bool" : "void";
	public string DefaultReturn => IsPure ? "return false;" : "return ;";
	public bool IsPure => Kind == BitOperationKind.Test;
	public bool RequiresCardinalityUpdate => !IsPure;

	public string FlagsMethod => Kind switch
	{
		BitOperationKind.Clear => "Bitwise.Flags.Remove",
		BitOperationKind.Set => "Bitwise.Flags.Add",
		BitOperationKind.Toggle => "Bitwise.Flags.Toggle",
		BitOperationKind.Test => "Bitwise.Flags.TestAny",
		_ => throw new System.ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown bit operation."),
	};
}

internal readonly struct BitSetEnumeratorSpec
{
	public BitSetEnumeratorSpec(string name, PrimitiveSpec resultType, bool hasStateFilterFields)
	{
		Name = name;
		ResultType = resultType;
		HasStateFilterFields = hasStateFilterFields;
	}

	public string Name { get; }
	public PrimitiveSpec ResultType { get; }
	public string ResultKeyword => ResultType.Keyword;
	public bool HasStateFilterFields { get; }
}
