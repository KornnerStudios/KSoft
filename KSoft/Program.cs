using System;

namespace KSoft
{
	internal static class KSoftConstants
	{
		/// <summary>Applied to enumeration members which act as padding. E.g., for enums which are streamed or bit-encoded and max be extended in the future so they have reserved members</summary>
		public const string kReservedMsg = "Reserved member. Don't use.";
		/// <summary>Applied to enumeration members which aren't currently supported yet in production code</summary>
		public const string kUnsupportedMsg = "Currently unsupported. Don't use.";
	};

	public static class Program
	{
		// Since static ctors in structs are pretty fucked (http://stackoverflow.com/a/3246817/444977)
		// we instead opt for explicit startup/shutdown

		public static void Initialize()
		{
			Util.ValueTypeInitializeComparer			<Memory.Strings.StringStorage>();
			Util.ValueTypeInitializeEquatableComparer	<Memory.Strings.StringStorage>();
			Util.ValueTypeInitializeComparer			<Shell.Platform>();
			Util.ValueTypeInitializeEquatableComparer	<Shell.Platform>();
			Util.ValueTypeInitializeComparer			<Shell.Processor>();
			Util.ValueTypeInitializeEquatableComparer	<Shell.Processor>();
			Util.ValueTypeInitializeComparer			<Values.KGuid>();
			Util.ValueTypeInitializeEquatableComparer	<Values.KGuid>();
			Util.ValueTypeInitializeComparer			<Values.PtrHandle>();
			Util.ValueTypeInitializeEquatableComparer	<Values.PtrHandle>();
		}

		public static void Dispose()
		{
		}
	};
}