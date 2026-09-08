using System;
using System.Collections.Generic;
using System.ComponentModel;
using KSoft.WPF.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.WPF.ViewModels;

[TestClass]
public sealed class DockWindowViewModelTests
{
	[TestMethod]
	public void GeneratedProperties_DifferentValues_AssignBeforeEventAndReuseArgs()
	{
		var model = new DockWindowViewModel();
		var receivedArgs = new List<PropertyChangedEventArgs>();
		var receivedTitles = new List<string?>();
		model.PropertyChanged += (sender, args) =>
		{
			Assert.AreSame(model, sender);
			receivedArgs.Add(args);
			receivedTitles.Add(model.Title);
		};

		model.Title = "first";
		model.Title = "second";

		CollectionAssert.AreEqual(new string?[] { "first", "second" }, receivedTitles);
		Assert.HasCount(2, receivedArgs);
		Assert.AreSame(receivedArgs[0], receivedArgs[1]);
		Assert.AreEqual(nameof(DockWindowViewModel.Title), receivedArgs[0].PropertyName);
	}

	[TestMethod]
	public void GeneratedProperties_EqualValues_DoNotNotify()
	{
		var model = new DockWindowViewModel();
		int notifications = 0;
		model.PropertyChanged += (_, _) => notifications++;

		model.Title = "value";
		model.Title = new string("value".ToCharArray());
		model.CanClose = true;
		model.IsClosed = false;

		Assert.AreEqual(1, notifications);
	}

	[TestMethod]
	public void GeneratedProperties_DifferentProperties_UseDifferentArgs()
	{
		var model = new DockWindowViewModel();
		var receivedArgs = new List<PropertyChangedEventArgs>();
		model.PropertyChanged += (_, args) => receivedArgs.Add(args);

		model.Title = "value";
		model.CanClose = false;

		Assert.HasCount(2, receivedArgs);
		Assert.AreNotSame(receivedArgs[0], receivedArgs[1]);
	}

	[TestMethod]
	public void Close_CanCloseControlsIsClosed()
	{
		var model = new DockWindowViewModel
		{
			CanClose = false,
		};

		model.Close();
		Assert.IsFalse(model.IsClosed);

		model.CanClose = true;
		model.Close();
		Assert.IsTrue(model.IsClosed);
	}

	[TestMethod]
	public void GeneratedProperty_WpfWeakEventManagerReceivesPropertyName()
	{
		var model = new DockWindowViewModel();
		var receivedNames = new List<string?>();
		EventHandler<PropertyChangedEventArgs> handler = (_, args) => receivedNames.Add(args.PropertyName);
		PropertyChangedEventManager.AddHandler(model, handler, nameof(DockWindowViewModel.Title));

		try
		{
			model.Title = "value";
		}
		finally
		{
			PropertyChangedEventManager.RemoveHandler(model, handler, nameof(DockWindowViewModel.Title));
		}

		CollectionAssert.AreEqual(new[] { nameof(DockWindowViewModel.Title) }, receivedNames);
	}

	[TestMethod]
	public void IsClosed_DockManagerMaintainsSingleMembershipAndUnsubscribes()
	{
		var manager = new DockManagerViewModel();
		manager.InitializeObservableCollections();
		var document = new DockWindowViewModel();

		Assert.IsTrue(manager.AddDocument(document));
		Assert.HasCount(1, manager.Documents!);

		document.IsClosed = false;
		document.IsClosed = false;
		Assert.HasCount(1, manager.Documents!);

		document.Close();
		Assert.IsEmpty(manager.Documents!);

		document.IsClosed = false;
		Assert.HasCount(1, manager.Documents!);

		Assert.IsTrue(manager.RemoveDocument(document));
		Assert.IsEmpty(manager.Documents!);

		document.IsClosed = true;
		Assert.IsEmpty(manager.Documents!);
	}

	[TestMethod]
	public void GeneratedProperty_ReentrantSetter_CompletesDepthFirst()
	{
		var model = new DockWindowViewModel();
		var observedValues = new List<string?>();
		model.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName != nameof(DockWindowViewModel.Title))
			{
				return;
			}

			observedValues.Add(model.Title);
			if (model.Title == "outer")
			{
				model.Title = "inner";
			}
		};

		model.Title = "outer";

		CollectionAssert.AreEqual(new string?[] { "outer", "inner" }, observedValues);
		Assert.AreEqual("inner", model.Title);
	}
}
