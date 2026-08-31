using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	using MS = Memory.Strings;

	/// <summary>This is a test class for StringStorageEncoding and is intended to contain all EStringStorageEncoding Unit Tests</summary>
	[TestClass]
	public class StringStorageEncodingTest : BaseTestClass
	{

		static byte[] WriteWithBitStream(string value, MS.StringStorage storage, int maxLength = -1)
		{
			using var stream = new System.IO.MemoryStream();
			using (var bitStream = new IO.BitStream(stream, System.IO.FileAccess.Write))
			{
				bitStream.StreamMode = System.IO.FileAccess.Write;
				bitStream.Write(value, storage, maxLength);
			}

			return stream.ToArray();
		}
		static void AssertBitStreamWriteBytes(byte[] expected, string value, MS.StringStorage storage, int maxLength = -1)
		{
			CollectionAssert.AreEqual(expected, WriteWithBitStream(value, storage, maxLength));
		}

		[TestMethod]
		public void StringStorage_InvalidFixedLengthArguments_ThrowExpectedExceptions()
		{
			AssertThrowsArgument("type",
				() => _ = new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageType.Pascal));
			AssertThrowsArgumentOutOfRange("fixedLength",
				() => _ = new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageType.CString, -1));
			AssertThrowsArgument("fixedLength",
				() => _ = new MS.StringStorage(MS.StringStorageWidthType.UTF8, MS.StringStorageType.CString, 1));
			AssertThrowsArgumentOutOfRange("fixedLength",
				() => _ = new MS.StringStorageMarkupAttribute(MS.StringStorageWidthType.Ascii,
					MS.StringStorageType.CString, -1));
		}

		[TestMethod]
		public void StringStorageEncoding_NullReadStreams_ThrowArgumentNullException()
		{
			var encoding = StringStorageEncoding.TryAndGetStaticEncoding(MS.StringStorage.CStringAscii);

			AssertThrowsArgumentNull("s", () => _ = encoding.ReadString((IO.EndianReader)null!, 0));
			AssertThrowsArgumentNull("s", () => _ = encoding.ReadString((IO.BitStream)null!, 0));
		}

		[TestMethod]
		public void StringStorageEncoding_Int7PascalAsciiRoundTripsAtBufferStartAndOffset()
		{
			var storage = new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageLengthPrefix.Int7);
			var encoding = StringStorageEncoding.TryAndGetStaticEncoding(storage);
			const string text = "hello";

			byte[] bytes = encoding.GetBytes(text);

			Assert.AreEqual(text.Length + 1, bytes.Length);
			Assert.AreEqual((byte)text.Length, bytes[0]);
			Assert.AreEqual(text, encoding.GetString(bytes));

			var wrappedBytes = new byte[bytes.Length + 1];
			wrappedBytes[0] = 0xEE;
			Array.Copy(bytes, 0, wrappedBytes, 1, bytes.Length);
			Assert.AreEqual(text, encoding.GetString(wrappedBytes, 1, bytes.Length));
		}

		[TestMethod]
		public void StringStorageEncoding_BitStreamWritesExactStorageBytes()
		{
			AssertBitStreamWriteBytes(
				[0x41, 0x42, 0x43],
				"ABC",
				new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageType.CharArray));
			AssertBitStreamWriteBytes(
				[0xC3, 0xA9, 0x00],
				"\u00E9",
				MS.StringStorage.CStringUtf8);
			AssertBitStreamWriteBytes(
				[0xC3, 0xA9, 0x00],
				"\u00E9X",
				MS.StringStorage.CStringUtf8,
				maxLength: 1);
			AssertBitStreamWriteBytes(
				[0x41, 0x00, 0x00, 0x00],
				"A",
				MS.StringStorage.CStringUnicode);
			AssertBitStreamWriteBytes(
				[0x00, 0x41, 0x00, 0x00],
				"A",
				MS.StringStorage.CStringUnicodeBigEndian);

			const string pascalText = "A\u00E9";
			AssertBitStreamWriteBytes(
				[0x02, 0x41, 0xC3, 0xA9],
				pascalText,
				new MS.StringStorage(MS.StringStorageWidthType.UTF8, MS.StringStorageLengthPrefix.Int7));
			AssertBitStreamWriteBytes(
				[0x02, 0x41, 0xC3, 0xA9],
				pascalText,
				new MS.StringStorage(MS.StringStorageWidthType.UTF8, MS.StringStorageLengthPrefix.Int8));
			AssertBitStreamWriteBytes(
				[0x00, 0x02, 0x41, 0xC3, 0xA9],
				pascalText,
				new MS.StringStorage(MS.StringStorageWidthType.UTF8, MS.StringStorageLengthPrefix.Int16));
			AssertBitStreamWriteBytes(
				[0x00, 0x00, 0x00, 0x02, 0x41, 0xC3, 0xA9],
				pascalText,
				new MS.StringStorage(MS.StringStorageWidthType.UTF8, MS.StringStorageLengthPrefix.Int32));

			var fixedCString = new MS.StringStorage(MS.StringStorageWidthType.Ascii,
				MS.StringStorageType.CString, fixedLength: 5);
			AssertBitStreamWriteBytes([0x41, 0x00, 0x00, 0x00, 0x00], "A", fixedCString);
			AssertBitStreamWriteBytes([0x41, 0x42, 0x43, 0x44, 0x00], "ABCDE", fixedCString);
			AssertBitStreamWriteBytes([0x41, 0x42, 0x00, 0x00, 0x00], "ABCDE", fixedCString, maxLength: 2);
			AssertBitStreamWriteBytes(
				[0x41, 0x42, 0x00, 0x00],
				"AB",
				new MS.StringStorage(MS.StringStorageWidthType.Ascii, MS.StringStorageType.CharArray, fixedLength: 4));
		}

		[TestMethod]
		public void StringStorageEncodingWriteTest()
		{
			const bool k_output_ms = true;

			using (var ms = new System.IO.MemoryStream())
			using (var io = new IO.EndianStream(ms))
			{
				MS.StringStorage storage =
					//Strings.StringStorage.kCStringUnicode;
					new(MS.StringStorageWidthType.Ascii, MS.StringStorageType.CString, 256);
				var encoding = StringStorageEncoding.TryAndGetStaticEncoding(storage);

				const string test1 = "This is a test",
					test2 = "Test this is",
					test3 = "wtf is apple juice?",
					test4 = "one more for good luck.";

				// Test case for strings which are greater-than-or-equal to the CString storage length (which includes a null terminating char)
				var sb = new System.Text.StringBuilder(storage.FixedLength);
				sb.Append('1', storage.FixedLength);
				string test5 = sb.ToString();

				io.Writer.Write(test1.AsSpan(), encoding);
				io.Writer.Write(test2.AsSpan(), encoding);
				io.Writer.Write(test3.AsSpan(), encoding);
				io.Writer.Write(test4.AsSpan(), encoding);
				io.Writer.Write(test5.AsSpan(), encoding);

				if (k_output_ms)
				{
					string output_ms_path = System.IO.Path.Combine(TestContext!.TestRunResultsDirectory!, "StringStorageEncodingTestWrite.bin");
					Console.WriteLine("Writing to: {0}", output_ms_path);

					using (var fs = new System.IO.FileStream(output_ms_path,
						System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
					{
						ms.WriteTo(fs);
					}
					TestContext.AddResultFile(output_ms_path);
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
