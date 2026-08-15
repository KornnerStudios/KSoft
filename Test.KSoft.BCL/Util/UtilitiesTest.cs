using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{
		static void AssertThrowsArgumentNull(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

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

		[TestMethod]
		public void Util_ThrowIfNullTest()
		{
			var value = new object();

			Assert.AreSame(value, Util.ThrowIfNull(value));

			value = null;
			var exception = Assert.ThrowsExactly<ArgumentNullException>(() => Util.ThrowIfNull(value));

			Assert.AreEqual(nameof(value), exception.ParamName);
		}

		[TestMethod]
		public void TypeExtensions_VirtualBufferLengthGuards_ThrowArgumentOutOfRangeException()
		{
			using var stream = new IO.EndianStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBuffer(0), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBuffer(-1), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBufferWithBookmark(0), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBufferWithBookmark(-1), "bufferLength");
		}

		[TestMethod]
		public void TypeExtensions_FromEncodingNull_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = TypeExtensions.FromEncoding(null), "enc");
		}
	};
}
