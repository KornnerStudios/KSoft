using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class TigerHashTest : BaseTestClass
	{
		static void AssertThrowsArgumentNull(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		static void AssertThrowsArgument(string parameterName, Action action)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(parameterName, exception.ParamName);
		}

		[TestMethod]
		public void Cryptography_TigerHashVersionOneTest()
		{
			TestTiger(TigerHash.kAlgorithmName,
				"",
				"3293ac630c13f0245f92bbb1766e16167a4e58492dde73f3");
			TestTiger(TigerHash.kAlgorithmName,
				"The quick brown fox jumps over the lazy dog",
				"6d12a41e72e644f017b6f0e2f7b44c6285f06dd5d2c5b075");
			TestTiger(TigerHash.kAlgorithmName,
				"The quick brown fox jumps over the lazy cog",
				"a8f04b0f7201a0d728101c9d26525b31764a3493fcd8458f");
		}

		[TestMethod]
		public void Cryptography_TigerHashVersionTwoTest()
		{
			TestTiger(TigerHash2.kAlgorithmName,
				"",
				"4441be75f6018773c206c22745374b924aa8313fef919f41");
			TestTiger(TigerHash2.kAlgorithmName,
				"The quick brown fox jumps over the lazy dog",
				"976abff8062a2e9dcea3a1ace966ed9c19cb85558b4976d8");
			TestTiger(TigerHash2.kAlgorithmName,
				"The quick brown fox jumps over the lazy cog",
				"09c11330283a27efb51930aa7dc1ec624ff738a8d9bdd3df");
		}

		[TestMethod]
		public void Create_RegisteredAlgorithms_ReturnsExpectedTigerHashType()
		{
			using TigerHash tiger = TigerHash.Create();
			using TigerHash2 tiger2 = TigerHash2.Create();
			using TigerHashBase tigerBase = TigerHashBase.Create(TigerHash.kAlgorithmName);
			using TigerHashBase tiger2Base = TigerHashBase.Create(TigerHash2.kAlgorithmName);

			Assert.IsInstanceOfType<TigerHash>(tiger);
			Assert.IsInstanceOfType<TigerHash2>(tiger2);
			Assert.IsInstanceOfType<TigerHash>(tigerBase);
			Assert.IsInstanceOfType<TigerHash2>(tiger2Base);
		}

		[TestMethod]
		public void Create_InvalidAlgorithmName_ThrowsArgumentException()
		{
			AssertThrowsArgumentNull("algName", () => TigerHashBase.Create(null!));
			AssertThrowsArgument("algName", () => TigerHashBase.Create("not-a-tiger-hash"));
			AssertThrowsArgument("algName", () => TigerHash.Create(TigerHash2.kAlgorithmName));
			AssertThrowsArgument("algName", () => TigerHash2.Create(TigerHash.kAlgorithmName));
		}

		static void TestTiger(string algName,
			string inputString,
			string expectedHashByteString)
		{
			using (var tiger = TigerHashBase.Create(algName))
			{
				var input = System.Text.Encoding.ASCII.GetBytes(inputString);
				var hash = tiger.ComputeHash(input);
				Assert.AreEqual(
					expectedHashByteString.ToUpper(KSoft.Util.InvariantCultureInfo),
					KSoft.Text.Util.ByteArrayToString(hash));
			}
		}
	};
}
