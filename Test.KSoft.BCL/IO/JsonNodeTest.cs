using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public class JsonNodeTest : BaseTestClass
	{
		[TestMethod]
		public void IO_JsonNodeGetRangesTest()
		{
			Dictionary<string, object> jsonData = null;
			int min = 0, max = 0;
			bool success;

			const string kJson1 = "{\"gold\":[10,50]}";
			{
				jsonData = MiniJSON.Json.Deserialize(kJson1) as Dictionary<string, object>;
				Assert.IsNotNull(jsonData);

				var jsonNode = new JsonNode(jsonData);

				min = 0; max = 0;
				success = jsonNode.GetRangeValues("gold", ref min, ref max);

				Assert.IsTrue(success);
				Assert.AreEqual(10, min);
				Assert.AreEqual(50, max);
			}

			const string kJson2 = "{\"gold\":[10]}";
			{
				jsonData = MiniJSON.Json.Deserialize(kJson2) as Dictionary<string, object>;
				Assert.IsNotNull(jsonData);

				var jsonNode = new JsonNode(jsonData);

				min = 0; max = 0;
				success = jsonNode.GetRangeValues("gold", ref min, ref max);

				Assert.IsTrue(success);
				Assert.AreEqual(10, min);
				Assert.AreEqual(10, max);
			}

			const string kJson3 = "{\"gold\":50}";
			{
				jsonData = MiniJSON.Json.Deserialize(kJson3) as Dictionary<string, object>;
				Assert.IsNotNull(jsonData);

				var jsonNode = new JsonNode(jsonData);

				min = 0; max = 0;
				success = jsonNode.GetRangeValues("gold", ref min, ref max);

				Assert.IsTrue(success);
				Assert.AreEqual(50, min);
				Assert.AreEqual(50, max);
			}
		}
	};
}