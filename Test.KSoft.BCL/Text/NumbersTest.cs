using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	[TestClass]
	public partial class NumbersTest : BaseTestClass
	{
		const string kInt32ListString = "-516,517,519,520,521,522,523,-1258";
		static readonly string kInt32ListStringWithSpaces = kInt32ListString.Replace(",", ", ");
		static readonly int[] kInt32List = {
			-516,517,519,520,521,522,523,-1258,
		};

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
		public void Text_NumbersStringListParseTest()
		{
			var desc = Numbers.StringListDesc.Default;

			var results = Numbers.TryParseInt32(desc, kInt32ListString);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32(desc, kInt32ListStringWithSpaces);
			VerifyTryParseInt32List(results);
		}
		[TestMethod]
		public void Text_NumbersToStringListTest()
		{
			var desc = Numbers.StringListDesc.Default;

			var result = Numbers.ToStringList(desc, kInt32List);
			Assert.AreEqual(kInt32ListString, result);
		}

		[TestMethod]
		public void Text_NumbersStringListParseWithTerminatorTest()
		{
			const string k_garbage_chars = "fsdfsdf";
			var desc = Numbers.StringListDesc.Default;
			desc.RequiresTerminator = true;

			var results = Numbers.TryParseInt32(desc, kInt32ListString + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);

			results = Numbers.TryParseInt32(desc, kInt32ListStringWithSpaces + desc.Terminator + k_garbage_chars);
			VerifyTryParseInt32List(results);
		}
		[TestMethod]
		public void Text_NumbersToStringListWithTerminatorTest()
		{
			var desc = Numbers.StringListDesc.Default;
			desc.RequiresTerminator = true;

			var result = Numbers.ToStringList(desc, kInt32List, e => true);
			Assert.AreEqual(kInt32ListString + desc.Terminator, result);
		}
		#endregion
	};
}