using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.WPF
{
	public sealed class BitVectorUserInterfaceData : IBitVectorUserInterfaceData
	{
		public sealed class BitUserInterfaceData
		{
			public string DisplayName;
			public string Description;
			public bool Visible;

			public bool CanNotBeRendered { get {
				if (Visible)
					return string.IsNullOrWhiteSpace(DisplayName);

				return false;
			} }
		};
		public static bool CanNotBeRendered(BitUserInterfaceData data)
		{
			if (data == null)
				return true;

			return data.CanNotBeRendered;
		}

		private BitUserInterfaceData[] mBitInfo;

		public int NumberOfBits { get { return mBitInfo != null ? mBitInfo.Length : 0; } }

		public string GetDisplayName(int bitIndex)
		{
			var info = mBitInfo[bitIndex];

			return info != null ? info.DisplayName : bitIndex.ToString();
		}

		public string GetDescription(int bitIndex)
		{
			var info = mBitInfo[bitIndex];

			return info != null ? info.Description : string.Empty;
		}

		public bool IsVisible(int bitIndex)
		{
			var info = mBitInfo[bitIndex];

			return info != null && info.Visible;
		}

		private void SetInfoFromFactoryData(List<BitUserInterfaceData> bitInfos)
		{
			if (bitInfos.Count > 0 && !bitInfos.TrueForAll(CanNotBeRendered))
			{
				for (int x = bitInfos.Count - 1; x >= 0; x--)
				{
					if (CanNotBeRendered(bitInfos[x]))
					{
						bitInfos.RemoveAt(x);
					}
					else
						break;
				}

				mBitInfo = bitInfos.ToArray();
			}
		}

		private static void SetBitInfoFromFieldInfo(BitUserInterfaceData bitInfo, System.Reflection.FieldInfo bitFieldInfo)
		{
			var attr_display_name = bitFieldInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
			if (attr_display_name != null)
				bitInfo.DisplayName = attr_display_name.Name;
			else
				bitInfo.DisplayName = bitFieldInfo.Name;

			var attr_description = bitFieldInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
			if (attr_description != null)
				bitInfo.Description = attr_description.Description;
			else
			{
				if (attr_display_name != null)
					bitInfo.Description = attr_display_name.Description;

				if (bitInfo.Description == null)
					bitInfo.Description = string.Empty;
			}

			var attr_browsable = bitFieldInfo.GetCustomAttribute<System.ComponentModel.BrowsableAttribute>();
			if (attr_browsable != null)
				bitInfo.Visible = attr_browsable.Browsable;
			else
				bitInfo.Visible = true;
		}

		public static BitVectorUserInterfaceData ForEnum(Type enumType, int explicitNumberOfBits = TypeExtensions.kNone)
		{
			Contract.Requires<ArgumentNullException>(enumType != null);
			Contract.Requires(Reflection.Util.IsEnumType(enumType));
			Contract.Requires(explicitNumberOfBits.IsNoneOrPositive());
			Contract.Ensures(Contract.Result<BitVectorUserInterfaceData>() != null);

			var bit_field_infos = Reflection.Util.GetEnumFields(enumType);
			var bit_ui_infos = new List<BitUserInterfaceData>(Bits.kInt64BitCount);

			bool find_highest_index = explicitNumberOfBits.IsNone();
			int highest_index = explicitNumberOfBits - 1;
			foreach (var bit_field_info in bit_field_infos)
			{
				int bit_index = Convert.ToInt32(bit_field_info.GetRawConstantValue());
				if (bit_index < 0)
					continue;

				if (find_highest_index)
				{
					highest_index = System.Math.Max(highest_index, bit_index);

					if (bit_field_info.Name == EnumBitEncoderBase.kEnumMaxMemberName ||
						bit_field_info.Name == EnumBitEncoderBase.kEnumNumberOfMemberName)
					{
						highest_index--;
						find_highest_index = false;
					}
				}

				if (!find_highest_index && bit_index > highest_index)
					continue;

				bit_ui_infos.EnsureCount(bit_index + 1);
				if (bit_ui_infos[bit_index] != null)
					continue;

				var bit_ui_info = new BitUserInterfaceData();
				SetBitInfoFromFieldInfo(bit_ui_info, bit_field_info);

				if (bit_ui_info.CanNotBeRendered)
					continue;

				bit_ui_infos[bit_index] = bit_ui_info;
			}

			var info = new BitVectorUserInterfaceData();
			info.SetInfoFromFactoryData(bit_ui_infos);
			return info;
		}

		public static BitVectorUserInterfaceData ForFlagsEnum(Type enumType, int explicitNumberOfBits = TypeExtensions.kNone)
		{
			Contract.Requires<ArgumentNullException>(enumType != null);
			Contract.Requires(Reflection.Util.IsEnumType(enumType));
			Contract.Requires(explicitNumberOfBits.IsNoneOrPositive());
			Contract.Ensures(Contract.Result<BitVectorUserInterfaceData>() != null);

			var bit_field_infos = Reflection.Util.GetEnumFields(enumType);
			var bit_ui_infos = new List<BitUserInterfaceData>(Bits.kInt64BitCount);

			bool find_highest_index = explicitNumberOfBits.IsNone();
			int highest_index = explicitNumberOfBits - 1;
			foreach (var bit_field_info in bit_field_infos)
			{
				ulong flag = Convert.ToUInt64(bit_field_info.GetRawConstantValue());
				if (Bits.BitCount(flag) > 0)
					continue;

				int bit_index = Bits.IndexOfHighestBitSet(flag);

				if (find_highest_index)
				{
					highest_index = System.Math.Max(highest_index, bit_index);

					if (bit_field_info.Name == EnumBitEncoderBase.kFlagsMaxMemberName)
					{
						highest_index--;
						find_highest_index = false;
					}
				}

				if (!find_highest_index && bit_index > highest_index)
					continue;

				bit_ui_infos.EnsureCount(bit_index + 1);
				if (bit_ui_infos[bit_index] != null)
					continue;

				var bit_ui_info = new BitUserInterfaceData();
				SetBitInfoFromFieldInfo(bit_ui_info, bit_field_info);

				if (bit_ui_info.CanNotBeRendered)
					continue;

				bit_ui_infos[bit_index] = bit_ui_info;
			}

			var info = new BitVectorUserInterfaceData();
			info.SetInfoFromFactoryData(bit_ui_infos);
			return info;
		}

		public static BitVectorUserInterfaceData ForExplicitData(IEnumerable<BitUserInterfaceData> bitInfos)
		{
			Contract.Ensures(Contract.Result<BitVectorUserInterfaceData>() != null);

			var info = new BitVectorUserInterfaceData();
			if (bitInfos != null)
			{
				info.mBitInfo = bitInfos.ToArray();
				if (info.mBitInfo.Length == 0 || Array.TrueForAll(info.mBitInfo, CanNotBeRendered))
					info.mBitInfo = null;
			}
			return info;
		}

		public static BitVectorUserInterfaceData ForStrings(IEnumerable<string> bitStrings)
		{
			Contract.Ensures(Contract.Result<BitVectorUserInterfaceData>() != null);

			var info = new BitVectorUserInterfaceData();
			if (bitStrings != null)
			{
				int bit_index = 0;
				var bit_ui_infos = new List<BitUserInterfaceData>(Bits.kInt64BitCount);
				foreach (var str in bitStrings)
				{
					var bit_ui_info = new BitUserInterfaceData();
					bit_ui_info.DisplayName = str;
					bit_ui_info.Visible = true;

					if (bit_ui_info.CanNotBeRendered)
						bit_ui_infos.Add(null);
					else
						bit_ui_infos.Add(bit_ui_info);

					bit_index++;
				}

				info.SetInfoFromFactoryData(bit_ui_infos);
			}
			return info;
		}
	};
}
