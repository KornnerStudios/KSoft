using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	[TestClass]
	public partial class RadixEncodingTest : BaseTestClass
	{
		static bool Validate(RadixEncoding encoding, byte[] bytes, bool writeToConsole = false)
		{
			string encoded = encoding.Encode(bytes);
			if (writeToConsole)
				Console.WriteLine(encoded);

			byte[] decoded = encoding.Decode(encoded);
			if (writeToConsole)
				Console.WriteLine(Util.ByteArrayToString(decoded));

			return bytes.EqualsArray(decoded);
		}
		[TestMethod]
		public void Text_RadixEncodingTest()
		{
			const string k_base36_digits = "abcdefghijklmnopqrstuvwxyz0123456789";
			var base36 = new RadixEncoding(k_base36_digits, Shell.EndianFormat.Little, true);
			var base36_no_zeros = new RadixEncoding(k_base36_digits, Shell.EndianFormat.Little, false);

			byte[] ends_with_zero_neg = { 0xFF, 0xFF, 0x00, 0x00 };
			byte[] ends_with_zero_pos = { 0xFF, 0x7F, 0x00, 0x00 };
			byte[] text = System.Text.Encoding.ASCII.GetBytes("A test 1234");

			Assert.IsTrue(Validate(base36, ends_with_zero_neg));
			Assert.IsTrue(Validate(base36, ends_with_zero_pos));
			Assert.IsTrue(Validate(base36_no_zeros, text));

			const string k_base32_digits = "abcdefghijklmnopqrstuvwxyz012345";
			var base32 = new RadixEncoding(k_base32_digits, Shell.EndianFormat.Big, true);

			byte[] bytes = { // fidm52dkvy545555i2ugzvmbd2kczbayaaaaaaaa
				0x05, 0x0d, 0xf6, 0xf9, 0x50, 0x15, 0x7f, 0xff, 0xff, 0xff, 0x88, 0x53, 0x93, 0x2b, 0x0b, 0x83, 0x2b, 0x91, 0x03, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00,
			};
			Assert.IsTrue(Validate(base32, bytes));

			// http://stackoverflow.com/questions/14110010/base-n-encoding-of-a-byte-array?noredirect=1#comment25188602_14110010
			var base32_no_zeros = new RadixEncoding(k_base32_digits, Shell.EndianFormat.Big, false);
			byte[] test = new byte[] {
				0x12, 0xE7, 0x22, 0x39, 0x3A, 0x40, 0x60, 0x31, 0xC4, 0x15, 0x32, 0xA1, 0xCF, 0xCA, 0xF4, 0x77, 0xA0, 0x34, 0x21, 0xC9, 0xC9, 0xA4, 0x18, 0x6A, 0x54, 0x2C, 0x47, 0x5E, 0x0F, 0xD1, 0xEB, 0xB1 /*- 0x80*/
			};
			Assert.IsTrue(Validate(base32, test, true));
			Assert.IsTrue(Validate(base32_no_zeros, test, true));
			Assert.IsTrue(Validate(base36, test, true));
			Assert.IsTrue(Validate(base36_no_zeros, test, true));
		}
	};
}