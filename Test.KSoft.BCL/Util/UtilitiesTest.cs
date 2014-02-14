using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{
		[TestMethod]
		public void Util_UnixTimeTest()
		{
			{
				long test = 0x4B71FD5B;
				var test_date = new System.DateTime(2010, 2, 10, 0, 27, 7, System.DateTimeKind.Utc);

				long value = Util.ConvertDateTimeToUnixTime(test_date);
				var value_date = Util.ConvertDateTimeFromUnixTime(test);

				Assert.AreEqual(test, value);
				Assert.AreEqual(test_date, value_date);
			}
			var now = System.DateTime.UtcNow;
			// We have to clamp it since datetime tracks TimeOfDay plus Milliseconds
			var now_clamped = new System.DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);

			var time_t = Util.ConvertDateTimeToUnixTime(now);
			var converted = Util.ConvertDateTimeFromUnixTime(time_t);

			Assert.AreEqual(now_clamped, converted);
		}

		[TestMethod]
		public void Util_GenericReferenceEqualsTest()
		{
			string x = "x", y = "y";

			Assert.IsTrue(Util.GenericReferenceEquals(x, x));
			Assert.IsTrue(Util.GenericReferenceEquals(x, y) == Util.GenericReferenceEquals(y, x));

			y = null;
			Assert.IsFalse(Util.GenericReferenceEquals(x, y));

			x = null;
			Assert.IsTrue(Util.GenericReferenceEquals(x, y));
		}
	};
}