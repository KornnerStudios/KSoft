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
			{
				Console.WriteLine(encoded);
			}

			byte[]? decoded = encoding.Decode(encoded);
			if (writeToConsole)
			{
				Console.WriteLine(Util.ByteArrayToString(decoded!));
			}

			return bytes.EqualsArray(decoded!);
		}

		[TestMethod]
		public void ConstructorsAndMethods_NullInputs_ThrowArgumentNullException()
		{
			var encoding = new RadixEncoding("01");

			AssertThrowsArgumentNull("digits", () => _ = new RadixEncoding(null!));
			AssertThrowsArgumentNull("radixChars", () => _ = encoding.Decode(null!));
		}

		[TestMethod]
		public void Encode_KnownInputs_PreservesPersistedMapping()
		{
			const string k_hex_digits = "0123456789abcdef";
			var little = new RadixEncoding(k_hex_digits, Shell.EndianFormat.Little, false);
			var big = new RadixEncoding(k_hex_digits, Shell.EndianFormat.Big, false);
			var little_with_zeros = new RadixEncoding(k_hex_digits, Shell.EndianFormat.Little, true);
			var big_with_zeros = new RadixEncoding(k_hex_digits, Shell.EndianFormat.Big, true);

			Assert.AreEqual(string.Empty, little.Encode(Array.Empty<byte>()));
			Assert.AreEqual("3412", little.Encode([0x12, 0x34]));
			Assert.AreEqual("2143", big.Encode([0x12, 0x34]));
			byte[] high_bit_input = [0x01, 0x80];
			byte[] high_bit_original = (byte[])high_bit_input.Clone();
			Assert.AreEqual("8001", little.Encode(high_bit_input));
			Assert.AreEqual("1008", big.Encode([0x01, 0x80]));
			CollectionAssert.AreEqual(high_bit_original, high_bit_input);
			Assert.AreEqual("12", little.Encode([0x12, 0x00]));
			Assert.AreEqual("21", big.Encode([0x12, 0x00]));
			Assert.AreEqual("0012", little_with_zeros.Encode([0x12, 0x00]));
			Assert.AreEqual("2100", big_with_zeros.Encode([0x12, 0x00]));

			byte[] input = [0xAA, 0x01, 0x80, 0xBB];
			byte[] original = (byte[])input.Clone();
			Assert.AreEqual("8001", little.Encode([0x01, 0x80]));
			Assert.AreEqual("8001", little.Encode(input.AsSpan(1, 2)));
			CollectionAssert.AreEqual(original, input);
		}

		[TestMethod]
		public void EncodeDecode_KnownByteSequences_RoundTrip()
		{
			const string k_base36_digits = "abcdefghijklmnopqrstuvwxyz0123456789";
			var base36 = new RadixEncoding(k_base36_digits, Shell.EndianFormat.Little, true);
			var base36_no_zeros = new RadixEncoding(k_base36_digits, Shell.EndianFormat.Little, false);

			byte[] ends_with_zero_neg = [0xFF, 0xFF, 0x00, 0x00];
			byte[] ends_with_zero_pos = [0xFF, 0x7F, 0x00, 0x00];
			byte[] text = System.Text.Encoding.ASCII.GetBytes("A test 1234");

			Assert.IsTrue(Validate(base36, ends_with_zero_neg));
			Assert.IsTrue(Validate(base36, ends_with_zero_pos));
			Assert.IsTrue(Validate(base36_no_zeros, text));

			const string k_base32_digits = "abcdefghijklmnopqrstuvwxyz012345";
			var base32 = new RadixEncoding(k_base32_digits, Shell.EndianFormat.Big, true);

			byte[] bytes = [ // fidm52dkvy545555i2ugzvmbd2kczbayaaaaaaaa
				0x05, 0x0d, 0xf6, 0xf9, 0x50, 0x15, 0x7f, 0xff, 0xff, 0xff, 0x88, 0x53, 0x93, 0x2b, 0x0b, 0x83, 0x2b, 0x91, 0x03, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00,
			];
			Assert.AreEqual("fidm52dkvy545555i2ugzvmbd2kczbayaaaaaaaa", base32.Encode(bytes));
			Assert.IsTrue(Validate(base32, bytes));

			// http://stackoverflow.com/questions/14110010/base-n-encoding-of-a-byte-array?noredirect=1#comment25188602_14110010
			var base32_no_zeros = new RadixEncoding(k_base32_digits, Shell.EndianFormat.Big, false);
			byte[] test = [
				0x12, 0xE7, 0x22, 0x39, 0x3A, 0x40, 0x60, 0x31, 0xC4, 0x15, 0x32, 0xA1, 0xCF, 0xCA, 0xF4, 0x77, 0xA0, 0x34, 0x21, 0xC9, 0xC9, 0xA4, 0x18, 0x6A, 0x54, 0x2C, 0x47, 0x5E, 0x0F, 0xD1, 0xEB, 0xB1 /*- 0x80*/
			];
			Assert.IsTrue(Validate(base32, test, true));
			Assert.IsTrue(Validate(base32_no_zeros, test, true));
			Assert.IsTrue(Validate(base36, test, true));
			Assert.IsTrue(Validate(base36_no_zeros, test, true));
		}
	};
}
