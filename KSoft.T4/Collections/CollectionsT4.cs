using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4
{
	public static class CollectionsT4
	{
		public sealed class BitSetEnumeratorDef
		{
			public string Name { get; set; }
			public PrimitiveCodeDefinition ResultCodeDef { get; set; }

			public BitSetEnumeratorDef(string name, PrimitiveCodeDefinition def)
			{
				Name = name;
				ResultCodeDef = def;
			}
		};
		public static IEnumerable<BitSetEnumeratorDef> BitSetEnumeratorDefs { get {
			yield return new BitSetEnumeratorDef("State", PrimitiveDefinitions.kBool);
			yield return new BitSetEnumeratorDef("StateFilter", PrimitiveDefinitions.kInt32);
		} }

		public sealed class BitStateDef
		{
			public string ApiName;
			public bool Value;

			public BitStateDef(string name, bool value)
			{
				ApiName = name;
				Value = value;
			}

			// name to use in XMLdoc contents
			public string DocName { get {
				return ApiName.ToLower();
			} }
			// C# bool keyword
			public string ValueKeyword { get {
				return Value.ToString().ToLower();
			} }
			public string BinaryName { get {
				return Value
					? "1"
					: "0";
			} }

			public string DocNameVerbose { get {
				return string.Format("{0} ({1})", BinaryName, DocName);
			} }
		};
		public static IEnumerable<BitStateDef> BitStateDefs { get {
			yield return new BitStateDef("Clear", false);
			yield return new BitStateDef("Set", true);
		} }
	};
}