using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace KSoft.WPF.Controls
{
	/// <summary>Edits raw 32/64-bit vectors and typed vector snapshots through indexed presentation metadata.</summary>
	/// <remarks>Typed values infer metadata from their associated enum unless an explicit presentation source is active. Edits assign replacement vector values through the binding. Built-in enum mismatch and metadata construction/range checks run while coercing candidate values; this is not a general rollback guarantee for arbitrary custom metadata, callbacks, or binding failures.</remarks>
	public partial class BitVectorControl : UserControl
	{
		#region BitItems
		[SuppressMessage("Microsoft.Design", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ObservableCollection<BitItemModel> BitItems
		{
			get { return (ObservableCollection<BitItemModel>)GetValue(BitItemsProperty); }
			set { SetValue(BitItemsProperty, value); }
		}
		public static readonly DependencyProperty BitItemsProperty = DependencyProperty.Register(
			nameof(BitItems), typeof(ObservableCollection<BitItemModel>), typeof(BitVectorControl),
			new PropertyMetadata(new ObservableCollection<BitItemModel>()));
		#endregion

		#region BitEnumType
		/// <summary>Gets or sets the explicit bit-index enum hint for presentation.</summary>
		/// <remarks>For a typed vector, a non-null hint must match its associated enum, even when <see cref="BitsUserInterfaceSource"/> is supplied.</remarks>
		public Type? BitsEnumType
		{
			get { return (Type)GetValue(BitsEnumTypeProperty); }
			set { SetValue(BitsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty BitsEnumTypeProperty = DependencyProperty.Register(
			nameof(BitsEnumType), typeof(Type), typeof(BitVectorControl),
			new PropertyMetadata(null, OnBitEnumTypePropertyChanged, CoerceBitsEnumType),
			Reflection.Util.IsEnumTypeOrNull);
		#endregion

		#region FlagsEnumType
		/// <summary>Gets or sets a legacy flags-enum presentation hint for an untyped vector.</summary>
		/// <remarks>This must be null for a typed index vector; it cannot override that vector's enum identity.</remarks>
		public Type? FlagsEnumType
		{
			get { return (Type)GetValue(FlagsEnumTypeProperty); }
			set { SetValue(FlagsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty FlagsEnumTypeProperty = DependencyProperty.Register(
			nameof(FlagsEnumType),
			typeof(Type), typeof(BitVectorControl),
			new PropertyMetadata(null, OnBitEnumTypePropertyChanged, CoerceFlagsEnumType),
			Reflection.Util.IsEnumTypeOrNull);
		#endregion

		#region BitsUserInterfaceSource
		/// <summary>Gets or sets presentation metadata that takes precedence over enum-based inference.</summary>
		/// <remarks>This does not rewrite the enum hints. Removing the source reactivates enum metadata, subject to candidate validation. Explicit type hints must still match a typed vector. Custom sources are not automatically observed or guaranteed to be transactionally validated.</remarks>
		public IBitVectorUserInterfaceData? BitsUserInterfaceSource
		{
			get { return (IBitVectorUserInterfaceData)GetValue(BitsUserInterfaceSourceProperty); }
			set { SetValue(BitsUserInterfaceSourceProperty, value); }
		}
		public static readonly DependencyProperty BitsUserInterfaceSourceProperty = DependencyProperty.Register(
			nameof(BitsUserInterfaceSource), typeof(IBitVectorUserInterfaceData), typeof(BitVectorControl),
			new PropertyMetadata(null, OnBitsUserInterfaceSourcePropertyChanged, CoerceBitsUserInterfaceSource));
		#endregion

		#region BitVector
		/// <summary>Gets or sets a raw <see cref="Collections.BitVector32"/>, raw <see cref="Collections.BitVector64"/>, or <see cref="Collections.IEnumBitVector"/> value.</summary>
		/// <remarks>Null is not accepted. KSoft's typed adapters are boxed snapshots; a checkbox edit replaces the value rather than mutating the retained box. This dependency property binds two-way by default.</remarks>
		public object BitVector
		{
			get { return GetValue(BitVectorProperty); }
			set { SetValue(BitVectorProperty, value); }
		}
		public static readonly DependencyProperty BitVectorProperty = DependencyProperty.Register(
			nameof(BitVector), typeof(object), typeof(BitVectorControl),
			new FrameworkPropertyMetadata(new Collections.BitVector32(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnVectorPropertyChanged, CoerceBitVector),
			IsValidBitVectorValue);

		static bool IsValidBitVectorValue(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is Collections.BitVector32 ||
				obj is Collections.BitVector64)
			{
				return true;
			}
			return obj is Collections.IEnumBitVector typed && typed.BitsEnumType.IsEnum;
		}
		#endregion

		bool mSynchronizingBitItems;
		bool mBitItemsNeedSynchronization;
		IBitVectorUserInterfaceData? mEffectiveSource;
		IBitVectorUserInterfaceData? mEnumSource;
		Type? mSourceEnumType;
		Type? mItemsEnumType;
		bool mSourceIsFlags;
		bool mSourceIsTyped;

		public BitVectorControl()
		{
			InitializeComponent();
		}

		#region OnCheckBox Checked/Unchecked
		private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
		{
			var cb = (CheckBox)sender;
			var model = (BitItemModel)cb.DataContext;
			OnBitChanged(model, true);
		}

		private void OnCheckBoxUnchecked(object sender, RoutedEventArgs e)
		{
			var cb = (CheckBox)sender;
			var model = (BitItemModel)cb.DataContext;
			OnBitChanged(model, false);
		}

		private void OnBitChanged(BitItemModel bitModel, bool newValue)
		{
			if (mSynchronizingBitItems || !BitItems.Contains(bitModel))
			{
				return;
			}

			var bit_vector = BitVector;
			if (bit_vector is Collections.IEnumBitVector typed)
			{
				if (typed.GetBit(bitModel.BitIndex) != newValue)
				{
					BitVector = typed.WithBit(bitModel.BitIndex, newValue);
				}
			}
			else if (bit_vector is Collections.BitVector32 vector32)
			{
				if (vector32[bitModel.BitIndex] != newValue)
				{
					vector32[bitModel.BitIndex] = newValue;
					BitVector = vector32;
				}
			}
			else if (bit_vector is Collections.BitVector64 vector64)
			{
				if (vector64[bitModel.BitIndex] != newValue)
				{
					vector64[bitModel.BitIndex] = newValue;
					BitVector = vector64;
				}
			}
		}
		#endregion

		public void ForceVisibilityRefreshOfAllBitItems()
		{
			var source = mEffectiveSource;
			if (source == null)
			{
				return;
			}

			foreach (var bit_model in BitItems)
			{
				int bit_index = bit_model.BitIndex;
				bit_model.IsVisible = source.IsVisible(bit_index);
			}
			SynchronizeBitItems();
		}

		private static void OnBitEnumTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;

			ctrl.RefreshBitItems(true);
		}

		private static void OnBitsUserInterfaceSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.RefreshBitItems(true);
		}

		private static object CoerceBitsEnumType(DependencyObject d, object value)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.ResolveSource(ctrl.BitVector, (Type?)value, ctrl.FlagsEnumType, ctrl.BitsUserInterfaceSource, ctrl.HasAssignedVector);
			return value;
		}

		private static object CoerceFlagsEnumType(DependencyObject d, object value)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.ResolveSource(ctrl.BitVector, ctrl.BitsEnumType, (Type?)value, ctrl.BitsUserInterfaceSource, ctrl.HasAssignedVector);
			return value;
		}

		private static object CoerceBitsUserInterfaceSource(DependencyObject d, object value)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.ResolveSource(ctrl.BitVector, ctrl.BitsEnumType, ctrl.FlagsEnumType, (IBitVectorUserInterfaceData?)value, ctrl.HasAssignedVector);
			return value;
		}

		private static object CoerceBitVector(DependencyObject d, object value)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.ResolveSource(value, ctrl.BitsEnumType, ctrl.FlagsEnumType, ctrl.BitsUserInterfaceSource, true);
			return value;
		}

		private IBitVectorUserInterfaceData? ResolveSource(object vector, Type? bitsEnumType, Type? flagsEnumType,
			IBitVectorUserInterfaceData? explicitSource, bool hasVector)
		{
			var typed = vector as Collections.IEnumBitVector;
			var associatedType = typed?.BitsEnumType;
			if (associatedType != null &&
				(flagsEnumType != null || (bitsEnumType != null && bitsEnumType != associatedType)))
			{
				throw new ArgumentException("Explicit enum metadata must match the typed vector's bit-index enum.");
			}
			if (explicitSource != null)
			{
				ValidateSourceRange(vector, explicitSource, hasVector);
				return explicitSource;
			}
			var enumType = bitsEnumType ?? flagsEnumType ?? associatedType;
			bool isFlags = bitsEnumType == null && flagsEnumType != null;
			bool isTyped = typed != null;
			if (enumType != mSourceEnumType || isFlags != mSourceIsFlags || isTyped != mSourceIsTyped)
			{
				// Publish the cache key only after construction succeeds, including during coercion.
				var enumSource = typed != null ? BitVectorUserInterfaceData.ForVector(typed)
					: enumType == null ? null : isFlags
						? BitVectorUserInterfaceData.ForFlagsEnum(enumType)
						: BitVectorUserInterfaceData.ForEnum(enumType);
				ValidateSourceRange(vector, enumSource, hasVector);
				mEnumSource = enumSource;
				mSourceEnumType = enumType;
				mSourceIsFlags = isFlags;
				mSourceIsTyped = isTyped;
			}
			else
			{
				ValidateSourceRange(vector, mEnumSource, hasVector);
			}
			return mEnumSource;
		}

		private static void ValidateSourceRange(object vector, IBitVectorUserInterfaceData? source, bool hasVector)
		{
			if (!hasVector || source == null)
			{
				return;
			}
			int length = GetVectorLength(vector);
			var typed = vector as Collections.IEnumBitVector;
			for (int index = length; index < source.NumberOfBits; index++)
			{
				if ((typed == null || typed.IsDefinedIndex(index)) && source.IsVisible(index))
				{
					throw new ArgumentOutOfRangeException(nameof(source), index,
						"Visible metadata refers to a bit outside the vector's capacity.");
				}
			}
		}

		private bool HasAssignedVector =>
			DependencyPropertyHelper.GetValueSource(this, BitVectorProperty).BaseValueSource != BaseValueSource.Default;

		private void RefreshBitItems(bool force)
		{
			var typed = BitVector as Collections.IEnumBitVector;
			var associatedType = typed?.BitsEnumType;
			var source = ResolveSource(BitVector, BitsEnumType, FlagsEnumType, BitsUserInterfaceSource, HasAssignedVector);
			if (!force && ReferenceEquals(source, mEffectiveSource) && associatedType == mItemsEnumType)
			{
				SynchronizeBitItems();
				return;
			}
			var newBitItems = new ObservableCollection<BitItemModel>();
			if (source != null)
			{
				for (int bit_index = 0; bit_index < source.NumberOfBits; bit_index++)
				{
					if (typed != null && !typed.IsDefinedIndex(bit_index))
					{
						continue;
					}
					var model = new BitItemModel
					{
						BitIndex = bit_index,
						DisplayName = source.GetDisplayName(bit_index),
						ToolTip = source.GetDescription(bit_index),
						IsVisible = source.IsVisible(bit_index)
					};
					newBitItems.Add(model);
				}
			}

			mEffectiveSource = source;
			mItemsEnumType = associatedType;
			BitItems = newBitItems;
			SynchronizeBitItems();
		}

		#region OnVectorPropertyChanged
		private static void OnVectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.RefreshBitItems(false);
		}

		private void SynchronizeBitItems()
		{
			if (mSynchronizingBitItems)
			{
				mBitItemsNeedSynchronization = true;
				return;
			}

			mSynchronizingBitItems = true;
			try
			{
				do
				{
					mBitItemsNeedSynchronization = false;
					var vector = BitVector;
					int length = GetVectorLength(vector);
					// Metadata can arrive before a value chooses a vector width, or after ClearValue.
					bool has_value = HasAssignedVector;

					foreach (var model in BitItems)
					{
						bool is_set = false;
						if (has_value && (model.IsVisible || (uint)model.BitIndex < (uint)length))
						{
							is_set = ReadBit(vector, model.BitIndex);
						}
						if (model.IsSet != is_set)
						{
							model.IsSet = is_set;
						}
						// An observer may replace the vector or metadata while an item is changing.
						if (mBitItemsNeedSynchronization)
						{
							break;
						}
					}
				} while (mBitItemsNeedSynchronization);
			}
			finally
			{
				mSynchronizingBitItems = false;
			}
		}

		private static int GetVectorLength(object vector) => vector switch
		{
			Collections.BitVector32 value => value.Length,
			Collections.BitVector64 value => value.Length,
			Collections.IEnumBitVector value => value.Length,
			_ => throw new ArgumentException("Unsupported bit vector type.", nameof(vector)),
		};

		private static bool ReadBit(object vector, int bitIndex) => vector switch
		{
			Collections.BitVector32 value => value[bitIndex],
			Collections.BitVector64 value => value[bitIndex],
			Collections.IEnumBitVector value => value.GetBit(bitIndex),
			_ => throw new ArgumentException("Unsupported bit vector type.", nameof(vector)),
		};
		#endregion

		public sealed class BitItemModel : DependencyObject
		{
			public int BitIndex
			{
				get { return (int)GetValue(BitIndexProperty); }
				set { SetValue(BitIndexProperty, value); }
			}
			public static readonly DependencyProperty BitIndexProperty = DependencyProperty.Register(
				nameof(BitIndex), typeof(int), typeof(BitItemModel),
				new PropertyMetadata(defaultValue: TypeExtensions.kNone));

			public bool IsValid
			{
				get { return (bool)GetValue(IsVisibleProperty); }
				set { SetValue(IsVisibleProperty, value); }
			}
			public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(
				nameof(IsValid), typeof(bool), typeof(BitItemModel));

			public string DisplayName
			{
				get { return (string)GetValue(DisplayNameProperty); }
				set { SetValue(DisplayNameProperty, value); }
			}
			public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
				nameof(DisplayName), typeof(string), typeof(BitItemModel));

			public string ToolTip
			{
				get { return (string)GetValue(ToolTipProperty); }
				set { SetValue(ToolTipProperty, value); }
			}
			public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
				nameof(ToolTip), typeof(string), typeof(BitItemModel));

			public bool IsVisible
			{
				get { return (bool)GetValue(IsVisibleProperty); }
				set { SetValue(IsVisibleProperty, value); }
			}
			public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
				nameof(IsVisible), typeof(bool), typeof(BitItemModel),
				new PropertyMetadata(defaultValue: Util.TrueObject));

			public bool IsSet
			{
				get { return (bool)GetValue(IsSetProperty); }
				set { SetValue(IsSetProperty, value); }
			}
			public static readonly DependencyProperty IsSetProperty = DependencyProperty.Register(
				nameof(IsSet), typeof(bool), typeof(BitItemModel),
				new FrameworkPropertyMetadata(Util.FalseObject, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		};
	};
}
