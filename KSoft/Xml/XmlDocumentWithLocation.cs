using System;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Xml
{
	public class XmlDocumentWithLocation : XmlDocument
	{
		IXmlLineInfo mLoadReader;

		public string Filename { get; set; }

		internal Text.TextLineInfo CurrentLineInfo { get {
			if (mLoadReader != null && mLoadReader.HasLineInfo())
				new Text.TextLineInfo(mLoadReader.LineNumber, mLoadReader.LinePosition);

			return Text.TextLineInfo.Empty;
		} }

		public override void Load(string filename)
		{
			Filename = filename;

			base.Load(filename);
		}

		public override void Load(XmlReader reader)
		{
			mLoadReader = (IXmlLineInfo)reader;
			base.Load(reader);
			mLoadReader = null;
		}

		public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
		{
			return new XmlAttributeWithLocation(prefix, localName, namespaceURI, this);
		}

		public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
		{
			return new XmlElementWithLocation(prefix, localName, namespaceURI, this);
		}

		string GetFileLocationStringWithLineOnly(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			const string k_format_string =
				"{0} ({1})";
			const string k_format_string_verbose =
				"{0} (Ln {1})";

			return string.Format(verboseString ? k_format_string_verbose : k_format_string,
				Filename, lineInfo.LineNumber.ToString());
		}
		string GetFileLocationStringWithColumn(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			const string k_format_string =
				"{0} ({1}, {2})";
			const string k_format_string_verbose =
				"{0} (Ln {1}, Col {2})";

			return string.Format(verboseString ? k_format_string_verbose : k_format_string,
				Filename, lineInfo.LineNumber.ToString(), lineInfo.LinePosition.ToString());
		}
		public string GetFileLocationString(XmlNode node, bool verboseString = false)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			Contract.Requires<ArgumentException>(node.OwnerDocument == this);
			Contract.Requires<ArgumentException>(node is XmlAttributeWithLocation || node is XmlElementWithLocation,
				"Can only retrieve location of nodes with location data");

			var loc_info = (Text.ITextLineInfo)node;

			if (!loc_info.HasLineInfo)
				return Filename;
			else if (loc_info.LinePosition != 0)
				return GetFileLocationStringWithColumn(loc_info, verboseString);
			else
				return GetFileLocationStringWithLineOnly(loc_info, verboseString);
		}
	};
}