using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class JenkinsHashTest : BaseTestClass
	{
		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgumentException(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void Cryptography_JenkinsHashTest()
		{
			const string k_input = "SET_TIME_ONE_DAY_FORWARD";
			const uint k_expected_output = /*0x2E5B068F*/0xBB56C2E5;

			uint ouptut = Security.Cryptography.JenkinsHash.Hash(k_input);
			Assert.IsTrue(k_expected_output == ouptut);
		}

		[TestMethod]
		public void Cryptography_JenkinsHashLookup2Test()
		{
			const string k_input = "Four score and seven years ago";

			Assert.AreEqual(0x50F2424Bu, JenkinsHashLookup2.Hash(k_input));
			Assert.AreEqual(0x89DEAE7Eu, JenkinsHashLookup2.Hash(k_input, 1));
		}

		void TestLookup3(string input, uint expected_output, uint seed = 0)
		{
			uint output = JenkinsHashLookup3.Hash(input, seed);
			Assert.AreEqual(expected_output, output);
		}
		[TestMethod]
		public void Cryptography_JenkinsHashLookup3Test()
		{
			string[] k_inputs = [
				"",
				"Four score and seven years ago",
				"Four score and seven years ago", // seed=1
			];
			uint[] k_expected_outputs = [
				0xDEADBEEF,
				0x17770551,
				0xCD628161, // seed=1
			];

			TestLookup3(k_inputs[0], k_expected_outputs[0]);
			TestLookup3(k_inputs[1], k_expected_outputs[1]);
			TestLookup3(k_inputs[2], k_expected_outputs[2], 1);
		}

		[TestMethod]
		public void Hash_AsciiByteCharAndStringInputs_AreEquivalent()
		{
			const string k_text = "ABCDEFGHIJKLMN";
			byte[] bytes = [65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78];
			char[] chars = k_text.ToCharArray();

			Assert.AreEqual(0xE625C1FCu, JenkinsHash.Hash(bytes));
			Assert.AreEqual(JenkinsHash.Hash(bytes), JenkinsHash.Hash(chars));
			Assert.AreEqual(JenkinsHash.Hash(bytes), JenkinsHash.Hash(k_text));

			Assert.AreEqual(0xBDE38428u, JenkinsHashLookup2.Hash(bytes));
			Assert.AreEqual(JenkinsHashLookup2.Hash(bytes), JenkinsHashLookup2.Hash(chars));
			Assert.AreEqual(JenkinsHashLookup2.Hash(bytes), JenkinsHashLookup2.Hash(k_text));

			Assert.AreEqual(0x2AAB409Bu, JenkinsHashLookup3.Hash(bytes));
			Assert.AreEqual(JenkinsHashLookup3.Hash(bytes), JenkinsHashLookup3.Hash(chars));
			Assert.AreEqual(JenkinsHashLookup3.Hash(bytes), JenkinsHashLookup3.Hash(k_text));
		}

		[TestMethod]
		public void Hash_BoundaryLengths_PreserveFinalBlockPacking()
		{
			const string k_text = "ABCDEFGHIJKLMN";
			byte[] bytes = [65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78];
			char[] chars = k_text.ToCharArray();
			uint[] jenkinsExpected = [0x00000002, 0xCA2E9442, 0x45E61E58, 0xED131F5B, 0xCD8B6206, 0xB98559FC, 0x0161526F, 0x4AC70178, 0x44D2D3E1, 0xC8B4CA7D, 0x7031289D, 0x37A218BA, 0x605B0340, 0x6D99F6DC];
			uint[] lookup2Expected = [0xBD49D10D, 0xA1614B4D, 0xB0A9479A, 0x3E36495D, 0x1C31FD95, 0xFD4870C0, 0xE019DD73, 0x2BB00418, 0x77FC8C38, 0x92952498, 0xEBF300C1, 0x98AB491F, 0xEA9C4F7A, 0xC9A8AD4D];
			uint[] lookup3Expected = [0xDEADBEEF, 0x01014BA1, 0xA9793993, 0x3F4B48AF, 0x470042B2, 0x6D82E3FE, 0x87706061, 0xA6841E06, 0xBF2B6C4E, 0x1F3B3DF9, 0x8BF94160, 0x5E41949E, 0x6E05006B, 0xEF91ECEC];

			for (int length = 0; length <= 13; length++)
			{
				Assert.AreEqual(jenkinsExpected[length], JenkinsHash.Hash(bytes, length: length));
				Assert.AreEqual(jenkinsExpected[length], JenkinsHash.Hash(chars, length: length));
				if (length > 0)
				{
					Assert.AreEqual(jenkinsExpected[length], JenkinsHash.Hash(k_text.Substring(0, length)));
				}

				Assert.AreEqual(lookup2Expected[length], JenkinsHashLookup2.Hash(bytes, length: length));
				Assert.AreEqual(lookup2Expected[length], JenkinsHashLookup2.Hash(chars, length: length));
				Assert.AreEqual(lookup2Expected[length], JenkinsHashLookup2.Hash(k_text.Substring(0, length)));

				Assert.AreEqual(lookup3Expected[length], JenkinsHashLookup3.Hash(bytes, length: length));
				Assert.AreEqual(lookup3Expected[length], JenkinsHashLookup3.Hash(chars, length: length));
				Assert.AreEqual(lookup3Expected[length], JenkinsHashLookup3.Hash(k_text.Substring(0, length)));
			}
		}

		[TestMethod]
		public void Hash_OffsetAndDefaultLength_PreserveLegacyBehavior()
		{
			byte[] bytes = [65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84];

			Assert.AreEqual(0xAB919056u, JenkinsHash.Hash(bytes, index: 2, length: 13));
			Assert.AreEqual(0x023EB72Eu, JenkinsHash.Hash(bytes, index: 2));

			Assert.AreEqual(0x8D8DE35Fu, JenkinsHashLookup2.Hash(bytes, index: 2, length: 13));
			Assert.AreEqual(0x8721504Cu, JenkinsHashLookup2.Hash(bytes, index: 2));

			Assert.AreEqual(0xDD22D384u, JenkinsHashLookup3.Hash(bytes, index: 2, length: 13));
			Assert.AreEqual(0x3A687C77u, JenkinsHashLookup3.Hash(bytes, index: 2));
		}

		[TestMethod]
		public void JenkinsHash_NonAsciiAndNormalizedPathCharacters_PreserveCharacterSemantics()
		{
			char[] nonAsciiChars = [(char)0x00C4, (char)0x03A9, '\\'];
			const string k_normalizedPath = "a/b/c";

			Assert.AreEqual(0x3CC57CC2u, JenkinsHash.Hash(nonAsciiChars));
			Assert.AreEqual(JenkinsHash.Hash(nonAsciiChars), JenkinsHash.Hash(new string(nonAsciiChars)));
			Assert.AreEqual(0x729D786Du, JenkinsHash.Hash("A\\B/C"));
			Assert.AreEqual(JenkinsHash.Hash(k_normalizedPath), JenkinsHash.Hash("A\\B/C"));
		}

		[TestMethod]
		public void LookupHashes_NonAsciiCharacters_PreserveCodeUnitPacking()
		{
			char[] nonAsciiChars = [(char)0x00C4, (char)0x03A9, '\\'];

			Assert.AreEqual(0xA8F395ABu, JenkinsHashLookup2.Hash(nonAsciiChars));
			Assert.AreEqual(JenkinsHashLookup2.Hash(nonAsciiChars), JenkinsHashLookup2.Hash(new string(nonAsciiChars)));

			Assert.AreEqual(0x157CF8B2u, JenkinsHashLookup3.Hash(nonAsciiChars));
			Assert.AreEqual(JenkinsHashLookup3.Hash(nonAsciiChars), JenkinsHashLookup3.Hash(new string(nonAsciiChars)));
		}

		[TestMethod]
		public void Hash_PublicSpanInputs_HashTheProvidedSlices()
		{
			const string k_text = "ABCDEFGHIJKLMN";
			byte[] bytes = Encoding.ASCII.GetBytes($".{k_text}.");
			char[] chars = ".ABCDEFGHIJKLMN.".ToCharArray();
			ReadOnlySpan<byte> byteSlice = bytes.AsSpan(1, k_text.Length);
			ReadOnlySpan<char> charSlice = chars.AsSpan(1, k_text.Length);
			ReadOnlySpan<char> stringSpan = k_text.AsSpan();

			Assert.AreEqual(0xE625C1FCu, JenkinsHash.Hash(byteSlice));
			Assert.AreEqual(JenkinsHash.Hash(byteSlice), JenkinsHash.Hash(charSlice));
			Assert.AreEqual(JenkinsHash.Hash(charSlice), JenkinsHash.Hash(stringSpan));

			Assert.AreEqual(0xBDE38428u, JenkinsHashLookup2.Hash(byteSlice));
			Assert.AreEqual(JenkinsHashLookup2.Hash(byteSlice), JenkinsHashLookup2.Hash(charSlice));
			Assert.AreEqual(JenkinsHashLookup2.Hash(charSlice), JenkinsHashLookup2.Hash(stringSpan));

			Assert.AreEqual(0x2AAB409Bu, JenkinsHashLookup3.Hash(byteSlice));
			Assert.AreEqual(JenkinsHashLookup3.Hash(byteSlice), JenkinsHashLookup3.Hash(charSlice));
			Assert.AreEqual(JenkinsHashLookup3.Hash(charSlice), JenkinsHashLookup3.Hash(stringSpan));
		}

		[TestMethod]
		public void LookupHashes_PublicSpanInputs_PreserveSeeds()
		{
			const string k_text = "Four score and seven years ago";
			byte[] bytes = Encoding.ASCII.GetBytes(k_text);
			ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
			ReadOnlySpan<char> charSpan = k_text.AsSpan();

			Assert.AreEqual(0x50F2424Bu, JenkinsHashLookup2.Hash(byteSpan));
			Assert.AreEqual(JenkinsHashLookup2.Hash(byteSpan), JenkinsHashLookup2.Hash(charSpan));
			Assert.AreEqual(0x89DEAE7Eu, JenkinsHashLookup2.Hash(byteSpan, 1));
			Assert.AreEqual(JenkinsHashLookup2.Hash(byteSpan, 1), JenkinsHashLookup2.Hash(charSpan, 1));

			Assert.AreEqual(0x17770551u, JenkinsHashLookup3.Hash(byteSpan));
			Assert.AreEqual(JenkinsHashLookup3.Hash(byteSpan), JenkinsHashLookup3.Hash(charSpan));
			Assert.AreEqual(0xCD628161u, JenkinsHashLookup3.Hash(byteSpan, 1));
			Assert.AreEqual(JenkinsHashLookup3.Hash(byteSpan, 1), JenkinsHashLookup3.Hash(charSpan, 1));
		}
		[TestMethod]
		public void Hash_InvalidBuffers_ThrowArgumentException()
		{
			AssertThrowsArgumentNull("buffer", () => JenkinsHash.Hash((byte[])null!));
			AssertThrowsArgumentNull("buffer", () => JenkinsHash.Hash((char[])null!));
			AssertThrowsArgumentException("buffer", () => JenkinsHash.Hash(""));
		}

		[TestMethod]
		public void LookupHashes_NullBuffers_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup2.Hash((byte[])null!));
			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup2.Hash((char[])null!));
			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup2.Hash((string)null!));

			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup3.Hash((byte[])null!));
			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup3.Hash((char[])null!));
			AssertThrowsArgumentNull("buffer", () => JenkinsHashLookup3.Hash((string)null!));
		}
	};
}