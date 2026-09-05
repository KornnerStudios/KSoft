
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Text.Test
{
	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{
		[TestMethod]
		public void ParseBooleanLazy_CommonInputs_ReturnExpectedValues()
		{
			Assert.IsTrue(Util.ParseBooleanLazy("1"));
			Assert.IsTrue(Util.ParseBooleanLazy("true"));
			Assert.IsTrue(Util.ParseBooleanLazy("on"));
			Assert.IsFalse(Util.ParseBooleanLazy(null));
			Assert.IsFalse(Util.ParseBooleanLazy("false"));
		}

		[TestMethod]
		public void DefaultTextParseErrorHandler_RepeatedAccess_ReturnsSameInstance()
		{
			var handler = Util.DefaultTextParseErrorHandler;
			Assert.IsNotNull(handler);
			Assert.AreSame(handler, Util.DefaultTextParseErrorHandler);
		}
	};
}
