using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	[TestClass]
	public partial class NumbersTest : BaseTestClass
	{
		const string kInt32ListString = "-516,517,519,520,521,522,523,-1258";
		static readonly string kInt32ListStringWithSpaces = kInt32ListString.Replace(",", ", ", StringComparison.Ordinal);
		static readonly int[] kInt32List = [
			-516,517,519,520,521,522,523,-1258,
		];

		#region StringListDesc related
		void VerifyTryParseInt32List(System.Collections.Generic.IEnumerable<int?> results)
		{
			int index = 0;
			foreach (var result in results)
			{
				Assert.IsTrue(result.HasValue);

				var expected = kInt32List[index++];
				Assert.AreEqual(expected, result);
			}
		}

		[TestMethod]
		public void StringListDescInvalidArgumentsThrowExpectedExceptions()
		{
			AssertThrowsExactly<ArgumentNullException>("digits",
				() => _ = new Numbers.StringListDesc(',', digits: null!));
			AssertThrowsExactly<ArgumentException>("digits",
				() => _ = new Numbers.StringListDesc(',', digits: string.Empty));
			AssertThrowsExactly<ArgumentException>("digits",
				() => _ = new Numbers.StringListDesc(',', radix: NumbersRadix.Hex, digits: "0123456789ABCDE"));
			AssertThrowsArgumentOutOfRange("radix",
				() => _ = new Numbers.StringListDesc(',', radix: (NumbersRadix)1, digits: "01"));
		}

		[TestMethod]
		public void StringListParseTest()
		{
			var desc = Numbers.StringListDesc.Default;

			var results = Numbers.TryParseInt32(desc, kInt32ListString);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32(desc, kInt32ListStringWithSpaces);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32Async(desc, kInt32ListString);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32Async(desc, kInt32ListStringWithSpaces);
			VerifyTryParseInt32List(results);
		}
		[TestMethod]
		public void ToStringListTest()
		{
			var desc = Numbers.StringListDesc.Default;

			var result = Numbers.ToStringList(desc, kInt32List);
			Assert.AreEqual(kInt32ListString, result);
		}

		[TestMethod]
		public void StringListParseWithTerminatorTest()
		{
			const string k_garbage_chars = "fsdfsdf";
			var desc = Numbers.StringListDesc.Default;
			desc.RequiresTerminator = true;

			var results = Numbers.TryParseInt32(desc, kInt32ListString + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32(desc, kInt32ListStringWithSpaces + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32Async(desc, kInt32ListString + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32Async(desc, kInt32ListStringWithSpaces + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);
		}
		[TestMethod]
		public void StringListPredictedCountStopsAtTerminatorTest()
		{
			var desc = Numbers.StringListDesc.Default;

			Assert.AreEqual(1, desc.PredictedCount(string.Empty));
			Assert.AreEqual(3, desc.PredictedCount("1,2,3"));
			Assert.AreEqual(3, desc.PredictedCount("1,2,"));
			Assert.AreEqual(3, desc.PredictedCount(",1,2;ignored,more"));
			Assert.AreEqual(1, desc.PredictedCount(";1,2,3"));
		}
		[TestMethod]
		public void ToStringListWithTerminatorTest()
		{
			var desc = Numbers.StringListDesc.Default;
			desc.RequiresTerminator = true;

			var result = Numbers.ToStringList(desc, kInt32List, e => true);
			Assert.AreEqual(kInt32ListString + desc.Terminator, result);
		}

		[TestMethod]
		public void ScalarToStringAndTryParseTest()
		{
			Assert.AreEqual("377", Numbers.ToString(byte.MaxValue, NumeralBase.Octal));
			Assert.AreEqual("-101", Numbers.ToString((short)-5, NumeralBase.Binary));
			Assert.AreEqual("1Z", Numbers.ToString(71U, 36));
			Assert.Throws<ArgumentOutOfRangeException>(() => Numbers.ToString(-1, NumeralBase.Hex));

			Assert.IsTrue(Numbers.TryParse("0xFF", out uint uintValue, 16));
			Assert.AreEqual(255U, uintValue);
			Assert.IsFalse(Numbers.TryParse(" 0xFF", out uintValue, 16));
			Assert.IsTrue(Numbers.TryParse("-101", out int intValue, NumeralBase.Binary));
			Assert.AreEqual(-5, intValue);
			Assert.IsFalse(Numbers.TryParse("256", out byte byteValue, Numbers.kBase10));
			Assert.AreEqual((byte)0, byteValue);
			Assert.IsTrue(Numbers.TryParseRange("xx7Fyy", out byteValue, 2, 2, NumeralBase.Hex));
			Assert.AreEqual((byte)0x7F, byteValue);
		}

		[TestMethod]
		public void TryParseInvalidRangesThrowExpectedExceptions()
		{
			AssertThrowsArgumentOutOfRange("startIndex",
				() => Numbers.TryParse("123", out int _, Numbers.kBase10, -1));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => Numbers.TryParse("123", out int _, NumeralBase.Decimal, -1));
			AssertThrowsArgumentOutOfRange("startIndex",
				() => Numbers.TryParseRange("123", out int _, -1, 1));
			AssertThrowsArgumentOutOfRange("length",
				() => Numbers.TryParseRange("123", out int _, 0, -1));
		}

		[TestMethod]
		public void ParseStringPreservesNoThrowAndErrorBehaviorTest()
		{
			int value = 0;
			Assert.IsTrue(Numbers.ParseString("123", ref value, noThrow: true));
			Assert.AreEqual(123, value);

			Assert.IsFalse(Numbers.ParseString(null, ref value, noThrow: true));
			Assert.Throws<ArgumentOutOfRangeException>(
				() => Numbers.ParseString("123", ref value, noThrow: false, startIndex: 3));
		}

		[TestMethod]
		[Description("dotTrace profiling method")]
		public void StringListParseTestProfile()
		{
			var desc = Numbers.StringListDesc.Default;

			for(int x = 0; x < 100000; x++)
			{
				var results_async = Numbers.TryParseInt32Async(desc, kInt32ListString);
				var results_sync = Numbers.TryParseInt32(desc, kInt32ListString);

				VerifyTryParseInt32List(results_sync);
				VerifyTryParseInt32List(results_async);
			}
		}
		#endregion
	};
}
