using System.IO;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public class SignatureMismatchException : System.Exception
	{
		const string kFormat = "Invalid signature! @{0} Expected '{1}', got '{2}'";
		const string kDescFormat = "Invalid '{0}' signature! Expected '{1}', got '{2}'";

		public SignatureMismatchException(string dataDescription, string expected, string found)
			: base(string.Format(kDescFormat, dataDescription, expected, found))
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
		}

		SignatureMismatchException(long pos, string expected, string found) : 
			base(string.Format(kFormat, pos.ToString("X8"), expected, found))
		{
		}

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
		public SignatureMismatchException(Stream s, string expected, string found) :
			this(s.Position - expected.Length, expected, found)
		{
			Contract.Requires(s != null);
		}
		#endregion

		#region EndianReader utils
		public static void Assert(IO.EndianReader s, byte expected)
		{
			Contract.Requires(s != null);

			byte signature;
			s.Read(out signature);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(IO.EndianReader s, ushort expected)
		{
			Contract.Requires(s != null);

			ushort signature;
			s.Read(out signature);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(IO.EndianReader s, uint expected)
		{
			Contract.Requires(s != null);

			uint signature;
			s.Read(out signature);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(IO.EndianReader s, ulong expected)
		{
			Contract.Requires(s != null);

			ulong signature;
			s.Read(out signature);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(IO.EndianReader s, string expected, Memory.Strings.StringStorage storage)
		{
			Contract.Requires(s != null);
			Contract.Requires(!string.IsNullOrEmpty(expected));

			string signature = s.ReadString(storage, expected.Length);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		public static void Assert(IO.EndianReader s, string expected, Text.StringStorageEncoding encoding)
		{
			Contract.Requires(s != null);
			Contract.Requires(!string.IsNullOrEmpty(expected));
			Contract.Requires(encoding != null);

			string signature = s.ReadString(encoding, expected.Length);
			if (signature != expected) throw new SignatureMismatchException(s.BaseStream,
				expected, signature);
		}
		#endregion
	};
}