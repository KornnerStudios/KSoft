using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test;

[TestClass]
public sealed class TextLineInfoGuardTest : BaseTestClass
{
	static void AssertThrowsArgumentNull(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}
	static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
	{
		var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

		Assert.AreEqual(paramName, exception.ParamName);
	}

	[TestMethod]
	public void TextLineInfo_NonPositiveLineNumber_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(0, 1), "lineNumber");
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(-1, 1), "lineNumber");
	}

	[TestMethod]
	public void TextLineInfo_NonPositiveLinePosition_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(1, 0), "linePosition");
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(1, -1), "linePosition");
	}

	[TestMethod]
	public void TextLineInfo_NullCopySource_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new TextLineInfo((ITextLineInfo)null!), "otherLineInfo");
	}

	[TestMethod]
	public void TextLineInfoException_NullLineInfo_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new TextLineInfoException((ITextLineInfo)null!), "lineInfo");
		AssertThrowsArgumentNull(
			() => _ = new TextLineInfoException(new InvalidOperationException(), null!),
			"lineInfo");
	}

	[TestMethod]
	public void TextStreamReadErrorState_NullStream_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new IO.TextStreamReadErrorState(null!), "textStream");
	}

	[TestMethod]
	public void TextLineInfoException_CopiesLineInfoAndStreamName()
	{
		var lineInfo = new TextLineInfo(4, 5);

		var exception = new TextLineInfoException(lineInfo, "file.txt");

		Assert.AreEqual("file.txt", exception.StreamName);
		Assert.AreEqual(4, exception.LineNumber);
		Assert.AreEqual(5, exception.LinePosition);
	}
}
