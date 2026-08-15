using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Xml.Test;

[TestClass]
public sealed class XmlDocumentWithLocationTest : BaseTestClass
{
	static XmlDocumentWithLocation CreateDocument()
	{
		var document = new XmlDocumentWithLocation
		{
			FileName = "test.xml",
			XmlResolver = null,
		};
		document.LoadXml("<root attr=\"value\">text</root>");

		return document;
	}

	[TestMethod]
	public void GetFileLocationString_NullNode_ThrowsArgumentNullException()
	{
		var document = CreateDocument();
		var exception = Assert.ThrowsExactly<ArgumentNullException>(() => document.GetFileLocationString(null));

		Assert.AreEqual("node", exception.ParamName);
	}

	[TestMethod]
	public void GetFileLocationString_ForeignDocumentNode_ThrowsArgumentException()
	{
		var document = CreateDocument();
		var foreignNode = new XmlDocument().CreateElement("foreign");
		var exception = Assert.ThrowsExactly<ArgumentException>(() => document.GetFileLocationString(foreignNode));

		Assert.AreEqual("node", exception.ParamName);
	}

	[TestMethod]
	public void GetFileLocationString_UnsupportedLocationNode_ThrowsArgumentException()
	{
		var document = CreateDocument();
		var textNode = document.CreateTextNode("text");
		var exception = Assert.ThrowsExactly<ArgumentException>(() => document.GetFileLocationString(textNode));

		Assert.AreEqual("node", exception.ParamName);
	}

	[TestMethod]
	public void GetFileLocationString_ElementOrAttributeWithLineInfo_ReturnsFileLocation()
	{
		var document = CreateDocument();

		Assert.AreEqual("test.xml (1, 2)", document.GetFileLocationString(document.DocumentElement));
		StringAssert.StartsWith(
			document.GetFileLocationString(document.DocumentElement.GetAttributeNode("attr")),
			"test.xml (1, ");
	}
}
