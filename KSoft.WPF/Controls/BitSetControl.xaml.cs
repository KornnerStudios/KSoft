using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace KSoft.WPF.Controls
{
	using BitItemModel = BitVectorControl.BitItemModel;

	/// <summary>
	/// Interaction logic for BitSetControl.xaml
	/// </summary>
	public partial class BitSetControl : UserControl
	{
		#region BitItems
		public ObservableCollection<BitItemModel> BitItems
		{
			get { return (ObservableCollection<BitItemModel>)GetValue(BitItemsProperty); }
			set { SetValue(BitItemsProperty, value); }
		}
		public static readonly DependencyProperty BitItemsProperty = DependencyProperty.Register(
			nameof(BitItems), typeof(ObservableCollection<BitItemModel>), typeof(BitSetControl),
			new PropertyMetadata(new ObservableCollection<BitItemModel>()));
		#endregion

		#region BitEnumType
		public Type BitsEnumType
		{
			get { return (Type)GetValue(BitsEnumTypeProperty); }
			set { SetValue(BitsEnumTypeProperty, value); }
		}
		public static readonly DependencyProperty BitsEnumTypeProperty = DependencyProperty.Register(
			nameof(BitsEnumType), typeof(Type), typeof(BitSetControl),
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
			nameof(BitsUserInterfaceSource), typeof(IBitVectorUserInterfaceData), typeof(BitSetControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnBitsUserInterfaceSourcePropertyChanged)));
		#endregion

		#region BitVector
		public Collections.BitSet BitVector
		{
			get { return (Collections.BitSet)GetValue(BitVectorProperty); }
			set { SetValue(BitVectorProperty, value); }
		}
		public static readonly DependencyProperty BitVectorProperty = DependencyProperty.Register(
			nameof(BitVector), typeof(Collections.BitSet), typeof(BitSetControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorPropertyChanged));
		#endregion

		public BitSetControl()
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
			if (bit_vector == null)
				return;

			int bit_index = bitModel.BitIndex;
			if (bit_index >= bit_vector.Length)
			{
				throw new ArgumentOutOfRangeException(string.Format(
					"Vector length is {0} which is less than the number of bits needed to represent one or more items. #{1} can't be updated ({2})",
					bit_vector.Length, bit_index, bitModel.DisplayName));
			}
			else if (bit_vector[bit_index] != newValue)
			{
				bit_vector[bit_index] = newValue;
			}
		}
		#endregion

		private void AssignItems(Collections.BitSet newBits)
		{
			if (newBits == null)
			{
				foreach (var bit_model in BitItems)
					if (bit_model.IsSet)
						bit_model.IsSet = false;
			}
			else
			{
				foreach (var bit_model in BitItems)
				{
					if (!bit_model.IsValid)
						continue;

					int bit_index = bit_model.BitIndex;

					if (bit_index >= newBits.Length)
					{
						throw new ArgumentOutOfRangeException(string.Format(
							"newBits length is {0} which is less than the number of bits needed to represent one or more items. #{1} can't be updated ({2})",
							newBits.Length, bit_index, bit_model.DisplayName));
					}

					bool old_bit = bit_model.IsSet;
					bool new_bit = newBits[bit_index];
					if (old_bit != new_bit)
						bit_model.IsSet = new_bit;
				}
			}
		}

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
			var ctrl = (BitSetControl)d;

			var bit_enum_type = (Type)e.NewValue;

			IBitVectorUserInterfaceData ui_source = null;
			if (bit_enum_type != null)
			{
				if (e.Property == BitsEnumTypeProperty)
					ui_source = BitVectorUserInterfaceData.ForEnum(bit_enum_type);
			}

			ctrl.BitsUserInterfaceSource = ui_source;
		}

		private static void OnBitsUserInterfaceSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitSetControl)d;
			var source = (IBitVectorUserInterfaceData)e.NewValue;

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
		}

		private static void OnVectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (BitSetControl)d;

			var vold = (Collections.BitSet)e.OldValue;
			var vnew = (Collections.BitSet)e.NewValue;
			if (vold == vnew)
				return;

			ctrl.AssignItems(vnew);
		}
	}
}
