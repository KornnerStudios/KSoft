using System;
using System.Collections.Generic;

namespace KSoft.T4
{
	public class PrimitiveDefinitions
	{
		class BooleanCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal BooleanCodeDefinition() : base("bool", TypeCode.Boolean) { }

			public override int SizeOfInBytes { get {
				return sizeof(bool);
			} }
		};

		class CharCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal CharCodeDefinition() : base("char", TypeCode.Char) { }

			public override int SizeOfInBytes { get {
				return sizeof(char);
			} }
		};

		class StringCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal StringCodeDefinition() : base("string", TypeCode.String) { }

			public override int SizeOfInBytes { get {
				return -1;
			} }
		};

		class KGuidCodeDefinition
			: PrimitiveCodeDefinition
		{
			internal KGuidCodeDefinition() : base("Values.KGuid", TypeCode.Object) { }

			public override int SizeOfInBytes { get {
				return 16;
			} }
		};

		#region Individual definitions
		internal static readonly NumberCodeDefinition kByte = new NumberCodeDefinition(TypeCode.Byte);
		internal static readonly NumberCodeDefinition kSByte = new NumberCodeDefinition(TypeCode.SByte);

		internal static readonly NumberCodeDefinition kUInt16 = new NumberCodeDefinition(TypeCode.UInt16);
		internal static readonly NumberCodeDefinition kInt16 = new NumberCodeDefinition(TypeCode.Int16);

		internal static readonly NumberCodeDefinition kUInt32 = new NumberCodeDefinition(TypeCode.UInt32);
		internal static readonly NumberCodeDefinition kInt32 = new NumberCodeDefinition(TypeCode.Int32);

		internal static readonly NumberCodeDefinition kUInt64 = new NumberCodeDefinition(TypeCode.UInt64);
		internal static readonly NumberCodeDefinition kInt64 = new NumberCodeDefinition(TypeCode.Int64);

		internal static readonly NumberCodeDefinition kSingle = new NumberCodeDefinition(TypeCode.Single);
		internal static readonly NumberCodeDefinition kDouble = new NumberCodeDefinition(TypeCode.Double);

		internal static readonly PrimitiveCodeDefinition kBool = new BooleanCodeDefinition();

		internal static readonly PrimitiveCodeDefinition kChar = new CharCodeDefinition();

		internal static readonly PrimitiveCodeDefinition kString = new StringCodeDefinition();

		internal static readonly PrimitiveCodeDefinition kKGuid = new KGuidCodeDefinition();
		#endregion

		/// <summary>All primitive type definitions that are numeric</summary>
		public static IEnumerable<NumberCodeDefinition> Numbers { get {
			yield return kByte;
			yield return kSByte;

			yield return kUInt16;
			yield return kInt16;

			yield return kUInt32;
			yield return kInt32;

			yield return kUInt64;
			yield return kInt64;

			yield return kSingle;
			yield return kDouble;
		} }

		/// <summary>All primitive type definitions (sans String)</summary>
		public static IEnumerable<PrimitiveCodeDefinition> Primitives { get {
			yield return kBool;

			yield return kChar;

			yield return kByte;
			yield return kSByte;

			yield return kUInt16;
			yield return kInt16;

			yield return kUInt32;
			yield return kInt32;

			yield return kUInt64;
			yield return kInt64;

			yield return kSingle;
			yield return kDouble;
		} }
	};
}