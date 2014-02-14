using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	partial class UtilitiesTest
	{
		public static class Strings
		{
			// 1337BEEF
			public static readonly byte[] kDataBytes = {
				0x13, 0x37, 0xBE, 0xEF,
			};
			public const string kDataString = "1337BEEF";

			public const string kDataStringLong = "The quick brown fox jumped over the fucking lazy ass bitch";
			public static readonly string kDataStringAsAlignedByteString =
				"54686520717569636B2062726F776E20" + System.Environment.NewLine +
				"666F78206A756D706564206F76657220" + System.Environment.NewLine +
				"746865206675636B696E67206C617A79" + System.Environment.NewLine +
				"20617373206269746368" + System.Environment.NewLine
			;
		};

		[TestMethod]
		public void Text_ByteArraysUtilTest()
		{
			// Test case: Entire byte array
			var test_value = Util.ByteArrayToString(Strings.kDataBytes);
			Assert.AreEqual(Strings.kDataString, test_value);
			// Test case: Partial byte array (explicit start)
			test_value = Util.ByteArrayToString(Strings.kDataBytes, 1);
			Assert.AreEqual(Strings.kDataString.Substring(2), test_value);
			// Test case: Partial byte array (explicit range)
			test_value = Util.ByteArrayToString(Strings.kDataBytes, 1, 2);
			Assert.AreEqual(Strings.kDataString.Substring(2, 4), test_value);
			// TODO: Test ByteArrayToStream


			byte[] test_data = System.Text.Encoding.ASCII.GetBytes(Strings.kDataStringLong);
			if (true) // Test the byte converter for strings of hex digits
			{
				test_value = Util.ByteArrayToString(test_data);
				test_data = Util.ByteStringToArray(test_value);
			}
			test_value = Util.ByteArrayToAlignedString(test_data);
			Assert.AreEqual(Strings.kDataStringAsAlignedByteString, test_value);
			// TODO: Test ByteArrayToAlignedOutput
		}


		[TestMethod]
		public void Text_ToAcceptableNumberBaseTest()
		{
			const NumeralBase k_invalid = 0;
			const NumeralBase k_nonstandard_36 = (NumeralBase)36;

			Assert.AreEqual(k_invalid, Util.ToAcceptableNumberBase(-2));
			Assert.AreEqual(k_invalid, Util.ToAcceptableNumberBase(0));
			Assert.AreEqual(k_invalid, Util.ToAcceptableNumberBase(64));

			Assert.AreEqual(k_nonstandard_36, Util.ToAcceptableNumberBase(36));
			Assert.AreEqual(NumeralBase.Hex, Util.ToAcceptableNumberBase(16));
		}

		#region Char Is/To Digit Tests
		static void TestCharDigitsAlpha(
			Func<char, int> to_digit, Func<char, bool> is_digit,
			char char_start, char char_end,
			int num_base)
		{
			for (int range = char_end - char_start, x = 0; x < range; x++)
			{
				int digit = num_base + x;
				char c = (char)(char_start + x);
				Assert.AreEqual(digit, to_digit(c));
				Assert.IsTrue(is_digit(c));
			}
		}
		static void TestCharDigits(
			Func<char, int> to_digit, Func<char, bool> is_digit,
			char nums_start, char nums_end,
			char uc_start, char uc_end,
			char lc_start, char lc_end,
			bool is_extended)
		{
			int num = 0; // will act as the 'base' value when we test upper/lower chars

			for (int range = nums_end - nums_start; num < range; num++)
			{
				char c = (char)(nums_start + num);
				Assert.AreEqual(num, to_digit(c));
				Assert.IsTrue(is_digit(c));
			}

			num++;
			TestCharDigitsAlpha(to_digit, is_digit, uc_start, uc_end, num);
			if (is_extended)
				num += (int)(uc_end-uc_start) + 1;
			TestCharDigitsAlpha(to_digit, is_digit, lc_start, lc_end, num);
		}

		[TestMethod]
		public void Text_CharAnyDigitTest()
		{
			const char k_numbers_start = '0';
			const char k_numbers_end = '9';

			const char k_upper_chars_start = 'A';
			const char k_upper_chars_end = 'Z';

			const char k_lower_chars_start = 'a';
			const char k_lower_chars_end = 'z';

			TestCharDigits(Util.CharToAnyDigit, Util.CharIsAnyDigit,
				k_numbers_start, k_numbers_end,
				k_upper_chars_start, k_upper_chars_end,
				k_lower_chars_start, k_lower_chars_end,
				false);
		}

		[TestMethod]
		public void Text_CharAnyDigitExtendedTest()
		{
			const char k_numbers_start = '0';
			const char k_numbers_end = '9';

			const char k_upper_chars_start = 'A';
			const char k_upper_chars_end = 'Z';

			const char k_lower_chars_start = 'a';
			const char k_lower_chars_end = 'z';

			TestCharDigits(Util.CharToAnyDigitExtended, Util.CharIsAnyDigitExtended,
				k_numbers_start, k_numbers_end,
				k_upper_chars_start, k_upper_chars_end,
				k_lower_chars_start, k_lower_chars_end,
				true);
		}

		[TestMethod]
		public void Text_CharDigitTest()
		{
			const char k_numbers_start = '0';
			const char k_numbers_end = '9';

			const char k_upper_chars_start = 'A';
			const char k_upper_chars_end = 'F';

			const char k_lower_chars_start = 'a';
			const char k_lower_chars_end = 'f';

			TestCharDigits(Util.CharToDigit, Util.CharIsDigit,
				k_numbers_start, k_numbers_end,
				k_upper_chars_start, k_upper_chars_end,
				k_lower_chars_start, k_lower_chars_end,
				false);
		}
		#endregion

		[TestMethod]
		public void Text_CharToIntTest()
		{
			Assert.AreEqual(1, Util.CharToInt('1', NumeralBase.Binary, 0));
			Assert.AreEqual(2, Util.CharToInt('1', NumeralBase.Binary, 1));
			Assert.AreEqual(4, Util.CharToInt('1', NumeralBase.Binary, 2));
			Assert.AreEqual(0, Util.CharToInt('0', NumeralBase.Binary, 3));

			Assert.AreEqual(7,   Util.CharToInt('7', NumeralBase.Octal, 0));
			Assert.AreEqual(56,  Util.CharToInt('7', NumeralBase.Octal, 1));
			Assert.AreEqual(448, Util.CharToInt('7', NumeralBase.Octal, 2));
			Assert.AreEqual(0,   Util.CharToInt('0', NumeralBase.Octal, 3));

			Assert.AreEqual(9,   Util.CharToInt('9', NumeralBase.Decimal, 0));
			Assert.AreEqual(90,  Util.CharToInt('9', NumeralBase.Decimal, 1));
			Assert.AreEqual(900, Util.CharToInt('9', NumeralBase.Decimal, 2));
			Assert.AreEqual(0,   Util.CharToInt('0', NumeralBase.Decimal, 3));

			Assert.AreEqual(15,   Util.CharToInt('F', NumeralBase.Hex, 0));
			Assert.AreEqual(240,  Util.CharToInt('F', NumeralBase.Hex, 1));
			Assert.AreEqual(3840, Util.CharToInt('F', NumeralBase.Hex, 2));
			Assert.AreEqual(0,    Util.CharToInt('0', NumeralBase.Hex, 3));

			var from_base = Util.ToAcceptableNumberBase(17);
			Assert.AreEqual(16,  Util.CharToInt('G', from_base, 0));
			Assert.AreEqual(272, Util.CharToInt('G', from_base, 1));
			Assert.AreEqual(4624,Util.CharToInt('G', from_base, 2));
			Assert.AreEqual(0,   Util.CharToInt('0', from_base, 3));
		}

		[TestMethod]
		public void Text_CharsToByteTest()
		{
			const int k_expected_0 = 51; // result expected when using chars starting at index 0
			const int k_expected_1 = 63; // result expected when using chars starting at index 1
			char[] chars = { '3', '3', 'F' }; // extra '3' prepended for testing offset params

			Assert.AreEqual(k_expected_0, Util.CharsToByte(NumeralBase.Hex, chars[0], chars[1]));
			Assert.AreEqual(k_expected_1, Util.CharsToByte(NumeralBase.Hex, chars[1], chars[2]));

			Assert.AreEqual(k_expected_0, Util.CharsToByte(NumeralBase.Hex, chars));
			Assert.AreEqual(k_expected_1, Util.CharsToByte(NumeralBase.Hex, chars, 1));

			string str = new string(chars);
			Assert.AreEqual(k_expected_0, Util.CharsToByte(NumeralBase.Hex, str));
			Assert.AreEqual(k_expected_1, Util.CharsToByte(NumeralBase.Hex, str, 1));
		}
	};
}