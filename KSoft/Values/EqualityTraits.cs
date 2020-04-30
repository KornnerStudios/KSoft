using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Describes how a value is compared for equality</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
	public enum EqualityTraits : byte
	{
		NotEqual = 0,
		Equal = 1 << 0,			// 1

		LessThan = 2 << 0,		// 2
		GreaterThan = 2 << 1,	// 4

		LessThanEqual =			// 3
			Equal | LessThan,
		GreaterThanEqual =		// 5
			Equal | GreaterThan,

		[System.Reflection.Obfuscation(Exclude=false)]
		kEqualityMask =			// 1
			NotEqual | Equal,
		[System.Reflection.Obfuscation(Exclude=false)]
		kInequalityMask =		// 6
			LessThan | GreaterThan,

		/// <remarks>3 bits</remarks>
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)]
		kAll =					// 7
			kEqualityMask | kInequalityMask,
	};
}

namespace KSoft
{
	partial class TypeExtensions
	{
		/// <summary>Valides that LessThan and GreaterThan are not set at the same time</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		static bool ValidInequalityBits(this Values.EqualityTraits value) =>
			(value & Values.EqualityTraits.kInequalityMask) != Values.EqualityTraits.kInequalityMask;

		[Contracts.Pure]
		static Values.EqualityTraits GetEqualityBits(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			return value & Values.EqualityTraits.kEqualityMask;
		}
		[Contracts.Pure]
		static Values.EqualityTraits GetInequalityBits(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			return value & Values.EqualityTraits.kInequalityMask;
		}

		/// <summary>Can the comparison be considered not equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsNotEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.NotEqual;

		/// <summary>Can the comparison be considered equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool IsEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.Equal;

		[Contracts.Pure]
		public static bool IsLessThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the LessThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.LessThan;

		[Contracts.Pure]
		public static bool IsGreaterThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the GreaterThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.GreaterThan;

		[Contracts.Pure]
		public static bool IsLessThanOrEqual(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			// Either the Equal or LessThan (or both) bits are set
			return (value & Values.EqualityTraits.LessThanEqual) != 0;
		}
		[Contracts.Pure]
		public static bool IsGreaterThanOrEqual(this Values.EqualityTraits value)
		{
			Contract.Assert(value.ValidInequalityBits());

			// Either the Equal or GreaterThan (or both) bits are set
			return (value & Values.EqualityTraits.GreaterThanEqual) != 0;
		}
	};
}
