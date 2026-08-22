#nullable enable

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class BasicViewModelTest : BaseTestClass
{
	sealed class Model : BasicViewModel
	{
		public void Notify(string propertyName) => OnPropertyChanged(propertyName);
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
}
