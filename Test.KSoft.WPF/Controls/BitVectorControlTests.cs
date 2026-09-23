using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using KSoft.Collections;
using KSoft.WPF;
using KSoft.WPF.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.WPF.Controls;

[TestClass]
public sealed class BitVectorControlTests
{
	enum UiBits
	{
		[Display(Name = "First bit", Description = "First description")]
		First,
		Second,
		[Browsable(false)]
		Hidden,
		kNumberOf,
	}

	enum WideBits { Highest = 63, kNumberOf }
	enum HiddenBits { [Browsable(false)] Hidden, kNumberOf }
	enum OverflowBits : ulong { TooLarge = 0x80000000UL }
	enum AliasUiBits : long
	{
		None = long.MinValue,
		kNumberOf = 4,
		[Display(Name = "")]
		Canonical = 0,
		[Display(Name = "Alias should not override canonical metadata")]
		Alias = Canonical,
		[Display(Name = "Last bit", Description = "Declared after the bound")]
		Last = 3,
	}

	public TestContext TestContext { get; set; }

	[STATestMethod]
	public void TypedBinding_InferredMetadataAndCopyUpdates_PreservesTypedSnapshot()
	{
		var initial = new BitVector32<UiBits>();
		var model = new FrameworkElement { Tag = initial };
		var control = new BitVectorControl();
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(FrameworkElement.Tag))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		var before = Assert.IsInstanceOfType<IEnumBitVector>(control.BitVector);
		Assert.IsNull(control.BitsUserInterfaceSource);
		Assert.HasCount(3, control.BitItems);
		Assert.AreEqual("First bit", control.BitItems[0].DisplayName);
		Assert.AreEqual("Second", control.BitItems[1].DisplayName);
		Assert.IsFalse(control.BitItems[2].IsVisible);

		GetCheckBox(control, 0).IsChecked = true;
		DrainDispatcher(control);

