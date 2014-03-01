using System;
using System.Collections.Generic;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	public static partial class BitwiseT4
	{
		const int kBitsPerByte = sizeof(byte) * 8;

		// Get the keyword used to define general constants for integer types (bit count, etc)
		public static string GetConstantKeyword(this NumberCodeDefinition def)
		{
			switch(def.Code)
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					return "Byte";

				case TypeCode.UInt16:
				case TypeCode.Int16:
					return "Int16";

				case TypeCode.UInt32:
				case TypeCode.Int32:
					return "Int32";

				case TypeCode.UInt64:
				case TypeCode.Int64:
					return "Int64";

				default:
					throw new InvalidOperationException(def.Code.ToString());
			}
		}
		public static string GetVectorsSuffix(this NumberCodeDefinition def)
		{
			if (!def.IsByte)
				return GetConstantKeyword(def);

			return "Bytes";
		}

		#region Bitstream related
		/// <summary>	The integer type to use for bitstream cache operations. </summary>
		public static NumberCodeDefinition BitStreamCacheWord { get {
			return PrimitiveDefinitions.kUInt32;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleIntegerTypes { get {
			yield return PrimitiveDefinitions.kChar;

			foreach (var num_type in PrimitiveDefinitions.Numbers)
				if (num_type.IsInteger)
					yield return num_type;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleNonIntegerTypes { get {
			yield return PrimitiveDefinitions.kBool;

			yield return PrimitiveDefinitions.kSingle;
			yield return PrimitiveDefinitions.kDouble;
		} }
		#endregion

		#region BittableTypes
		static readonly IReadOnlyList<NumberCodeDefinition> kBittableTypes_Unsigned = new List<NumberCodeDefinition> {
			PrimitiveDefinitions.kByte,
			PrimitiveDefinitions.kUInt16,
			PrimitiveDefinitions.kUInt32,
			PrimitiveDefinitions.kUInt64,
		};
		public static IReadOnlyList<NumberCodeDefinition> BittableTypes_Unsigned { get {
			return kBittableTypes_Unsigned;
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypes_MajorWords { get {
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kUInt64;
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypes { get {
			foreach (var num_type in PrimitiveDefinitions.Numbers)
				if (num_type.IsInteger)
					yield return num_type;
		} }
		#endregion

		public class IntegerByteAccessCodeGenerator
		{
			static readonly string kByteKeyword = NumberCodeDefinition.TypeCodeToKeyword(TypeCode.Byte);

			NumberCodeDefinition mDef;
			string mByteName;

			string mBufferName;
			string mOffsetName; // NOTE: offset variable must be mutable!

			public IntegerByteAccessCodeGenerator(NumberCodeDefinition def,
				string byteName, string bufferName, string offsetName = null)
			{
				mDef = def;
				mByteName = byteName;

				mBufferName = bufferName;
				mOffsetName = offsetName;
			}

			public string GenerateByteDeclarations()
			{
				var sb = new System.Text.StringBuilder();

				sb.AppendFormat("{0} ", kByteKeyword);
				for (int x = 0; x < mDef.SizeOfInBytes; x++)
				{
					sb.AppendFormat("{0}{1}", mByteName, x);

					if (x < (mDef.SizeOfInBytes-1))
						sb.Append(", ");
				}
				sb.Append(';');

				return sb.ToString();
			}
			public string GenerateBytesFromBuffer()
			{
				Debug.Assert(mOffsetName != null, "generator not setup for read/write from/to a buffer");

				var sb = new System.Text.StringBuilder();

				for (int x = 0; x < mDef.SizeOfInBytes; x++, sb.Append("; "))
				{
					sb.AppendFormat("{0}{1} = ", mByteName, x);
					sb.AppendFormat("{0}[{1}++]", mBufferName, mOffsetName);
				}

				return sb.ToString();
			}
			public string GenerateBytesFromValue(string valueName, bool littleEndian = true)
			{
				var sb = new System.Text.StringBuilder();

				int bit_offset = littleEndian
					? mDef.SizeOfInBits
					: 0 - kBitsPerByte;
				int bit_adjustment = littleEndian
					? -kBitsPerByte
					: +kBitsPerByte;

				for (int x = 0; x < mDef.SizeOfInBytes; x++, sb.Append("; "))
				{
					sb.AppendFormat("{0}{1} = ", mByteName, x);
					sb.AppendFormat("({0})({1} >> {2,2})", kByteKeyword, valueName, bit_offset += bit_adjustment);
				}

				return sb.ToString();
			}
			public string GenerateWriteBytesToBuffer(bool useSwapFormat = true)
			{
				const string k_swap_format =		"{0}[--{1}] = ";
				const string k_replacement_format =	"{0}[{1}++] = ";

				Debug.Assert(mOffsetName != null);

				var sb = new System.Text.StringBuilder();
				string format = useSwapFormat
					? k_swap_format
					: k_replacement_format;

				for (int x = 0; x < mDef.SizeOfInBytes; x++, sb.Append("; "))
				{
					sb.AppendFormat(format, mBufferName, mOffsetName);
					sb.AppendFormat("{0}{1}", mByteName, x);
				}

				return sb.ToString();
			}
		};

		public abstract class BitUtilCodeGenerator
		{
			protected TextTemplating.TextTransformation mFile;
			protected NumberCodeDefinition mDef;

			protected BitUtilCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
			{
				mFile = ttFile;
				mDef = def;
			}

			public void Generate()
			{
				using (mFile.EnterCodeBlock())
				using (mFile.EnterCodeBlock())
				{
					GenerateXmlDoc();
					GenerateMethodSignature();

					using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
					{
						GeneratePrologue();
						GenerateCode();
						GenerateEpilogue();
					}
				}
			}

			protected abstract void GenerateXmlDoc();
			protected abstract void GenerateMethodSignature();
			protected abstract void GeneratePrologue();
			protected abstract void GenerateEpilogue();
			protected abstract void GenerateCode();
		};
	};
}