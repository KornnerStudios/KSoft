using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class UtilitiesTest : BaseTestClass
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used as an expression target in property-name utility tests.")]
	sealed class SampleModel
	{
		public string Name { get; set; } = string.Empty;
		public int Count { get; set; }
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
		AssertThrowsArgumentNull("propertyExpr", () => Util.CreatePropertyChangedEventArgs<SampleModel>(null!));
		AssertThrowsArgumentNull("propertyExpr", () => Util.CreatePropertyChangedEventArgs<SampleModel, int>(null!));
		AssertThrowsArgument("propertyExpr", () => Util.CreatePropertyChangedEventArgs<SampleModel>(model => new object()));
		AssertThrowsArgument("propertyExpr", () => Util.CreatePropertyChangedEventArgs<SampleModel, int>(model => model.Count + 1));
	}
}
