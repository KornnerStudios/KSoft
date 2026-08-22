using System;

namespace KSoft.IO
{
	/// <summary>Base type for enum bit streamers.</summary>
	public abstract class EnumBitStreamerBase
	{
	};

	public static class EnumBitStreamer
	{
		public static IEnumBitStreamer<TEnum> For<TEnum, TStreamType, TOptions>()
			where TEnum : struct, Enum
			where TStreamType : struct
			where TOptions : EnumBitStreamerOptions, new()
		{

			return EnumBitStreamer<TEnum, TStreamType, TOptions>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, Enum
			where TStreamType : struct
		{

			return EnumBitStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum>()
			where TEnum : struct, Enum
		{

			return EnumBitStreamer<TEnum>.Instance;
		}

		public static IEnumBitStreamer<TEnum> ForWithOptions<TEnum, TOptions>()
			where TEnum : struct, Enum
			where TOptions : EnumBitStreamerOptions, new()
		{

			return EnumBitStreamerWithOptions<TEnum, TOptions>.Instance;
		}
	};
}
