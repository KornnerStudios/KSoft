using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class TigerHashTest : BaseTestClass
	{

		[TestMethod]
		public void Tiger1_KnownVectors_ReturnExpectedHashes()
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
		public void Tiger2_KnownVectors_ReturnExpectedHashes()
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

		[TestMethod]
		public void TryGetAsTiger192_SpanSlice_WritesOnlyDestination()
		{
			using var tiger = new TigerHash();
			byte[] hash = tiger.ComputeHash(System.Text.Encoding.ASCII.GetBytes("span"));
			byte[] destination = new byte[hash.Length + 2];
			Array.Fill(destination, (byte)0xCC);

			Assert.IsFalse(tiger.TryGetAsTiger192(destination.AsSpan(1, hash.Length - 1)));
			Assert.IsTrue(tiger.TryGetAsTiger192(destination.AsSpan(1, hash.Length)));

			Assert.AreEqual((byte)0xCC, destination[0]);
			CollectionAssert.AreEqual(hash, destination[1..^1]);
			Assert.AreEqual((byte)0xCC, destination[^1]);
		}

		[TestMethod]
		public void Tiger1_ChunkedInput_MatchesOneShotHashAcrossBlockBoundaries()
		{
			var input = new byte[127];
			for (int x = 0; x < input.Length; x++)
			{
				input[x] = (byte)(x * 37);
			}

			using var expectedAlgorithm = new TigerHash();
			byte[] expected = expectedAlgorithm.ComputeHash(input);
			using var actualAlgorithm = new TigerHash();
			int offset = 0;
			const int chunkSize = 17;
			while (input.Length - offset > chunkSize)
			{
				actualAlgorithm.TransformBlock(input, offset, chunkSize, null, 0);
				offset += chunkSize;
			}
			actualAlgorithm.TransformFinalBlock(input, offset, input.Length - offset);

			CollectionAssert.AreEqual(expected, actualAlgorithm.Hash);
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
					Convert.ToHexString(hash));
			}
		}
	};
}
