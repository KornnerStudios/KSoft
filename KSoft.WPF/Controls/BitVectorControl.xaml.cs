using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace KSoft.WPF.Controls
{
	/// <summary>
	/// Interaction logic for BitVectorControl.xaml
	/// </summary>
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
		public Type? BitsEnumType
		{
			get { return (Type)GetValue(BitsEnumTypeProperty); }
			set { SetValue(BitsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty BitsEnumTypeProperty = DependencyProperty.Register(
			nameof(BitsEnumType), typeof(Type), typeof(BitVectorControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnBitEnumTypePropertyChanged)),
			Reflection.Util.IsEnumTypeOrNull);
		#endregion

		#region FlagsEnumType
		public Type? FlagsEnumType
		{
			get { return (Type)GetValue(FlagsEnumTypeProperty); }
			set { SetValue(FlagsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty FlagsEnumTypeProperty = DependencyProperty.Register(
			nameof(FlagsEnumType),
			typeof(Type), typeof(BitVectorControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnBitEnumTypePropertyChanged)),
			Reflection.Util.IsEnumTypeOrNull);
		#endregion

		#region BitsUserInterfaceSource
		public IBitVectorUserInterfaceData? BitsUserInterfaceSource
		{
			get { return (IBitVectorUserInterfaceData)GetValue(BitsUserInterfaceSourceProperty); }
			set { SetValue(BitsUserInterfaceSourceProperty, value); }
		}
		public static readonly DependencyProperty BitsUserInterfaceSourceProperty = DependencyProperty.Register(
			nameof(BitsUserInterfaceSource), typeof(IBitVectorUserInterfaceData), typeof(BitVectorControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnBitsUserInterfaceSourcePropertyChanged)));
		#endregion

		#region BitVector
		public object BitVector
		{
			get { return GetValue(BitVectorProperty); }
			set { SetValue(BitVectorProperty, value); }
		}
		public static readonly DependencyProperty BitVectorProperty = DependencyProperty.Register(
			nameof(BitVector), typeof(object), typeof(BitVectorControl),
			new FrameworkPropertyMetadata(new Collections.BitVector32(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorPropertyChanged),
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

			return false;
		}
		#endregion

		bool mSynchronizingBitItems;
		bool mBitItemsNeedSynchronization;

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
			if (bit_vector is Collections.BitVector32 vector32)
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
			var source = this.BitsUserInterfaceSource;
			if (source == null)
			{
				return;
			}

			foreach (var bit_model in BitItems)
			{
				int bit_index = bit_model.BitIndex;
				bit_model.IsVisible = source.IsVisible(bit_index);
			}
		}

		private static void OnBitEnumTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;

			var bit_enum_type = e.NewValue as Type;

			IBitVectorUserInterfaceData? ui_source = null;
			if (bit_enum_type != null)
			{
				if (e.Property == BitsEnumTypeProperty)
				{
					ui_source = BitVectorUserInterfaceData.ForEnum(bit_enum_type);
				}
				else if (e.Property == FlagsEnumTypeProperty)
				{
					ui_source = BitVectorUserInterfaceData.ForFlagsEnum(bit_enum_type);
				}
			}

			ctrl.BitsUserInterfaceSource = ui_source;
		}

		private static void OnBitsUserInterfaceSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;
			var source = e.NewValue as IBitVectorUserInterfaceData;

			var newBitItems = new ObservableCollection<BitItemModel>();
			if (source != null)
			{
				for (int bit_index = 0; bit_index < source.NumberOfBits; bit_index++)
				{
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

			ctrl.BitItems = newBitItems;
			ctrl.SynchronizeBitItems();
		}

		#region OnVectorPropertyChanged
		private static void OnVectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;
			ctrl.SynchronizeBitItems();
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
					bool has_value = DependencyPropertyHelper.GetValueSource(this, BitVectorProperty).BaseValueSource
						!= BaseValueSource.Default;

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
			_ => throw new ArgumentException("Unsupported bit vector type.", nameof(vector)),
		};

		private static bool ReadBit(object vector, int bitIndex) => vector switch
		{
			Collections.BitVector32 value => value[bitIndex],
			Collections.BitVector64 value => value[bitIndex],
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
