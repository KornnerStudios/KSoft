using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

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
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum, TStreamType, TOptions>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, Enum
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum>()
			where TEnum : struct, Enum
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum>.Instance;
		}

		public static IEnumBitStreamer<TEnum> ForWithOptions<TEnum, TOptions>()
			where TEnum : struct, Enum
			where TOptions : EnumBitStreamerOptions, new()
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamerWithOptions<TEnum, TOptions>.Instance;
		}
	};
}
