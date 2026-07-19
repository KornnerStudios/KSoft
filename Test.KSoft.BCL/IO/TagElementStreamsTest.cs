using System;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test;

[TestClass]
public sealed class TagElementStreamsTest : BaseTestClass
{
	const string kGuidText = "00112233-4455-6677-8899-aabbccddeeff";

	[Flags]
	enum SampleFlags
	{
		None = 0,
		Alpha = 1,
		Beta = 2,
	}

	[TestMethod]
	public void XmlElementStream_WriteGeneratedSurfaces_ProducesExpectedShapeTest()
	{
		using var stream = XmlElementStream.CreateForWrite("root");
		var guid = new Values.KGuid(kGuidText);

		stream.WriteAttribute("name", "Vita");
		stream.WriteAttribute("enabled", true);
		stream.WriteAttribute("guid", guid);
		stream.WriteElement("title", "Generator");
		stream.WriteElement("letter", 'K');
		stream.WriteElement("count", 42);
		stream.WriteElement("hex", 42, NumeralBase.Hex);
		stream.WriteElementEnum("mode", SampleFlags.Alpha | SampleFlags.Beta, isFlags: true);
		stream.WriteElement("guidElement", guid);
		stream.WriteComment("note");
		stream.WriteElementBegin("nested", out XmlElement oldCursor);
		stream.WriteAttribute("active", true);
		stream.WriteElement("value", 7);
		stream.WriteElementEnd(ref oldCursor);

		// Keep this XML whitespace-free: OuterXml is compared exactly and formatted raw strings change the shape.
		const string expected =
			"""<root name="Vita" enabled="true" guid="00112233-4455-6677-8899-aabbccddeeff">""" +
			"""<title>Generator</title><letter>K</letter><count>42</count><hex>2A</hex><mode>Alpha, Beta</mode>""" +
			"""<guidElement>00112233-4455-6677-8899-aabbccddeeff</guidElement><!--note-->""" +
			"""<nested active="true"><value>7</value></nested></root>""";
		Assert.AreEqual(expected, stream.Document.OuterXml);
		Assert.IsNull(oldCursor);
		Assert.AreEqual("root", stream.CursorName);
	}

	[TestMethod]
	public void XmlElementStream_ReadGeneratedSurfaces_ParsesExpectedValuesTest()
	{
		// Keep this XML whitespace-free so traversal only sees the elements being characterized.
		const string xml =
			$"""<root name="Vita" enabled="on" hex="2A" mode="Alpha, Beta" guid="{kGuidText}">""" +
			"""<title>Generator</title><letter>K</letter><count>42</count><ratio>1.25f</ratio>""" +
			$"""<guidElement>{kGuidText}</guidElement><empty /></root>""";
		using var stream = CreateReadStream(xml);

		string title = null;
		char letter = default;
		int count = default;
		float ratio = default;
		bool enabled = default;
		int hex = default;
		SampleFlags flags = default;
		var guidAttribute = default(Values.KGuid);
		var guidElement = default(Values.KGuid);

		stream.ReadElement("title", ref title);
		stream.ReadElement("letter", ref letter);
		stream.ReadElement("count", ref count);
		stream.ReadElement("ratio", ref ratio);
		stream.ReadAttribute("enabled", ref enabled);
		stream.ReadAttribute("hex", ref hex, NumeralBase.Hex);
		stream.ReadAttributeEnum("mode", ref flags);
		stream.ReadAttribute("guid", ref guidAttribute);
		stream.ReadElement("guidElement", ref guidElement);

		Assert.AreEqual("Generator", title);
		Assert.AreEqual('K', letter);
		Assert.AreEqual(42, count);
		Assert.AreEqual(1.25f, ratio);
		Assert.IsTrue(enabled);
		Assert.AreEqual(42, hex);
		Assert.AreEqual(SampleFlags.Alpha | SampleFlags.Beta, flags);
		Assert.AreEqual(new Values.KGuid(kGuidText), guidAttribute);
		Assert.AreEqual(new Values.KGuid(kGuidText), guidElement);
	}

	[TestMethod]
	public void XmlElementStream_ReadOptionalGeneratedSurfaces_PreservesMissingValueSemanticsTest()
	{
		using var stream = CreateReadStream("<root present=\"123\"><empty /></root>");
		string missingString = "keep";
		int missingInt = 123;
		string emptyString = "keep";
		int present = 0;

		Assert.IsFalse(stream.ReadElementOpt("missing", ref missingString));
		Assert.IsNull(missingString);
		Assert.IsFalse(stream.ReadElementOpt("missing", ref missingInt));
		Assert.AreEqual(123, missingInt);
		Assert.IsFalse(stream.ReadElementOpt("empty", ref emptyString));
		Assert.IsNull(emptyString);
		Assert.IsTrue(stream.ReadAttributeOpt("present", ref present));
		Assert.AreEqual(123, present);
	}

	[TestMethod]
	public void TagElementStreamFactory_OpenUnsupportedFormats_ThrowsExpectedExceptionsTest()
	{
		Assert.Throws<NotImplementedException>(() =>
			TagElementStreamFactory.Open(new MemoryStream(), TagElementStreamFormat.Json, FileAccess.Read));
		Assert.Throws<NotImplementedException>(() =>
			TagElementStreamFactory.Open(new MemoryStream(), TagElementStreamFormat.Json | TagElementStreamFormat.Binary,
				FileAccess.Read));
		Assert.Throws<NotImplementedException>(() =>
			TagElementStreamFactory.Open(new MemoryStream(), TagElementStreamFormat.Xml | TagElementStreamFormat.Binary,
				FileAccess.Read));
		Assert.Throws<NotSupportedException>(() =>
			TagElementStreamFactory.Open(new MemoryStream(), TagElementStreamFormat.Yaml | TagElementStreamFormat.Binary,
				FileAccess.Read));
		Assert.Throws<ArgumentException>(() =>
			TagElementStreamFactory.Open(new MemoryStream(), TagElementStreamFormat.Undefined, FileAccess.Read));
	}

	static XmlElementStream CreateReadStream(string xml)
	{
		var document = new Xml.XmlDocumentWithLocation
		{
			XmlResolver = null,
		};
		document.LoadXml(xml);

		return new XmlElementStream(document, document.DocumentElement, FileAccess.Read);
	}
}
