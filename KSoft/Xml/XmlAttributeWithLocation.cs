using System;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Xml
{
	class XmlAttributeWithLocation : XmlAttribute, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo;

		internal XmlAttributeWithLocation(string prefix, string localName, string namespaceURI, XmlDocumentWithLocation document)
			: base(prefix, localName, namespaceURI, document)
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