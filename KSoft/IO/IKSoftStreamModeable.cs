using System;
using System.IO;

namespace KSoft.IO
{
	// For the lack of a better name...

	/// <summary>Exposes data streaming state information and control</summary>
	public interface IKSoftStreamModeable
	{
		/// <summary>Supported access permissions for the stream</summary>
		FileAccess StreamPermissions { get; }

		/// <summary>Current data streaming state</summary>
		/// <remarks>Read or Write, not both</remarks>
		FileAccess StreamMode { get; set; }
	};

	internal static class StreamModeUtil
	{
		const string kUnsupportedAccessModeMessage = "Stream doesn't support the requested access mode";

		public static void ValidateMode(FileAccess value, FileAccess permissions)
		{
			ArgumentOutOfRangeException.ThrowIfNegative((int)value, nameof(value));
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)value, (int)FileAccess.ReadWrite, nameof(value));
			if ((permissions & value) != value)
				throw new InvalidOperationException(kUnsupportedAccessModeMessage);
		}

		public static FileAccess ToInitialMode(FileAccess permissions)
		{
			ArgumentOutOfRangeException.ThrowIfNegative((int)permissions, nameof(permissions));
			ArgumentOutOfRangeException.ThrowIfGreaterThan((int)permissions, (int)FileAccess.ReadWrite, nameof(permissions));

			return permissions == FileAccess.ReadWrite
				? 0
				: permissions;
		}
	};
}
