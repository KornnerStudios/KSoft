using System.IO;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class SignatureMismatchException
	{
		#region Stream ctors
		public SignatureMismatchException(Stream s, byte expected, byte found) :
			this(s.Position - 1, expected.ToString("X2"), found.ToString("X2"))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, ushort expected, ushort found) :
			this(s.Position - 2, expected.ToString("X4"), found.ToString("X4"))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, uint expected, uint found) :
			this(s.Position - 4, expected.ToString("X8"), found.ToString("X8"))
		{
			Contract.Requires(s != null);
		}

		public SignatureMismatchException(Stream s, ulong expected, ulong found) :
			this(s.Position - 8, expected.ToString("X16"), found.ToString("X16"))
		{
			Contract.Requires(s != null);
		}

		#endregion

		#region EndianReader util
		public static void Assert(IO.EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadByte();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt16();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt32();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		public static void Assert(IO.EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			var version = s.ReadUInt64();
			if (version != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, version);
		}

		#endregion
	};
}