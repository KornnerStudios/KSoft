using System;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	partial class BitwiseT4
	{
		#region Bit pattern constants
		// 0101 0101
		const byte kMaskEvenBits = 0x55;
		// 1010 1010
		const byte kMaskOddBits = unchecked((byte)~kMaskEvenBits);

		// 0011 0011
		const byte kMaskConsecutivePairsLsb = 0x33;
		// 1100 1100
		const byte kMaskConsecutivePairsMsb = unchecked((byte)~kMaskConsecutivePairsLsb);

		// 0000 1111
		const byte kMaskNibbleLsb = 0x0F;
		// 1111 0000
		const byte kMaskNibbleMsb = unchecked((byte)~kMaskNibbleLsb);
		#endregion

		public abstract class BitFondleCodeGenerator : BitUtilCodeGenerator
		{
			protected const string kBitsParamName = "bits";
			protected const string kFondleVarName = "x";

			public BitFondleCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			protected override void GeneratePrologue()
			{
				if (mDef.SizeOfInBits == PrimitiveDefinitions.kUInt64.SizeOfInBits)
					mFile.Write(PrimitiveDefinitions.kUInt64.Keyword);
				else
					mFile.Write(PrimitiveDefinitions.kUInt32.Keyword);

				mFile.WriteLine(" {0} = {1};", kFondleVarName, kBitsParamName);
			}

			protected string BuildBitMaskForInteger(byte byteMask)
			{
				var sb = new System.Text.StringBuilder("0x");

				var mask_str = byteMask.ToString("X2");
				for (int x = 0; x < mDef.SizeOfInBytes; x++)
					sb.Append(mask_str);

				return sb.ToString();
			}
			string BuildWordMaskForInteger(int wordSize, bool lhs)
			{
				var mask_str = new string('F', wordSize);
				var zero_str = new string('0', wordSize);

				// mask is for the left-hand-side/most-significant bits
				if (lhs)
					mask_str = mask_str + zero_str;
				else // rhs/lsb
					mask_str = zero_str + mask_str;

				var sb = new System.Text.StringBuilder("0x");
				for (int x = 0; x < mDef.SizeOfInBytes; x += wordSize)
					sb.Append(mask_str);

				return sb.ToString();
			}
			protected string BuildWordMaskForIntegerMsb(int wordSize) { return BuildWordMaskForInteger(wordSize, true); }
			protected string BuildWordMaskForIntegerLsb(int wordSize) { return BuildWordMaskForInteger(wordSize, false); }
		};

		public class BitReverseCodeGenerator : BitFondleCodeGenerator
		{
			public BitReverseCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			protected override void GenerateXmlDoc()
			{
				mFile.WriteXmlDocSummary("Get the bit-reversed equivalent of an unsigned integer");
				mFile.WriteXmlDocParameter(kBitsParamName,
					"Integer to bit-reverse");
				mFile.WriteXmlDocReturns("");
			}
			protected override void GenerateMethodSignature()
			{
				string param_bits = string.Format("{0} {1}", mDef.Keyword, kBitsParamName);

				mFile.WriteLine("[Contracts.Pure]");
				mFile.WriteLine("public static {0} {1}({2})",
					mDef.Keyword,
					"BitReverse",
					param_bits);
			}
			protected override void GenerateEpilogue()
			{
				mFile.Write("return ");

				if (mDef.SizeOfInBits < PrimitiveDefinitions.kUInt32.SizeOfInBits)
					mFile.Write("({0})", mDef.Keyword);

				mFile.Write(kFondleVarName);
				mFile.WriteLine(";");
			}

			void GenerateOperationCode(string maskLHS, string maskRHS, int shiftAmount, string doc)
			{
				mFile.Write("{0} = (({0} & {1}) >> {3,2}) | (({0} & {2}) << {3,2});",
					kFondleVarName,
					maskLHS,
					maskRHS,
					shiftAmount);
				if (doc != null)
					mFile.WriteLine(" // {0}", doc);
				else
					mFile.WriteLine("");
			}
			void GenerateBitOperationCode(byte byteMaskLHS, byte byteMaskRHS, int shiftAmount, string doc = null)
			{
				GenerateOperationCode(
					BuildBitMaskForInteger(byteMaskLHS),
					BuildBitMaskForInteger(byteMaskRHS),
					shiftAmount, doc);
			}
			void GenerateWordOperationCode(int wordSize, string doc = null)
			{
				string mask_lhs = BuildWordMaskForIntegerMsb(wordSize);
				string mask_rhs = BuildWordMaskForIntegerLsb(wordSize);

				GenerateOperationCode(
					mask_lhs,
					mask_rhs,
					(wordSize * kBitsPerByte) / 2, doc);
			}
			protected override void GenerateCode()
			{
				GenerateBitOperationCode(kMaskOddBits, kMaskEvenBits, 1,
					"swap odd and even bits");
				GenerateBitOperationCode(kMaskConsecutivePairsMsb, kMaskConsecutivePairsLsb, 2,
					"swap consecutive pairs");
				GenerateBitOperationCode(kMaskNibbleMsb, kMaskNibbleLsb, 4,
					"swap nibbles");

				if (mDef.SizeOfInBytes >= sizeof(ushort))
					GenerateWordOperationCode(sizeof(ushort), "swap bytes");
				if (mDef.SizeOfInBytes >= sizeof(uint))
					GenerateWordOperationCode(sizeof(uint), "swap halfs");
				if (mDef.SizeOfInBytes >= sizeof(ulong))
					GenerateWordOperationCode(sizeof(ulong), "swap words");

				mFile.WriteLine("");
			}
		};

		public class BitCountCodeGenerator : BitFondleCodeGenerator
		{
			public BitCountCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			protected override void GenerateXmlDoc()
			{
				mFile.WriteXmlDocSummary("Count the number of 'on' bits in an unsigned integer");
				mFile.WriteXmlDocParameter(kBitsParamName,
					"Integer whose bits to count");
				mFile.WriteXmlDocReturns("");
			}
			protected override void GenerateMethodSignature()
			{
				string param_bits = string.Format("{0} {1}", mDef.Keyword, kBitsParamName);

				mFile.WriteLine("[Contracts.Pure]");
				mFile.WriteLine("public static {0} {1}({2})",
					PrimitiveDefinitions.kInt32.Keyword,
					"BitCount",
					param_bits);
			}
			protected override void GenerateEpilogue()
			{
				mFile.WriteLine("return ({0}){1};", 
					PrimitiveDefinitions.kInt32.Keyword,
					kFondleVarName);
			}

			void GenerateOperationCode(string maskLHS, string maskRHS, int shiftAmount, string doc)
			{
				mFile.Write("{0} = (({0} & {1}) >> {3,2}) + ({0} & {2});",
					kFondleVarName,
					maskLHS,
					maskRHS,
					shiftAmount);
				if (doc != null)
					mFile.WriteLine(" // {0}", doc);
				else
					mFile.WriteLine("");
			}
			void GenerateBitOperationCode(byte byteMaskLHS, byte byteMaskRHS, int shiftAmount, string doc = null)
			{
				GenerateOperationCode(
					BuildBitMaskForInteger(byteMaskLHS),
					BuildBitMaskForInteger(byteMaskRHS),
					shiftAmount, doc);
			}
			void GenerateWordOperationCode(int wordSize, string doc = null)
			{
				string mask_lhs = BuildWordMaskForIntegerMsb(wordSize);
				string mask_rhs = BuildWordMaskForIntegerLsb(wordSize);

				GenerateOperationCode(
					mask_lhs,
					mask_rhs,
					(wordSize * kBitsPerByte) / 2, doc);
			}
			protected override void GenerateCode()
			{
				GenerateBitOperationCode(kMaskOddBits, kMaskEvenBits, 1);
				GenerateBitOperationCode(kMaskConsecutivePairsMsb, kMaskConsecutivePairsLsb, 2);
				GenerateBitOperationCode(kMaskNibbleMsb, kMaskNibbleLsb, 4);

				if (mDef.SizeOfInBytes >= sizeof(ushort))
					GenerateWordOperationCode(sizeof(ushort));
				if (mDef.SizeOfInBytes >= sizeof(uint))
					GenerateWordOperationCode(sizeof(uint));
				if (mDef.SizeOfInBytes >= sizeof(ulong))
					GenerateWordOperationCode(sizeof(ulong));

				mFile.WriteLine("");
			}
		};
	};
}