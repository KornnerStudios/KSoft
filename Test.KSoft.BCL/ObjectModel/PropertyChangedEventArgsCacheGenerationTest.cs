using System.Collections.Generic;
using System.ComponentModel;
using KSoft.PropertyChanged.SourceGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class PropertyChangedEventArgsCacheGenerationTest
{
	[TestMethod]
	public void HandwrittenSetter_UsesGeneratedCachedEventArgs()
	{
		var model = new CacheOnlyModel();
		var receivedArgs = new List<PropertyChangedEventArgs>();
		model.PropertyChanged += (sender, args) =>
		{
			Assert.AreSame(model, sender);
			receivedArgs.Add(args);
		};

		model.Value = 1;
		model.Value = 1;

		Assert.HasCount(2, receivedArgs);
		Assert.AreSame(receivedArgs[0], receivedArgs[1]);
		Assert.AreEqual(nameof(CacheOnlyModel.Value), receivedArgs[0].PropertyName);
	}
}

internal sealed partial class CacheOnlyModel
	: INotifyPropertyChanged
{
	private int mValue;

	public event PropertyChangedEventHandler? PropertyChanged;

	[GeneratedPropertyChangedEventArgs]
	public int Value
	{
		get => mValue;
		set
		{
			mValue = value;
			PropertyChanged?.Invoke(this, kValueChangedEventArgs);
		}
	}
}
