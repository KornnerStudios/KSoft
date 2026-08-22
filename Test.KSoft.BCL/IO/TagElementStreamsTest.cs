using System;
using System.Collections.Generic;
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

	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}
	static void AssertThrowsArgument(Action action, string paramName)
	{
		try
		{
			action();
			Assert.Fail("Expected an ArgumentException.");
		}
		catch (ArgumentException exception)
		{
			Assert.AreEqual(paramName, exception.ParamName);
		}
	}
	static void AssertThrowsInvalidStreamMode(Action action)
	{
		var exception = Assert.ThrowsExactly<InvalidOperationException>(action);

		Assert.AreEqual("Stream doesn't support the requested access mode", exception.Message);
	}

	[TestMethod]
	public void XmlElementStream_WriteGeneratedSurfaces_ProducesExpectedShapeTest()
	{
		var stream = XmlElementStream.CreateForWrite("root");
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
	public void XmlElementStream_GeneratedCollectionSurfaces_StreamExpectedValuesTest()
	{
		using var writeStream = XmlElementStream.CreateForWrite("root");
		var writeValues = new List<int> { 10, 11 };
		var streamValues = new List<int> { 12, 13 };
		char[] letters = ['A', 'B'];

		writeStream.WriteElements("item", writeValues, NumeralBase.Hex);
		writeStream.StreamElements("streamed", streamValues, NumeralBase.Hex);
		writeStream.StreamFixedArray("letter", letters);

		// Keep this XML whitespace-free: OuterXml is compared exactly and formatted raw strings change the shape.
		const string expected =
			"""<root><item>A</item><item>B</item><streamed>C</streamed><streamed>D</streamed>""" +
			"""<letter>A</letter><letter>B</letter></root>""";
		Assert.AreEqual(expected, writeStream.Document.OuterXml);

		using var readStream = CreateReadStream(writeStream.Document.OuterXml);
		var readValues = new List<int>();
		var readStreamValues = new List<int>();
		char[] readLetters = new char[3];

		readStream.ReadElements("item", readValues, NumeralBase.Hex);
		readStream.StreamElements("streamed", readStreamValues, NumeralBase.Hex);
		int readLetterCount = readStream.ReadFixedArray("letter", readLetters);

		CollectionAssert.AreEqual(writeValues, readValues);
		CollectionAssert.AreEqual(streamValues, readStreamValues);
		Assert.AreEqual(2, readLetterCount);
		Assert.AreEqual('A', readLetters[0]);
		Assert.AreEqual('B', readLetters[1]);
		Assert.AreEqual(default, readLetters[2]);
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

	[TestMethod]
	public void XmlElementStream_StreamModeInitialization_SeparatesPermissionsFromModeTest()
	{
		using var readWriteStream = CreateReadWriteStream("<root />");
		using var readStream = CreateReadStream("<root />");
		using var writeStream = XmlElementStream.CreateForWrite("root");

		Assert.AreEqual(FileAccess.ReadWrite, readWriteStream.StreamPermissions);
		Assert.AreEqual((FileAccess)0, readWriteStream.StreamMode);
		Assert.AreEqual(FileAccess.Read, readStream.StreamPermissions);
		Assert.AreEqual(FileAccess.Read, readStream.StreamMode);
		Assert.AreEqual(FileAccess.Write, writeStream.StreamPermissions);
		Assert.AreEqual(FileAccess.Write, writeStream.StreamMode);
	}

	[TestMethod]
	public void XmlElementStream_StreamModeSetterRejectsInvalidModesTest()
	{
		using var readWriteStream = CreateReadWriteStream("<root />");
		using var readStream = CreateReadStream("<root />");
		using var writeStream = XmlElementStream.CreateForWrite("root");

		readWriteStream.StreamMode = FileAccess.Read;
		readWriteStream.StreamMode = FileAccess.Write;
		readWriteStream.StreamMode = 0;
		AssertThrowsArgumentOutOfRange(() => readWriteStream.StreamMode = FileAccess.ReadWrite, "value");
		AssertThrowsArgumentOutOfRange(() => readWriteStream.StreamMode = (FileAccess)4, "value");
		AssertThrowsInvalidStreamMode(() => readStream.StreamMode = FileAccess.Write);
		AssertThrowsInvalidStreamMode(() => writeStream.StreamMode = FileAccess.Read);
	}

	[TestMethod]
	public void XmlElementStream_InvalidElementNames_ThrowExpectedExceptionsTest()
	{
		using var stream = CreateReadStream("<root><child /></root>");

		AssertThrowsArgument(() => stream.ReadElementBegin(null!, out _), "name");
		AssertThrowsArgument(() => stream.ReadElementBegin(string.Empty, out _), "name");
		AssertThrowsArgument(() => stream.ElementsByName(null!).GetEnumerator().MoveNext(), "localName");
		AssertThrowsArgument(() => stream.ElementsByName(string.Empty).GetEnumerator().MoveNext(), "localName");
	}

	[TestMethod]
	public void XmlElementStream_LifecycleNullStates_AreExposedByContractsTest()
	{
		using var stream = XmlElementStream.CreateForWrite("root");

		Assert.IsNull(stream.Owner);
		Assert.IsNull(stream.UserData);
		Assert.IsNull(stream.StreamName);

		var owner = new object();
		var userData = new object();
		stream.Owner = owner;
		stream.UserData = userData;

		using (var ownerBookmark = new IKSoftStreamOwnerBookmark(stream, null))
		using (var userDataBookmark = new IKSoftStreamUserDataBookmark(stream, null))
		{
			Assert.IsNull(stream.Owner);
			Assert.IsNull(stream.UserData);
		}

		Assert.AreSame(owner, stream.Owner);
		Assert.AreSame(userData, stream.UserData);

		stream.Dispose();

		Assert.IsNull(stream.Owner);
		Assert.IsNull(stream.Cursor);
	}

	[TestMethod]
	public void XmlElementStream_WriteAttributeWithoutCursor_ThrowsInvalidOperationTest()
	{
		var stream = XmlElementStream.CreateForWrite("root");
		stream.Dispose();

		var exception = Assert.ThrowsExactly<InvalidOperationException>(() => stream.WriteAttribute("name", "value"));

		Assert.AreEqual("Element cursor must not be null when writing an attribute.", exception.Message);
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
	static XmlElementStream CreateReadWriteStream(string xml)
	{
		var document = new Xml.XmlDocumentWithLocation
		{
			XmlResolver = null,
		};
		document.LoadXml(xml);

		return new XmlElementStream(document, document.DocumentElement);
	}
}
