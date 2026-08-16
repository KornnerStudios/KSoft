using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class UtilitiesTest : BaseTestClass
{
	sealed class SampleModel
	{
		public string Name { get; set; } = string.Empty;
		public int Count { get; set; }
	}

	static void AssertArgumentNull(Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual("propertyExpr", exception.ParamName);
	}

	static void AssertArgument(Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentException>(action);

		Assert.AreEqual("propertyExpr", exception.ParamName);
	}

	[TestMethod]
	public void CreatePropertyChangedEventArgs_PropertyExpressionsReturnPropertyName()
	{
		var objectArgs = Util.CreatePropertyChangedEventArgs<SampleModel>(model => model.Name);
		var typedArgs = Util.CreatePropertyChangedEventArgs<SampleModel, int>(model => model.Count);

		Assert.AreEqual(nameof(SampleModel.Name), objectArgs.PropertyName);
		Assert.AreEqual(nameof(SampleModel.Count), typedArgs.PropertyName);
	}

	[TestMethod]
	public void CreatePropertyChangedEventArgs_InvalidExpressionsThrowExpectedExceptions()
	{
		AssertArgumentNull(() => Util.CreatePropertyChangedEventArgs<SampleModel>(null!));
		AssertArgumentNull(() => Util.CreatePropertyChangedEventArgs<SampleModel, int>(null!));
		AssertArgument(() => Util.CreatePropertyChangedEventArgs<SampleModel>(model => new object()));
		AssertArgument(() => Util.CreatePropertyChangedEventArgs<SampleModel, int>(model => model.Count + 1));
	}
}
