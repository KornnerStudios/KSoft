using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.ObjectModel.Test;

[TestClass]
public sealed class BasicViewModelSetFieldTest
{
	[TestMethod]
	public void CachedSetField_DifferentValue_AssignsBeforeEventAndReusesArgs()
	{
		var model = new Model();
		var eventArgs = new PropertyChangedEventArgs(nameof(Model.Text));
		var receivedArgs = new List<PropertyChangedEventArgs>();
		var receivedValues = new List<string?>();
		var receivedSenders = new List<object?>();
		model.PropertyChanged += (sender, args) =>
		{
			receivedSenders.Add(sender);
			receivedArgs.Add(args);
			receivedValues.Add(model.Text);
		};

		Assert.IsTrue(model.SetTextCached("first", eventArgs));
		Assert.IsTrue(model.SetTextCached("second", eventArgs));

		CollectionAssert.AreEqual(new string?[] { "first", "second" }, receivedValues);
		Assert.IsTrue(receivedSenders.TrueForAll(sender => ReferenceEquals(model, sender)));
		Assert.HasCount(2, receivedArgs);
		Assert.AreSame(eventArgs, receivedArgs[0]);
		Assert.AreSame(eventArgs, receivedArgs[1]);
	}

	[TestMethod]
	public void ExistingSetField_DifferentValues_AllocateDifferentArgs()
	{
		var model = new Model();
		var receivedArgs = new List<PropertyChangedEventArgs>();
		model.PropertyChanged += (_, args) => receivedArgs.Add(args);

		Assert.IsTrue(model.SetText("first"));
		Assert.IsTrue(model.SetText("second"));

		Assert.HasCount(2, receivedArgs);
		Assert.AreNotSame(receivedArgs[0], receivedArgs[1]);
	}

	[TestMethod]
	public void CachedSetField_EqualValue_DoesNotAssignOrNotify()
	{
		var model = new Model();
		var eventArgs = new PropertyChangedEventArgs(nameof(Model.Text));
		int notifications = 0;
		model.PropertyChanged += (_, _) => notifications++;

		Assert.IsTrue(model.SetTextCached("value", eventArgs));
		Assert.IsFalse(model.SetTextCached(new string("value".ToCharArray()), eventArgs));

		Assert.AreEqual("value", model.Text);
		Assert.AreEqual(1, notifications);
	}

	[TestMethod]
	public void ExistingSetFieldVal_ForcedEqualValue_BypassesEqualityAndNotifies()
	{
		var current = new MutatingEquatable(equalsResult: true, throwOnEquals: true);
		var replacement = new MutatingEquatable(equalsResult: true, throwOnEquals: false);
		var model = new Model(current);
		PropertyChangedEventArgs? receivedArgs = null;
		model.PropertyChanged += (_, args) => receivedArgs = args;

		Assert.IsTrue(model.SetValue(replacement, overrideChecks: true));

		Assert.AreEqual(replacement, model.Value);
		Assert.IsNotNull(receivedArgs);
		Assert.AreEqual(nameof(Model.Value), receivedArgs.PropertyName);
	}

	[TestMethod]
	public void SetFieldVal_MutableEqualsReceiver_PreservesExistingSemantics()
	{
		var initial = new MutatingEquatable(equalsResult: true, throwOnEquals: false);
		var model = new Model(initial);
		var eventArgs = new PropertyChangedEventArgs(nameof(Model.Value));

		Assert.IsFalse(model.SetValue(new MutatingEquatable()));
		Assert.AreEqual(1, model.Value.EqualsCallCount);

		model.ResetValue(initial);

		Assert.IsFalse(model.SetValueCached(new MutatingEquatable(), eventArgs));
		Assert.AreEqual(1, model.Value.EqualsCallCount);
	}

	[TestMethod]
	public void SetFieldVal_ThrowingEquals_MutatesFieldBeforeException()
	{
		var initial = new MutatingEquatable(equalsResult: false, throwOnEquals: true);
		var model = new Model(initial);
		var eventArgs = new PropertyChangedEventArgs(nameof(Model.Value));

		Assert.ThrowsExactly<InvalidOperationException>(
			() => model.SetValueCached(new MutatingEquatable(), eventArgs));

		Assert.AreEqual(1, model.Value.EqualsCallCount);
	}

	[TestMethod]
	public void SetFieldObj_OldValueEqualsNull_PreservesLegacySemantics()
	{
		var current = new NullAwareEquatable(equalsNull: true);
		var model = new Model(current);
		int notifications = 0;
		model.PropertyChanged += (_, _) => notifications++;

		Assert.IsFalse(model.SetObject(null!));
		Assert.AreSame(current, model.ObjectValue);
		Assert.AreEqual(1, current.EqualsNullCallCount);
		Assert.AreEqual(0, notifications);
	}

	[TestMethod]
	public void CachedSetFieldEnum_EqualAndDifferentValues_MatchExistingBehavior()
	{
		var model = new Model();
		var eventArgs = new PropertyChangedEventArgs(nameof(Model.Mode));
		int notifications = 0;
		model.PropertyChanged += (_, _) => notifications++;

		Assert.IsFalse(model.SetModeCached(SampleMode.None, eventArgs));
		Assert.IsTrue(model.SetModeCached((SampleMode)42, eventArgs));
		Assert.IsFalse(model.SetModeCached((SampleMode)42, eventArgs));

		Assert.AreEqual((SampleMode)42, model.Mode);
		Assert.AreEqual(1, notifications);
	}

