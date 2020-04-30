
namespace KSoft.IO
{
	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region WriteElement impl
		protected override void WriteElementEnum<TEnum>(TCursor n, TEnum value, bool isFlags)
		{
			WriteElement(n, isFlags ?
				Text.Util.EnumToFlagsString(value) :
				Text.Util.EnumToString(value));
		}

		protected override void WriteElement(TCursor n, Values.KGuid value)
		{
			WriteElement(n, value.ToString(mGuidFormatString, KSoft.Util.InvariantCultureInfo));
		}
		#endregion

		#region WriteAttribute
		protected abstract void CursorWriteAttribute(string name, string value);

		public override void WriteAttributeEnum<TEnum>(string name, TEnum value, bool isFlags = false)
		{
			CursorWriteAttribute(name, isFlags ?
				Text.Util.EnumToFlagsString(value) :
				Text.Util.EnumToString(value));
		}

		public override void WriteAttribute(string name, Values.KGuid value)
		{
			CursorWriteAttribute(name, value.ToString(mGuidFormatString, Util.InvariantCultureInfo));
		}
		#endregion
	};
}
