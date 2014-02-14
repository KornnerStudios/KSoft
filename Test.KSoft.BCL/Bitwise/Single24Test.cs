using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class Single24Test : BaseTestClass
	{
		static uint[] kTestConversionsList = {
			Single24.kMinInt,  0xCFFFFFC0,
			Single24.kMaxInt,  0x4FFFFFC0,

			0x39C000, 0x3E700000,
			0x407921, 0x401E4840,
			0x5870F0, 0x461C3C00,
		};

		[TestMethod]
		public void Single24_TestConversions()
		{
			for (int x = 0; x < kTestConversionsList.Length; x += 2)
			{
				uint input = kTestConversionsList[x + 0];
				uint expected = kTestConversionsList[x + 1];

				float single = Single24.ToSingle(input);
				uint output = Single24.FromSingle(single);

				Assert.AreEqual(ByteSwap.SingleFromUInt32(expected), single); // Test ToSingle
				Assert.AreEqual(input, output); // Test FromSingle
			}
		}
	};
}