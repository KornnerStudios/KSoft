using System;

namespace KSoft.Values
{
	/// <summary>Describes how a value is compared for equality</summary>
	[System.Reflection.Obfuscation(Exclude=false, ApplyToMembers=false)]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "this is equal to Equal, but it's for masking purposes")]
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
		static bool ValidInequalityBits(this Values.EqualityTraits value) =>
			(value & Values.EqualityTraits.kInequalityMask) != Values.EqualityTraits.kInequalityMask;

		static void ThrowIfInvalidInequalityBits(this Values.EqualityTraits value)
		{
			if (!value.ValidInequalityBits())
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Equality traits cannot contain both LessThan and GreaterThan; actual value is {0}.",
					value));
			}
		}

		static Values.EqualityTraits GetEqualityBits(this Values.EqualityTraits value)
		{
			value.ThrowIfInvalidInequalityBits();

			return value & Values.EqualityTraits.kEqualityMask;
		}
		static Values.EqualityTraits GetInequalityBits(this Values.EqualityTraits value)
		{
			value.ThrowIfInvalidInequalityBits();

			return value & Values.EqualityTraits.kInequalityMask;
		}

		/// <summary>Can the comparison be considered not equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNotEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.NotEqual;

		/// <summary>Can the comparison be considered equal?</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsEqual(this Values.EqualityTraits value) =>
			// Ignores inequality state (the Equal bit is the only thing that really matters)
			value.GetEqualityBits() == Values.EqualityTraits.Equal;

		public static bool IsLessThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the LessThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.LessThan;

		public static bool IsGreaterThan(this Values.EqualityTraits value) =>
			// Ignores equality state (the GreaterThan bit is the only thing that really matters)
			value.GetInequalityBits() == Values.EqualityTraits.GreaterThan;

		public static bool IsLessThanOrEqual(this Values.EqualityTraits value)
		{
			value.ThrowIfInvalidInequalityBits();

			// Either the Equal or LessThan (or both) bits are set
			return (value & Values.EqualityTraits.LessThanEqual) != 0;
		}
		public static bool IsGreaterThanOrEqual(this Values.EqualityTraits value)
		{
			value.ThrowIfInvalidInequalityBits();

			// Either the Equal or GreaterThan (or both) bits are set
			return (value & Values.EqualityTraits.GreaterThanEqual) != 0;
		}
	};
}
