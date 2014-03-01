using System;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	partial class BitwiseT4
	{
		public static IEnumerable<NumberCodeDefinition> ByteSwapableIntegers { get {
			yield return PrimitiveDefinitions.kUInt16;
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kUInt64;
		} }

		static System.Text.StringBuilder GenerateByteSwapBegin(string valueName, bool signed, string resultName, string keyword,
			bool cast)
		{
			var sb = new System.Text.StringBuilder();
			if (resultName != null)
				sb.AppendFormat("{0} = ", resultName);

			// smaller integers are promoted to larger types when bit operated on
			// this is for casting the final result back into the smaller integer type
			if (cast)
				sb.AppendFormat("({0})( ", keyword);

			return sb;
		}
		static string GenerateByteSwapEnd(string valueName, bool signed, string resultName, string keyword,
			bool cast, System.Text.StringBuilder sb)
		{
			// close the cast operation group
			if (cast)
				sb.Append(" )");

//			if (resultName != null)
//				sb.Append(';');

			return sb.ToString();
		}
		static void GenerateByteSwapStep(System.Text.StringBuilder sb, string valueName, 
			int shift, string mask, bool signed, bool lastOperation = false)
		{
			// mask is not added when this is the last op in signed expression as the mask will be an unsigned value
			bool mask_op = !signed || !lastOperation;
			if(mask_op)
				sb.Append('(');

			// start the shift operation group
			sb.AppendFormat("({0}", valueName);

			// LHS with positive numbers, RHS with negative
			if (shift > 0)
				sb.Append(" << ");
			else
			{
				sb.Append(" >> ");
				shift = -shift;
			}
			sb.AppendFormat("{0,2}", shift);

			// close the shift operation group
			sb.Append(')');

			// add the mask operation
			if (mask_op)
			{
				sb.Append(" & ");
				sb.AppendFormat("0x{0})", mask);
			}

			// not the last operation so OR this with the next operation
			if (!lastOperation)
				sb.Append(" | ");
		}
/*
(ushort)(
	((u >> 8) & 0x00FF) |
	((u << 8) & 0xFF00)
);
*/
		static string GenerateByteSwapInt16(string valueName, bool signed = false, string resultName = null)
		{
			const bool k_cast = true;
			const string k_hex_format = "X4";

			var keyword = NumberCodeDefinition.TypeCodeToKeyword(signed ? TypeCode.Int16 : TypeCode.UInt16);
			var sb = GenerateByteSwapBegin(valueName, signed, resultName, keyword, k_cast);

			GenerateByteSwapStep(sb, valueName, -8, 0x00FF.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, +8, 0xFF00.ToString(k_hex_format), signed, true);

			return GenerateByteSwapEnd(valueName, signed, resultName, keyword, k_cast, sb);
		}
/*
	((u >> 24) & 0x000000FF) |
	((u >>  8) & 0x0000FF00) |
	((u <<  8) & 0x00FF0000) |
	((u << 24) & 0xFF000000)
*/
		static string GenerateByteSwapInt32(string valueName, bool signed = false, string resultName = null)
		{
			const bool k_cast = false;
			const string k_hex_format = "X8";

			var keyword = NumberCodeDefinition.TypeCodeToKeyword(signed ? TypeCode.Int32 : TypeCode.UInt32);
			var sb = GenerateByteSwapBegin(valueName, signed, resultName, keyword, k_cast);

			GenerateByteSwapStep(sb, valueName, -24, 0x000000FF.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, - 8, 0x0000FF00.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, + 8, 0x00FF0000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, +24, 0xFF000000.ToString(k_hex_format), signed, true);

			return GenerateByteSwapEnd(valueName, signed, resultName, keyword, k_cast, sb);
		}
/*
	((u >> 56) & 0x00000000000000FF) |
	((u >> 40) & 0x000000000000FF00) |
	((u >> 24) & 0x0000000000FF0000) |
	((u >>  8) & 0x00000000FF000000) |
	((u <<  8) & 0x000000FF00000000) |
	((u << 24) & 0x0000FF0000000000) |
	((u << 40) & 0x00FF000000000000) |
	((u << 56) & 0xFF00000000000000)	// in signed longs, leave out the mask (which is unsigned) to avoid any additional casting
*/
		static string GenerateByteSwapInt64(string valueName, bool signed = false, string resultName = null)
		{
			const bool k_cast = false;
			const string k_hex_format = "X16";

			var keyword = NumberCodeDefinition.TypeCodeToKeyword(signed ? TypeCode.Int64 : TypeCode.UInt64);
			var sb = GenerateByteSwapBegin(valueName, signed, resultName, keyword, k_cast);

			GenerateByteSwapStep(sb, valueName, -56, 0x00000000000000FF.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, -40, 0x000000000000FF00.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, -24, 0x0000000000FF0000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, - 8, 0x00000000FF000000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, + 8, 0x000000FF00000000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, +24, 0x0000FF0000000000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, +40, 0x00FF000000000000.ToString(k_hex_format), signed);
			GenerateByteSwapStep(sb, valueName, +56, 0xFF00000000000000.ToString(k_hex_format), signed, true);

			return GenerateByteSwapEnd(valueName, signed, resultName, keyword, k_cast, sb);
		}

		public static string GenerateByteSwap(TypeCode typeCode,
			string valueName, string resultName = null)
		{
			bool signed = false;
			Func<string, bool, string, string> GenerateByteSwapProc;

			switch (typeCode)
			{
				case TypeCode.UInt16:
					GenerateByteSwapProc = GenerateByteSwapInt16;
					break;
				case TypeCode.Int16:
					signed = true;
					GenerateByteSwapProc = GenerateByteSwapInt16;
					break;

				case TypeCode.UInt32:
					GenerateByteSwapProc = GenerateByteSwapInt32;
					break;
				case TypeCode.Int32:
					signed = true;
					GenerateByteSwapProc = GenerateByteSwapInt32;
					break;

				case TypeCode.UInt64:
					GenerateByteSwapProc = GenerateByteSwapInt64;
					break;
				case TypeCode.Int64:
					signed = true;
					GenerateByteSwapProc = GenerateByteSwapInt64;
					break;

				default:
					throw new ArgumentException(typeCode.ToString());
			}

			return GenerateByteSwapProc(valueName, signed, resultName);
		}
	};
}