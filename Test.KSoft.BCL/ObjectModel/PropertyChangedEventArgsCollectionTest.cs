using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class PropertyChangedEventArgsCollectionTest : BaseTestClass
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used as an expression target in property-name caching tests.")]
	sealed class Model
	{
		public string Name { get; set; } = string.Empty;
		public int Count { get; set; }
	}

	[TestMethod]
	public void CreateArgs_PropertyExpression_AddsCachedArgsAndReturnsThis()
	{
		var collection = new PropertyChangedEventArgsCollection();

		var returned = collection.CreateArgs<Model, string>(out var eventArgs, model => model.Name);

		Assert.AreSame(collection, returned);
		Assert.AreEqual(nameof(Model.Name), eventArgs.PropertyName);
		CollectionAssert.AreEqual(new[] { eventArgs }, new List<PropertyChangedEventArgs>(collection));
	}

	[TestMethod]
	public void Branch_CreatesSeparateSnapshot()
	{
		var collection = new PropertyChangedEventArgsCollection()
			.CreateArgs<Model, string>(out var nameArgs, model => model.Name);

		var branch = collection.Branch();
		collection.CreateArgs<Model, int>(out var countArgs, model => model.Count);

		Assert.AreNotSame(collection, branch);
		CollectionAssert.AreEqual(new[] { nameArgs }, new List<PropertyChangedEventArgs>(branch));
		CollectionAssert.AreEqual(new[] { nameArgs, countArgs }, new List<PropertyChangedEventArgs>(collection));
	}

	[TestMethod]
	public void NotifyPropertiesChanged_HandlerReceivesCachedArgs()
	{
		var sender = new object();
		var collection = new PropertyChangedEventArgsCollection()
			.CreateArgs<Model, string>(out var nameArgs, model => model.Name)
			.CreateArgs<Model, int>(out var countArgs, model => model.Count);
		var received = new List<PropertyChangedEventArgs>();

		collection.NotifyPropertiesChanged(sender, (actualSender, args) =>
		{
			Assert.AreSame(sender, actualSender);
			received.Add(args);
		});

		CollectionAssert.AreEqual(new[] { nameArgs, countArgs }, received);
	}

	[TestMethod]
	public void NotifyPropertiesChanged_NullHandler_DoesNotThrow()
	{
		var collection = new PropertyChangedEventArgsCollection()
			.CreateArgs<Model, string>(out _, model => model.Name);

		collection.NotifyPropertiesChanged(sender: null, handler: null);
	}
}
