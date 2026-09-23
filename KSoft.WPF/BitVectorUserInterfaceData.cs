using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace KSoft.WPF
{
	/// <summary>Presentation labels, descriptions, and visibility indexed by bit position.</summary>
	/// <remarks>For enum-derived metadata, no <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> means the C# member-name fallback. <see cref="System.ComponentModel.DescriptionAttribute"/> overrides the description in DisplayAttribute; an absent description gives empty text. <see cref="System.ComponentModel.BrowsableAttribute"/> with Browsable=false hides presentation without invalidating a bit. An otherwise visible member with an explicitly blank display name is not renderable. Its absent slot can be trimmed, or retain an index-label fallback; no visible blank checkbox is promised.</remarks>
	public sealed class BitVectorUserInterfaceData : IBitVectorUserInterfaceData
	{
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public sealed class BitUserInterfaceData
		{
			public string? DisplayName;
			public string? Description;
			public bool Visible;

			public bool CanNotBeRendered { get {
				if (Visible)
				{
					return string.IsNullOrWhiteSpace(DisplayName);
				}

				return false;
			} }
		};
		public static bool CanNotBeRendered(BitUserInterfaceData? data)
		{
			if (data == null)
			{
				return true;
			}

			return data.CanNotBeRendered;
		}

		private BitUserInterfaceData?[]? mBitInfo;

		/// <inheritdoc/>
		public int NumberOfBits { get { return mBitInfo != null ? mBitInfo.Length : 0; } }

		private void ValidateBitIndex(int bitIndex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(bitIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, NumberOfBits, nameof(bitIndex));
		}

		/// <summary>Gets the stored label, or the invariant decimal index for an absent label in a retained slot.</summary>
		/// <param name="bitIndex">An index inside <see cref="NumberOfBits"/>.</param>
		/// <returns>The presentation label.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is outside the metadata bound.</exception>
		public string GetDisplayName(int bitIndex)
		{
			ValidateBitIndex(bitIndex);

			var info = mBitInfo![bitIndex];

			return info?.DisplayName ?? bitIndex.ToString(KSoft.Util.InvariantCultureInfo);
		}

		/// <inheritdoc/>
		public string GetDescription(int bitIndex)
		{
			ValidateBitIndex(bitIndex);

			var info = mBitInfo![bitIndex];

			return info?.Description ?? string.Empty;
		}

		/// <inheritdoc/>
		public bool IsVisible(int bitIndex)
		{
			ValidateBitIndex(bitIndex);

			var info = mBitInfo![bitIndex];

			return info != null && info.Visible;
		}

		private static void ValidateEnumFactoryArguments(Type enumType, int explicitNumberOfBits)
		{
			ArgumentNullException.ThrowIfNull(enumType);
			if (!Reflection.Util.IsEnumType(enumType))
			{
				throw new ArgumentException("Type must be an enum.", nameof(enumType));
			}
			if (!explicitNumberOfBits.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(explicitNumberOfBits));
			}
		}

		private void SetInfoFromFactoryData(List<BitUserInterfaceData?> bitInfos)
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
					{
						break;
					}
				}

				mBitInfo = bitInfos.ToArray();
			}
		}

		private static void SetBitInfoFromFieldInfo(BitUserInterfaceData bitInfo, System.Reflection.FieldInfo bitFieldInfo)
		{
			var attr_display_name = bitFieldInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
			if (attr_display_name != null)
			{
				bitInfo.DisplayName = attr_display_name.Name;
			}
			else
			{
				bitInfo.DisplayName = bitFieldInfo.Name;
			}

			var attr_description = bitFieldInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
			if (attr_description != null)
			{
				bitInfo.Description = attr_description.Description;
			}
			else
			{
				if (attr_display_name != null)
				{
					bitInfo.Description = attr_display_name.Description;
				}

				if (bitInfo.Description == null)
				{
					bitInfo.Description = string.Empty;
				}
			}

			bitInfo.Visible = IsEnumMemberVisible(bitFieldInfo);
		}

		// Presentation policy is separate from display text and whether a member is a usable bit.
		private static bool IsEnumMemberVisible(System.Reflection.FieldInfo field) =>
			field.GetCustomAttribute<System.ComponentModel.BrowsableAttribute>()?.Browsable ?? true;

		/// <summary>Builds typed-vector presentation from its traits-backed member indices and canonical names.</summary>
		/// <param name="vector">The typed adapter supplying membership and canonical names.</param>
		/// <returns>Attribute-derived presentation metadata; visibility does not change membership.</returns>
		/// <remarks>Only the canonical alias supplies attributes. A later, more displayable alias is not substituted when the canonical member lacks a renderable label.</remarks>
		internal static BitVectorUserInterfaceData ForVector(Collections.IEnumBitVector vector)
		{
			var enumType = vector.BitsEnumType;
			var bitInfos = new List<BitUserInterfaceData?>(vector.Length);
			for (int index = 0; index < vector.Length; index++)
			{
				BitUserInterfaceData? info = null;
				if (vector.IsDefinedIndex(index))
				{
					var name = vector.GetBitName(index);
					var field = enumType.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
						?? throw new InvalidOperationException($"The bit member '{name}' was not found in {enumType}.");
					info = new BitUserInterfaceData();
					SetBitInfoFromFieldInfo(info, field);
					if (info.CanNotBeRendered)
					{
						info = null;
					}
				}
				bitInfos.Add(info);
			}
			var source = new BitVectorUserInterfaceData();
			source.SetInfoFromFactoryData(bitInfos);
			return source;
		}

		/// <summary>Builds legacy index-based UI metadata directly from enum declarations.</summary>
		/// <param name="enumType">The enum containing numeric bit positions.</param>
		/// <param name="explicitNumberOfBits">An optional exclusive metadata bound, or -1 to derive it from declarations.</param>
		/// <returns>Presentation metadata, possibly with trailing absent slots trimmed.</returns>
		/// <remarks>This legacy factory is not an EnumBitTraits domain validator and may choose a later renderable alias. Inferred typed-vector metadata instead uses the traits' canonical member.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="enumType"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="enumType"/> is not an enum.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="explicitNumberOfBits"/> is less than -1.</exception>
		/// <exception cref="OverflowException">A nonnegative member value cannot be represented as an int index.</exception>
		public static BitVectorUserInterfaceData ForEnum(Type enumType, int explicitNumberOfBits = TypeExtensions.kNone)
		{
			ValidateEnumFactoryArguments(enumType, explicitNumberOfBits);

			var bit_field_infos = Reflection.Util.GetEnumFields(enumType);
			bit_field_infos.Sort(static (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
			var bit_ui_infos = new List<BitUserInterfaceData?>(Bits.kInt64BitCount);

			bool find_highest_index = explicitNumberOfBits.IsNone();
			int highest_index = explicitNumberOfBits - 1;
			foreach (var bit_field_info in bit_field_infos)
			{
				decimal numeric_index = Convert.ToDecimal(bit_field_info.GetRawConstantValue(), Util.InvariantCultureInfo);
				if (numeric_index < 0)
				{
					continue;
				}
				int bit_index = checked((int)numeric_index);

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
				{
					continue;
				}

				bit_ui_infos.EnsureCount(bit_index + 1);
				if (bit_ui_infos[bit_index] != null)
				{
					continue;
				}

				var bit_ui_info = new BitUserInterfaceData();
				SetBitInfoFromFieldInfo(bit_ui_info, bit_field_info);

				if (bit_ui_info.CanNotBeRendered)
				{
					continue;
				}

				bit_ui_infos[bit_index] = bit_ui_info;
			}

			var info = new BitVectorUserInterfaceData();
			info.SetInfoFromFactoryData(bit_ui_infos);
			return info;
		}

		public static BitVectorUserInterfaceData ForFlagsEnum(Type enumType, int explicitNumberOfBits = TypeExtensions.kNone)
		{
			ValidateEnumFactoryArguments(enumType, explicitNumberOfBits);

			var bit_field_infos = Reflection.Util.GetEnumFields(enumType);
			var bit_ui_infos = new List<BitUserInterfaceData?>(Bits.kInt64BitCount);

			bool find_highest_index = explicitNumberOfBits.IsNone();
			int highest_index = explicitNumberOfBits - 1;
			foreach (var bit_field_info in bit_field_infos)
			{
				ulong flag = Convert.ToUInt64(bit_field_info.GetRawConstantValue(), Util.InvariantCultureInfo);
				if (System.Numerics.BitOperations.PopCount(flag) > 0)
				{
					continue;
				}

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
				{
					continue;
				}

				bit_ui_infos.EnsureCount(bit_index + 1);
				if (bit_ui_infos[bit_index] != null)
				{
					continue;
				}

				var bit_ui_info = new BitUserInterfaceData();
				SetBitInfoFromFieldInfo(bit_ui_info, bit_field_info);

				if (bit_ui_info.CanNotBeRendered)
				{
					continue;
				}

				bit_ui_infos[bit_index] = bit_ui_info;
			}

			var info = new BitVectorUserInterfaceData();
			info.SetInfoFromFactoryData(bit_ui_infos);
			return info;
		}

		public static BitVectorUserInterfaceData ForExplicitData(IEnumerable<BitUserInterfaceData?>? bitInfos)
		{
			var info = new BitVectorUserInterfaceData();
			if (bitInfos != null)
			{
				info.mBitInfo = bitInfos.ToArray();
				if (info.mBitInfo.Length == 0 || Array.TrueForAll(info.mBitInfo, CanNotBeRendered))
				{
					info.mBitInfo = null;
				}
			}
			return info;
		}

		public static BitVectorUserInterfaceData ForStrings(IEnumerable<string?>? bitStrings)
		{
			var info = new BitVectorUserInterfaceData();
			if (bitStrings != null)
			{
				int bit_index = 0;
				var bit_ui_infos = new List<BitUserInterfaceData?>(Bits.kInt64BitCount);
				foreach (var str in bitStrings)
				{
					var bit_ui_info = new BitUserInterfaceData
					{
						DisplayName = str,
						Visible = true
					};

					if (bit_ui_info.CanNotBeRendered)
					{
						bit_ui_infos.Add(null);
					}
					else
					{
						bit_ui_infos.Add(bit_ui_info);
					}

					bit_index++;
				}

				info.SetInfoFromFactoryData(bit_ui_infos);
			}
			return info;
		}
	};
}
