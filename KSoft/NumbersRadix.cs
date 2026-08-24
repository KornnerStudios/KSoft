
namespace KSoft
{
	/// <summary>Valid numerical bases (radix) that can be used in this library suite</summary>
	[EnumBitEncoderDisable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = "Values represent mutually exclusive numerical bases.")]
	public enum NumeralBase : byte
	{
		Binary	= 2,
		Octal	= 8,
		Decimal = 10,
		Hex		= 16,
	};

	/// <summary>Valid numerical bases (radix) that can be used in <see cref="KSoft.Numbers"/></summary>
	[EnumBitEncoderDisable]
	public enum NumbersRadix : byte
	{
		Binary	= NumeralBase.Binary,
		Octal	= NumeralBase.Octal,
		Decimal = NumeralBase.Decimal,
		Hex		= NumeralBase.Hex,
		Base36	= 36,
		Base64	= 64,
	};
}
