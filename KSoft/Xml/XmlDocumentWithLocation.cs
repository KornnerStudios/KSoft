#nullable enable

using System;
using System.Xml;

namespace KSoft.Xml
{
	public class XmlDocumentWithLocation : XmlDocument
	{
		IXmlLineInfo? mLoadReader;

		public string? FileName { get; set; }

		internal Text.TextLineInfo CurrentLineInfo { get {
			var loadReader = mLoadReader;
			if (loadReader != null && loadReader.HasLineInfo())
				{
					return new Text.TextLineInfo(loadReader.LineNumber, loadReader.LinePosition);
				}

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
		public override XmlAttribute CreateAttribute(string? prefix, string localName, string? namespaceURI)
		{
			return new XmlAttributeWithLocation(prefix!, localName, namespaceURI!, this);
		}

		public override XmlCDataSection CreateCDataSection(string? data)
		{
			return new XmlCDataSectionWithLocation(data!, this);
		}

		public override XmlElement CreateElement(string? prefix, string localName, string? namespaceURI)
		{
			return new XmlElementWithLocation(prefix!, localName, namespaceURI!, this);
		}

		public override XmlText CreateTextNode(string? text)
		{
			return new XmlTextWithLocation(text!, this);
		}
		#endregion

		string GetFileLocationStringWithLineOnly(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			return string.Format(Util.InvariantCultureInfo,
				"{0} ({1})",
				FileName, Text.TextLineInfo.ToStringLineOnly(lineInfo, verboseString));
		}
		string GetFileLocationStringWithColumn(Text.ITextLineInfo lineInfo, bool verboseString)
		{
			return string.Format(Util.InvariantCultureInfo,
				"{0} ({1})",
				FileName, Text.TextLineInfo.ToString(lineInfo, verboseString));
		}
		public string? GetFileLocationString(XmlNode node, bool verboseString = false)
		{
			ArgumentNullException.ThrowIfNull(node);

			if (node.OwnerDocument != this)
			{
				throw new ArgumentException("Can only retrieve locations for nodes owned by this document.", nameof(node));
			}

			if (node is not XmlAttributeWithLocation && node is not XmlElementWithLocation)
			{
				throw new ArgumentException("Can only retrieve location of nodes with location data", nameof(node));
			}

			var loc_info = (Text.ITextLineInfo)node;

			if (!loc_info.HasLineInfo)
			{
				return FileName;
			}
			else if (loc_info.LinePosition != 0)
			{
				return GetFileLocationStringWithColumn(loc_info, verboseString);
			}
			else
			{
				return GetFileLocationStringWithLineOnly(loc_info, verboseString);
			}
		}
	};
}
