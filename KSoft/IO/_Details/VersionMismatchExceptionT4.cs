using System.IO;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class VersionMismatchException
	{
		#region UInt32 ctors
		public VersionMismatchException(string dataDescription, uint found)
			: base(string.Format("Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, uint expected, uint found)
			: base(string.Format(kDescFormat, dataDescription, expected, found, VersionCompareDesc(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion
		#region Int32 ctors
		public VersionMismatchException(string dataDescription, int found)
			: base(string.Format("Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, int expected, int found)
			: base(string.Format(kDescFormat, dataDescription, expected, found, VersionCompareDesc(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion

		#region Stream ctors
		public VersionMismatchException(Stream s, byte expected, byte found) :
			this(s.Position - 1, VersionCompareDesc(expected, found), expected.ToString("X2"), found.ToString("X2"))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, ushort expected, ushort found) :
			this(s.Position - 2, VersionCompareDesc(expected, found), expected.ToString("X4"), found.ToString("X4"))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, uint expected, uint found) :
			this(s.Position - 4, VersionCompareDesc(expected, found), expected.ToString("X8"), found.ToString("X8"))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, ulong expected, ulong found) :
			this(s.Position - 8, VersionCompareDesc(expected, found), expected.ToString("X16"), found.ToString("X16"))
		{
			Contract.Requires(s != null);
		}

		#endregion

		#region EndianReader util
		public static void Assert(IO.EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadByte();
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt16();
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt32();
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt64();
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}

		#endregion
	};
}