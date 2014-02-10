using System.IO;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public class VersionMismatchException : System.Exception
	{
		const string kFormat = "Invalid version! @{0} Expected '{1}', got '{2}' ({3} data)";
		const string kDescFormat = "Invalid '{0}' version! Expected '{1}', got '{2}' ({3} data)";

		static string CmpStr(long expected, long found)
		{
			if (found > expected)
				return "newer";

			return "older";
		}
		static string CmpStr(ulong expected, ulong found)
		{
			if (found > expected)
				return "newer";

			return "older";
		}

		#region Int32 ctors
		public VersionMismatchException(string dataDescription, int found)
			: base(string.Format("Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, int expected, int found)
			: base(string.Format(kDescFormat, dataDescription, expected, found, CmpStr(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion
		#region UInt32 ctors
		public VersionMismatchException(string dataDescription, uint found)
			: base(string.Format("Invalid '{0}' version '{1}'!", dataDescription, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		public VersionMismatchException(string dataDescription, uint expected, uint found)
			: base(string.Format(kDescFormat, dataDescription, expected, found, CmpStr(expected, found)))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}
		#endregion

		VersionMismatchException(long pos, string cmp, string expected, string found) :
			base(string.Format(kFormat, pos.ToString("X8"), expected, found, cmp))
		{
		}
		#region Stream ctors
		public VersionMismatchException(Stream s, byte expected, byte found) :
			this(s.Position - 1, CmpStr(expected, found), expected.ToString("X2"), found.ToString("X2"))
		{
			Contract.Requires(s != null);
		}
		public VersionMismatchException(Stream s, ushort expected, ushort found) :
			this(s.Position - 2, CmpStr(expected, found), expected.ToString("X4"), found.ToString("X4"))
		{
			Contract.Requires(s != null);
		}
		public VersionMismatchException(Stream s, uint expected, uint found) :
			this(s.Position - 4, CmpStr(expected, found), expected.ToString("X8"), found.ToString("X8"))
		{
			Contract.Requires(s != null);
		}

		public VersionMismatchException(Stream s, ulong expected, ulong found) :
			this(s.Position - 8, CmpStr(expected, found), expected.ToString("X16"), found.ToString("X16"))
		{
			Contract.Requires(s != null);
		}
		#endregion

		#region EndianReader util
		public static void Assert(IO.EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			byte version;
			s.Read(out version);
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}
		public static void Assert(IO.EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			ushort version;
			s.Read(out version);
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}
		public static void Assert(IO.EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			uint version;
			s.Read(out version);
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}
		public static void Assert(IO.EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			ulong version;
			s.Read(out version);
			if (version != expected) throw new VersionMismatchException(s.BaseStream,
				expected, version);
		}
		#endregion
	};
}