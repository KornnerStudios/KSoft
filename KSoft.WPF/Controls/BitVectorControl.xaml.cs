using System;
using System.Collections.ObjectModel;
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
		public Type BitsEnumType
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
		public Type FlagsEnumType
		{
			get { return (Type)GetValue(FlagsEnumTypeProperty); }
			set { SetValue(FlagsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty FlagsEnumTypeProperty = DependencyProperty.Register(
			nameof(FlagsEnumType), typeof(Type), typeof(BitVectorControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnBitEnumTypePropertyChanged)),
			Reflection.Util.IsEnumTypeOrNull);
		#endregion

		#region BitsUserInterfaceSource
		public IBitVectorUserInterfaceData BitsUserInterfaceSource
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
				return false;

			if (obj is Collections.BitVector32 ||
				obj is Collections.BitVector64)
				return true;

			return false;
		}
		#endregion

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
			var bit_vector = BitVector;
			if (bit_vector is Collections.BitVector32)
			{
				var vector = (Collections.BitVector32)bit_vector;

				if (vector[bitModel.BitIndex] != newValue)
				{
					vector[bitModel.BitIndex] = newValue;
					BitVector = vector;
				}
			}
			else if (bit_vector is Collections.BitVector64)
			{
				var vector = (Collections.BitVector64)bit_vector;

				if (vector[bitModel.BitIndex] != newValue)
				{
					vector[bitModel.BitIndex] = newValue;
					BitVector = vector;
				}
			}
		}
		#endregion

		public void ForceVisibilityRefreshOfAllBitItems()
		{
			var source = this.BitsUserInterfaceSource;
			if (source == null)
				return;

			foreach (var bit_model in BitItems)
			{
				int bit_index = bit_model.BitIndex;
				bit_model.IsVisible = source.IsVisible(bit_index);
			}
		}

		private static void OnBitEnumTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;

			var bit_enum_type = (Type)e.NewValue;

			IBitVectorUserInterfaceData ui_source = null;
			if (bit_enum_type != null)
			{
				if (e.Property == BitsEnumTypeProperty)
					ui_source = BitVectorUserInterfaceData.ForEnum(bit_enum_type);
				else if (e.Property == FlagsEnumTypeProperty)
					ui_source = BitVectorUserInterfaceData.ForFlagsEnum(bit_enum_type);
			}

			ctrl.BitsUserInterfaceSource = ui_source;
		}

		private static void OnBitsUserInterfaceSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;
			var source = (IBitVectorUserInterfaceData)e.NewValue;

			ctrl.BitItems.Clear();
			if (source != null)
			{
				for (int bit_index = 0; bit_index < source.NumberOfBits; bit_index++)
				{
					var model = new BitItemModel();
					model.BitIndex = bit_index;
					model.DisplayName = source.GetDisplayName(bit_index);
					model.ToolTip = source.GetDescription(bit_index);
					model.IsVisible = source.IsVisible(bit_index);
					ctrl.BitItems.Add(model);
				}
			}
		}

		#region OnVectorPropertyChanged
		private static void OnVectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitVectorControl)d;

			Type vector_type = null;
			if (e.OldValue != null)
				vector_type = e.OldValue.GetType();
			else if (e.NewValue != null)
				vector_type = e.NewValue.GetType();
			else
				return;

			if (vector_type == typeof(Collections.BitVector32))
			{
				OnVector32PropertyChanged(ctrl, e);
			}
			else if (vector_type == typeof(Collections.BitVector32))
			{
				OnVector64PropertyChanged(ctrl, e);
			}
		}
		private static void OnVector32PropertyChanged(BitVectorControl d, DependencyPropertyChangedEventArgs e)
		{
			var vold = (Collections.BitVector32)e.OldValue;
			var vnew = (Collections.BitVector32)e.NewValue;
			if (vold == vnew)
				return;

			// optimize for the case were only one bit was changed
			var vdiff = vold.Xor(vnew);
			if (vdiff.Cardinality == 1)
			{
				int changed_bit_index = vdiff.NextSetBitIndex();

				bool bit_value = vnew[changed_bit_index];
				var model = d.BitItems[changed_bit_index];
				model.IsSet = bit_value;
			}
			else
			{
				foreach (var changed_bit_index in vdiff.SetBitIndices)
				{
					bool bit_value = vnew[changed_bit_index];
					var model = d.BitItems[changed_bit_index];
					model.IsSet = bit_value;
				}
			}
		}
		private static void OnVector64PropertyChanged(BitVectorControl d, DependencyPropertyChangedEventArgs e)
		{
			var vold = (Collections.BitVector64)e.OldValue;
			var vnew = (Collections.BitVector64)e.NewValue;
			if (vold == vnew)
				return;

			// optimize for the case were only one bit was changed
			var vdiff = vold.Xor(vnew);
			if (vdiff.Cardinality == 1)
			{
				int changed_bit_index = vdiff.NextSetBitIndex();

				bool bit_value = vnew[changed_bit_index];
				var model = d.BitItems[changed_bit_index];
				model.IsSet = bit_value;
			}
			else
			{
				foreach (var changed_bit_index in vdiff.SetBitIndices)
				{
					bool bit_value = vnew[changed_bit_index];
					var model = d.BitItems[changed_bit_index];
					model.IsSet = bit_value;
				}
			}
		}
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