		Assert.IsTrue(Assert.IsInstanceOfType<BitVector32<UiBits>>(model.Tag).Test(UiBits.First));
		Assert.IsFalse(before.GetBit(0));
		Assert.IsFalse(initial.Test(UiBits.First));
		Assert.IsTrue(BindingOperations.IsDataBound(control, BitVectorControl.BitVectorProperty));
		model.Tag = initial.With(UiBits.Second);
		DrainDispatcher(control);
		Assert.IsFalse(GetCheckBox(control, 0).IsChecked);
		Assert.IsTrue(GetCheckBox(control, 1).IsChecked);
	}

	[STATestMethod]
	public void TypedBinding_64BitSparseMetadata_EditsNamedHighestBit()
	{
		var model = new FrameworkElement { Tag = new BitVector64<WideBits>() };
		var control = new BitVectorControl();
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(FrameworkElement.Tag))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		Assert.HasCount(1, control.BitItems);
		GetCheckBox(control, 63).IsChecked = true;
		DrainDispatcher(control);
		Assert.IsTrue(Assert.IsInstanceOfType<BitVector64<WideBits>>(model.Tag).Test(WideBits.Highest));
	}

	[STATestMethod]
	public void TypedMetadata_ExplicitOverrideAndMismatch_HaveDefinedPrecedence()
	{
		var control = new BitVectorControl { BitVector = new BitVector32<UiBits>().With(UiBits.Second) };
		var source = BitVectorUserInterfaceData.ForStrings(new[] { "Override first", "Override second" });
		control.BitsUserInterfaceSource = source;
		Assert.AreSame(source, control.BitsUserInterfaceSource);
		Assert.AreEqual("Override first", control.BitItems[0].DisplayName);
		Assert.IsTrue(control.BitItems[1].IsSet);
		control.BitsUserInterfaceSource = null;
		Assert.AreEqual("First bit", control.BitItems[0].DisplayName);
		Assert.ThrowsExactly<ArgumentException>(() => control.BitsEnumType = typeof(WideBits));
	}

	[STATestMethod]
	public void Metadata_FactoryFailure_PreservesTypeItemsAndFutureUpdates()
	{
		var control = new BitVectorControl
		{
			BitsEnumType = typeof(UiBits),
			BitVector = new BitVector32(1),
		};
		var items = control.BitItems;

		for (int attempt = 0; attempt < 2; attempt++)
		{
			Assert.ThrowsExactly<OverflowException>(() => control.SetValue(
				BitVectorControl.BitsEnumTypeProperty, typeof(OverflowBits)));
			Assert.AreEqual(typeof(UiBits), control.BitsEnumType);
			Assert.AreSame(items, control.BitItems);
			control.BitVector = new BitVector32(2);
			Assert.IsFalse(control.BitItems[0].IsSet);
			Assert.IsTrue(control.BitItems[1].IsSet);
			Assert.AreEqual("First bit", control.BitItems[0].DisplayName);
		}
		control.BitsEnumType = typeof(HiddenBits);
		Assert.HasCount(1, control.BitItems);
		Assert.IsFalse(control.BitItems[0].IsVisible);
	}

	[STATestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void TypedMetadata_RejectedMismatch_DoesNotInstallInvalidPropertyValue(bool flagsMetadata)
	{
		var control = new BitVectorControl
		{
			BitsEnumType = typeof(UiBits),
			BitVector = new BitVector32<UiBits>(),
		};
		var items = control.BitItems;
		var property = flagsMetadata ? BitVectorControl.FlagsEnumTypeProperty : BitVectorControl.BitsEnumTypeProperty;

		for (int attempt = 0; attempt < 2; attempt++)
		{
			Assert.ThrowsExactly<ArgumentException>(() => control.SetValue(property, typeof(WideBits)));
			Assert.AreEqual(typeof(UiBits), control.BitsEnumType);
			Assert.IsNull(control.FlagsEnumType);
			Assert.AreSame(items, control.BitItems);
			control.BitVector = new BitVector32<UiBits>().With(UiBits.Second);
			Assert.IsTrue(control.BitItems[1].IsSet);
		}
	}

	[STATestMethod]
	public void TypedVector_RejectedMetadataMismatch_PreservesPreviousVector()
	{
		var control = new BitVectorControl { BitsEnumType = typeof(UiBits), BitVector = new BitVector32(1) };
		var previous = control.BitVector;

		Assert.ThrowsExactly<ArgumentException>(() =>
			control.SetValue(BitVectorControl.BitVectorProperty, new BitVector64<WideBits>()));

		Assert.AreSame(previous, control.BitVector);
		Assert.IsTrue(control.BitItems[0].IsSet);
		control.BitVector = new BitVector32<UiBits>().With(UiBits.Second);
		Assert.IsFalse(control.BitItems[0].IsSet);
		Assert.IsTrue(control.BitItems[1].IsSet);
	}

	[STATestMethod]
	public void TypedBinding_RejectedMetadata_KeepsCheckboxAndSourceUpdatesWorking()
	{
		var model = new FrameworkElement { Tag = new BitVector32<UiBits>() };
		var control = new BitVectorControl { BitsEnumType = typeof(UiBits) };
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(FrameworkElement.Tag))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		var expression = BindingOperations.GetBindingExpression(control, BitVectorControl.BitVectorProperty);
		var first = GetCheckBox(control, 0);

		Assert.ThrowsExactly<ArgumentException>(() => control.BitsEnumType = typeof(WideBits));
		Assert.AreEqual(typeof(UiBits), control.BitsEnumType);
		Assert.AreSame(expression, BindingOperations.GetBindingExpression(control, BitVectorControl.BitVectorProperty));
		Assert.ThrowsExactly<ArgumentException>(() =>
			control.SetCurrentValue(BitVectorControl.BitVectorProperty, new BitVector64<WideBits>()));
		Assert.AreSame(expression, BindingOperations.GetBindingExpression(control, BitVectorControl.BitVectorProperty));
		Assert.IsInstanceOfType<BitVector32<UiBits>>(control.BitVector);

		first.IsChecked = true;
		DrainDispatcher(control);
		Assert.IsTrue(Assert.IsInstanceOfType<BitVector32<UiBits>>(model.Tag).Test(UiBits.First));
		model.Tag = new BitVector32<UiBits>().With(UiBits.Second);
		DrainDispatcher(control);
		Assert.IsFalse(first.IsChecked);
		Assert.IsTrue(GetCheckBox(control, 1).IsChecked);
	}

	[STATestMethod]
	public void TypedMetadata_CanonicalAlias_ComesFromTraitsWhileLegacyFactoryKeepsItsBehavior()
	{
		var typed = new BitVector32<AliasUiBits>().With(AliasUiBits.Alias).With(AliasUiBits.Last);
		Assert.AreEqual("Canonical,Last", typed.ToFlagsString());
		var control = new BitVectorControl { BitsEnumType = typeof(AliasUiBits), BitVector = typed };

		Assert.HasCount(2, control.BitItems);
		Assert.AreEqual(0, control.BitItems[0].BitIndex);
		Assert.IsFalse(control.BitItems[0].IsVisible);
		Assert.IsTrue(control.BitItems[0].IsSet);
		Assert.AreEqual(3, control.BitItems[1].BitIndex);
		Assert.AreEqual("Last bit", control.BitItems[1].DisplayName);
		Assert.AreEqual("Declared after the bound", control.BitItems[1].ToolTip);
		Assert.IsTrue(control.BitItems[1].IsSet);

		var legacy = BitVectorUserInterfaceData.ForEnum(typeof(AliasUiBits));
		Assert.IsTrue(legacy.IsVisible(0));
		Assert.AreEqual("Alias should not override canonical metadata", legacy.GetDisplayName(0));
		control.BitVector = typed.ToRaw();
		Assert.IsTrue(control.BitItems[0].IsVisible);
		Assert.AreEqual(legacy.GetDisplayName(0), control.BitItems[0].DisplayName);
		control.BitVector = typed;
		Assert.IsFalse(control.BitItems[0].IsVisible);
	}

	[STATestMethod]
	public void BitVector_First64BitAssignmentAndLaterChanges_SynchronizeHighestBit()
	{
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(WideBits)),
			BitVector = new BitVector64(1UL << 63),
		};

		Assert.IsTrue(control.BitItems[63].IsSet);
		control.BitVector = new BitVector64();
		Assert.IsFalse(control.BitItems[63].IsSet);
		control.BitVector = new BitVector64(1UL << 63);
		Assert.IsTrue(control.BitItems[63].IsSet);
		control.ClearValue(BitVectorControl.BitVectorProperty);
		Assert.IsFalse(control.BitItems[63].IsSet);
	}

	[STATestMethod]
	public void BitVector_RuntimeWidthChanges_ReplaceItemStates()
	{
		var control = CreateControl();
		control.BitVector = new BitVector32(1);
		control.BitVector = new BitVector64(2);
		Assert.IsFalse(control.BitItems[0].IsSet);
		Assert.IsTrue(control.BitItems[1].IsSet);

		control.BitVector = new BitVector32(1);
		Assert.IsTrue(control.BitItems[0].IsSet);
		Assert.IsFalse(control.BitItems[1].IsSet);
	}

	[STATestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public void Metadata_EitherInitializationOrderAndReplacement_UsesCurrentValue(bool metadataFirst)
	{
		var control = new BitVectorControl();
		var metadata = BitVectorUserInterfaceData.ForEnum(typeof(UiBits));
		if (metadataFirst)
		{
			control.BitsUserInterfaceSource = metadata;
		}
		control.BitVector = new BitVector32(1);
		control.BitsUserInterfaceSource = metadata;

		Assert.IsTrue(control.BitItems[0].IsSet);
		Assert.IsFalse(control.BitItems[1].IsSet);
		var previousItem = control.BitItems[0];
		control.BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(UiBits));
		Assert.AreNotSame(previousItem, control.BitItems[0]);
		Assert.IsTrue(control.BitItems[0].IsSet);
		Assert.AreEqual("First bit", control.BitItems[0].DisplayName);
		Assert.AreEqual("First description", control.BitItems[0].ToolTip);
		Assert.AreEqual(nameof(UiBits.Second), control.BitItems[1].DisplayName);
		Assert.IsTrue(control.BitItems[1].IsVisible);
	}

	[STATestMethod]
	public void BitVector_HiddenTrailingNamedBit_RemainsHiddenAndOperational()
	{
		var control = CreateControl();
		Assert.HasCount(3, control.BitItems);

		control.BitVector = new BitVector32().Set(UiBits.Hidden);

		Assert.HasCount(3, control.BitItems);
		Assert.IsFalse(control.BitItems[0].IsSet);
		Assert.IsFalse(control.BitItems[1].IsSet);
		Assert.IsTrue(control.BitItems[2].IsSet);
		Assert.IsFalse(control.BitItems[2].IsVisible);
		Assert.IsTrue(Assert.IsInstanceOfType<BitVector32>(control.BitVector).Test(UiBits.Hidden));
	}

	[STATestMethod]
	public void BitVector_AllHiddenMetadata_AcceptsStorageChanges()
	{
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(HiddenBits)),
		};

		control.BitVector = new BitVector32(1);

		Assert.HasCount(1, control.BitItems);
		Assert.IsFalse(control.BitItems[0].IsVisible);
		Assert.AreEqual(1, Assert.IsInstanceOfType<BitVector32>(control.BitVector).Data);
	}

	[STATestMethod]
	public void BitVector_ExplicitShorterMetadata_DoesNotRequireAnItemForEveryBit()
	{
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForStrings(new[] { "First", "Second" }),
			BitVector = new BitVector32().Set(UiBits.Hidden),
		};

		Assert.HasCount(2, control.BitItems);
		Assert.IsFalse(control.BitItems[0].IsSet);
		Assert.IsFalse(control.BitItems[1].IsSet);
		Assert.IsTrue(Assert.IsInstanceOfType<BitVector32>(control.BitVector).Test(UiBits.Hidden));
	}

	[STATestMethod]
	public void BitVector_EmptyMetadata_AcceptsStorageChanges()
	{
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForStrings(Array.Empty<string>()),
			BitVector = new BitVector32(1),
		};

		Assert.IsEmpty(control.BitItems);
		Assert.AreEqual(1, Assert.IsInstanceOfType<BitVector32>(control.BitVector).Data);
	}

	[STATestMethod]
	public void Metadata_VisibleIndexOutsideAssignedVector_ThrowsExplicitly()
	{
		var control = CreateControl();
		control.BitVector = new BitVector32(1);
		var source = control.BitsUserInterfaceSource;
		var items = control.BitItems;

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			control.BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(WideBits)));
		Assert.AreSame(source, control.BitsUserInterfaceSource);
		Assert.AreSame(items, control.BitItems);
		control.BitVector = new BitVector32(2);
		Assert.IsTrue(control.BitItems[1].IsSet);
	}

	[STATestMethod]
	public void ExplicitMetadata_UnusedEnumHintDoesNotOverrideSourceOrPoisonFailureRecovery()
	{
		var source = BitVectorUserInterfaceData.ForStrings(new[] { "Explicit label" });
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = source,
			BitVector = new BitVector32(1),
			BitsEnumType = typeof(OverflowBits),
		};
		Assert.AreEqual("Explicit label", control.BitItems[0].DisplayName);

		Assert.ThrowsExactly<OverflowException>(() => control.BitsUserInterfaceSource = null);
		Assert.AreSame(source, control.BitsUserInterfaceSource);
		control.BitVector = new BitVector32();
		Assert.IsFalse(control.BitItems[0].IsSet);
		control.BitsEnumType = typeof(UiBits);
		control.BitsUserInterfaceSource = null;
		Assert.AreEqual("First bit", control.BitItems[0].DisplayName);
	}

	[STATestMethod]
	public void Vector_VisibleMetadataBeyondCandidateWidth_RejectsWithoutChangingCurrentValue()
	{
		var vector = new BitVector64(1UL << 63);
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(WideBits)),
			BitVector = vector,
		};
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => control.BitVector = new BitVector32());
		Assert.AreEqual(vector, Assert.IsInstanceOfType<BitVector64>(control.BitVector));
		Assert.IsTrue(control.BitItems[63].IsSet);
	}

	[STATestMethod]
	public void Synchronization_ReentrantVectorReplacement_KeepsLatestState()
	{
		var control = CreateControl();
		var first = control.BitItems[0];
		var descriptor = DependencyPropertyDescriptor.FromProperty(
			BitVectorControl.BitItemModel.IsSetProperty, typeof(BitVectorControl.BitItemModel));
		EventHandler handler = (_, _) =>
		{
			if (first.IsSet)
			{
				control.BitVector = new BitVector32(1);
			}
		};
		descriptor.AddValueChanged(first, handler);
		try
		{
			control.BitVector = new BitVector32(3);

			Assert.AreEqual(1, Assert.IsInstanceOfType<BitVector32>(control.BitVector).Data);
			Assert.IsTrue(control.BitItems[0].IsSet);
			Assert.IsFalse(control.BitItems[1].IsSet);
		}
		finally
		{
			descriptor.RemoveValueChanged(first, handler);
		}
	}

	[STATestMethod]
	public void TwoWayBinding_RepeatedCheckboxAndSourceEdits_PreservesBindingAndNotifications()
	{
		var model = new BitsViewModel();
		var control = CreateControl();
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(BitsViewModel.Bits))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		var observed = new List<int>();
		model.PropertyChanged += (_, _) => observed.Add(model.Bits.Data);
		var first = GetCheckBox(control, 0);
		var second = GetCheckBox(control, 1);

		first.IsChecked = true;
		DrainDispatcher(control);
		Assert.AreEqual(1, model.Bits.Data);
		second.IsChecked = true;
		DrainDispatcher(control);
		Assert.AreEqual(3, model.Bits.Data);
		first.IsChecked = false;
		DrainDispatcher(control);
		Assert.AreEqual(2, model.Bits.Data);
		Assert.IsTrue(BindingOperations.IsDataBound(control, BitVectorControl.BitVectorProperty));

		int setterCalls = model.SetterCalls;
		model.Bits = new BitVector32(1);
		DrainDispatcher(control);
		Assert.AreEqual(setterCalls + 1, model.SetterCalls);
		Assert.IsTrue(first.IsChecked);
		Assert.IsFalse(second.IsChecked);
		model.Bits = new BitVector32(1);
		DrainDispatcher(control);

		CollectionAssert.AreEqual(new[] { 1, 3, 2, 1 }, observed);
		Assert.AreEqual(setterCalls + 2, model.SetterCalls);
		control.BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(UiBits));
		DrainDispatcher(control);
		Assert.AreEqual(setterCalls + 2, model.SetterCalls);
		Assert.IsTrue(GetCheckBox(control, 0).IsChecked);
		Assert.IsFalse(GetCheckBox(control, 1).IsChecked);
	}

	[STATestMethod]
	public void TwoWayBinding_Raw64BitValue_EditsHighestBit()
	{
		var model = new FrameworkElement { Tag = new BitVector64() };
		var control = new BitVectorControl
		{
			BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(WideBits)),
		};
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(FrameworkElement.Tag))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		var highest = GetCheckBox(control, 63);

		highest.IsChecked = true;
		DrainDispatcher(control);
		Assert.AreEqual(long.MinValue, Assert.IsInstanceOfType<BitVector64>(model.Tag).Data);
		highest.IsChecked = false;
		DrainDispatcher(control);
		Assert.AreEqual(0L, Assert.IsInstanceOfType<BitVector64>(model.Tag).Data);
		Assert.IsTrue(BindingOperations.IsDataBound(control, BitVectorControl.BitVectorProperty));

		model.Tag = new BitVector64(1UL << 63);
		DrainDispatcher(control);
		Assert.IsTrue(highest.IsChecked);
	}

	[STATestMethod]
	public void TwoWayBinding_ReentrantSourceEdit_UpdatesCurrentValueWithoutExtraWrites()
	{
		var model = new BitsViewModel();
		var control = CreateControl();
		BindingOperations.SetBinding(control, BitVectorControl.BitVectorProperty, new Binding(nameof(BitsViewModel.Bits))
		{
			Source = model,
			Mode = BindingMode.TwoWay,
		});
		using var host = new ControlHost(control);
		DrainDispatcher(control);
		var observed = new List<int>();
		model.PropertyChanged += (_, _) =>
		{
			observed.Add(model.Bits.Data);
			if (model.Bits.Data == 3)
			{
				model.Bits = new BitVector32(1);
			}
		};
		int setterCalls = model.SetterCalls;

		model.Bits = new BitVector32(3);
		DrainDispatcher(control);

		CollectionAssert.AreEqual(new[] { 3, 1 }, observed);
		Assert.AreEqual(setterCalls + 2, model.SetterCalls);
		Assert.AreEqual(1, model.Bits.Data);
		Assert.IsTrue(GetCheckBox(control, 0).IsChecked);
		Assert.IsFalse(GetCheckBox(control, 1).IsChecked);
	}

	static BitVectorControl CreateControl() => new()
	{
		BitsUserInterfaceSource = BitVectorUserInterfaceData.ForEnum(typeof(UiBits)),
	};

	void DrainDispatcher(DispatcherObject value) =>
		value.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle,
			TestContext.CancellationToken, TimeSpan.FromSeconds(5));

	static CheckBox GetCheckBox(DependencyObject root, int index) =>
		Assert.ContainsSingle(FindCheckBoxes(root).Where(checkbox =>
			checkbox.DataContext is BitVectorControl.BitItemModel model && model.BitIndex == index));

	static IEnumerable<CheckBox> FindCheckBoxes(DependencyObject parent)
	{
		if (parent is CheckBox checkbox)
		{
			yield return checkbox;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			foreach (var child in FindCheckBoxes(VisualTreeHelper.GetChild(parent, i)))
			{
				yield return child;
			}
		}
	}

	public sealed class BitsViewModel : INotifyPropertyChanged
	{
		BitVector32 mBits;
		public int SetterCalls { get; private set; }
		public BitVector32 Bits
		{
			get => mBits;
			set
			{
				SetterCalls++;
				if (mBits == value)
				{
					return;
				}
				mBits = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bits)));
			}
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class ControlHost : IDisposable
	{
		readonly Window mWindow;
		readonly BitVectorControl mControl;

		public ControlHost(BitVectorControl control)
		{
			mControl = control;
			mWindow = new Window
			{
				Content = control,
				Width = 250,
				Height = 150,
				Left = -10000,
				Top = -10000,
				ShowActivated = false,
				ShowInTaskbar = false,
				WindowStyle = WindowStyle.None,
			};
			mWindow.Show();
			mWindow.UpdateLayout();
		}

		public void Dispose()
		{
			BindingOperations.ClearBinding(mControl, BitVectorControl.BitVectorProperty);
			mWindow.Close();
		}
	}
}
