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
	public void NonPositiveLineNumber_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(0, 1), "lineNumber");
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(-1, 1), "lineNumber");
	}

	[TestMethod]
	public void NonPositiveLinePosition_ThrowsArgumentOutOfRangeException()
	{
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(1, 0), "linePosition");
		AssertThrowsArgumentOutOfRange(() => _ = new TextLineInfo(1, -1), "linePosition");
	}

	[TestMethod]
	public void NullCopySource_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull(() => _ = new TextLineInfo((ITextLineInfo)null!), "otherLineInfo");
	}

	[TestMethod]
	public void EqualityAndComparison_PreserveNullContractBehavior()
	{
		var lineInfo = new TextLineInfo(4, 5);
		var sameLineInfo = new TextLineInfo(4, 5);
		var laterColumn = new TextLineInfo(4, 6);
		var laterLine = new TextLineInfo(5, 1);
		ITextLineInfo? nullLineInfo = null;

		Assert.IsTrue(lineInfo.Equals(sameLineInfo));
		Assert.IsFalse(lineInfo.Equals(laterColumn));
		Assert.IsTrue(lineInfo.Equals((object)sameLineInfo));
		Assert.IsFalse(lineInfo.Equals((object)laterColumn));
		Assert.IsFalse(lineInfo.Equals((object)null!));
		Assert.AreEqual(0, lineInfo.CompareTo(sameLineInfo));
		Assert.AreEqual(-1, lineInfo.CompareTo(laterColumn));
		Assert.AreEqual(-1, lineInfo.CompareTo(laterLine));
		Assert.ThrowsExactly<NullReferenceException>(() => lineInfo.Equals(nullLineInfo));
		Assert.ThrowsExactly<NullReferenceException>(() => lineInfo.CompareTo(nullLineInfo));
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
	public void TextStreamReadErrorState_WithoutLineInfo_ThrowsInvalidOperationException()
	{
		using var stream = new IO.EndianStream(new System.IO.MemoryStream());
		var errorState = new IO.TextStreamReadErrorState(stream);
		var detailsException = new InvalidOperationException("details");
		const string expectedMessage =
			"A Text stream reader implementation failed to set the LastReadLineInfo before a read took place. " +
			"Guess what? Said read just failed";

		Assert.IsNull(errorState.LastReadLineInfo);
		Assert.AreEqual(expectedMessage,
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = errorState.GetLineInfoException()).Message);
		Assert.AreEqual(expectedMessage,
			Assert.ThrowsExactly<InvalidOperationException>(
				() => errorState.ThrowReadExeception(detailsException)).Message);
		Assert.AreEqual(expectedMessage,
			Assert.ThrowsExactly<InvalidOperationException>(
				() => errorState.LogReadExceptionWarning(detailsException)).Message);
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

	[TestMethod]
	public void TextLineInfoException_AllowsOptionalExceptionAndUnknownStreamName()
	{
		var lineInfo = new TextLineInfo(4, 5);

		var nullNameException = new TextLineInfoException(null!, lineInfo, null);
		var emptyNameException = new TextLineInfoException(lineInfo, string.Empty);

		Assert.IsNull(nullNameException.InnerException);
		Assert.AreEqual("<unknown text stream>", nullNameException.StreamName);
		Assert.AreEqual("<unknown text stream>", emptyNameException.StreamName);
	}
}
