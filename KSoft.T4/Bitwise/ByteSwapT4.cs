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

		public class IntegerByteSwapCodeGenerator
		{
			TextTemplating.TextTransformation mFile;
			NumberCodeDefinition mDef;

			string mValueName;

			public IntegerByteSwapCodeGenerator(TextTemplating.TextTransformation ttFile,
				NumberCodeDefinition def, string valueName)
			{
				mFile = ttFile;
				mDef = def;

				mValueName = valueName;
			}

			void GeneratePrologue(bool cast, bool signed)
			{
				// smaller integers are promoted to larger types when bit operated on
				// this is for casting the final result back into the smaller integer type
				if (cast)
				{
					var def = signed ? mDef.TryGetSignedDefinition() : mDef;

					mFile.WriteLine("({0})( ", def.Keyword);
					mFile.PushIndent(TextTransformationCodeBlockBookmark.kIndent);
				}
			}
			void GenerateEpilogue(bool cast)
			{
				// close the cast operation group
				if (cast)
				{
					mFile.PopIndent();
					mFile.Write(")");
				}
			}
			void GenerateStep(bool signed, int shift, string mask, bool lastOperation = false)
			{
				// mask is not added when this is the last op in signed expression as the mask will be an unsigned value
				bool mask_op = !signed || !lastOperation;
				if (mask_op)
					mFile.Write("(");
				else
					mFile.Write(" "); // add a space to keep aligned with statements prefixed with '('

				// start the shift operation group
				mFile.Write("({0}", mValueName);

				// LHS with positive numbers, RHS with negative
				if (shift > 0)
					mFile.Write(" << ");
				else
				{
					mFile.Write(" >> ");
					shift = -shift;
				}
				mFile.Write("{0,2}", shift);

				// close the shift operation group
				mFile.Write(")");

				// add the mask operation
				if (mask_op)
				{
					mFile.Write(" & ");
					mFile.Write("0x{0})", mask);
				}

				// not the last operation so OR this with the next operation
				if (!lastOperation)
					mFile.Write(" | ");
			}

			void GenerateCode(bool signed)
			{
				// amount to increase 'shift' after each step
				const int k_step_shift_inc = kBitsPerByte * 2;

				// do we need to cast the result?
				bool cast = mDef.BitOperatorsImplicitlyUpCast;
				string hex_format = mDef.ToStringHexFormat;

				GeneratePrologue(cast, signed);

				// GenerateStep's 'shift' is negative when the operation is a RHS (>>), and positive for LHS (<<)
				int shift = (0-mDef.SizeOfInBits) + kBitsPerByte;
				// our byte mask for each step
				ulong mask = byte.MaxValue;
				// While 'shift' is negative, we're generating steps to swap the MSB bytes to the LSBs half.
				// Once 'shift' becomes positive, we're generating steps to swap the LSB bytes to the MSBs half.
				for(int x = mDef.SizeOfInBytes-1; x >= 0; x--, shift += k_step_shift_inc, mask <<= kBitsPerByte)
				{
					GenerateStep(signed, shift, mask.ToString(hex_format), x == 0);
					mFile.WriteLine("");
				}

				GenerateEpilogue(cast);
			}
			public void Generate(bool signed = false)
			{
				// indent to method code body's indention level, plus one (l-value statement should be on the line before)
				using (mFile.EnterCodeBlock(indentCount: 3+1))
				{
					GenerateCode(signed);

					mFile.WriteLine(";");
				}
			}
		};
	};
}