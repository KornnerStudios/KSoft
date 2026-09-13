
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class BasicViewModelTest : BaseTestClass
{
	sealed class Model : BasicViewModel
	{
		public void Notify(string propertyName) => OnPropertyChanged(propertyName);
		public void NotifyAll() => OnPropertyChanged(null!);
		public void NotifyCached(PropertyChangedEventArgs eventArgs) => OnPropertyChangedCached(eventArgs);
	}

	sealed class TrackingModel : BasicViewModel
	{
		public int CachedNotificationCalls { get; private set; }

		public void NotifyCached(PropertyChangedEventArgs eventArgs) => OnPropertyChangedCached(eventArgs);

		protected override void OnPropertyChangedCached(PropertyChangedEventArgs eventArgs)
		{
			CachedNotificationCalls++;
			base.OnPropertyChangedCached(eventArgs);
		}
	}

	[TestMethod]
	public void OnPropertyChanged_SubscribedHandlerReceivesSenderAndPropertyName()
	{
		var model = new Model();
		object? receivedSender = null;
		PropertyChangedEventArgs? receivedArgs = null;
		model.PropertyChanged += (sender, args) =>
		{
			receivedSender = sender;
			receivedArgs = args;
		};

		model.Notify("Value");

		Assert.AreSame(model, receivedSender);
		Assert.IsNotNull(receivedArgs);
		Assert.AreEqual("Value", receivedArgs.PropertyName);
	}

	[TestMethod]
	public void OnPropertyChanged_CachedArgsReuseReferenceAndDispatchVirtually()
	{
		var model = new TrackingModel();
		var eventArgs = new PropertyChangedEventArgs("Value");
		var receivedArgs = new List<PropertyChangedEventArgs>();
		object? receivedSender = null;
		model.PropertyChanged += (sender, args) =>
		{
			receivedSender = sender;
			receivedArgs.Add(args);
		};

		model.NotifyCached(eventArgs);
		model.NotifyCached(eventArgs);

		Assert.AreEqual(2, model.CachedNotificationCalls);
		Assert.AreSame(model, receivedSender);
		Assert.HasCount(2, receivedArgs);
		Assert.AreSame(eventArgs, receivedArgs[0]);
		Assert.AreSame(eventArgs, receivedArgs[1]);
	}

	[TestMethod]
	public void OnPropertyChanged_CachedArgsRejectsNull()
	{
		var model = new Model();

		Assert.ThrowsExactly<ArgumentNullException>(() => model.NotifyCached(null!));
	}

	[TestMethod]
	public void OnPropertyChanged_StringNullStillRaisesAllPropertiesChanged()
	{
		var model = new Model();
		PropertyChangedEventArgs? receivedArgs = null;
		model.PropertyChanged += (_, args) => receivedArgs = args;

		model.NotifyAll();

		Assert.IsNotNull(receivedArgs);
		Assert.IsNull(receivedArgs.PropertyName);
	}

	[TestMethod]
	public void OnPropertyChanged_CachedArgsPropagatesSubscriberException()
	{
		var model = new Model();
		var expected = new InvalidOperationException("subscriber");
		var eventArgs = new PropertyChangedEventArgs("Value");
		model.PropertyChanged += (_, _) => throw expected;

		var exception = Assert.ThrowsExactly<InvalidOperationException>(
			() => model.NotifyCached(eventArgs));

		Assert.AreSame(expected, exception);
	}
}
