using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	/// <summary>Nothing public to see here, move along.</summary>
	public abstract class EnumBitEncoderBase
	{
		/// <summary>
		/// Applied to enumeration members <b>kMax</b> and <b>kAll</b> which aren't meant to be used in operational code
		/// </summary>
		public const string kObsoleteMsg = "For 'KSoft.IO.EnumBitEncoderBase' use only!";

		protected const string kEnumMaxMemberName = "kMax";
		protected const string kEnumNumberOfMemberName = "kNumberOf";
		protected const string kFlagsMaxMemberName = "kAll";

		protected static bool ValidateTypeIsNotEncoderDisabled(Type t)
		{
			var attr = t.GetCustomAttributes(typeof(EnumBitEncoderDisableAttribute), false);

			return attr.Length == 0;
		}

		protected static void InitializeBase(Type t)
		{
			Reflection.EnumUtils.AssertTypeIsEnum(t);

			if (!ValidateTypeIsNotEncoderDisabled(t))
				throw new ArgumentException(string.Format(
					"EnumBitEncoder can't operate on enumerations with an EnumBitEncoderDisableAttribute! {0}",
					t.FullName));
		}

		[System.Diagnostics.Conditional("TRACE")]
		protected static void ProcessMembers_DebugCheckMemberName(Type t, bool isFlags, string memberName)
		{
			if (isFlags && (memberName == kEnumNumberOfMemberName || memberName == kEnumMaxMemberName))
				Debug.Trace.IO.TraceInformation("Flags enum '{0}' has the Enum EnumBitEncoder member. Is this intentional?", t);
			else if (!isFlags && memberName == kFlagsMaxMemberName)
				Debug.Trace.IO.TraceInformation("Enum '{0}' has the Flags EnumBitEncoder member. Is this intentional?", t);
		}
		protected static bool IsMaxMemberName(bool isFlags, string memberName)
		{
			bool result = false;

			if (isFlags)
				result = memberName == kFlagsMaxMemberName;
			else
			{
				result =memberName == kEnumNumberOfMemberName ||
						memberName == kEnumMaxMemberName;
			}

			return result;
		}

		public abstract int BitCountTrait { get; }
	};

	public interface IEnumBitEncoder<TUInt>
	{
		bool IsFlags { get; }
		bool HasNone { get; }
		/// <summary>Max value of the enum. NONE encoding is NOT factored in</summary>
		TUInt MaxValueTrait { get; }
		/// <summary>Masking value that can be used to single out this enumeration's value(s)</summary>
		TUInt BitmaskTrait { get; }
		/// <summary>How many bits the enumeration consumes</summary>
		int BitCountTrait { get; }

		/// <summary>The bit index assumed when one isn't provided</summary>
		int DefaultBitIndex { get; }
	};
}