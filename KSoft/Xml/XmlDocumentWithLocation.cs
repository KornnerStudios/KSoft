using System;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Xml
{
	public class XmlDocumentWithLocation : XmlDocument
	{
		IXmlLineInfo mLoadReader;

		public string FileName { get; set; }

		internal Text.TextLineInfo CurrentLineInfo { get {
			if (mLoadReader != null && mLoadReader.HasLineInfo())
				return new Text.TextLineInfo(mLoadReader.LineNumber, mLoadReader.LinePosition);

			return Text.TextLineInfo.Empty;
		} }

		public override void Load(string filename)
		{
			FileName = filename;

			base.Load(filename);
		}

		public override void Load(XmlReader reader)
		{
			mLoadReader = (IXmlLineInfo)reader;
			base.Load(reader);
			mLoadReader = null;
		}

		#region Create overrides
		public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
		{
			return new XmlAttributeWithLocation(prefix, localName, namespaceURI, this);
		}

		public override XmlCDataSection CreateCDataSection(string data)
		{
			return new XmlCDataSectionWithLocation(data, this);
		}

		public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
		{
			return new XmlElementWithLocation(prefix, localName, namespaceURI, this);
		}

		public override XmlText CreateTextNode(string text)
		{
			return new XmlTextWithLocation(text, this);
		}
		#endregion

		string GetFileLocationStringWithLineOnly(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			return string.Format("{0} ({1})",
				FileName, Text.TextLineInfo.ToStringLineOnly(lineInfo, verboseString));
		}
		string GetFileLocationStringWithColumn(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			return string.Format("{0} ({1})",
				FileName, Text.TextLineInfo.ToString(lineInfo, verboseString));
		}
		public string GetFileLocationString(XmlNode node, bool verboseString = false)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			Contract.Requires<ArgumentException>(node.OwnerDocument == this);
			Contract.Requires<ArgumentException>(node is XmlAttributeWithLocation || node is XmlElementWithLocation,
				"Can only retrieve location of nodes with location data");

			var loc_info = (Text.ITextLineInfo)node;

			if (!loc_info.HasLineInfo)
				return FileName;
			else if (loc_info.LinePosition != 0)
				return GetFileLocationStringWithColumn(loc_info, verboseString);
			else
				return GetFileLocationStringWithLineOnly(loc_info, verboseString);
		}
	};
}