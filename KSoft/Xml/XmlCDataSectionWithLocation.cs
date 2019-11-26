using System.Xml;

namespace KSoft.Xml
{
	class XmlCDataSectionWithLocation : XmlCDataSection, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo;

		internal XmlCDataSectionWithLocation(string data, XmlDocumentWithLocation document)
			: base(data, document)
		{
			mLineInfo = document.CurrentLineInfo;
		}

		internal Text.TextLineInfo LineInfo { get { return mLineInfo; } }

		public bool HasLineInfo { get { return mLineInfo.HasLineInfo; } }
		public int LineNumber	{ get { return mLineInfo.LineNumber; } }
		public int LinePosition	{ get { return mLineInfo.LinePosition; } }

		#region IXmlLineInfo Members
		bool IXmlLineInfo.HasLineInfo()	{ return mLineInfo.HasLineInfo; }
		int IXmlLineInfo.LineNumber		{ get { return mLineInfo.LineNumber; } }
		int IXmlLineInfo.LinePosition	{ get { return mLineInfo.LinePosition; } }
		#endregion
	};
}