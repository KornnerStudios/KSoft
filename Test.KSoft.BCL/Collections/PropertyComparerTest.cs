using System;
using KSoft.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class PropertyComparerTest : BaseTestClass
{
	sealed class Item
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	static void AssertThrowsArgumentNull(string parameterName, Action action)
	{
		var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

		Assert.AreEqual(parameterName, exception.ParamName);
	}

	[TestMethod]
	public void Constructor_NullPropertyInfo_ThrowsArgumentNullException()
	{
		AssertThrowsArgumentNull("property", () =>
			new PropertyComparer<Item>((System.Reflection.PropertyInfo)null!));
	}

	[TestMethod]
	public void Compare_DefaultProperty_ComparesFirstProperty()
	{
		var comparer = new PropertyComparer<Item>();

		Assert.IsLessThan(0, comparer.Compare(new Item { Id = 1 }, new Item { Id = 2 }));
		Assert.IsGreaterThan(0, comparer.Compare(new Item { Id = 2 }, new Item { Id = 1 }));
		Assert.AreEqual(0, comparer.Compare(new Item { Id = 1 }, new Item { Id = 1 }));
	}

	[TestMethod]
	public void Compare_NamedProperty_ComparesSelectedProperty()
	{
		var comparer = new PropertyComparer<Item>(nameof(Item.Name));

		Assert.IsLessThan(0, comparer.Compare(new Item { Name = "a" }, new Item { Name = "b" }));
		Assert.IsGreaterThan(0, comparer.Compare(new Item { Name = "b" }, new Item { Name = "a" }));
		Assert.AreEqual(0, comparer.Compare(new Item { Name = "a" }, new Item { Name = "a" }));
	}

	[TestMethod]
	public void Compare_DescendingDirection_InvertsComparison()
	{
		var comparer = new PropertyComparer<Item>(nameof(Item.Id), SortDirection.Descending);

		Assert.IsGreaterThan(0, comparer.Compare(new Item { Id = 1 }, new Item { Id = 2 }));
		Assert.IsLessThan(0, comparer.Compare(new Item { Id = 2 }, new Item { Id = 1 }));
		Assert.AreEqual(0, comparer.Compare(new Item { Id = 1 }, new Item { Id = 1 }));
	}
}
