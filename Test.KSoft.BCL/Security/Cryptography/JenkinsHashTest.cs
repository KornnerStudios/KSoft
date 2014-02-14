using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public class JenkinsHashTest : BaseTestClass
	{
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
			// TODO: find test vectors
		}

		void TestLookup3(string input, uint expected_output, uint seed = 0)
		{
			uint output = JenkinsHashLookup3.Hash(input, seed);
			Assert.AreEqual(expected_output, output);
		}
		[TestMethod]
		public void Cryptography_JenkinsHashLookup3Test()
		{
			string[] k_inputs = {
				"",
				"Four score and seven years ago",
				"Four score and seven years ago", // seed=1
			};
			uint[] k_expected_outputs = {
				0xDEADBEEF,
				0x17770551,
				0xCD628161, // seed=1
			};

			TestLookup3(k_inputs[0], k_expected_outputs[0]);
			TestLookup3(k_inputs[1], k_expected_outputs[1]);
			TestLookup3(k_inputs[2], k_expected_outputs[2], 1);
		}
	};
}