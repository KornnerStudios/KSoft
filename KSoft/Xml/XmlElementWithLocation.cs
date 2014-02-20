using System;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Xml
{
	class XmlElementWithLocation : XmlElement, IXmlLineInfo, Text.ITextLineInfo
	{
		readonly Text.TextLineInfo mLineInfo;

		internal XmlElementWithLocation(string prefix, string localName, string namespaceURI, XmlDocumentWithLocation document)
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

		XmlAttributeWithLocation GetAttributeWithLocation(string name)
		{
			var attr = Attributes[name];

			return (XmlAttributeWithLocation)attr;
		}

		public Text.TextLineInfo GetAttributeLineInfo(string name)
		{
			var attr_with_location = GetAttributeWithLocation(name);

			return attr_with_location != null
				? attr_with_location.LineInfo
				: Text.TextLineInfo.Empty;
		}
	};
}