	[TestMethod]
	public void CachedSetField_HandlerSnapshotAndExceptionBehavior_MatchExistingPath()
	{
		var model = new Model();
		PropertyChangedEventHandler? removingHandler = null;
		int removingHandlerCalls = 0;
		int laterHandlerCalls = 0;
		removingHandler = (_, _) => removingHandlerCalls++;
		model.PropertyChanged += removingHandler;
		model.PropertyChanged += (_, _) => laterHandlerCalls++;

		var current = new CallbackEquatable(
			() => model.PropertyChanged -= removingHandler,
			equalsResult: false);
		model.ResetCallback(current);

		var eventArgs = new PropertyChangedEventArgs(nameof(Model.CallbackValue));
		Assert.IsTrue(model.SetCallbackCached(new CallbackEquatable(null, false), eventArgs));
		Assert.AreEqual(1, removingHandlerCalls);
		Assert.AreEqual(1, laterHandlerCalls);

		var calls = new List<int>();
		model.PropertyChanged += (_, _) =>
		{
			calls.Add(1);
			throw new InvalidOperationException("stop");
		};
		model.PropertyChanged += (_, _) => calls.Add(2);

		Assert.ThrowsExactly<InvalidOperationException>(
			() => model.SetTextCached("throws", new PropertyChangedEventArgs(nameof(Model.Text))));
		CollectionAssert.AreEqual(new[] { 1 }, calls);
		Assert.AreEqual("throws", model.Text);
	}

	private enum SampleMode
	{
		None,
		One,
	}

	private struct MutatingEquatable : IEquatable<MutatingEquatable>
	{
		private readonly bool mEqualsResult;
		private readonly bool mThrowOnEquals;

		public MutatingEquatable(bool equalsResult, bool throwOnEquals)
		{
			mEqualsResult = equalsResult;
			mThrowOnEquals = throwOnEquals;
		}

		public int EqualsCallCount { get; private set; }

		public bool Equals(MutatingEquatable other)
		{
			EqualsCallCount++;
			if (mThrowOnEquals)
			{
				throw new InvalidOperationException("Equals failed.");
			}

			return mEqualsResult;
		}
	}

	private sealed class NullAwareEquatable
		: IEquatable<NullAwareEquatable>
	{
		private readonly bool mEqualsNull;

		public NullAwareEquatable(bool equalsNull)
		{
			mEqualsNull = equalsNull;
		}

		public int EqualsNullCallCount { get; private set; }

		public bool Equals(NullAwareEquatable? other)
		{
			if (other is null)
			{
				EqualsNullCallCount++;
				return mEqualsNull;
			}

			return ReferenceEquals(this, other);
		}

	}

	private sealed class CallbackEquatable
		: IEquatable<CallbackEquatable>
	{
		private readonly Action? mOnEquals;
		private readonly bool mEqualsResult;

		public CallbackEquatable(Action? onEquals, bool equalsResult)
		{
			mOnEquals = onEquals;
			mEqualsResult = equalsResult;
		}

		public bool Equals(CallbackEquatable? other)
		{
			mOnEquals?.Invoke();
			return mEqualsResult;
		}
	}

	private sealed class Model
		: BasicViewModel
	{
		private string? mText;
		private MutatingEquatable mValue;
		private NullAwareEquatable mObjectValue;
		private SampleMode mMode;
		private CallbackEquatable mCallbackValue = new(null, false);

		public Model()
			: this(new NullAwareEquatable(equalsNull: false))
		{
		}

		public Model(MutatingEquatable value)
			: this(new NullAwareEquatable(equalsNull: false))
		{
			mValue = value;
		}

		public Model(NullAwareEquatable objectValue)
		{
			mObjectValue = objectValue;
		}

		public string? Text => mText;
		public MutatingEquatable Value => mValue;
		public NullAwareEquatable ObjectValue => mObjectValue;
		public SampleMode Mode => mMode;
		public CallbackEquatable CallbackValue => mCallbackValue;

		public bool SetText(string? value) =>
			SetField(ref mText, value, propertyName: nameof(Text));

		public bool SetTextCached(string? value, PropertyChangedEventArgs eventArgs) =>
			SetField(ref mText, value, eventArgs);

		public bool SetValue(MutatingEquatable value, bool overrideChecks = false) =>
			SetFieldVal(ref mValue, value, overrideChecks, nameof(Value));

		public bool SetValueCached(
			MutatingEquatable value,
			PropertyChangedEventArgs eventArgs) =>
			SetFieldVal(ref mValue, value, eventArgs);

		public bool SetObject(NullAwareEquatable value) =>
			SetFieldObj(ref mObjectValue, value, propertyName: nameof(ObjectValue));

		public bool SetModeCached(SampleMode value, PropertyChangedEventArgs eventArgs) =>
			SetFieldEnum(ref mMode, value, eventArgs);

		public bool SetCallbackCached(CallbackEquatable value, PropertyChangedEventArgs eventArgs) =>
			SetField(ref mCallbackValue, value, eventArgs);

		public void ResetValue(MutatingEquatable value) => mValue = value;
		public void ResetCallback(CallbackEquatable value) => mCallbackValue = value;
	}
}
