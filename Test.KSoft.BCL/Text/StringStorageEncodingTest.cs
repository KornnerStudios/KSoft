using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	using MS = Memory.Strings;

	/// <summary>This is a test class for StringStorageEncoding and is intended to contain all EStringStorageEncoding Unit Tests</summary>
	[TestClass]
	public class StringStorageEncodingTest : BaseTestClass
	{
		[TestMethod]
		public void Text_StringStorageEncodingWriteTest()
		{
			const bool k_output_ms = true;

			using (var ms = new System.IO.MemoryStream())
			using (var io = new IO.EndianStream(ms))
			{
				MS.StringStorage storage =
					//Strings.StringStorage.kCStringUnicode;
					new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageType.CString, 256);
				var encoding = StringStorageEncoding.TryAndGetStaticEncoding(storage);

				const string test1 = "This is a test",
					test2 = "Test this is",
					test3 = "wtf is apple juice?",
					test4 = "one more for good luck.";

				// Test case for strings which are greater-than-or-equal to the CString storage length (which includes a null terminating char)
				var sb = new System.Text.StringBuilder(storage.FixedLength);
				sb.Append('1', storage.FixedLength);
				string test5 = sb.ToString();

				io.Writer.Write(test1, encoding);
				io.Writer.Write(test2, encoding);
				io.Writer.Write(test3, encoding);
				io.Writer.Write(test4, encoding);
				io.Writer.Write(test5, encoding);

				if(k_output_ms) using (var fs = new System.IO.FileStream(System.IO.Path.Combine(kTestResultsPath, "StringStorageEncodingTestWrite.bin"), 
					System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
				{
					ms.WriteTo(fs);
				}

				ms.Seek(0, System.IO.SeekOrigin.Begin);
				Assert.AreEqual(test1, io.Reader.ReadString(encoding));
				Assert.AreEqual(test2, io.Reader.ReadString(encoding, test2.Length));
				Assert.AreEqual(test3, io.Reader.ReadString(encoding));
				Assert.AreEqual(test4, io.Reader.ReadString(encoding, test4.Length));
				Assert.AreNotEqual(test5, io.Reader.ReadString(encoding));
			}
		}
	};
}